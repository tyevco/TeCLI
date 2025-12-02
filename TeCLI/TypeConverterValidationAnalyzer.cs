using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Validates that TypeConverter attribute references a valid type that implements ITypeConverter{T}.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TypeConverterValidationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor InvalidConverterTypeRule = new(
        "CLI035",
        "Invalid type converter",
        "Type '{0}' does not implement ITypeConverter<T>",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The type specified in [TypeConverter] must implement the ITypeConverter<T> interface.");

    private static readonly DiagnosticDescriptor ConverterTypeMismatchRule = new(
        "CLI035",
        "Type converter target type mismatch",
        "Type converter '{0}' converts to '{1}', but the option/argument is of type '{2}'",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The type converter's target type T in ITypeConverter<T> must match the option or argument type.");

    private static readonly DiagnosticDescriptor AbstractConverterRule = new(
        "CLI035",
        "Abstract type converter",
        "Type converter '{0}' is abstract and cannot be instantiated",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Type converters must be concrete types that can be instantiated.");

    private static readonly DiagnosticDescriptor NoParameterlessConstructorRule = new(
        "CLI035",
        "Type converter missing parameterless constructor",
        "Type converter '{0}' does not have a public parameterless constructor",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Type converters must have a public parameterless constructor to be instantiated by the framework.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            InvalidConverterTypeRule,
            ConverterTypeMismatchRule,
            AbstractConverterRule,
            NoParameterlessConstructorRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(propertyDeclaration) is not IPropertySymbol propertySymbol)
            return;

        // Only analyze properties with Option or Argument attributes
        if (!propertySymbol.HasAttribute(AttributeNames.OptionAttribute) &&
            !propertySymbol.HasAttribute(AttributeNames.ArgumentAttribute))
            return;

        var converterAttr = propertySymbol.GetAttribute(AttributeNames.TypeConverterAttribute);
        if (converterAttr == null)
            return;

        ValidateTypeConverter(context, converterAttr, propertySymbol.Type, propertyDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameterSyntax = (ParameterSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
            return;

        var converterAttr = parameterSymbol.GetAttribute(AttributeNames.TypeConverterAttribute);
        if (converterAttr == null)
            return;

        ValidateTypeConverter(context, converterAttr, parameterSymbol.Type, parameterSyntax.GetLocation());
    }

    private void ValidateTypeConverter(SyntaxNodeAnalysisContext context, AttributeData converterAttr, ITypeSymbol targetType, Location location)
    {
        // Get the converter type from the attribute
        if (converterAttr.ConstructorArguments.Length == 0 || converterAttr.ConstructorArguments[0].IsNull)
            return;

        if (converterAttr.ConstructorArguments[0].Value is not INamedTypeSymbol converterType)
            return;

        var converterTypeName = converterType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        // Check if the converter type is abstract
        if (converterType.IsAbstract)
        {
            var diagnostic = Diagnostic.Create(AbstractConverterRule, location, converterTypeName);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Check for parameterless constructor
        var hasParameterlessConstructor = converterType.Constructors.Any(c =>
            c.DeclaredAccessibility == Accessibility.Public &&
            c.Parameters.Length == 0);

        if (!hasParameterlessConstructor)
        {
            var diagnostic = Diagnostic.Create(NoParameterlessConstructorRule, location, converterTypeName);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Find ITypeConverter<T> interface implementation
        var typeConverterInterface = FindTypeConverterInterface(converterType);

        if (typeConverterInterface == null)
        {
            var diagnostic = Diagnostic.Create(InvalidConverterTypeRule, location, converterTypeName);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Get the T from ITypeConverter<T>
        var converterTargetType = typeConverterInterface.TypeArguments[0];

        // Handle collection types - extract element type
        var actualTargetType = GetElementTypeIfCollection(targetType) ?? targetType;

        // Handle nullable types
        actualTargetType = GetUnderlyingTypeIfNullable(actualTargetType) ?? actualTargetType;

        // Check if the converter's target type matches the option/argument type
        if (!SymbolEqualityComparer.Default.Equals(converterTargetType, actualTargetType))
        {
            var diagnostic = Diagnostic.Create(
                ConverterTypeMismatchRule,
                location,
                converterTypeName,
                converterTargetType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                actualTargetType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static INamedTypeSymbol? FindTypeConverterInterface(INamedTypeSymbol type)
    {
        // Check all interfaces for ITypeConverter<T>
        foreach (var iface in type.AllInterfaces)
        {
            if (iface.IsGenericType &&
                iface.Name == "ITypeConverter" &&
                iface.TypeArguments.Length == 1)
            {
                return iface;
            }
        }

        return null;
    }

    private static ITypeSymbol? GetElementTypeIfCollection(ITypeSymbol type)
    {
        // Check for array type
        if (type is IArrayTypeSymbol arrayType)
        {
            return arrayType.ElementType;
        }

        // Check for generic collection types (List<T>, IEnumerable<T>, etc.)
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var typeArguments = namedType.TypeArguments;
            if (typeArguments.Length == 1)
            {
                var typeName = namedType.OriginalDefinition.ToDisplayString();

                if (typeName == "System.Collections.Generic.List<T>" ||
                    typeName == "System.Collections.Generic.IEnumerable<T>" ||
                    typeName == "System.Collections.Generic.ICollection<T>" ||
                    typeName == "System.Collections.Generic.IList<T>" ||
                    typeName == "System.Collections.Generic.IReadOnlyCollection<T>" ||
                    typeName == "System.Collections.Generic.IReadOnlyList<T>")
                {
                    return typeArguments[0];
                }
            }
        }

        return null;
    }

    private static ITypeSymbol? GetUnderlyingTypeIfNullable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return namedType.TypeArguments[0];
        }

        return null;
    }
}
