using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Suggests adding a CancellationToken parameter to async action methods
/// for proper cancellation support (e.g., Ctrl+C handling).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AsyncWithoutCancellationTokenAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI023",
        "Async action without CancellationToken",
        "Async action method '{0}' does not have a CancellationToken parameter. Consider adding one for graceful cancellation support.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Async action methods should accept a CancellationToken parameter to support graceful cancellation (e.g., when user presses Ctrl+C).");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        // Check if this is an action method
        var isAction = methodSymbol.HasAttribute(AttributeNames.ActionAttribute) ||
                       methodSymbol.HasAttribute(AttributeNames.PrimaryAttribute);

        if (!isAction)
            return;

        // Check if method is async (either has async keyword or returns Task-like type)
        var isAsync = methodSymbol.IsAsync || methodSymbol.HasTaskLikeReturnType();

        if (!isAsync)
            return;

        // Check if method already has a CancellationToken parameter
        if (methodSymbol.HasCancellationTokenParameter())
            return;

        var diagnostic = Diagnostic.Create(
            Rule,
            methodDeclaration.Identifier.GetLocation(),
            methodSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}
