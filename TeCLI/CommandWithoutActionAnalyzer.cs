using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Warns when a command class has no action methods ([Primary] or [Action]),
/// making it non-executable.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CommandWithoutActionAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI024",
        "Command class without action methods",
        "Command class '{0}' has no action methods. Add at least one method with [Primary] or [Action] attribute.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Command classes should have at least one action method (marked with [Primary] or [Action]) to be executable.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return;

        // Only analyze command classes
        if (!classSymbol.HasAttribute(AttributeNames.CommandAttribute))
            return;

        // Check if class has any action methods
        var hasActionMethod = classSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m => m.HasAttribute(AttributeNames.ActionAttribute) ||
                      m.HasAttribute(AttributeNames.PrimaryAttribute));

        if (!hasActionMethod)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                classDeclaration.Identifier.GetLocation(),
                classSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
