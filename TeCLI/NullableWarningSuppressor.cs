namespace TeCLI.Analyzers;

using TeCLI.Attributes;
using TeCLI.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullableWarningSuppressor : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor Rule = new(
        id: "CLI900",
        suppressedDiagnosticId: "CS8618",
        justification:
        "Non-nullable property is guaranteed to be initialized due to being marked with Argument or Option attributes.");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(Rule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            var tree = diagnostic.Location.SourceTree;
            if (tree is not null)
            {
                var root = tree.GetRoot(context.CancellationToken);
                var node = root.FindNode(diagnostic.Location.SourceSpan);

                // Ensure the node is a property declaration
                if (node is PropertyDeclarationSyntax propertyDeclaration)
                {
                    if (ContainsParameters(propertyDeclaration))
                    {
                        context.ReportSuppression(Suppression.Create(Rule, diagnostic));
                    }
                }
            }
        }
    }

    private bool ContainsParameters(PropertyDeclarationSyntax propertyDecl)
    {
        return propertyDecl.AttributeLists.Any(al => al.Attributes.HasAnyAttribute<ArgumentAttribute, OptionAttribute>());
    }
}