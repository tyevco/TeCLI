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
                    cb.AppendLine($"throw new Exception();");
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
                    cb.AppendLine($"// Now determine which overload to call for the action.");

                    var paramsList = parameterDetails.Where(x => x.Required).Select(p => $"p{p.ParameterIndex}");

                    var actionFormat = methodSymbol.MapAsync(
                                () => $"actionToInvoke = async command => await command.{methodSymbol.Name}({{0}});",
                                () => $"actionToInvoke = command => command.{methodSymbol.Name}({{0}});");

                    cb.AppendLine(
                            methodSymbol.MapAsync(
                                () => $"Func<{methodSymbol.ContainingSymbol.Name}, Task> {string.Format(actionFormat, string.Join(", ", paramsList))}",
                                () => $"Action<{methodSymbol.ContainingSymbol.Name}> {string.Format(actionFormat, string.Join(", ", paramsList))}"));

                    var optionalParameters = parameterDetails.Where(x => x.Optional).ToList();
                    var bitsTotal = Math.Pow(2, optionalParameters.Count) - 1;

                    CodeBuilder.IfElseBuilder? ifBuilder = null;
                    for (long bitMask = 1; bitMask <= bitsTotal; bitMask++)
                    {
                        StringBuilder ifExprBuilder = new StringBuilder();
                        List<string> additionalParams = [];
                        for (int p = 0; p < optionalParameters.Count; p++)
                        {
                            var parameter = optionalParameters[p];
                            var isOn = (bitMask & (1 << p)) == 1 << p;
                            if (ifExprBuilder.Length > 0)
                            {
                                ifExprBuilder.Append(" && ");
                            }

                            if (isOn)
                            {
                                additionalParams.Add($"{parameter.ParameterName}: p{parameter.ParameterIndex}");
                            }
                            else
                            {
                                ifExprBuilder.Append("!");
                            }

                            ifExprBuilder.AppendFormat("p{0}Set", parameter.ParameterIndex);
                        }

                        if (ifBuilder != null)
                        {
                            ifBuilder.Else(ifExprBuilder.ToString());
                        }
                        else
                        {
                            ifBuilder = cb.AddIf(ifExprBuilder.ToString());
                        }

                        cb.AppendLine(string.Format(actionFormat, string.Join(", ", paramsList.Concat(additionalParams))));
                    }
                    ifBuilder?.Dispose();

                    cb.AddBlankLine();
                    cb.AppendLine(
                            methodSymbol.MapAsync(
                                () => $"await {methodInvokerName}(actionToInvoke);",
                                () => $"{methodInvokerName}(actionToInvoke);"));
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

    private static void GenerateCodeForParameter(CodeBuilder cb, IParameterSymbol parameter, int index)
    {
        // Assume args is the input array of strings from the command line
        var parameterType = parameter.Type;

        if (parameter.GetAttributes().Any(a => a.AttributeClass!.Name == "OptionAttribute" || a.AttributeClass.Name == "ArgumentAttribute"))
        {
            // Example for simple types, expand this logic based on your needs
            cb.AppendLine($"Convert.ChangeType(args[{index}], typeof({parameterType})),");
        }
        else
        {
            // Default handling for types without specific attributes
            cb.AppendLine($"default({parameterType}), // Placeholder, implement actual parsing logic here");
        }
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

                psi.Name = optionName.IsNull ? parameterSymbol.Name : optionName.Value!.ToString();

                var shortName = optionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ShortName").Value;

                psi.ShortName = shortName.IsNull ? '\0' : (char)shortName.Value!;

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
                        psi.Name = optionName.IsNull ? psi.Name : optionName.Value!.ToString();

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

                psi.Name = optionName.IsNull ? propertySymbol.Name : optionName.Value!.ToString();

                var shortName = optionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ShortName").Value;

                psi.ShortName = shortName.IsNull ? '\0' : (char)shortName.Value!;

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
                        psi.Name = optionName.IsNull ? psi.Name : optionName.Value!.ToString();

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
                // cb.AppendLine($"{variableName} = default;");
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
                cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})Convert.ChangeType(args[{variableName}Index + 1], typeof({sourceInfo.DisplayType}));");
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
            // if the field is required, we want to throw an exception if it is not provided.
            //if (sourceInfo.Required)
            //{
            //    using (cb.AddBlock("else"))
            //    {
            //        cb.AppendLine($"""throw new ArgumentException("Required option '--{sourceInfo.Name}' not provided.");""");
            //    }
            //}
        }
    }

}
