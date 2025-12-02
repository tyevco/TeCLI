using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Suggests using a container class (parameter object) when a method has 4 or more options.
/// This improves code organization and makes the command definition more maintainable.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ContainerParameterSuggestionAnalyzer : DiagnosticAnalyzer
{
    private const int OptionThreshold = 4;

    private static readonly DiagnosticDescriptor Rule = new(
        "CLI014",
        "Consider using container parameter",
        "Method '{0}' has {1} options. Consider grouping related options into a container class for better organization.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: $"When a method has {OptionThreshold} or more options, consider using a container class (parameter object pattern) to group related options for better maintainability.");

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

        // Check if this method is an action method
        var isAction = methodSymbol.HasAttribute(AttributeNames.ActionAttribute) ||
                       methodSymbol.HasAttribute(AttributeNames.PrimaryAttribute);

        if (!isAction)
            return;

        // Count options in parameters
        var optionCount = methodSymbol.Parameters
            .Count(p => p.HasAttribute(AttributeNames.OptionAttribute));

        // Check if any parameter is already a container (non-primitive type without specific attributes)
        var hasContainerParameter = methodSymbol.Parameters
            .Any(p => IsContainerParameter(p));

        // Only suggest if threshold is met and no container parameter exists
        if (optionCount >= OptionThreshold && !hasContainerParameter)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                methodDeclaration.Identifier.GetLocation(),
                methodSymbol.Name,
                optionCount);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsContainerParameter(IParameterSymbol parameter)
    {
        // Skip if it has explicit CLI attributes
        if (parameter.HasAttribute(AttributeNames.OptionAttribute) ||
            parameter.HasAttribute(AttributeNames.ArgumentAttribute))
            return false;

        // Check if it's a class or struct type (potential container)
        var type = parameter.Type;

        // Skip primitive types and common types
        if (type.IsValidOptionType())
            return false;

        // Skip CancellationToken
        if (type.IsCancellationToken())
            return false;

        // It's likely a container type
        return type.TypeKind == TypeKind.Class || type.TypeKind == TypeKind.Struct;
    }
}
