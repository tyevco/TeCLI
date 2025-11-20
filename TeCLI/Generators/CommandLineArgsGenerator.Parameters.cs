using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using TeCLI.Extensions;
using static TeCLI.Constants;

namespace TeCLI.Generators;

public partial class CommandLineArgsGenerator
{
    public void GenerateParameterCode(CodeBuilder cb, IMethodSymbol methodSymbol, string methodInvokerName)
    {
        var parameterDetails = ParameterInfoExtractor.GetParameterDetails(methodSymbol.Parameters);

        // need to figure out if there are any arguments or required options.
        if (parameterDetails.Count == 0)
        {
            // no parameters were specified, so this action can be called.
            cb.AppendLine(
                    methodSymbol.MapAsync(
                        () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}());",
                        () => $"{methodInvokerName}(command => command.{methodSymbol.Name}());"));
        }
        else
        {
            using (cb.AddBlock("if (args.Length == 0)"))
            {
                if (!parameterDetails.Any(p => p.Required))
                {
                    // no parameters were required, so this action can be called.
                    cb.AppendLine(
                        methodSymbol.MapAsync(
                            () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}());",
                            () => $"{methodInvokerName}(command => command.{methodSymbol.Name}());"));
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
                foreach (var parameterDetail in parameterDetails)
                {
                    var variableName = $"p{parameterDetail.ParameterIndex}";
                    ParameterCodeGenerator.GenerateParameterParsingCode(cb, methodSymbol, parameterDetail, variableName);
                    if (parameterDetail.Optional)
                    {
                        hasOptionalValues = true;
                    }
                }

                cb.AddBlankLine();
                if (hasOptionalValues)
                {
                    cb.AppendLine($"// Build parameter list with optional values handled via ternary operators");

                    // Build the complete parameter list with all parameters (required and optional)
                    List<string> allParams = [];

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
                    cb.AppendLine(
                            methodSymbol.MapAsync(
                                () => $"await {methodInvokerName}(async command => await command.{methodSymbol.Name}({string.Join(", ", parameterDetails.Select(p => $"p{p.ParameterIndex}"))}));",
                                () => $"{methodInvokerName}(command => command.{methodSymbol.Name}({string.Join(", ", parameterDetails.Select(p => $"p{p.ParameterIndex}"))}));"));
                }
            }
        }
    }
}
