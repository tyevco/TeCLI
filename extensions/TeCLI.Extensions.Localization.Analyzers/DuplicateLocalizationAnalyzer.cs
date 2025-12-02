using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace TeCLI.Localization.Analyzers;

/// <summary>
/// Detects when both a Description property and LocalizedDescription attribute are used on the same element.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DuplicateLocalizationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "TCLI18N002",
        "Redundant description with localization",
        "{0} has both Description property and [LocalizedDescription] attribute. The localized description will be used at runtime, making Description redundant.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "When using [LocalizedDescription], the Description property becomes redundant as the localized version will be used at runtime. Consider removing the Description property to avoid confusion.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return;

        // Check for [Command] with Description and [LocalizedDescription]
        CheckForDuplicateDescription(context, classSymbol, "CommandAttribute", "Command", classDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol methodSymbol)
            return;

        // Check for [Action] with Description and [LocalizedDescription]
        CheckForDuplicateDescription(context, methodSymbol, "ActionAttribute", "Action", methodDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(propertyDeclaration) is not IPropertySymbol propertySymbol)
            return;

        // Check for [Option] with Description and [LocalizedDescription]
        CheckForDuplicateDescription(context, propertySymbol, "OptionAttribute", "Option", propertyDeclaration.Identifier.GetLocation());
        // Check for [Argument] with Description and [LocalizedDescription]
        CheckForDuplicateDescription(context, propertySymbol, "ArgumentAttribute", "Argument", propertyDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameterSyntax = (ParameterSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
            return;

        // Check for [Option] with Description and [LocalizedDescription]
        CheckForDuplicateDescription(context, parameterSymbol, "OptionAttribute", "Option", parameterSyntax.GetLocation());
        // Check for [Argument] with Description and [LocalizedDescription]
        CheckForDuplicateDescription(context, parameterSymbol, "ArgumentAttribute", "Argument", parameterSyntax.GetLocation());
    }

    private void CheckForDuplicateDescription(SyntaxNodeAnalysisContext context, ISymbol symbol, string cliAttributeName, string elementType, Location location)
    {
        var attributes = symbol.GetAttributes();

        // Find the CLI attribute (Command, Action, Option, or Argument)
        var cliAttr = attributes.FirstOrDefault(a =>
            a.AttributeClass?.Name == cliAttributeName ||
            a.AttributeClass?.Name == cliAttributeName.Replace("Attribute", ""));

        if (cliAttr == null)
            return;

        // Check if it has a Description property set
        var hasDescription = cliAttr.NamedArguments.Any(arg =>
            arg.Key == "Description" && !string.IsNullOrWhiteSpace(arg.Value.Value?.ToString()));

        if (!hasDescription)
            return;

        // Check if there's also a LocalizedDescription attribute
        var hasLocalizedDescription = attributes.Any(a =>
            a.AttributeClass?.Name == "LocalizedDescriptionAttribute" ||
            a.AttributeClass?.Name == "LocalizedDescription");

        if (hasLocalizedDescription)
        {
            var diagnostic = Diagnostic.Create(Rule, location, elementType);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
