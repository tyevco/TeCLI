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
/// Warns about potential name collisions with reserved CLI switches like --help, --version, etc.
/// These switches are commonly reserved by CLI frameworks and shells.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ReservedSwitchNameAnalyzer : DiagnosticAnalyzer
{
    // Reserved long option names
    private static readonly HashSet<string> ReservedLongNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "help",
        "version",
        "verbose",
        "debug",
        "quiet",
        "silent",
        "config",
        "configuration"
    };

    // Reserved short option names
    private static readonly HashSet<char> ReservedShortNames = new()
    {
        'h', // help
        'v', // version or verbose
        'V', // version (case-sensitive in some CLIs)
        'q', // quiet
        'd', // debug
        '?'  // help (Windows convention)
    };

    private static readonly DiagnosticDescriptor ReservedLongNameRule = new(
        "CLI017",
        "Option name conflicts with reserved switch",
        "Option name '{0}' may conflict with the commonly reserved '--{1}' switch. Consider using a different name.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Some option names are commonly reserved by CLI frameworks (e.g., --help, --version). Using these names may cause unexpected behavior.");

    private static readonly DiagnosticDescriptor ReservedShortNameRule = new(
        "CLI017",
        "Short name conflicts with reserved switch",
        "Short name '-{0}' may conflict with a commonly reserved switch. Consider using a different short name.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Some short option names are commonly reserved by CLI frameworks (e.g., -h for help, -v for version). Using these may cause unexpected behavior.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(ReservedLongNameRule, ReservedShortNameRule);

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

        CheckOptionAttribute(context, optionAttr, propertySymbol.Name, propertyDeclaration.GetLocation());
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

        optionName ??= fallbackName;

        // Check if the option name is reserved
        if (!string.IsNullOrEmpty(optionName) && ReservedLongNames.Contains(optionName))
        {
            var diagnostic = Diagnostic.Create(
                ReservedLongNameRule,
                location,
                optionName,
                optionName.ToLowerInvariant());
            context.ReportDiagnostic(diagnostic);
        }

        // Get the short name
        var shortNameArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "ShortName").Value;
        if (!shortNameArg.IsNull && shortNameArg.Value is char shortName && shortName != '\0')
        {
            // Check if the short name is reserved
            if (ReservedShortNames.Contains(shortName))
            {
                var diagnostic = Diagnostic.Create(
                    ReservedShortNameRule,
                    location,
                    shortName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
