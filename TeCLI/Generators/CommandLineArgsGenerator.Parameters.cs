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

        if (parameterDetails.Count == 0)
        {
            var globalOptionsArg = globalOptionsParam != null ? "_globalOptions" : "";
            var invokeStatement = methodSymbol.MapAsync(
                () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({globalOptionsArg}));",
                () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({globalOptionsArg}));");
            statements.Add(ParseStatement(invokeStatement));
        }
        else
        {
            // Generate the if/else structure using CodeBuilder internally, then parse the result
            var cb = new CodeBuilder();
            GenerateParameterCodeInternal(cb, methodSymbol, methodInvokerName, parameterDetails, globalOptionsParam);

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
        List<ParameterSourceInfo> parameterDetails, ParameterSourceInfo? globalOptionsParam)
    {
        using (cb.AddBlock("if (args.Length == 0)"))
        {
            if (!parameterDetails.Any(p => p.Required))
            {
                var globalOptionsArg = globalOptionsParam != null ? "_globalOptions" : "";
                cb.AppendLine(
                    methodSymbol.MapAsync(
                        () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({globalOptionsArg}));",
                        () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({globalOptionsArg}));"));
            }
            else
            {
                cb.AppendLine($"""throw new ArgumentException("{ErrorMessages.RequiredParametersNotProvided}");""");
            }
        }

        bool hasOptionalValues = false;
        using (cb.AddBlock("else"))
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
                            cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.UnknownOptionWithSuggestion}", arg, suggestion));""");
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

                cb.AppendLine(
                        methodSymbol.MapAsync(
                            () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({string.Join(", ", allParams)}));",
                            () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({string.Join(", ", allParams)}));"));
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

                cb.AppendLine(
                        methodSymbol.MapAsync(
                            () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({string.Join(", ", invokeParams)}));",
                            () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({string.Join(", ", invokeParams)}));"));
            }
        }
    }
}
