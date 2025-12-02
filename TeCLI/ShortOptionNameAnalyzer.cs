using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Suggests using ShortName property when an option's long name is a single character.
/// Single-character names should use the short name syntax (-x) instead of long name (--x).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ShortOptionNameAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI026",
        "Single-character option name",
        "Option '{0}' has a single-character name. Consider using ShortName property instead for '-{1}' syntax.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Single-character option names are typically specified as short names (e.g., -v) rather than long names (--v). Use the ShortName property for single characters.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        var optionAttr = propertySymbol.GetAttribute(AttributeNames.OptionAttribute);
        if (optionAttr == null)
            return;

        CheckOptionAttribute(context, optionAttr, propertySymbol.Name, propertyDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameterSyntax = (ParameterSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
            return;

        var optionAttr = parameterSymbol.GetAttribute(AttributeNames.OptionAttribute);
        if (optionAttr == null)
            return;

        CheckOptionAttribute(context, optionAttr, parameterSymbol.Name, parameterSyntax.GetLocation());
    }

    private void CheckOptionAttribute(SyntaxNodeAnalysisContext context, AttributeData optionAttr, string fallbackName, Location location)
    {
        // Get the option name
        string? optionName = null;

        var nameArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
        if (!nameArg.IsNull)
        {
            optionName = nameArg.Value?.ToString();
        }
        else if (optionAttr.ConstructorArguments.Length > 0 && !optionAttr.ConstructorArguments[0].IsNull)
        {
            optionName = optionAttr.ConstructorArguments[0].Value?.ToString();
        }

        // If no explicit name, don't warn (the property name is likely fine)
        if (string.IsNullOrEmpty(optionName))
            return;

        // Check if it's a single character
        if (optionName!.Length == 1 && char.IsLetter(optionName[0]))
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                fallbackName,
                optionName);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
