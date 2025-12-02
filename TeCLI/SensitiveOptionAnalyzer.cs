using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Suggests security considerations when options have sensitive-looking names
/// like password, secret, token, or key.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SensitiveOptionAnalyzer : DiagnosticAnalyzer
{
    private static readonly HashSet<string> SensitiveTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "passwd",
        "pwd",
        "secret",
        "token",
        "apikey",
        "api-key",
        "api_key",
        "accesskey",
        "access-key",
        "access_key",
        "privatekey",
        "private-key",
        "private_key",
        "credential",
        "credentials",
        "auth",
        "authorization"
    };

    private static readonly DiagnosticDescriptor Rule = new(
        "CLI032",
        "Sensitive option detected",
        "Option '{0}' appears to handle sensitive data. Consider using secure input methods to avoid exposing values in shell history.",
        "Security",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Options that handle sensitive data like passwords, tokens, or API keys should use secure input methods (e.g., prompting without echo) to avoid exposing values in shell history or process listings.");

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

        var optionName = GetOptionName(optionAttr, propertySymbol.Name);
        CheckSensitiveName(context, optionName, propertySymbol.Name, propertyDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameterSyntax = (ParameterSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
            return;

        var optionAttr = parameterSymbol.GetAttribute(AttributeNames.OptionAttribute);
        if (optionAttr == null)
            return;

        var optionName = GetOptionName(optionAttr, parameterSymbol.Name);
        CheckSensitiveName(context, optionName, parameterSymbol.Name, parameterSyntax.GetLocation());
    }

    private static string GetOptionName(AttributeData optionAttr, string fallbackName)
    {
        var nameArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
        if (!nameArg.IsNull)
        {
            return nameArg.Value?.ToString() ?? fallbackName;
        }

        if (optionAttr.ConstructorArguments.Length > 0 && !optionAttr.ConstructorArguments[0].IsNull)
        {
            return optionAttr.ConstructorArguments[0].Value?.ToString() ?? fallbackName;
        }

        return fallbackName;
    }

    private void CheckSensitiveName(SyntaxNodeAnalysisContext context, string optionName, string memberName, Location location)
    {
        // Check both the option name and the member name
        if (ContainsSensitiveTerm(optionName) || ContainsSensitiveTerm(memberName))
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                optionName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool ContainsSensitiveTerm(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        // Check exact match
        if (SensitiveTerms.Contains(name))
            return true;

        // Check if name contains any sensitive term
        var normalizedName = name.ToLowerInvariant();
        foreach (var term in SensitiveTerms)
        {
            if (normalizedName.Contains(term))
                return true;
        }

        return false;
    }
}
