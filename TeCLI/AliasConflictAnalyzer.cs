using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Detects conflicting alias values across commands and actions within the same scope.
/// Aliases must be unique within their respective scopes to avoid ambiguity.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AliasConflictAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor CommandAliasConflictRule = new(
        "CLI033",
        "Conflicting command alias",
        "Command alias '{0}' conflicts with another command's name or alias",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Command aliases must be unique across all commands. Each alias can only refer to a single command.");

    private static readonly DiagnosticDescriptor ActionAliasConflictRule = new(
        "CLI033",
        "Conflicting action alias",
        "Action alias '{0}' in command '{1}' conflicts with another action's name or alias",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Action aliases must be unique within a command. Each alias can only refer to a single action.");

    private static readonly DiagnosticDescriptor CommandAliasDuplicateRule = new(
        "CLI033",
        "Duplicate alias in command",
        "Command '{0}' has duplicate alias '{1}'",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The same alias appears multiple times in the command's Aliases array.");

    private static readonly DiagnosticDescriptor ActionAliasDuplicateRule = new(
        "CLI033",
        "Duplicate alias in action",
        "Action '{0}' has duplicate alias '{1}'",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The same alias appears multiple times in the action's Aliases array.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            CommandAliasConflictRule,
            ActionAliasConflictRule,
            CommandAliasDuplicateRule,
            ActionAliasDuplicateRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var commandInfos = new List<CommandInfo>();

        context.RegisterSyntaxNodeAction(ctx => CollectCommandInfo(ctx, commandInfos), SyntaxKind.ClassDeclaration);
        context.RegisterCompilationEndAction(ctx => AnalyzeCommandConflicts(ctx, commandInfos));
    }

    private void CollectCommandInfo(SyntaxNodeAnalysisContext context, List<CommandInfo> commandInfos)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return;

        var commandAttr = classSymbol.GetAttribute(AttributeNames.CommandAttribute);
        if (commandAttr == null)
            return;

        var commandName = GetCommandName(commandAttr, classSymbol.Name);
        var commandAliases = GetAliases(commandAttr);
        var location = classDeclaration.Identifier.GetLocation();

        // Check for duplicate aliases within the same command
        CheckDuplicateAliases(context, commandAliases, commandName, location, isAction: false);

        var commandInfo = new CommandInfo
        {
            Name = commandName,
            Aliases = commandAliases,
            Location = location,
            ClassSymbol = classSymbol
        };

        // Collect action info for this command
        foreach (var member in classSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            var actionAttr = member.GetAttribute(AttributeNames.ActionAttribute);
            if (actionAttr == null)
                continue;

            var actionName = GetActionName(actionAttr, member.Name);
            var actionAliases = GetAliases(actionAttr);
            var actionLocation = member.GetSyntax<MethodDeclarationSyntax>()?.Identifier.GetLocation() ?? location;

            // Check for duplicate aliases within the same action
            CheckDuplicateAliases(context, actionAliases, actionName, actionLocation, isAction: true);

            commandInfo.Actions.Add(new ActionInfo
            {
                Name = actionName,
                Aliases = actionAliases,
                Location = actionLocation
            });
        }

        lock (commandInfos)
        {
            commandInfos.Add(commandInfo);
        }

        // Analyze action conflicts within this command immediately
        AnalyzeActionConflicts(context, commandInfo);
    }

    private void CheckDuplicateAliases(SyntaxNodeAnalysisContext context, List<string> aliases, string name, Location location, bool isAction)
    {
        var seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var alias in aliases)
        {
            if (!seen.Add(alias))
            {
                var rule = isAction ? ActionAliasDuplicateRule : CommandAliasDuplicateRule;
                var diagnostic = Diagnostic.Create(rule, location, name, alias);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private void AnalyzeActionConflicts(SyntaxNodeAnalysisContext context, CommandInfo commandInfo)
    {
        var nameToAction = new Dictionary<string, ActionInfo>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var action in commandInfo.Actions)
        {
            // Check the primary action name
            if (nameToAction.TryGetValue(action.Name, out var existing))
            {
                var diagnostic = Diagnostic.Create(
                    ActionAliasConflictRule,
                    action.Location,
                    action.Name,
                    commandInfo.Name);
                context.ReportDiagnostic(diagnostic);
            }
            else
            {
                nameToAction[action.Name] = action;
            }

            // Check each alias
            foreach (var alias in action.Aliases)
            {
                if (nameToAction.TryGetValue(alias, out existing))
                {
                    var diagnostic = Diagnostic.Create(
                        ActionAliasConflictRule,
                        action.Location,
                        alias,
                        commandInfo.Name);
                    context.ReportDiagnostic(diagnostic);
                }
                else
                {
                    nameToAction[alias] = action;
                }
            }
        }
    }

    private void AnalyzeCommandConflicts(CompilationAnalysisContext context, List<CommandInfo> commandInfos)
    {
        var nameToCommand = new Dictionary<string, CommandInfo>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var command in commandInfos)
        {
            // Check the primary command name
            if (nameToCommand.TryGetValue(command.Name, out var existing))
            {
                var diagnostic = Diagnostic.Create(
                    CommandAliasConflictRule,
                    command.Location,
                    command.Name);
                context.ReportDiagnostic(diagnostic);
            }
            else
            {
                nameToCommand[command.Name] = command;
            }

            // Check each alias
            foreach (var alias in command.Aliases)
            {
                if (nameToCommand.TryGetValue(alias, out existing))
                {
                    var diagnostic = Diagnostic.Create(
                        CommandAliasConflictRule,
                        command.Location,
                        alias);
                    context.ReportDiagnostic(diagnostic);
                }
                else
                {
                    nameToCommand[alias] = command;
                }
            }
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

    private static string GetActionName(AttributeData attr, string fallbackName)
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

    private static List<string> GetAliases(AttributeData attr)
    {
        var aliases = new List<string>();
        var aliasesArg = attr.NamedArguments.FirstOrDefault(arg => arg.Key == "Aliases").Value;

        if (!aliasesArg.IsNull && aliasesArg.Values.Length > 0)
        {
            foreach (var value in aliasesArg.Values)
            {
                if (value.Value is string alias && !string.IsNullOrEmpty(alias))
                {
                    aliases.Add(alias);
                }
            }
        }

        return aliases;
    }

    private class CommandInfo
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Aliases { get; set; } = new();
        public Location Location { get; set; } = Location.None;
        public INamedTypeSymbol? ClassSymbol { get; set; }
        public List<ActionInfo> Actions { get; set; } = new();
    }

    private class ActionInfo
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Aliases { get; set; } = new();
        public Location Location { get; set; } = Location.None;
    }
}
