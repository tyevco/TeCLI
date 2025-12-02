using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace TeCLI.Configuration.Analyzers;

/// <summary>
/// Suggests naming conventions for options that will map to configuration file keys.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConfigurationKeyNamingAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor KebabCaseRule = new(
        "TCLCFG001",
        "Option name uses kebab-case",
        "Option '{0}' uses kebab-case ('{1}'). In config files, this typically maps to camelCase or snake_case. Consider using a name that matches your config file convention.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "When using configuration file binding, option names are mapped to config keys. kebab-case option names (e.g., 'my-option') may need to be written as 'myOption' or 'my_option' in config files depending on the format.");

    private static readonly DiagnosticDescriptor UnconventionalNameRule = new(
        "TCLCFG001",
        "Option name contains unusual characters",
        "Option '{0}' contains characters that may not be valid in all configuration file formats",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Some configuration file formats have restrictions on key names. Using only alphanumeric characters, hyphens, and underscores ensures maximum compatibility.");

    private static readonly Regex KebabCasePattern = new(@"-[a-z]", RegexOptions.Compiled);
    private static readonly Regex UnusualCharsPattern = new(@"[^a-zA-Z0-9_-]", RegexOptions.Compiled);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(KebabCaseRule, UnconventionalNameRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(propertyDeclaration) is not IPropertySymbol propertySymbol)
            return;

        var optionAttr = propertySymbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Name == "OptionAttribute" || a.AttributeClass?.Name == "Option");

        if (optionAttr == null)
            return;

        var optionName = GetOptionName(optionAttr, propertySymbol.Name);
        AnalyzeOptionName(context, optionName, propertySymbol.Name, propertyDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameterSyntax = (ParameterSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
            return;

        var optionAttr = parameterSymbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Name == "OptionAttribute" || a.AttributeClass?.Name == "Option");

        if (optionAttr == null)
            return;

        var optionName = GetOptionName(optionAttr, parameterSymbol.Name);
        AnalyzeOptionName(context, optionName, parameterSymbol.Name, parameterSyntax.GetLocation());
    }

    private void AnalyzeOptionName(SyntaxNodeAnalysisContext context, string optionName, string memberName, Location location)
    {
        // Check for unusual characters that might not work in config files
        if (UnusualCharsPattern.IsMatch(optionName))
        {
            var diagnostic = Diagnostic.Create(UnconventionalNameRule, location, memberName);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Check for kebab-case which is common in CLI but less common in config files
        if (KebabCasePattern.IsMatch(optionName))
        {
            var diagnostic = Diagnostic.Create(KebabCaseRule, location, memberName, optionName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string GetOptionName(AttributeData optionAttr, string fallbackName)
    {
        // Check constructor argument
        if (optionAttr.ConstructorArguments.Length > 0 && !optionAttr.ConstructorArguments[0].IsNull)
        {
            return optionAttr.ConstructorArguments[0].Value?.ToString() ?? fallbackName;
        }

        // Check Name property
        var nameArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
        if (!nameArg.IsNull)
        {
            return nameArg.Value?.ToString() ?? fallbackName;
        }

        return fallbackName;
    }
}
