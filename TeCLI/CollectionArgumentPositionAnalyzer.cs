using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Ensures collection/array arguments are in the last position, as they consume
/// all remaining positional arguments.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CollectionArgumentPositionAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI021",
        "Collection argument not in last position",
        "Collection argument '{0}' must be the last positional argument as it consumes all remaining values.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Collection or array arguments consume all remaining positional values and must therefore be the last positional argument in the method signature.");

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

        // Get all argument parameters (not options)
        var argumentParameters = methodSymbol.Parameters
            .Where(p => IsArgumentParameter(p))
            .ToList();

        if (argumentParameters.Count < 2)
            return;

        // Find collection arguments that are not in the last position
        for (int i = 0; i < argumentParameters.Count - 1; i++)
        {
            var param = argumentParameters[i];
            if (IsCollectionType(param.Type))
            {
                var paramSyntax = param.GetSyntax<ParameterSyntax>();
                if (paramSyntax != null)
                {
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        paramSyntax.GetLocation(),
                        param.Name);
                    context.ReportDiagnostic(diagnostic);
                }
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
            (parameter.Type.IsValidOptionType() || IsCollectionType(parameter.Type)))
            return true;

        return false;
    }

    private static bool IsCollectionType(ITypeSymbol type)
    {
        // Array types
        if (type is IArrayTypeSymbol)
            return true;

        // Generic collection types
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var typeName = namedType.OriginalDefinition.ToDisplayString();
            return typeName.StartsWith("System.Collections.Generic.List<") ||
                   typeName.StartsWith("System.Collections.Generic.IEnumerable<") ||
                   typeName.StartsWith("System.Collections.Generic.ICollection<") ||
                   typeName.StartsWith("System.Collections.Generic.IList<") ||
                   typeName.StartsWith("System.Collections.Generic.IReadOnlyCollection<") ||
                   typeName.StartsWith("System.Collections.Generic.IReadOnlyList<");
        }

        return false;
    }
}
