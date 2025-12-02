using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Detects inconsistent naming conventions in option and command names.
/// Recommends using kebab-case for CLI consistency.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InconsistentNamingConventionAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI025",
        "Inconsistent naming convention",
        "{0} name '{1}' uses {2}. Consider using kebab-case for CLI consistency.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "CLI conventions typically use kebab-case (e.g., 'my-option') for options and commands. Mixing conventions can confuse users.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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
        if (commandAttr == null || commandAttr.ConstructorArguments.Length == 0)
            return;

        var commandName = commandAttr.ConstructorArguments[0].Value?.ToString();
        if (string.IsNullOrWhiteSpace(commandName))
            return;

        var convention = DetectNamingConvention(commandName!);
        if (convention != NamingConvention.KebabCase && convention != NamingConvention.LowerCase)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                classDeclaration.Identifier.GetLocation(),
                "Command",
                commandName,
                GetConventionName(convention));
            context.ReportDiagnostic(diagnostic);
        }
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(propertyDeclaration) is not IPropertySymbol propertySymbol)
            return;

        CheckOptionOrArgumentName(context, propertySymbol, propertyDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameterSyntax = (ParameterSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
            return;

        CheckOptionOrArgumentName(context, parameterSymbol, parameterSyntax.GetLocation());
    }

    private void CheckOptionOrArgumentName(SyntaxNodeAnalysisContext context, ISymbol symbol, Location location)
    {
        var optionAttr = symbol.GetAttribute(AttributeNames.OptionAttribute);
        if (optionAttr != null)
        {
            var name = GetOptionName(optionAttr, symbol.Name);
            CheckName(context, "Option", name, location);
            return;
        }

        var argumentAttr = symbol.GetAttribute(AttributeNames.ArgumentAttribute);
        if (argumentAttr != null)
        {
            var nameArg = argumentAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
            if (!nameArg.IsNull)
            {
                var name = nameArg.Value?.ToString();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    CheckName(context, "Argument", name!, location);
                }
            }
        }
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

    private void CheckName(SyntaxNodeAnalysisContext context, string type, string name, Location location)
    {
        var convention = DetectNamingConvention(name);
        if (convention != NamingConvention.KebabCase && convention != NamingConvention.LowerCase)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                type,
                name,
                GetConventionName(convention));
            context.ReportDiagnostic(diagnostic);
        }
    }

    private enum NamingConvention
    {
        KebabCase,      // my-option
        CamelCase,      // myOption
        PascalCase,     // MyOption
        SnakeCase,      // my_option
        LowerCase,      // myoption
        Mixed           // Unknown/mixed
    }

    private static NamingConvention DetectNamingConvention(string name)
    {
        if (string.IsNullOrEmpty(name))
            return NamingConvention.Mixed;

        bool hasHyphen = name.Contains('-');
        bool hasUnderscore = name.Contains('_');
        bool hasUpperCase = name.Any(char.IsUpper);
        bool startsWithUpper = char.IsUpper(name[0]);

        if (hasHyphen && !hasUnderscore && !hasUpperCase)
            return NamingConvention.KebabCase;

        if (hasUnderscore && !hasHyphen && !hasUpperCase)
            return NamingConvention.SnakeCase;

        if (!hasHyphen && !hasUnderscore && !hasUpperCase)
            return NamingConvention.LowerCase;

        if (hasUpperCase && !hasHyphen && !hasUnderscore)
        {
            return startsWithUpper ? NamingConvention.PascalCase : NamingConvention.CamelCase;
        }

        return NamingConvention.Mixed;
    }

    private static string GetConventionName(NamingConvention convention)
    {
        return convention switch
        {
            NamingConvention.CamelCase => "camelCase",
            NamingConvention.PascalCase => "PascalCase",
            NamingConvention.SnakeCase => "snake_case",
            NamingConvention.Mixed => "mixed naming",
            _ => "unknown convention"
        };
    }
}
