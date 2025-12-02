using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace TeCLI.Console.Analyzers;

/// <summary>
/// Suggests that IProgressContext works best with async methods for smooth UI updates.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ProgressContextSyncMethodAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "TCLCON001",
        "IProgressContext in synchronous method",
        "Action '{0}' uses IProgressContext but is not async. Progress indicators work best with async methods for responsive UI updates.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "When using IProgressContext, async methods allow the progress UI to update smoothly. Synchronous methods may block the UI thread, causing progress indicators to appear unresponsive.");

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

        // Only analyze action methods
        var hasActionAttribute = methodSymbol.GetAttributes().Any(a =>
            a.AttributeClass?.Name == "ActionAttribute" || a.AttributeClass?.Name == "Action");

        if (!hasActionAttribute)
            return;

        // Check if the method has an IProgressContext parameter
        var hasProgressContext = methodSymbol.Parameters.Any(p => IsProgressContextType(p.Type));

        if (!hasProgressContext)
            return;

        // Check if the method is async (returns Task, Task<T>, ValueTask, etc.)
        var isAsync = methodSymbol.IsAsync || IsTaskLikeReturnType(methodSymbol.ReturnType);

        if (!isAsync)
        {
            var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsProgressContextType(ITypeSymbol type)
    {
        var typeName = type.ToDisplayString();
        return typeName == "TeCLI.Console.IProgressContext" ||
               typeName == "IProgressContext" ||
               (type.Name == "IProgressContext" && type.ContainingNamespace?.ToDisplayString() == "TeCLI.Console");
    }

    private static bool IsTaskLikeReturnType(ITypeSymbol returnType)
    {
        var typeName = returnType.Name;
        var namespaceName = returnType.ContainingNamespace?.ToDisplayString();

        // Check for Task, Task<T>, ValueTask, ValueTask<T>
        if (namespaceName == "System.Threading.Tasks")
        {
            return typeName == "Task" || typeName == "ValueTask";
        }

        return false;
    }
}
