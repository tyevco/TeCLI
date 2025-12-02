using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Validates combinations of validation attributes on command properties and parameters.
/// Detects conflicting or inappropriate attribute combinations.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValidationAttributeCombinationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor ConflictingPathValidationRule = new(
        "CLI016",
        "Conflicting path validation attributes",
        "Property '{0}' has both [FileExists] and [DirectoryExists] attributes. A path cannot be both a file and a directory.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A property cannot have both [FileExists] and [DirectoryExists] validation attributes as they are mutually exclusive.");

    private static readonly DiagnosticDescriptor RangeOnNonNumericRule = new(
        "CLI016",
        "Range validation on non-numeric type",
        "Property '{0}' has [Range] attribute but is of type '{1}' which is not a numeric type.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The [Range] validation attribute should only be applied to numeric types (int, double, decimal, etc.).");

    private static readonly DiagnosticDescriptor RegexOnNonStringRule = new(
        "CLI016",
        "RegularExpression validation on non-string type",
        "Property '{0}' has [RegularExpression] attribute but is of type '{1}' which is not a string type.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The [RegularExpression] validation attribute should only be applied to string types.");

    private static readonly DiagnosticDescriptor FileValidationOnInvalidTypeRule = new(
        "CLI016",
        "File validation on incompatible type",
        "Property '{0}' has [{1}] attribute but is of type '{2}'. Expected string, FileInfo, or DirectoryInfo.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "File and directory validation attributes should be applied to string, FileInfo, or DirectoryInfo types.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            ConflictingPathValidationRule,
            RangeOnNonNumericRule,
            RegexOnNonStringRule,
            FileValidationOnInvalidTypeRule);

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

        // Only analyze properties with CLI attributes
        if (!propertySymbol.HasAttribute(AttributeNames.OptionAttribute) &&
            !propertySymbol.HasAttribute(AttributeNames.ArgumentAttribute))
            return;

        ValidateSymbol(context, propertySymbol, propertySymbol.Type, propertyDeclaration.GetLocation());
    }

    private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameterSyntax = (ParameterSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
            return;

        ValidateSymbol(context, parameterSymbol, parameterSymbol.Type, parameterSyntax.GetLocation());
    }

    private void ValidateSymbol(SyntaxNodeAnalysisContext context, ISymbol symbol, ITypeSymbol type, Location location)
    {
        var hasFileExists = symbol.HasAttribute(AttributeNames.FileExistsAttribute);
        var hasDirectoryExists = symbol.HasAttribute(AttributeNames.DirectoryExistsAttribute);
        var hasRange = symbol.HasAttribute(AttributeNames.RangeAttribute);
        var hasRegex = symbol.HasAttribute(AttributeNames.RegularExpressionAttribute);

        // Check for conflicting path validation attributes
        if (hasFileExists && hasDirectoryExists)
        {
            var diagnostic = Diagnostic.Create(
                ConflictingPathValidationRule,
                location,
                symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        // Check [Range] on non-numeric types
        if (hasRange && !IsNumericType(type))
        {
            var diagnostic = Diagnostic.Create(
                RangeOnNonNumericRule,
                location,
                symbol.Name,
                type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            context.ReportDiagnostic(diagnostic);
        }

        // Check [RegularExpression] on non-string types
        if (hasRegex && !IsStringType(type))
        {
            var diagnostic = Diagnostic.Create(
                RegexOnNonStringRule,
                location,
                symbol.Name,
                type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            context.ReportDiagnostic(diagnostic);
        }

        // Check file/directory validation on incompatible types
        if (hasFileExists && !IsPathCompatibleType(type))
        {
            var diagnostic = Diagnostic.Create(
                FileValidationOnInvalidTypeRule,
                location,
                symbol.Name,
                "FileExists",
                type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            context.ReportDiagnostic(diagnostic);
        }

        if (hasDirectoryExists && !IsPathCompatibleType(type))
        {
            var diagnostic = Diagnostic.Create(
                FileValidationOnInvalidTypeRule,
                location,
                symbol.Name,
                "DirectoryExists",
                type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsNumericType(ITypeSymbol type)
    {
        // Handle nullable types
        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            type = namedType.TypeArguments[0];
        }

        return type.SpecialType switch
        {
            SpecialType.System_Byte => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            SpecialType.System_Decimal => true,
            _ => false
        };
    }

    private static bool IsStringType(ITypeSymbol type)
    {
        return type.SpecialType == SpecialType.System_String;
    }

    private static bool IsPathCompatibleType(ITypeSymbol type)
    {
        // String is compatible
        if (type.SpecialType == SpecialType.System_String)
            return true;

        // FileInfo and DirectoryInfo are compatible
        var fullTypeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return fullTypeName == "global::System.IO.FileInfo" ||
               fullTypeName == "global::System.IO.DirectoryInfo";
    }
}
