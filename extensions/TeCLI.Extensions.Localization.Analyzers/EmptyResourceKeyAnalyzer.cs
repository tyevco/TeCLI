using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace TeCLI.Localization.Analyzers;

/// <summary>
/// Detects empty or whitespace resource keys in localization attributes.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EmptyResourceKeyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "TCLI18N001",
        "Empty resource key",
        "Resource key in [{0}] is empty or whitespace",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Localization attributes require a non-empty resource key to look up localized strings.");

    private static readonly string[] LocalizationAttributes = new[]
    {
        "LocalizedDescription",
        "LocalizedDescriptionAttribute",
        "LocalizedPrompt",
        "LocalizedPromptAttribute",
        "LocalizedErrorMessage",
        "LocalizedErrorMessageAttribute"
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;
        var attributeName = attributeSyntax.Name.ToString();

        // Check if this is a localization attribute
        if (!LocalizationAttributes.Any(a => attributeName.EndsWith(a.Replace("Attribute", "")) || attributeName.EndsWith(a)))
            return;

        // Get the semantic model info
        var symbolInfo = context.SemanticModel.GetSymbolInfo(attributeSyntax);
        if (symbolInfo.Symbol is not IMethodSymbol attributeConstructor)
            return;

        var attributeClass = attributeConstructor.ContainingType;
        if (!LocalizationAttributes.Contains(attributeClass.Name))
            return;

        // Check constructor argument (resource key)
        if (attributeSyntax.ArgumentList?.Arguments.Count > 0)
        {
            var firstArg = attributeSyntax.ArgumentList.Arguments[0];

            // Skip if it's a named argument for something other than the resource key
            if (firstArg.NameEquals != null && firstArg.NameEquals.Name.ToString() != "resourceKey")
                return;

            var constantValue = context.SemanticModel.GetConstantValue(firstArg.Expression);
            if (constantValue.HasValue && constantValue.Value is string resourceKey)
            {
                if (string.IsNullOrWhiteSpace(resourceKey))
                {
                    var attrName = attributeClass.Name.Replace("Attribute", "");
                    var diagnostic = Diagnostic.Create(Rule, attributeSyntax.GetLocation(), attrName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
