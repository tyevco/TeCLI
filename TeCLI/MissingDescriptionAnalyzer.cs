using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Suggests adding descriptions to commands, options, and arguments for better help text.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MissingDescriptionAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor CommandRule = new(
        "CLI019",
        "Missing command description",
        "Command '{0}' does not have a description. Consider adding a Description for better help text.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Commands should have descriptions to provide helpful information to users.");

    private static readonly DiagnosticDescriptor OptionRule = new(
        "CLI019",
        "Missing option description",
        "Option '{0}' does not have a description. Consider adding a Description for better help text.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Options should have descriptions to provide helpful information to users.");

    private static readonly DiagnosticDescriptor ArgumentRule = new(
        "CLI019",
        "Missing argument description",
        "Argument '{0}' does not have a description. Consider adding a Description for better help text.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Arguments should have descriptions to provide helpful information to users.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CommandRule, OptionRule, ArgumentRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return;

        var commandAttr = classSymbol.GetAttribute(AttributeNames.CommandAttribute);
        if (commandAttr == null)
            return;

        // Check if Description is set
        var hasDescription = commandAttr.NamedArguments.Any(arg =>
            arg.Key == "Description" && !string.IsNullOrWhiteSpace(arg.Value.Value?.ToString()));

        if (!hasDescription)
        {
            var diagnostic = Diagnostic.Create(
                CommandRule,
                classDeclaration.Identifier.GetLocation(),
                classSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(propertyDeclaration) is not IPropertySymbol propertySymbol)
            return;

        // Check Option attribute
        var optionAttr = propertySymbol.GetAttribute(AttributeNames.OptionAttribute);
        if (optionAttr != null)
        {
            var hasDescription = optionAttr.NamedArguments.Any(arg =>
                arg.Key == "Description" && !string.IsNullOrWhiteSpace(arg.Value.Value?.ToString()));

            if (!hasDescription)
            {
                var diagnostic = Diagnostic.Create(
                    OptionRule,
                    propertyDeclaration.Identifier.GetLocation(),
                    propertySymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
            return;
        }

        // Check Argument attribute
        var argumentAttr = propertySymbol.GetAttribute(AttributeNames.ArgumentAttribute);
        if (argumentAttr != null)
        {
            var hasDescription = argumentAttr.NamedArguments.Any(arg =>
                arg.Key == "Description" && !string.IsNullOrWhiteSpace(arg.Value.Value?.ToString()));

            if (!hasDescription)
            {
                var diagnostic = Diagnostic.Create(
                    ArgumentRule,
                    propertyDeclaration.Identifier.GetLocation(),
                    propertySymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameterSyntax = (ParameterSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
            return;

        // Check Option attribute
        var optionAttr = parameterSymbol.GetAttribute(AttributeNames.OptionAttribute);
        if (optionAttr != null)
        {
            var hasDescription = optionAttr.NamedArguments.Any(arg =>
                arg.Key == "Description" && !string.IsNullOrWhiteSpace(arg.Value.Value?.ToString()));

            if (!hasDescription)
            {
                var diagnostic = Diagnostic.Create(
                    OptionRule,
                    parameterSyntax.GetLocation(),
                    parameterSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
            return;
        }

        // Check Argument attribute
        var argumentAttr = parameterSymbol.GetAttribute(AttributeNames.ArgumentAttribute);
        if (argumentAttr != null)
        {
            var hasDescription = argumentAttr.NamedArguments.Any(arg =>
                arg.Key == "Description" && !string.IsNullOrWhiteSpace(arg.Value.Value?.ToString()));

            if (!hasDescription)
            {
                var diagnostic = Diagnostic.Create(
                    ArgumentRule,
                    parameterSyntax.GetLocation(),
                    parameterSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
