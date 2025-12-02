using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Suggests clarifying intent when a nullable type is used without a default value
/// and is not marked as required. This could indicate ambiguous optional behavior.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullableWithoutDefaultAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI029",
        "Nullable option without explicit default or required",
        "Option '{0}' is nullable but has no default value and is not marked as required. Consider clarifying intent.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Nullable option types without an explicit default value or required flag may have ambiguous behavior. Consider specifying a default value or marking as required to clarify intent.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(propertyDeclaration) is not IPropertySymbol propertySymbol)
            return;

        var optionAttr = propertySymbol.GetAttribute(AttributeNames.OptionAttribute);
        if (optionAttr == null)
            return;

        // Check if the type is nullable
        if (!IsNullableType(propertySymbol.Type))
            return;

        // Check if it's marked as required
        var requiredArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Required").Value;
        bool isRequired = !requiredArg.IsNull && requiredArg.Value is true;

        if (isRequired)
            return;

        // Check if property has an initializer (default value)
        var hasInitializer = propertyDeclaration.Initializer != null;

        if (!hasInitializer)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                propertyDeclaration.Identifier.GetLocation(),
                propertySymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsNullableType(ITypeSymbol type)
    {
        // Nullable value types (int?, bool?, etc.)
        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return true;
        }

        // Nullable reference types
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return true;
        }

        return false;
    }
}
