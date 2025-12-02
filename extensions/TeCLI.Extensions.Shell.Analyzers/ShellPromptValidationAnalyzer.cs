using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace TeCLI.Shell.Analyzers;

/// <summary>
/// Validates the Prompt property in [Shell] attribute for usability issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ShellPromptValidationAnalyzer : DiagnosticAnalyzer
{
    private const int MaxRecommendedPromptLength = 30;

    private static readonly DiagnosticDescriptor EmptyPromptRule = new(
        "TCLSHELL003",
        "Empty shell prompt",
        "Shell prompt is empty. Users may not realize they are in shell mode. Consider using a meaningful prompt.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "An empty or blank prompt can confuse users about whether they are in interactive shell mode.");

    private static readonly DiagnosticDescriptor LongPromptRule = new(
        "TCLSHELL003",
        "Excessively long shell prompt",
        "Shell prompt is {0} characters long. Long prompts reduce available space for commands. Consider shortening to under {1} characters.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Very long prompts can make the command line hard to use, especially on narrow terminals.");

    private static readonly DiagnosticDescriptor NoTrailingSpaceRule = new(
        "TCLSHELL003",
        "Shell prompt without trailing space",
        "Shell prompt '{0}' doesn't end with a space. Commands may appear to run into the prompt.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Prompts typically end with a space to visually separate them from user input.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(EmptyPromptRule, LongPromptRule, NoTrailingSpaceRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        // Check if this is a Shell attribute
        var symbolInfo = context.SemanticModel.GetSymbolInfo(attributeSyntax);
        if (symbolInfo.Symbol is not IMethodSymbol attributeConstructor)
            return;

        var attributeClass = attributeConstructor.ContainingType;
        if (attributeClass.Name != "ShellAttribute" && attributeClass.Name != "Shell")
            return;

        string? prompt = null;
        Location? promptLocation = null;

        // Check constructor argument first (if Shell(prompt) constructor is used)
        if (attributeSyntax.ArgumentList?.Arguments.Count > 0)
        {
            var firstArg = attributeSyntax.ArgumentList.Arguments[0];
            if (firstArg.NameEquals == null)
            {
                var constantValue = context.SemanticModel.GetConstantValue(firstArg.Expression);
                if (constantValue.HasValue && constantValue.Value is string promptValue)
                {
                    prompt = promptValue;
                    promptLocation = firstArg.GetLocation();
                }
            }
        }

        // Check for Prompt named property
        if (attributeSyntax.ArgumentList != null)
        {
            foreach (var arg in attributeSyntax.ArgumentList.Arguments)
            {
                if (arg.NameEquals?.Name.ToString() == "Prompt")
                {
                    var constantValue = context.SemanticModel.GetConstantValue(arg.Expression);
                    if (constantValue.HasValue && constantValue.Value is string promptValue)
                    {
                        prompt = promptValue;
                        promptLocation = arg.GetLocation();
                        break;
                    }
                }
            }
        }

        if (prompt == null || promptLocation == null)
            return;

        // Check for empty prompt
        if (string.IsNullOrWhiteSpace(prompt))
        {
            var diagnostic = Diagnostic.Create(EmptyPromptRule, promptLocation);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Check for excessively long prompt
        if (prompt.Length > MaxRecommendedPromptLength)
        {
            var diagnostic = Diagnostic.Create(LongPromptRule, promptLocation, prompt.Length, MaxRecommendedPromptLength);
            context.ReportDiagnostic(diagnostic);
        }

        // Check for missing trailing space
        if (!prompt.EndsWith(" "))
        {
            var diagnostic = Diagnostic.Create(NoTrailingSpaceRule, promptLocation, prompt);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
