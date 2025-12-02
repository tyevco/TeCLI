using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConflictingShortNameAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI010",
        "Conflicting short names",
        "Short name '{0}' is used by multiple options in method '{1}'. Each option must have a unique short name.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Option short names must be unique within a method.");

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

        // Check if this method is an action or in a command class
        var isAction = methodSymbol.HasAttribute(AttributeNames.ActionAttribute) || methodSymbol.HasAttribute(AttributeNames.PrimaryAttribute);
        var containingClass = methodSymbol.ContainingType;
        var isInCommandClass = containingClass?.HasAttribute(AttributeNames.CommandAttribute) ?? false;

        if (!isAction && !isInCommandClass)
            return;

        // Collect all short names and their parameters
        var shortNames = new Dictionary<char, List<IParameterSymbol>>();

        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.TryGetAttribute(out var optionAttr, AttributeNames.OptionAttribute) && optionAttr != null)
            {
                var shortNameArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "ShortName").Value;

                if (!shortNameArg.IsNull && shortNameArg.Value is char shortName && shortName != '\0')
                {
                    if (!shortNames.ContainsKey(shortName))
                    {
                        shortNames[shortName] = new List<IParameterSymbol>();
                    }
                    shortNames[shortName].Add(parameter);
                }
            }
        }

        // Report conflicts
        foreach (var kvp in shortNames)
        {
            var shortName = kvp.Key;
            var parameters = kvp.Value;
            if (parameters.Count > 1)
            {
                // Report diagnostic for all but the first parameter
                foreach (var parameter in parameters.Skip(1))
                {
                    var paramSyntax = parameter.GetSyntax<ParameterSyntax>();
                    if (paramSyntax != null)
                    {
                        var diagnostic = Diagnostic.Create(
                            Rule,
                            paramSyntax.GetLocation(),
                            shortName,
                            methodSymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
