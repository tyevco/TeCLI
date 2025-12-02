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
/// Detects duplicate command names across different command classes.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DuplicateCommandNameAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI018",
        "Duplicate command name",
        "Command name '{0}' is already used by another command class. Each command must have a unique name.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Command names must be unique across all command classes to avoid routing conflicts.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var commandNames = new ConcurrentDictionary<string, ConcurrentBag<(INamedTypeSymbol Symbol, Location Location)>>(
            System.StringComparer.OrdinalIgnoreCase);

        context.RegisterSyntaxNodeAction(ctx => CollectCommandNames(ctx, commandNames), SyntaxKind.ClassDeclaration);
        context.RegisterCompilationEndAction(ctx => ReportDuplicates(ctx, commandNames));
    }

    private void CollectCommandNames(
        SyntaxNodeAnalysisContext context,
        ConcurrentDictionary<string, ConcurrentBag<(INamedTypeSymbol, Location)>> commandNames)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return;

        var commandAttr = classSymbol.GetAttribute(AttributeNames.CommandAttribute);
        if (commandAttr == null)
            return;

        string? commandName = null;

        if (commandAttr.ConstructorArguments.Length > 0)
        {
            commandName = commandAttr.ConstructorArguments[0].Value?.ToString();
        }

        if (string.IsNullOrWhiteSpace(commandName))
            return;

        var bag = commandNames.GetOrAdd(commandName!, _ => new ConcurrentBag<(INamedTypeSymbol, Location)>());
        bag.Add((classSymbol, classDeclaration.Identifier.GetLocation()));
    }

    private void ReportDuplicates(
        CompilationAnalysisContext context,
        ConcurrentDictionary<string, ConcurrentBag<(INamedTypeSymbol Symbol, Location Location)>> commandNames)
    {
        foreach (var kvp in commandNames)
        {
            var name = kvp.Key;
            var commands = kvp.Value.ToList();

            if (commands.Count > 1)
            {
                // Report on all but the first occurrence
                foreach (var (_, location) in commands.Skip(1))
                {
                    var diagnostic = Diagnostic.Create(Rule, location, name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
