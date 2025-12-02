using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Warns when multiple classes are marked with [GlobalOptions], which may cause
/// ambiguity in option resolution.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MultipleGlobalOptionsAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI031",
        "Multiple GlobalOptions classes",
        "Multiple classes are marked with [GlobalOptions]. This may cause ambiguity in option resolution.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Having multiple classes marked with [GlobalOptions] attribute may cause confusion or conflicts. Consider consolidating into a single global options class.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var globalOptionsClasses = new ConcurrentBag<(INamedTypeSymbol Symbol, Location Location)>();

        context.RegisterSyntaxNodeAction(ctx => CollectGlobalOptions(ctx, globalOptionsClasses), SyntaxKind.ClassDeclaration);
        context.RegisterCompilationEndAction(ctx => ReportMultiple(ctx, globalOptionsClasses));
    }

    private void CollectGlobalOptions(
        SyntaxNodeAnalysisContext context,
        ConcurrentBag<(INamedTypeSymbol, Location)> globalOptionsClasses)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return;

        if (classSymbol.HasAttribute(AttributeNames.GlobalOptionsAttribute))
        {
            globalOptionsClasses.Add((classSymbol, classDeclaration.Identifier.GetLocation()));
        }
    }

    private void ReportMultiple(
        CompilationAnalysisContext context,
        ConcurrentBag<(INamedTypeSymbol Symbol, Location Location)> globalOptionsClasses)
    {
        var classes = globalOptionsClasses.ToList();

        if (classes.Count > 1)
        {
            // Report on all classes
            foreach (var (_, location) in classes)
            {
                var diagnostic = Diagnostic.Create(Rule, location);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
