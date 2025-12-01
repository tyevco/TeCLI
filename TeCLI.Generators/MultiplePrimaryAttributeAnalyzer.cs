using TeCLI.Attributes;
using TeCLI.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace TeCLI.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MultiplePrimaryAttributeAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI003",
        "Multiple primary actions defined",
        "Command class '{0}' has multiple methods marked with [Primary]. Only one primary action is allowed per command.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A command class can have only one method marked with the [Primary] attribute.");

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

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is INamedTypeSymbol classSymbol)
        {
            // Check if this is a command class
            if (!classSymbol.HasAttribute<CommandAttribute>())
                return;

            // Get all methods with PrimaryAttribute
            var primaryMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol, PrimaryAttribute>()?.ToList();

            if (primaryMethods != null && primaryMethods.Count > 1)
            {
                // Report diagnostic for each method after the first one
                foreach (var method in primaryMethods.Skip(1))
                {
                    var methodSyntax = method.GetSyntax<MethodDeclarationSyntax>();
                    if (methodSyntax != null)
                    {
                        var diagnostic = Diagnostic.Create(Rule, methodSyntax.GetLocation(), classSymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
