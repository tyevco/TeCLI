using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using TeCLI.Attributes;
using TeCLI.Extensions;
using TeCLI.Generators;

namespace TeCLI.Generators;

/// <summary>
/// Extracts parameter information from Roslyn symbols
/// </summary>
internal static class ParameterInfoExtractor
{
    public static List<ParameterSourceInfo> GetParameterDetails(IEnumerable<IParameterSymbol> parameters)
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

    public static List<ParameterSourceInfo> GetParameterDetails(IEnumerable<IPropertySymbol> properties)
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
                ExtractOptionInfo(psi, optionAttribute, parameterSymbol.Name, parameterSymbol.Type.SpecialType);
            }
            else
            {
                // this is an argument or container
                if (parameterSymbol.Type.IsValidOptionType())
                {
                    ExtractArgumentInfo(psi, parameterSymbol, ref argumentCount);
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
                ExtractOptionInfo(psi, optionAttribute, propertySymbol.Name, propertySymbol.Type.SpecialType);
            }
            else
            {
                // this is an argument or container
                if (propertySymbol.Type.IsValidOptionType())
                {
                    ExtractArgumentInfo(psi, propertySymbol, ref argumentCount);
                }
                else
                {
                    psi.ParameterType = ParameterType.Container;
                }
            }
        }

        return psi;
    }

    private static void ExtractOptionInfo(ParameterSourceInfo psi, AttributeData optionAttribute, string symbolName, SpecialType typeSpecialType)
    {
        psi.ParameterType = ParameterType.Option;

        var optionName = optionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
        if (optionName.IsNull && optionAttribute.ConstructorArguments.Length > 0)
        {
            optionName = optionAttribute.ConstructorArguments[0];
        }

        var description = optionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Description").Value;

        psi.Name = optionName.IsNull ? symbolName : optionName.Value?.ToString() ?? symbolName;

        var shortName = optionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ShortName").Value;
        psi.ShortName = !shortName.IsNull && shortName.Value is char ch ? ch : '\0';

        bool isBoolean = typeSpecialType == SpecialType.System_Boolean;
        if (isBoolean)
        {
            psi.IsSwitch = true;
        }
    }

    private static void ExtractArgumentInfo(ParameterSourceInfo psi, IParameterSymbol parameterSymbol, ref int argumentCount)
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

    private static void ExtractArgumentInfo(ParameterSourceInfo psi, IPropertySymbol propertySymbol, ref int argumentCount)
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
}
