using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CommandOptionNameValidationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor InvalidCommandNameRule = new(
        "CLI004",
        "Invalid command name",
        "Command name '{0}' contains invalid characters. Command names should only contain letters, numbers, and hyphens.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Command names should follow CLI naming conventions.");

    private static readonly DiagnosticDescriptor InvalidOptionNameRule = new(
        "CLI005",
        "Invalid option name",
        "Option name '{0}' contains invalid characters. Option names should only contain letters, numbers, and hyphens.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Option names should follow CLI naming conventions.");

    private static readonly DiagnosticDescriptor EmptyNameRule = new(
        "CLI006",
        "Empty name not allowed",
        "{0} name cannot be empty or whitespace",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Command, action, and option names must not be empty.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(InvalidCommandNameRule, InvalidOptionNameRule, EmptyNameRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is INamedTypeSymbol classSymbol)
        {
            var commandAttr = classSymbol.GetAttribute(AttributeNames.CommandAttribute);
            if (commandAttr != null && commandAttr.ConstructorArguments.Length > 0)
            {
                var commandName = commandAttr.ConstructorArguments[0].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(commandName))
                {
                    ValidateName(context, classDeclaration.GetLocation(), commandName!, "Command", InvalidCommandNameRule);
                }
                else
                {
                    var diagnostic = Diagnostic.Create(EmptyNameRule, classDeclaration.GetLocation(), "Command");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is IMethodSymbol methodSymbol)
        {
            var actionAttr = methodSymbol.GetAttribute(AttributeNames.ActionAttribute);
            if (actionAttr != null && actionAttr.ConstructorArguments.Length > 0)
            {
                var actionName = actionAttr.ConstructorArguments[0].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(actionName))
                {
                    ValidateName(context, methodDeclaration.GetLocation(), actionName!, "Action", InvalidCommandNameRule);
                }
                else
                {
                    var diagnostic = Diagnostic.Create(EmptyNameRule, methodDeclaration.GetLocation(), "Action");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(propertyDeclaration) is IPropertySymbol propertySymbol)
        {
            var optionAttr = propertySymbol.GetAttribute(AttributeNames.OptionAttribute);
            if (optionAttr != null)
            {
                string? optionName = null;

                // Try to get the name from named arguments first
                var nameArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
                if (!nameArg.IsNull)
                {
                    optionName = nameArg.Value?.ToString();
                }
                // Fall back to constructor argument
                else if (optionAttr.ConstructorArguments.Length > 0)
                {
                    optionName = optionAttr.ConstructorArguments[0].Value?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(optionName))
                {
                    ValidateName(context, propertyDeclaration.GetLocation(), optionName!, "Option", InvalidOptionNameRule);
                }
                else if (optionName != null) // Name was explicitly set to empty
                {
                    var diagnostic = Diagnostic.Create(EmptyNameRule, propertyDeclaration.GetLocation(), "Option");
                    context.ReportDiagnostic(diagnostic);
                }
            }

            var argumentAttr = propertySymbol.GetAttribute(AttributeNames.ArgumentAttribute);
            if (argumentAttr != null)
            {
                string? argumentName = null;

                var nameArg = argumentAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
                if (!nameArg.IsNull)
                {
                    argumentName = nameArg.Value?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(argumentName))
                {
                    ValidateName(context, propertyDeclaration.GetLocation(), argumentName!, "Argument", InvalidCommandNameRule);
                }
                else if (argumentName != null) // Name was explicitly set to empty
                {
                    var diagnostic = Diagnostic.Create(EmptyNameRule, propertyDeclaration.GetLocation(), "Argument");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static void ValidateName(SyntaxNodeAnalysisContext context, Location location, string name, string type, DiagnosticDescriptor rule)
    {
        // Check if name contains only valid characters (alphanumeric, hyphen, underscore)
        // First character should be alphanumeric
        if (string.IsNullOrWhiteSpace(name))
            return;

        bool hasInvalidChars = false;
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            bool isValid = char.IsLetterOrDigit(c) || c == '-' || c == '_';

            if (!isValid)
            {
                hasInvalidChars = true;
                break;
            }
        }

        if (hasInvalidChars)
        {
            var diagnostic = Diagnostic.Create(rule, location, name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
