using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AsyncMethodReturnTypeAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI011",
        "Async method must return Task or Task<T>",
        "Action method '{0}' is marked as async but does not return Task, Task<T>, ValueTask, or ValueTask<T>",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Async action methods should return Task, Task<T>, ValueTask, or ValueTask<T> for proper asynchronous execution.");

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
        var isAction = methodSymbol.HasAttribute(AttributeNames.ActionAttribute) || methodSymbol.HasAttribute(AttributeNames.PrimaryAttribute);
        if (!isAction)
            return;

        // Check if method is async
        if (!methodSymbol.IsAsync)
            return;

        // Check return type
        var returnType = methodSymbol.ReturnType;
        var returnTypeNamespace = returnType.ContainingNamespace?.ToDisplayString();
        var returnTypeName = returnType.Name;

        // Valid async return types: Task, Task<T>, ValueTask, ValueTask<T>, void (for async void)
        bool isValidReturnType = returnTypeNamespace == "System.Threading.Tasks" &&
                                 (returnTypeName == "Task" || returnTypeName == "ValueTask");

        if (!isValidReturnType && returnType.SpecialType != SpecialType.System_Void)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                methodDeclaration.Identifier.GetLocation(),
                methodSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
