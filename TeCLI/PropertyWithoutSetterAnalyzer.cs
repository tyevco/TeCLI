using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Detects properties marked with [Option] or [Argument] that don't have a setter,
/// making them impossible to populate from command-line input.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PropertyWithoutSetterAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI022",
        "Option/Argument property without setter",
        "Property '{0}' is marked as {1} but has no setter. The CLI framework cannot set its value.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Properties used as options or arguments must have a setter (public, private, or init) to allow the CLI framework to populate them from command-line input.");

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

        string? attributeType = null;

        if (propertySymbol.HasAttribute(AttributeNames.OptionAttribute))
        {
            attributeType = "Option";
        }
        else if (propertySymbol.HasAttribute(AttributeNames.ArgumentAttribute))
        {
            attributeType = "Argument";
        }

        if (attributeType == null)
            return;

        // Check if property has a setter (any accessibility)
        if (propertySymbol.SetMethod == null)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                propertyDeclaration.Identifier.GetLocation(),
                propertySymbol.Name,
                attributeType);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
