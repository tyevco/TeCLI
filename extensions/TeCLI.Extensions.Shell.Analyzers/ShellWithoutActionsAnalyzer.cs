using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace TeCLI.Shell.Analyzers;

/// <summary>
/// Detects when [Shell] attribute is applied to a command that has no actions.
/// A shell without actions would have nothing for users to do.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ShellWithoutActionsAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "TCLSHELL001",
        "Shell attribute on command without actions",
        "Command '{0}' has [Shell] attribute but no action methods. The shell would have no commands to execute.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A command marked with [Shell] should have action methods that can be executed in the interactive shell mode.");

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

        // Check if it has [Shell] attribute
        var shellAttr = classSymbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Name == "ShellAttribute" || a.AttributeClass?.Name == "Shell");

        if (shellAttr == null)
            return;

        // Check if it has [Command] attribute
        var commandAttr = classSymbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Name == "CommandAttribute" || a.AttributeClass?.Name == "Command");

        if (commandAttr == null)
            return;

        // Check if there are any action methods
        var hasActions = classSymbol.GetMembers().OfType<IMethodSymbol>().Any(method =>
            method.GetAttributes().Any(a =>
                a.AttributeClass?.Name == "ActionAttribute" || a.AttributeClass?.Name == "Action"));

        if (!hasActions)
        {
            var commandName = GetCommandName(commandAttr, classSymbol.Name);
            var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), commandName);
            context.ReportDiagnostic(diagnostic);
        }
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
