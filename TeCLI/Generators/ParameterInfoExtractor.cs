using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using TeCLI.Attributes;
using TeCLI.Attributes.Validation;
using TeCLI.TypeConversion;
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

            // Check if this is an enum type
            if (parameterSymbol.Type.IsEnumType())
            {
                psi.IsEnum = true;
                psi.IsFlags = parameterSymbol.Type.IsFlagsEnum();
            }

            // Check if this is a common convertible type
            if (parameterSymbol.Type.IsCommonConvertibleType())
            {
                psi.IsCommonType = true;
                psi.CommonTypeParseMethod = parameterSymbol.Type.GetCommonTypeParseMethod();
            }

            // Check if this is a collection type
            if (parameterSymbol.Type.IsCollectionType(out var elementType))
            {
                psi.IsCollection = true;
                psi.ElementType = elementType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                psi.ElementSpecialType = elementType?.SpecialType ?? SpecialType.None;

                // Check if the element type is an enum
                if (elementType?.IsEnumType() == true)
                {
                    psi.IsElementEnum = true;
                    psi.IsElementFlags = elementType.IsFlagsEnum();
                }

                // Check if the element type is a common convertible type
                if (elementType?.IsCommonConvertibleType() == true)
                {
                    psi.IsElementCommonType = true;
                    psi.ElementCommonTypeParseMethod = elementType.GetCommonTypeParseMethod();
                }
            }

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

            // Extract validation attributes
            ExtractValidationInfo(psi, parameterSymbol);

            // Extract custom type converter
            ExtractCustomConverterInfo(psi, parameterSymbol);
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

            // Check if this is an enum type
            if (propertySymbol.Type.IsEnumType())
            {
                psi.IsEnum = true;
                psi.IsFlags = propertySymbol.Type.IsFlagsEnum();
            }

            // Check if this is a common convertible type
            if (propertySymbol.Type.IsCommonConvertibleType())
            {
                psi.IsCommonType = true;
                psi.CommonTypeParseMethod = propertySymbol.Type.GetCommonTypeParseMethod();
            }

            // Check if this is a collection type
            if (propertySymbol.Type.IsCollectionType(out var elementType))
            {
                psi.IsCollection = true;
                psi.ElementType = elementType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                psi.ElementSpecialType = elementType?.SpecialType ?? SpecialType.None;

                // Check if the element type is an enum
                if (elementType?.IsEnumType() == true)
                {
                    psi.IsElementEnum = true;
                    psi.IsElementFlags = elementType.IsFlagsEnum();
                }

                // Check if the element type is a common convertible type
                if (elementType?.IsCommonConvertibleType() == true)
                {
                    psi.IsElementCommonType = true;
                    psi.ElementCommonTypeParseMethod = elementType.GetCommonTypeParseMethod();
                }
            }

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

            // Extract validation attributes
            ExtractValidationInfo(psi, propertySymbol);

            // Extract custom type converter
            ExtractCustomConverterInfo(psi, propertySymbol);
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

        // Extract the Required property from the attribute
        var required = optionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Required").Value;
        if (!required.IsNull && required.Value is bool isRequired)
        {
            psi.Required = isRequired;
        }
        else
        {
            // If Required is not explicitly set in the attribute, keep the default behavior
            // (based on whether there's a default value)
        }

        // Extract the EnvVar property from the attribute
        var envVar = optionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "EnvVar").Value;
        if (!envVar.IsNull && envVar.Value is string envVarName)
        {
            psi.EnvVar = envVarName;
        }

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

    private static void ExtractValidationInfo(ParameterSourceInfo psi, ISymbol symbol)
    {
        var attributes = symbol.GetAttributes();

        // Check for RangeAttribute
        var rangeAttr = attributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(RangeAttribute));
        if (rangeAttr != null && rangeAttr.ConstructorArguments.Length == 2)
        {
            var min = rangeAttr.ConstructorArguments[0].Value;
            var max = rangeAttr.ConstructorArguments[1].Value;
            psi.Validations.Add(new ValidationInfo
            {
                AttributeTypeName = "Range",
                ValidationCode = $"new TeCLI.Attributes.Validation.RangeAttribute({min}, {max}).Validate({{0}}, \"{psi.Name}\")"
            });
        }

        // Check for RegularExpressionAttribute
        var regexAttr = attributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(RegularExpressionAttribute));
        if (regexAttr != null && regexAttr.ConstructorArguments.Length >= 1)
        {
            var pattern = regexAttr.ConstructorArguments[0].Value?.ToString() ?? "";
            var errorMessage = regexAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "ErrorMessage").Value;

            string validationCode;
            if (!errorMessage.IsNull && errorMessage.Value != null)
            {
                var errorMsg = errorMessage.Value.ToString();
                validationCode = $"new TeCLI.Attributes.Validation.RegularExpressionAttribute(@\"{pattern}\") {{ ErrorMessage = \"{errorMsg}\" }}.Validate({{0}}, \"{psi.Name}\")";
            }
            else
            {
                validationCode = $"new TeCLI.Attributes.Validation.RegularExpressionAttribute(@\"{pattern}\").Validate({{0}}, \"{psi.Name}\")";
            }

            psi.Validations.Add(new ValidationInfo
            {
                AttributeTypeName = "RegularExpression",
                ValidationCode = validationCode
            });
        }

        // Check for FileExistsAttribute
        var fileExistsAttr = attributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(FileExistsAttribute));
        if (fileExistsAttr != null)
        {
            var errorMessage = fileExistsAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "ErrorMessage").Value;

            string validationCode;
            if (!errorMessage.IsNull && errorMessage.Value != null)
            {
                var errorMsg = errorMessage.Value.ToString();
                validationCode = $"new TeCLI.Attributes.Validation.FileExistsAttribute() {{ ErrorMessage = \"{errorMsg}\" }}.Validate({{0}}, \"{psi.Name}\")";
            }
            else
            {
                validationCode = $"new TeCLI.Attributes.Validation.FileExistsAttribute().Validate({{0}}, \"{psi.Name}\")";
            }

            psi.Validations.Add(new ValidationInfo
            {
                AttributeTypeName = "FileExists",
                ValidationCode = validationCode
            });
        }

        // Check for DirectoryExistsAttribute
        var dirExistsAttr = attributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(DirectoryExistsAttribute));
        if (dirExistsAttr != null)
        {
            var errorMessage = dirExistsAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "ErrorMessage").Value;

            string validationCode;
            if (!errorMessage.IsNull && errorMessage.Value != null)
            {
                var errorMsg = errorMessage.Value.ToString();
                validationCode = $"new TeCLI.Attributes.Validation.DirectoryExistsAttribute() {{ ErrorMessage = \"{errorMsg}\" }}.Validate({{0}}, \"{psi.Name}\")";
            }
            else
            {
                validationCode = $"new TeCLI.Attributes.Validation.DirectoryExistsAttribute().Validate({{0}}, \"{psi.Name}\")";
            }

            psi.Validations.Add(new ValidationInfo
            {
                AttributeTypeName = "DirectoryExists",
                ValidationCode = validationCode
            });
        }
    }

    private static void ExtractCustomConverterInfo(ParameterSourceInfo psi, ISymbol symbol)
    {
        var attributes = symbol.GetAttributes();

        // Check for TypeConverterAttribute
        var converterAttr = attributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(TypeConverterAttribute));
        if (converterAttr != null && converterAttr.ConstructorArguments.Length >= 1)
        {
            var converterTypeSymbol = converterAttr.ConstructorArguments[0].Value as INamedTypeSymbol;
            if (converterTypeSymbol != null)
            {
                var converterType = converterTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                if (psi.IsCollection)
                {
                    // For collections, the converter is for the element type
                    psi.HasElementCustomConverter = true;
                    psi.ElementCustomConverterType = converterType;
                }
                else
                {
                    // For non-collections, the converter is for the parameter type
                    psi.HasCustomConverter = true;
                    psi.CustomConverterType = converterType;
                }
            }
        }
    }
}
