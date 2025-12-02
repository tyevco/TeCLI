using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Detects when an Option or Argument name explicitly matches the property/parameter name,
/// which is redundant since the framework uses the member name by default.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RedundantNameAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI027",
        "Redundant name specification",
        "{0} name '{1}' matches the {2} name. This is redundant and can be omitted.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "When the Option or Argument name matches the property or parameter name, it's redundant to specify it explicitly as the framework uses the member name by default.");

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

        CheckSymbol(context, propertySymbol, propertySymbol.Name, "property", propertyDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameterSyntax = (ParameterSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
            return;

        CheckSymbol(context, parameterSymbol, parameterSymbol.Name, "parameter", parameterSyntax.GetLocation());
    }

    private void CheckSymbol(SyntaxNodeAnalysisContext context, ISymbol symbol, string memberName, string memberType, Location location)
    {
        // Check Option attribute
        var optionAttr = symbol.GetAttribute(AttributeNames.OptionAttribute);
        if (optionAttr != null)
        {
            var specifiedName = GetSpecifiedName(optionAttr);
            if (NamesMatch(specifiedName, memberName))
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    "Option",
                    specifiedName,
                    memberType);
                context.ReportDiagnostic(diagnostic);
            }
            return;
        }

        // Check Argument attribute
        var argumentAttr = symbol.GetAttribute(AttributeNames.ArgumentAttribute);
        if (argumentAttr != null)
        {
            var nameArg = argumentAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
            if (!nameArg.IsNull)
            {
                var specifiedName = nameArg.Value?.ToString();
                if (NamesMatch(specifiedName, memberName))
                {
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        location,
                        "Argument",
                        specifiedName,
                        memberType);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static string? GetSpecifiedName(AttributeData optionAttr)
    {
        var nameArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
        if (!nameArg.IsNull)
        {
            return nameArg.Value?.ToString();
        }

        if (optionAttr.ConstructorArguments.Length > 0 && !optionAttr.ConstructorArguments[0].IsNull)
        {
            return optionAttr.ConstructorArguments[0].Value?.ToString();
        }

        return null;
    }

    private static bool NamesMatch(string? specifiedName, string memberName)
    {
        if (string.IsNullOrEmpty(specifiedName))
            return false;

        // Case-insensitive comparison since CLI names are often case-insensitive
        return string.Equals(specifiedName, memberName, StringComparison.OrdinalIgnoreCase);
    }
}
