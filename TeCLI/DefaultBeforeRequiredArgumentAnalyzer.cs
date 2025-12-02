using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Analyzes method parameters to detect when an argument with a default value
/// appears before a required argument (one without a default value).
/// This is problematic because CLI argument parsing relies on positional order.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DefaultBeforeRequiredArgumentAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI013",
        "Optional argument before required argument",
        "Argument '{0}' has a default value but appears before required argument '{1}'. Required arguments should come before optional arguments.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Arguments with default values should come after required arguments to ensure correct positional parsing.");

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

        // Check if this method is an action method or in a command class
        var isAction = methodSymbol.HasAttribute(AttributeNames.ActionAttribute) ||
                       methodSymbol.HasAttribute(AttributeNames.PrimaryAttribute);
        var containingClass = methodSymbol.ContainingType;
        var isInCommandClass = containingClass?.HasAttribute(AttributeNames.CommandAttribute) ?? false;

        if (!isAction && !isInCommandClass)
            return;

        // Collect argument parameters (those with [Argument] attribute or treated as positional)
        var argumentParameters = methodSymbol.Parameters
            .Where(p => IsArgumentParameter(p))
            .ToList();

        if (argumentParameters.Count < 2)
            return;

        // Find the first required argument (no default value)
        IParameterSymbol? firstRequiredAfterOptional = null;
        IParameterSymbol? lastOptionalBeforeRequired = null;

        bool foundOptional = false;

        foreach (var param in argumentParameters)
        {
            bool hasDefaultValue = param.HasExplicitDefaultValue;

            if (hasDefaultValue)
            {
                foundOptional = true;
                lastOptionalBeforeRequired = param;
            }
            else if (foundOptional && firstRequiredAfterOptional == null)
            {
                // Found a required argument after an optional one
                firstRequiredAfterOptional = param;
            }
        }

        if (lastOptionalBeforeRequired != null && firstRequiredAfterOptional != null)
        {
            var paramSyntax = lastOptionalBeforeRequired.GetSyntax<ParameterSyntax>();
            if (paramSyntax != null)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    paramSyntax.GetLocation(),
                    lastOptionalBeforeRequired.Name,
                    firstRequiredAfterOptional.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsArgumentParameter(IParameterSymbol parameter)
    {
        // Explicitly marked with [Argument]
        if (parameter.HasAttribute(AttributeNames.ArgumentAttribute))
            return true;

        // Not marked with [Option] and is a valid option type (treated as positional argument)
        if (!parameter.HasAttribute(AttributeNames.OptionAttribute) &&
            parameter.Type.IsValidOptionType())
            return true;

        return false;
    }
}
