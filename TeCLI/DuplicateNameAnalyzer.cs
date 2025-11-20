using TeCLI.Attributes;
using TeCLI.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace TeCLI.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DuplicateNameAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor DuplicateActionNameRule = new(
        "CLI007",
        "Duplicate action name",
        "Action name '{0}' is used multiple times in command class '{1}'. Each action must have a unique name.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Action names must be unique within a command class.");

    private static readonly DiagnosticDescriptor DuplicateOptionNameRule = new(
        "CLI008",
        "Duplicate option name",
        "Option name '{0}' is used multiple times. Each option must have a unique name.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Option names must be unique within a command.");

    private static readonly DiagnosticDescriptor DuplicateArgumentIndexRule = new(
        "CLI009",
        "Conflicting argument positions",
        "Multiple arguments defined at the same position. Arguments must have distinct positions.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Argument positions must be unique.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DuplicateActionNameRule, DuplicateOptionNameRule, DuplicateArgumentIndexRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeCommandClass, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeCommandClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return;

        // Only analyze command classes
        if (!classSymbol.HasAttribute<CommandAttribute>())
            return;

        // Check for duplicate action names
        CheckDuplicateActionNames(context, classSymbol);

        // Check for duplicate option names and argument conflicts
        CheckDuplicateOptionsAndArguments(context, classSymbol);
    }

    private void CheckDuplicateActionNames(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol)
    {
        var actionMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol, ActionAttribute>()?.ToList();
        if (actionMethods == null || actionMethods.Count == 0)
            return;

        var actionNames = new Dictionary<string, List<IMethodSymbol>>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var method in actionMethods)
        {
            var actionAttr = method.GetAttribute<ActionAttribute>();
            if (actionAttr == null || actionAttr.ConstructorArguments.Length == 0)
                continue;

            var actionName = actionAttr.ConstructorArguments[0].Value?.ToString();
            if (string.IsNullOrWhiteSpace(actionName))
                continue;

            if (!actionNames.ContainsKey(actionName))
            {
                actionNames[actionName] = new List<IMethodSymbol>();
            }

            actionNames[actionName].Add(method);
        }

        // Report duplicates
        foreach (var (name, methods) in actionNames)
        {
            if (methods.Count > 1)
            {
                // Report diagnostic for all methods after the first
                foreach (var method in methods.Skip(1))
                {
                    var methodSyntax = method.GetSyntax<MethodDeclarationSyntax>();
                    if (methodSyntax != null)
                    {
                        var diagnostic = Diagnostic.Create(
                            DuplicateActionNameRule,
                            methodSyntax.GetLocation(),
                            name,
                            classSymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }

    private void CheckDuplicateOptionsAndArguments(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol)
    {
        // Get all methods in the command class
        var methods = classSymbol.GetMembers().OfType<IMethodSymbol>();

        foreach (var method in methods)
        {
            // Check parameters of each method
            var optionNames = new Dictionary<string, List<IParameterSymbol>>(System.StringComparer.OrdinalIgnoreCase);
            var argumentIndices = new Dictionary<int, List<IParameterSymbol>>();

            int argumentIndex = 0;

            foreach (var parameter in method.Parameters)
            {
                // Check for option duplicates
                if (parameter.TryGetAttribute<OptionAttribute>(out var optionAttr))
                {
                    string? optionName = null;

                    var nameArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
                    if (!nameArg.IsNull)
                    {
                        optionName = nameArg.Value?.ToString();
                    }
                    else if (optionAttr.ConstructorArguments.Length > 0)
                    {
                        optionName = optionAttr.ConstructorArguments[0].Value?.ToString();
                    }

                    optionName ??= parameter.Name;

                    if (!string.IsNullOrWhiteSpace(optionName))
                    {
                        if (!optionNames.ContainsKey(optionName))
                        {
                            optionNames[optionName] = new List<IParameterSymbol>();
                        }
                        optionNames[optionName].Add(parameter);
                    }
                }
                // Check for argument position conflicts
                else if (parameter.HasAttribute<ArgumentAttribute>() ||
                         (parameter.Type.IsValidOptionType() && !parameter.HasAttribute<OptionAttribute>()))
                {
                    if (!argumentIndices.ContainsKey(argumentIndex))
                    {
                        argumentIndices[argumentIndex] = new List<IParameterSymbol>();
                    }
                    argumentIndices[argumentIndex].Add(parameter);
                    argumentIndex++;
                }
            }

            // Report duplicate option names
            foreach (var (name, parameters) in optionNames)
            {
                if (parameters.Count > 1)
                {
                    foreach (var param in parameters.Skip(1))
                    {
                        var paramSyntax = param.GetSyntax<ParameterSyntax>();
                        if (paramSyntax != null)
                        {
                            var diagnostic = Diagnostic.Create(
                                DuplicateOptionNameRule,
                                paramSyntax.GetLocation(),
                                name);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }

            // Report argument position conflicts
            foreach (var (index, parameters) in argumentIndices)
            {
                if (parameters.Count > 1)
                {
                    foreach (var param in parameters.Skip(1))
                    {
                        var paramSyntax = param.GetSyntax<ParameterSyntax>();
                        if (paramSyntax != null)
                        {
                            var diagnostic = Diagnostic.Create(
                                DuplicateArgumentIndexRule,
                                paramSyntax.GetLocation());
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
    }
}
