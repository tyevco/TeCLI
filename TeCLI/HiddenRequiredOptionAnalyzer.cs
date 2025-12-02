using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Warns when an option is marked as both hidden and required, which is contradictory
/// since hidden options shouldn't be required from users.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HiddenRequiredOptionAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI028",
        "Hidden option marked as required",
        "Option '{0}' is marked as both hidden and required. Hidden options should not be required since users cannot see them in help.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Marking an option as both hidden and required is contradictory. Hidden options are not shown in help output, so requiring them creates a poor user experience.");

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

    private void CheckOptionAttribute(SyntaxNodeAnalysisContext context, AttributeData optionAttr, string name, Location location)
    {
        var hiddenArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Hidden").Value;
        var requiredArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Required").Value;

        bool isHidden = !hiddenArg.IsNull && hiddenArg.Value is true;
        bool isRequired = !requiredArg.IsNull && requiredArg.Value is true;

        if (isHidden && isRequired)
        {
            var diagnostic = Diagnostic.Create(Rule, location, name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
