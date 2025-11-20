using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeCLI.Attributes;
using TeCLI.Extensions;

namespace TeCLI.Generators;

public partial class CommandLineArgsGenerator
{
    public void GenerateParameterCode(CodeBuilder cb, IMethodSymbol methodSymbol, string methodInvokerName)
    {
        var parameterDetails = GetParameterDetails(methodSymbol.Parameters);

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
                    cb.AppendLine($"""throw new ArgumentException("Required parameters not provided. Use --help for usage information.");""");
                }
            }

            bool hasOptionalValues = false;
            using (cb.AddBlock("else"))
            {
                foreach (var parameterDetail in parameterDetails)
                {
                    var variableName = $"p{parameterDetail.ParameterIndex}";
                    GenerateParameterParsingCode(cb, methodSymbol, parameterDetail, variableName);
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

    private static List<ParameterSourceInfo> GetParameterDetails(IEnumerable<IParameterSymbol> parameters)
    {
        List<ParameterSourceInfo> items = [];
        int paramIndex = 0;
        int argumentCount = 0;
        foreach (var parameter in parameters)
        {
            ParameterSourceInfo? psi = BuildParameterSourceInfo(parameter, ref paramIndex, ref argumentCount);
            if (psi != null)
            {
                if (psi.ParameterType == ParameterType.Container)
                {
                    psi.Children = GetParameterDetails(parameter.GetPropertiesOfParameterType());
                }

                items.Add(psi);
            }
        }

        return items;
    }

    private static List<ParameterSourceInfo> GetParameterDetails(IEnumerable<IPropertySymbol> properties)
    {
        List<ParameterSourceInfo> items = [];
        int paramIndex = 0;
        int argumentCount = 0;
        foreach (var property in properties)
        {
            ParameterSourceInfo? psi = BuildParameterSourceInfo(property, ref paramIndex, ref argumentCount);
            if (psi != null)
            {
                if (psi.ParameterType == ParameterType.Container)
                {
                    psi.Children = GetParameterDetails(property.GetPropertiesOfParameterType());
                }

                items.Add(psi);
            }
        }

        return items;
    }

    private static ParameterSourceInfo? BuildParameterSourceInfo(IParameterSymbol parameterSymbol, ref int paramIndex, ref int argumentCount)
    {
        ParameterSourceInfo? psi = null;

        var parameterSyntax = parameterSymbol.GetSyntax<ParameterSyntax>();
        if (parameterSyntax != null)
        {
            psi = new();
            psi.ParameterIndex = paramIndex++;
            psi.ParameterName = parameterSymbol.Name;
            psi.Required = parameterSyntax.Default == null;

            // Capture the default value if present
            if (parameterSyntax.Default != null)
            {
                psi.DefaultValue = parameterSyntax.Default.Value.ToString();
            }

            psi.SpecialType = parameterSymbol.Type.SpecialType;
            psi.DisplayType = parameterSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (parameterSymbol.TryGetAttribute<OptionAttribute>(out var optionAttribute))
            {
                psi.ParameterType = ParameterType.Option;

                var optionName = optionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
                if (optionName.IsNull)
                {
                    optionName = optionAttribute.ConstructorArguments[0];
                }

                var description = optionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Description").Value;

                psi.Name = optionName.IsNull ? parameterSymbol.Name : optionName.Value?.ToString() ?? parameterSymbol.Name;

                var shortName = optionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ShortName").Value;

                psi.ShortName = !shortName.IsNull && shortName.Value is char ch ? ch : '\0';

                bool isBoolean = parameterSymbol.Type.SpecialType == SpecialType.System_Boolean;

                if (isBoolean)
                {
                    psi.IsSwitch = true;
                }
            }
            else
            {
                // this is an argument?
                if (parameterSymbol.Type.IsValidOptionType())
                {
                    psi.ParameterType = ParameterType.Argument;
                    psi.ArgumentIndex = argumentCount++;

                    psi.Name = parameterSymbol.Name;
                    if (parameterSymbol.TryGetAttribute<ArgumentAttribute>(out var argumentAttribute))
                    {
                        var optionName = argumentAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
                        psi.Name = optionName.IsNull ? psi.Name : optionName.Value?.ToString() ?? psi.Name;

                        var description = argumentAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Description").Value;
                        psi.Description = description;
                    }
                }
                else
                {
                    psi.ParameterType = ParameterType.Container;
                }
            }
        }

        return psi;
    }

    private static ParameterSourceInfo? BuildParameterSourceInfo(IPropertySymbol propertySymbol, ref int paramIndex, ref int argumentCount)
    {
        ParameterSourceInfo? psi = null;

        var propertySyntax = propertySymbol.GetSyntax<PropertyDeclarationSyntax>();
        bool hasInitializer = propertySyntax?.Initializer != null ||
                              (propertySyntax?.AccessorList?.Accessors.Any(a =>
                                  a.Kind() == SyntaxKind.SetAccessorDeclaration && a.Body != null) ?? false);

        if (propertySyntax != null)
        {
            psi = new();
            psi.ParameterIndex = paramIndex++;
            psi.ParameterName = propertySymbol.Name;
            psi.Parent = propertySyntax.Parent;
            psi.Required = !hasInitializer;

            // Capture the default value from initializer if present
            if (propertySyntax.Initializer != null)
            {
                psi.DefaultValue = propertySyntax.Initializer.Value.ToString();
            }

            psi.SpecialType = propertySymbol.Type.SpecialType;
            psi.DisplayType = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            if (propertySymbol.TryGetAttribute<OptionAttribute>(out var optionAttribute))
            {
                psi.ParameterType = ParameterType.Option;

                var optionName = optionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
                if (optionName.IsNull)
                {
                    optionName = optionAttribute.ConstructorArguments[0];
                }


                var description = optionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Description").Value;

                psi.Name = optionName.IsNull ? propertySymbol.Name : optionName.Value?.ToString() ?? propertySymbol.Name;

                var shortName = optionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ShortName").Value;

                psi.ShortName = !shortName.IsNull && shortName.Value is char ch ? ch : '\0';

                bool isBoolean = propertySymbol.Type.SpecialType == SpecialType.System_Boolean;

                if (isBoolean)
                {
                    psi.IsSwitch = true;
                }
            }
            else
            {
                // this is an argument?
                if (propertySymbol.Type.IsValidOptionType())
                {
                    psi.ParameterType = ParameterType.Argument;
                    psi.ArgumentIndex = argumentCount++;
                    psi.Name = propertySymbol.Name;
                    if (propertySymbol.TryGetAttribute<ArgumentAttribute>(out var argumentAttribute))
                    {
                        var optionName = argumentAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
                        psi.Name = optionName.IsNull ? psi.Name : optionName.Value?.ToString() ?? psi.Name;

                        var description = argumentAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Description").Value;
                        psi.Description = description;
                    }
                }
                else
                {
                    psi.ParameterType = ParameterType.Container;
                }
            }
        }

        return psi;
    }

    private static void GenerateParameterParsingCode(CodeBuilder cb, IMethodSymbol methodSymbol, ParameterSourceInfo sourceInfo, string variableName)
    {
        cb.AppendLine($"{sourceInfo.DisplayType} {variableName} = default;");
        if (sourceInfo.Optional)
        {
            cb.AppendLine($"bool {variableName}Set = false;");
        }

        using (cb.AddBlankScope())
        {
            if (sourceInfo.ParameterType == ParameterType.Option)
            {
                GenerateOptionSource(cb, methodSymbol, sourceInfo, variableName);
            }
            else if (sourceInfo.ParameterType == ParameterType.Argument)
            {
                GenerateArgumentSource(cb, methodSymbol, sourceInfo, variableName);
            }
            else if (sourceInfo.ParameterType == ParameterType.Container)
            {
                // Container parameters are handled by their nested properties
            }
        }
    }

    private static void GenerateOptionSource(CodeBuilder cb, IMethodSymbol methodSymbol, ParameterSourceInfo sourceInfo, string variableName)
    {
        if (sourceInfo.IsSwitch)
        {
            string switchCheck = sourceInfo.ShortName != '\0'
                ? $"args.Contains(\"-{sourceInfo.ShortName}\") || args.Contains(\"--{sourceInfo.Name}\")"
                : $"args.Contains(\"--{sourceInfo.Name}\")";

            cb.AppendLine($"{variableName} = {switchCheck};");
        }
        else
        {
            string optionCheck = sourceInfo.ShortName != '\0'
                ? $"arg == \"-{sourceInfo.ShortName}\" || arg == \"--{sourceInfo.Name}\""
                : $"arg == \"--{sourceInfo.Name}\"";

            cb.AppendLine($"var {variableName}Index = Array.FindIndex(args, arg => {optionCheck});");
            using (cb.AddBlock($"if ({variableName}Index != -1 && args.Length > {variableName}Index + 1)"))
            {
                using (var tb = cb.AddTry())
                {
                    cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})Convert.ChangeType(args[{variableName}Index + 1], typeof({sourceInfo.DisplayType}));");
                    if (sourceInfo.Optional)
                    {
                        cb.AppendLine($"{variableName}Set = true;");
                    }
                    tb.Catch();
                    cb.AppendLine($"""throw new ArgumentException("Invalid value provided for option '--{sourceInfo.Name}'.");""");
                }
            }

            // if the field is required, we want to throw an exception if it is not provided.
            if (sourceInfo.Required)
            {
                using (cb.AddBlock("else"))
                {
                    cb.AppendLine($"""throw new ArgumentException("Required option '--{sourceInfo.Name}' not provided.");""");
                }
            }
        }
    }

    private static void GenerateArgumentSource(CodeBuilder cb, IMethodSymbol methodSymbol, ParameterSourceInfo sourceInfo, string variableName)
    {
        using (cb.AddBlock($"if (args.Length < {sourceInfo.ArgumentIndex + 1})"))
        {
            cb.AppendLine($"""throw new ArgumentException("Required argument '{sourceInfo.Name}' not provided.");""");
        }
        using (cb.AddBlock("else"))
        {
            using (var tb = cb.AddTry())
            {
                cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})Convert.ChangeType(args[{sourceInfo.ArgumentIndex}], typeof({sourceInfo.DisplayType}));");
                tb.Catch();
                cb.AppendLine($"""throw new ArgumentException("Invalid syntax provided for argument '{sourceInfo.Name}'.");""");
            }
        }
    }

}
