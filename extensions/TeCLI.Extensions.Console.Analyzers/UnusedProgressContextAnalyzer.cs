using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace TeCLI.Console.Analyzers;

/// <summary>
/// Detects when IProgressContext is declared as a parameter but never used in the method body.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnusedProgressContextAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "TCLCON002",
        "Unused IProgressContext parameter",
        "Parameter '{0}' of type IProgressContext is declared but never used. Remove it if progress indication is not needed.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Declaring an IProgressContext parameter that is never used adds unnecessary overhead and may confuse other developers about the method's behavior.");

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

        // Find IProgressContext parameters
        foreach (var parameter in methodSymbol.Parameters)
        {
            if (!IsProgressContextType(parameter.Type))
                continue;

            // Check if the parameter is used in the method body
            if (methodDeclaration.Body == null && methodDeclaration.ExpressionBody == null)
                continue;

            var dataFlow = methodDeclaration.Body != null
                ? context.SemanticModel.AnalyzeDataFlow(methodDeclaration.Body)
                : context.SemanticModel.AnalyzeDataFlow(methodDeclaration.ExpressionBody!.Expression);

            if (dataFlow == null || !dataFlow.Succeeded)
                continue;

            // Check if the parameter is read or written
            var isUsed = dataFlow.ReadInside.Contains(parameter) ||
                         dataFlow.WrittenInside.Contains(parameter) ||
                         IsReferencedInBody(methodDeclaration, parameter.Name, context.SemanticModel);

            if (!isUsed)
            {
                var parameterSyntax = methodDeclaration.ParameterList.Parameters
                    .FirstOrDefault(p => p.Identifier.Text == parameter.Name);

                if (parameterSyntax != null)
                {
                    var diagnostic = Diagnostic.Create(Rule, parameterSyntax.GetLocation(), parameter.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool IsProgressContextType(ITypeSymbol type)
    {
        var typeName = type.ToDisplayString();
        return typeName == "TeCLI.Console.IProgressContext" ||
               typeName == "IProgressContext" ||
               (type.Name == "IProgressContext" && type.ContainingNamespace?.ToDisplayString() == "TeCLI.Console");
    }

    private static bool IsReferencedInBody(MethodDeclarationSyntax method, string parameterName, SemanticModel semanticModel)
    {
        // Look for any identifier nodes that reference the parameter
        var identifiers = method.DescendantNodes().OfType<IdentifierNameSyntax>();

        foreach (var identifier in identifiers)
        {
            if (identifier.Identifier.Text == parameterName)
            {
                var symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
                if (symbol is IParameterSymbol)
                    return true;
            }
        }

        return false;
    }
}
