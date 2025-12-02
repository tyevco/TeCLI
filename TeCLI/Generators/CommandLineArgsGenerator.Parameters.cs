using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using TeCLI.Extensions;
using static TeCLI.Constants;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TeCLI.Generators;

public partial class CommandLineArgsGenerator
{
    /// <summary>
    /// Generates parameter parsing statements (Roslyn syntax version)
    /// </summary>
    public void GenerateParameterStatements(List<StatementSyntax> statements, IMethodSymbol methodSymbol, string methodInvokerName, GlobalOptionsSourceInfo? globalOptions = null)
    {
        var parameterDetails = ParameterInfoExtractor.GetParameterDetails(methodSymbol.Parameters);

        // Check if any parameter is the global options type
        ParameterSourceInfo? globalOptionsParam = null;
        if (globalOptions != null)
        {
            globalOptionsParam = parameterDetails.FirstOrDefault(p =>
                p.DisplayType == globalOptions.FullTypeName ||
                p.DisplayType == globalOptions.TypeName);

            if (globalOptionsParam != null)
            {
                parameterDetails.Remove(globalOptionsParam);
            }
        }

        // Check if the method has a CancellationToken parameter
        bool hasCancellationToken = methodSymbol.HasCancellationTokenParameter();

        if (parameterDetails.Count == 0)
        {
            var methodParams = new List<string>();
            if (globalOptionsParam != null) methodParams.Add("_globalOptions");
            if (hasCancellationToken) methodParams.Add("cancellationToken");
            var methodParamsStr = string.Join(", ", methodParams);

            var invokeStatement = methodSymbol.MapAsync(
                () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({methodParamsStr}), cancellationToken);",
                () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({methodParamsStr}), cancellationToken);");
            statements.Add(ParseStatement(invokeStatement));
        }
        else
        {
            // Generate the if/else structure using CodeBuilder internally, then parse the result
            var cb = new CodeBuilder();
            GenerateParameterCodeInternal(cb, methodSymbol, methodInvokerName, parameterDetails, globalOptionsParam, hasCancellationToken);

            // Parse all the statements from the CodeBuilder
            var code = cb.ToString();
            if (!string.IsNullOrWhiteSpace(code))
            {
                // Wrap in a block to parse multiple statements
                var parsed = ParseStatement($"{{ {code} }}");
                if (parsed is BlockSyntax block)
                {
                    statements.AddRange(block.Statements);
                }
                else
                {
                    statements.Add(parsed);
                }
            }
        }
    }

    private void GenerateParameterCodeInternal(CodeBuilder cb, IMethodSymbol methodSymbol, string methodInvokerName,
        List<ParameterSourceInfo> parameterDetails, ParameterSourceInfo? globalOptionsParam, bool hasCancellationToken = false)
    {
        // Check if there are required parameters without environment variable fallback
        // If all required params have env vars, we should try parsing even if args.Length == 0
        var requiredParams = parameterDetails.Where(p => p.Required).ToList();
        var hasRequiredWithoutEnvVar = parameterDetails.Any(p => p.Required && string.IsNullOrEmpty(p.EnvVar));

        if (hasRequiredWithoutEnvVar)
        {
            // Only throw early if there are required params that can't be satisfied by env vars
            using (cb.AddBlock("if (args.Length == 0)"))
            {
                // Throw error with the first required option name
                var firstRequired = requiredParams.First();
                cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.RequiredOptionNotProvided}", "{firstRequired.Name}"));""");
            }
            cb.AddBlankLine();
        }

        // The parsing code - always runs (will check env vars for params that have them)
        // When hasRequiredWithoutEnvVar is true, this code is only reached if args.Length > 0
        // When hasRequiredWithoutEnvVar is false, this code always runs (env vars may satisfy requirements)
        bool hasOptionalValues = false;
        using (cb.AddBlankScope())
        {
            var optionParameters = parameterDetails.Where(p => p.ParameterType == ParameterType.Option).ToList();
            if (optionParameters.Any())
            {
                cb.AppendLine("// Valid options for this action");
                cb.Append("var validOptions = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { ");
                var validOptions = new List<string>();
                foreach (var param in optionParameters)
                {
                    validOptions.Add($"\"--{param.Name}\"");
                    if (param.ShortName != '\0')
                    {
                        validOptions.Add($"\"-{param.ShortName}\"");
                    }
                }
                cb.Append(string.Join(", ", validOptions));
                cb.AppendLine(" };");
                cb.AddBlankLine();
            }

            foreach (var parameterDetail in parameterDetails)
            {
                var variableName = $"p{parameterDetail.ParameterIndex}";
                ParameterCodeGenerator.GenerateParameterParsingCode(cb, methodSymbol, parameterDetail, variableName);
                if (parameterDetail.Optional)
                {
                    hasOptionalValues = true;
                }
            }

            if (optionParameters.Any())
            {
                cb.AddBlankLine();
                cb.AppendLine("// Check for unknown options");
                using (cb.AddBlock("foreach (var arg in args)"))
                {
                    using (cb.AddBlock("if ((arg.StartsWith(\"--\") || arg.StartsWith(\"-\")) && !validOptions.Contains(arg))"))
                    {
                        cb.AppendLine("var optionNames = validOptions.Select(o => o).ToArray();");
                        cb.AppendLine("var suggestion = TeCLI.StringSimilarity.FindMostSimilar(arg, optionNames);");
                        using (cb.AddBlock("if (suggestion != null)"))
                        {
                            cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.UnknownOptionWithSuggestion.Replace("\n", "\\n")}", arg, suggestion));""");
                        }
                        using (cb.AddBlock("else"))
                        {
                            cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.UnknownOption}", arg));""");
                        }
                    }
                }
            }

            cb.AddBlankLine();
            if (hasOptionalValues)
            {
                cb.AppendLine($"// Build parameter list with optional values handled via ternary operators");

                List<string> allParams = [];

                if (globalOptionsParam != null)
                {
                    allParams.Add($"{globalOptionsParam.ParameterName}: _globalOptions");
                }

                foreach (var param in parameterDetails)
                {
                    if (param.Required)
                    {
                        allParams.Add($"{param.ParameterName}: p{param.ParameterIndex}");
                    }
                    else if (param.IsSwitch)
                    {
                        allParams.Add($"{param.ParameterName}: p{param.ParameterIndex}");
                    }
                    else
                    {
                        var defaultValue = param.DefaultValue ?? $"default({param.DisplayType})";
                        allParams.Add($"{param.ParameterName}: p{param.ParameterIndex}Set ? p{param.ParameterIndex} : {defaultValue}");
                    }
                }

                // Add CancellationToken if the method accepts it
                if (hasCancellationToken)
                {
                    var ctParam = methodSymbol.GetCancellationTokenParameter();
                    if (ctParam != null)
                    {
                        allParams.Add($"{ctParam.Name}: cancellationToken");
                    }
                }

                cb.AppendLine(
                        methodSymbol.MapAsync(
                            () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({string.Join(", ", allParams)}), cancellationToken);",
                            () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({string.Join(", ", allParams)}), cancellationToken);"));
            }
            else
            {
                cb.AppendLine($"// Now invoke the method with the parsed parameters");

                var invokeParams = new List<string>();
                if (globalOptionsParam != null)
                {
                    invokeParams.Add("_globalOptions");
                }
                invokeParams.AddRange(parameterDetails.Select(p => $"p{p.ParameterIndex}"));

                // Add CancellationToken if the method accepts it
                if (hasCancellationToken)
                {
                    invokeParams.Add("cancellationToken");
                }

                cb.AppendLine(
                        methodSymbol.MapAsync(
                            () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({string.Join(", ", invokeParams)}), cancellationToken);",
                            () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({string.Join(", ", invokeParams)}), cancellationToken);"));
            }
        }
    }

    // Legacy CodeBuilder version still used by Actions.cs
    public void GenerateParameterCode(CodeBuilder cb, IMethodSymbol methodSymbol, string methodInvokerName, GlobalOptionsSourceInfo? globalOptions = null)
    {
        var parameterDetails = ParameterInfoExtractor.GetParameterDetails(methodSymbol.Parameters);

        // Check if any parameter is the global options type
        ParameterSourceInfo? globalOptionsParam = null;
        if (globalOptions != null)
        {
            globalOptionsParam = parameterDetails.FirstOrDefault(p =>
                p.DisplayType == globalOptions.FullTypeName ||
                p.DisplayType == globalOptions.TypeName);

            if (globalOptionsParam != null)
            {
                parameterDetails.Remove(globalOptionsParam);
            }
        }

        // Check if the method has a CancellationToken parameter
        bool hasCancellationToken = methodSymbol.HasCancellationTokenParameter();

        if (parameterDetails.Count == 0)
        {
            var methodParams = new List<string>();
            if (globalOptionsParam != null) methodParams.Add("_globalOptions");
            if (hasCancellationToken) methodParams.Add("cancellationToken");
            var methodParamsStr = string.Join(", ", methodParams);

            cb.AppendLine(
                    methodSymbol.MapAsync(
                        () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({methodParamsStr}), cancellationToken);",
                        () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({methodParamsStr}), cancellationToken);"));
        }
        else
        {
            // Check if there are required parameters without environment variable fallback
            // If all required params have env vars, we should try parsing even if args.Length == 0
            var requiredParams = parameterDetails.Where(p => p.Required).ToList();
            var hasRequiredWithoutEnvVar = parameterDetails.Any(p => p.Required && string.IsNullOrEmpty(p.EnvVar));

            if (hasRequiredWithoutEnvVar)
            {
                // Only throw early if there are required params that can't be satisfied by env vars
                using (cb.AddBlock("if (args.Length == 0)"))
                {
                    // Throw error with the first required option name
                    var firstRequired = requiredParams.First();
                    cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.RequiredOptionNotProvided}", "{firstRequired.Name}"));""");
                }
                cb.AddBlankLine();
            }

            // The parsing code - always runs (will check env vars for params that have them)
            // When hasRequiredWithoutEnvVar is true, this code is only reached if args.Length > 0
            // When hasRequiredWithoutEnvVar is false, this code always runs (env vars may satisfy requirements)
            bool hasOptionalValues = false;
            using (cb.AddBlankScope())
            {
                var optionParameters = parameterDetails.Where(p => p.ParameterType == ParameterType.Option).ToList();
                if (optionParameters.Any())
                {
                    cb.AppendLine("// Valid options for this action");
                    cb.Append("var validOptions = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { ");
                    var validOptions = new List<string>();
                    foreach (var param in optionParameters)
                    {
                        validOptions.Add($"\"--{param.Name}\"");
                        if (param.ShortName != '\0')
                        {
                            validOptions.Add($"\"-{param.ShortName}\"");
                        }
                    }
                    cb.Append(string.Join(", ", validOptions));
                    cb.AppendLine(" };");
                    cb.AddBlankLine();
                }

                foreach (var parameterDetail in parameterDetails)
                {
                    var variableName = $"p{parameterDetail.ParameterIndex}";
                    ParameterCodeGenerator.GenerateParameterParsingCode(cb, methodSymbol, parameterDetail, variableName);
                    if (parameterDetail.Optional)
                    {
                        hasOptionalValues = true;
                    }
                }

                if (optionParameters.Any())
                {
                    cb.AddBlankLine();
                    cb.AppendLine("// Check for unknown options");
                    using (cb.AddBlock("foreach (var arg in args)"))
                    {
                        using (cb.AddBlock("if ((arg.StartsWith(\"--\") || arg.StartsWith(\"-\")) && !validOptions.Contains(arg))"))
                        {
                            cb.AppendLine("var optionNames = validOptions.Select(o => o).ToArray();");
                            cb.AppendLine("var suggestion = TeCLI.StringSimilarity.FindMostSimilar(arg, optionNames);");
                            using (cb.AddBlock("if (suggestion != null)"))
                            {
                                cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.UnknownOptionWithSuggestion.Replace("\n", "\\n")}", arg, suggestion));""");
                            }
                            using (cb.AddBlock("else"))
                            {
                                cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.UnknownOption}", arg));""");
                            }
                        }
                    }
                }

                cb.AddBlankLine();
                if (hasOptionalValues)
                {
                    cb.AppendLine($"// Build parameter list with optional values handled via ternary operators");

                    List<string> allParams = [];

                    if (globalOptionsParam != null)
                    {
                        allParams.Add($"{globalOptionsParam.ParameterName}: _globalOptions");
                    }

                    foreach (var param in parameterDetails)
                    {
                        if (param.Required)
                        {
                            allParams.Add($"{param.ParameterName}: p{param.ParameterIndex}");
                        }
                        else if (param.IsSwitch)
                        {
                            allParams.Add($"{param.ParameterName}: p{param.ParameterIndex}");
                        }
                        else
                        {
                            var defaultValue = param.DefaultValue ?? $"default({param.DisplayType})";
                            allParams.Add($"{param.ParameterName}: p{param.ParameterIndex}Set ? p{param.ParameterIndex} : {defaultValue}");
                        }
                    }

                    // Add CancellationToken if the method accepts it
                    if (hasCancellationToken)
                    {
                        var ctParam = methodSymbol.GetCancellationTokenParameter();
                        if (ctParam != null)
                        {
                            allParams.Add($"{ctParam.Name}: cancellationToken");
                        }
                    }

                    cb.AppendLine(
                            methodSymbol.MapAsync(
                                () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({string.Join(", ", allParams)}), cancellationToken);",
                                () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({string.Join(", ", allParams)}), cancellationToken);"));
                }
                else
                {
                    cb.AppendLine($"// Now invoke the method with the parsed parameters");

                    var invokeParams = new List<string>();
                    if (globalOptionsParam != null)
                    {
                        invokeParams.Add("_globalOptions");
                    }
                    invokeParams.AddRange(parameterDetails.Select(p => $"p{p.ParameterIndex}"));

                    // Add CancellationToken if the method accepts it
                    if (hasCancellationToken)
                    {
                        invokeParams.Add("cancellationToken");
                    }

                    cb.AppendLine(
                            methodSymbol.MapAsync(
                                () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({string.Join(", ", invokeParams)}), cancellationToken);",
                                () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({string.Join(", ", invokeParams)}), cancellationToken);"));
                }
            }
        }
    }
}
