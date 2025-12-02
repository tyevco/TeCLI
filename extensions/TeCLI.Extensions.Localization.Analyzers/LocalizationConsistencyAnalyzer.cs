using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace TeCLI.Localization.Analyzers;

/// <summary>
/// Detects inconsistent localization usage within a command - some items localized while others are not.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LocalizationConsistencyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "TCLI18N003",
        "Inconsistent localization usage",
        "Command '{0}' has mixed localization usage: {1} items use localization, {2} items do not. Consider localizing all items for consistency.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "For a consistent user experience, consider using localization for all descriptions within a command, or none at all.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return;

        // Only analyze command classes
        var commandAttr = classSymbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Name == "CommandAttribute" || a.AttributeClass?.Name == "Command");

        if (commandAttr == null)
            return;

        int localizedCount = 0;
        int nonLocalizedCount = 0;

        // Check the command itself
        if (HasLocalizedDescription(classSymbol))
            localizedCount++;
        else if (HasDescription(commandAttr))
            nonLocalizedCount++;

        // Check all actions
        foreach (var method in classSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            var actionAttr = method.GetAttributes().FirstOrDefault(a =>
                a.AttributeClass?.Name == "ActionAttribute" || a.AttributeClass?.Name == "Action");

            if (actionAttr == null)
                continue;

            if (HasLocalizedDescription(method))
                localizedCount++;
            else if (HasDescription(actionAttr))
                nonLocalizedCount++;

            // Check action parameters
            foreach (var param in method.Parameters)
            {
                var optionAttr = param.GetAttributes().FirstOrDefault(a =>
                    a.AttributeClass?.Name == "OptionAttribute" || a.AttributeClass?.Name == "Option");
                var argAttr = param.GetAttributes().FirstOrDefault(a =>
                    a.AttributeClass?.Name == "ArgumentAttribute" || a.AttributeClass?.Name == "Argument");

                var cliAttr = optionAttr ?? argAttr;
                if (cliAttr == null)
                    continue;

                if (HasLocalizedDescription(param))
                    localizedCount++;
                else if (HasDescription(cliAttr))
                    nonLocalizedCount++;
            }
        }

        // Check properties
        foreach (var property in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            var optionAttr = property.GetAttributes().FirstOrDefault(a =>
                a.AttributeClass?.Name == "OptionAttribute" || a.AttributeClass?.Name == "Option");
            var argAttr = property.GetAttributes().FirstOrDefault(a =>
                a.AttributeClass?.Name == "ArgumentAttribute" || a.AttributeClass?.Name == "Argument");

            var cliAttr = optionAttr ?? argAttr;
            if (cliAttr == null)
                continue;

            if (HasLocalizedDescription(property))
                localizedCount++;
            else if (HasDescription(cliAttr))
                nonLocalizedCount++;
        }

        // Report if there's a mix (some localized, some not)
        if (localizedCount > 0 && nonLocalizedCount > 0)
        {
            var commandName = GetCommandName(commandAttr, classSymbol.Name);
            var diagnostic = Diagnostic.Create(
                Rule,
                classDeclaration.Identifier.GetLocation(),
                commandName,
                localizedCount,
                nonLocalizedCount);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool HasLocalizedDescription(ISymbol symbol)
    {
        return symbol.GetAttributes().Any(a =>
            a.AttributeClass?.Name == "LocalizedDescriptionAttribute" ||
            a.AttributeClass?.Name == "LocalizedDescription");
    }

    private static bool HasDescription(AttributeData attr)
    {
        return attr.NamedArguments.Any(arg =>
            arg.Key == "Description" && !string.IsNullOrWhiteSpace(arg.Value.Value?.ToString()));
    }

    private static string GetCommandName(AttributeData attr, string fallbackName)
    {
        if (attr.ConstructorArguments.Length > 0 && !attr.ConstructorArguments[0].IsNull)
        {
            return attr.ConstructorArguments[0].Value?.ToString() ?? fallbackName;
        }

        var nameArg = attr.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
        if (!nameArg.IsNull)
        {
            return nameArg.Value?.ToString() ?? fallbackName;
        }

        return fallbackName;
    }
}
