using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using TeCLI.Extensions;
using static TeCLI.Constants;

namespace TeCLI.Generators;

public partial class CommandLineArgsGenerator
{
    public void GenerateParameterCode(CodeBuilder cb, IMethodSymbol methodSymbol, string methodInvokerName, GlobalOptionsSourceInfo? globalOptions = null)
    {
        var parameterDetails = ParameterInfoExtractor.GetParameterDetails(methodSymbol.Parameters);

        // Check if any parameter is the global options type
        ParameterSourceInfo? globalOptionsParam = null;
        if (globalOptions != null)
        {
            globalOptionsParam = parameterDetails.FirstOrDefault(p =>
                p.Type == globalOptions.FullTypeName ||
                p.DisplayType == globalOptions.TypeName);

            // Remove global options parameter from parameterDetails since we'll handle it separately
            if (globalOptionsParam != null)
            {
                parameterDetails.Remove(globalOptionsParam);
            }
        }

        // need to figure out if there are any arguments or required options.
        if (parameterDetails.Count == 0)
        {
            // no parameters were specified (other than possibly global options), so this action can be called.
            var globalOptionsArg = globalOptionsParam != null ? "_globalOptions" : "";
            cb.AppendLine(
                    methodSymbol.MapAsync(
                        () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({globalOptionsArg}));",
                        () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({globalOptionsArg}));"));
        }
        else
        {
            using (cb.AddBlock("if (args.Length == 0)"))
            {
                if (!parameterDetails.Any(p => p.Required))
                {
                    // no parameters were required, so this action can be called.
                    var globalOptionsArg = globalOptionsParam != null ? "_globalOptions" : "";
                    cb.AppendLine(
                        methodSymbol.MapAsync(
                            () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({globalOptionsArg}));",
                            () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({globalOptionsArg}));"));
                }
                else
                {
                    // there were not enough parameters specified. the help text should be displayed for this action.
                    cb.AppendLine($"""throw new ArgumentException("{ErrorMessages.RequiredParametersNotProvided}");""");
                }
            }

            bool hasOptionalValues = false;
            using (cb.AddBlock("else"))
            {
                // Generate list of valid options for unknown option detection
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

                // Add unknown option detection
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

                    // Build the complete parameter list with all parameters (required and optional)
                    List<string> allParams = [];

                    // Add global options first if present
                    if (globalOptionsParam != null)
                    {
                        allParams.Add($"{globalOptionsParam.ParameterName}: _globalOptions");
                    }

                    foreach (var param in parameterDetails)
                    {
                        if (param.Required)
                        {
                            // Required parameters are passed directly
                            allParams.Add($"{param.ParameterName}: p{param.ParameterIndex}");
                        }
                        else if (param.IsSwitch)
                        {
                            // Switches are always evaluated (true/false), no need for ternary
                            allParams.Add($"{param.ParameterName}: p{param.ParameterIndex}");
                        }
                        else
                        {
                            // Optional non-switch parameters use ternary: if set, use parsed value, otherwise use default
                            var defaultValue = param.DefaultValue ?? $"default({param.DisplayType})";
                            allParams.Add($"{param.ParameterName}: p{param.ParameterIndex}Set ? p{param.ParameterIndex} : {defaultValue}");
                        }
                    }

                    // Generate single method call with all parameters
                    cb.AppendLine(
                            methodSymbol.MapAsync(
                                () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({string.Join(", ", allParams)}));",
                                () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({string.Join(", ", allParams)}));"));
                }
                else
                {
                    cb.AppendLine($"// Now invoke the method with the parsed parameters");

                    // Build parameter list
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
}
