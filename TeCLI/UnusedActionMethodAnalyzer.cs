using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Detects [Action] methods that won't be invoked because they're in non-command classes,
/// are inaccessible, or have other structural issues preventing CLI discovery.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnusedActionMethodAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor OrphanedActionRule = new(
        "CLI015",
        "Action method in non-command class",
        "Action method '{0}' is defined in class '{1}' which is not marked with [Command]. This action will not be discoverable.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Action methods should be defined in classes marked with [Command] attribute to be discoverable by the CLI framework.");

    private static readonly DiagnosticDescriptor InaccessibleActionRule = new(
        "CLI015",
        "Inaccessible action method",
        "Action method '{0}' has private accessibility and may not be invocable by the CLI framework.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Action methods should have sufficient accessibility (public or internal) to be invoked by the CLI framework.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(OrphanedActionRule, InaccessibleActionRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol methodSymbol)
            return;

        // Check if this method has [Action] or [Primary] attribute
        var hasActionAttribute = methodSymbol.HasAttribute(AttributeNames.ActionAttribute);
        var hasPrimaryAttribute = methodSymbol.HasAttribute(AttributeNames.PrimaryAttribute);

        if (!hasActionAttribute && !hasPrimaryAttribute)
            return;

        var containingClass = methodSymbol.ContainingType;
        if (containingClass == null)
            return;

        // Check if containing class is a Command class
        var isInCommandClass = containingClass.HasAttribute(AttributeNames.CommandAttribute);

        if (!isInCommandClass)
        {
            // Report orphaned action
            var diagnostic = Diagnostic.Create(
                OrphanedActionRule,
                methodDeclaration.Identifier.GetLocation(),
                methodSymbol.Name,
                containingClass.Name);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Check accessibility - private methods cannot be invoked
        if (methodSymbol.DeclaredAccessibility == Accessibility.Private)
        {
            var diagnostic = Diagnostic.Create(
                InaccessibleActionRule,
                methodDeclaration.Identifier.GetLocation(),
                methodSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
