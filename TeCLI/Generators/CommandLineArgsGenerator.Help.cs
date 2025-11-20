using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using TeCLI.Attributes;
using TeCLI.Extensions;

namespace TeCLI.Generators;

public partial class CommandLineArgsGenerator
{
    private void GenerateApplicationDocumentation(SourceProductionContext context, Compilation compilation)
    {
        // This is called with the full list of command classes, but we need to collect them
        // Since this is called per-command, we'll generate application-level help differently
        CodeBuilder cb = new CodeBuilder("System", "System.Reflection");

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                using (cb.AddBlock("public static void DisplayApplicationHelp()"))
                {
                    cb.AppendLine("Console.WriteLine(\"Usage: <command> [action] [options] [arguments]\");");
                    cb.AppendLine("Console.WriteLine();");
                    cb.AppendLine("Console.WriteLine(\"Global Options:\");");
                    cb.AppendLine("Console.WriteLine(\"  --help, -h                     Display help information\");");
                    cb.AppendLine("Console.WriteLine(\"  --version                      Display version information\");");
                    cb.AppendLine("Console.WriteLine(\"  --generate-completion <shell>  Generate shell completion script\");");
                    cb.AppendLine("Console.WriteLine(\"                                 Supported shells: bash, zsh, powershell, fish\");");
                    cb.AppendLine("Console.WriteLine();");
                    cb.AppendLine("Console.WriteLine(\"Available commands:\");");
                    cb.AppendLine("Console.WriteLine();");
                    cb.AppendLine("// Commands will be listed by individual command help generators");
                }

                cb.AddBlankLine();

                using (cb.AddBlock("public static void DisplayVersion()"))
                {
                    cb.AppendLine("var assembly = System.Reflection.Assembly.GetEntryAssembly();");
                    cb.AppendLine("if (assembly != null)");
                    cb.AppendLine("{");
                    cb.AppendLine("    var assemblyName = assembly.GetName();");
                    cb.AppendLine("    var appName = assemblyName.Name ?? \"app\";");
                    cb.AppendLine("    ");
                    cb.AppendLine("    // Try to get AssemblyInformationalVersion first (e.g., \"1.2.3-beta\")");
                    cb.AppendLine("    var infoVersionAttr = assembly.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>();");
                    cb.AppendLine("    if (infoVersionAttr != null && !string.IsNullOrEmpty(infoVersionAttr.InformationalVersion))");
                    cb.AppendLine("    {");
                    cb.AppendLine("        Console.WriteLine($\"{appName} {infoVersionAttr.InformationalVersion}\");");
                    cb.AppendLine("    }");
                    cb.AppendLine("    else if (assemblyName.Version != null)");
                    cb.AppendLine("    {");
                    cb.AppendLine("        // Fallback to AssemblyVersion");
                    cb.AppendLine("        Console.WriteLine($\"{appName} {assemblyName.Version}\");");
                    cb.AppendLine("    }");
                    cb.AppendLine("    else");
                    cb.AppendLine("    {");
                    cb.AppendLine("        Console.WriteLine($\"{appName} (version unknown)\");");
                    cb.AppendLine("    }");
                    cb.AppendLine("}");
                    cb.AppendLine("else");
                    cb.AppendLine("{");
                    cb.AppendLine("    Console.WriteLine(\"Version information not available\");");
                    cb.AppendLine("}");
                }
            }
        }

        context.AddSource("CommandDispatcher.Documentation.cs", SourceText.From(cb, Encoding.UTF8));
    }

    /// <summary>
    /// Generates command documentation from CommandSourceInfo (supports nested subcommands)
    /// </summary>
    private void GenerateCommandDocumentation(SourceProductionContext context, Compilation compilation, CommandSourceInfo commandInfo)
    {
        var classSymbol = commandInfo.TypeSymbol!;
        string commandName = commandInfo.CommandName!;
        string? commandDesc = commandInfo.Description;

        CodeBuilder cb = new CodeBuilder("System", "System.Linq");

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                using (cb.AddBlock($"public static void DisplayCommand{classSymbol.Name}Help(string actionName = null)"))
                {
                    // Display command header with full path and aliases
                    string fullPath = commandInfo.GetFullCommandPath();
                    if (commandInfo.Aliases.Count > 0)
                    {
                        var aliasesStr = string.Join(", ", commandInfo.Aliases.Select(a => $"\"{a}\""));
                        cb.AppendLine($"Console.WriteLine(\"Command: {fullPath} (aliases: {aliasesStr})\");");
                    }
                    else
                    {
                        cb.AppendLine($"Console.WriteLine(\"Command: {fullPath}\");");
                    }
                    if (!string.IsNullOrEmpty(commandDesc))
                    {
                        cb.AppendLine($"Console.WriteLine(\"Description: {commandDesc}\");");
                    }
                    cb.AppendLine("Console.WriteLine();");

                    // Get all actions
                    var actionMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol, ActionAttribute>().ToList();
                    var primaryMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol, PrimaryAttribute>().ToList();

                    // Build usage patterns
                    if (primaryMethods.Count > 0 || actionMethods.Count > 0 || commandInfo.Subcommands.Count > 0)
                    {
                        cb.AppendLine("Console.WriteLine(\"Usage:\");");

                        // Primary action usage
                        if (primaryMethods.Count > 0)
                        {
                            var primaryMethod = primaryMethods.First();
                            string usagePattern = BuildUsagePatternFromInfo(commandInfo, null, primaryMethod);
                            cb.AppendLine($"Console.WriteLine(\"  {usagePattern}\");");
                        }

                        // Subcommand usage
                        if (commandInfo.Subcommands.Count > 0)
                        {
                            cb.AppendLine($"Console.WriteLine(\"  {fullPath} <subcommand> [options]\");");
                        }

                        // Named actions usage
                        foreach (var action in actionMethods)
                        {
                            var actionAttr = action.GetAttribute<ActionAttribute>();
                            if (actionAttr != null && actionAttr.ConstructorArguments.Length > 0)
                            {
                                string actionName = actionAttr.ConstructorArguments[0].Value?.ToString() ?? action.Name;
                                string usagePattern = BuildUsagePatternFromInfo(commandInfo, actionName, action);
                                cb.AppendLine($"Console.WriteLine(\"  {usagePattern}\");");
                            }
                        }

                        cb.AppendLine("Console.WriteLine();");
                    }

                    // Display subcommands if any
                    if (commandInfo.Subcommands.Count > 0)
                    {
                        cb.AppendLine("Console.WriteLine(\"Subcommands:\");");

                        foreach (var subcommand in commandInfo.Subcommands)
                        {
                            string subDisplay = subcommand.CommandName!;
                            if (subcommand.Aliases.Count > 0)
                            {
                                var aliasesStr = string.Join(", ", subcommand.Aliases);
                                subDisplay = $"{subcommand.CommandName} ({aliasesStr})";
                            }

                            if (!string.IsNullOrEmpty(subcommand.Description))
                            {
                                cb.AppendLine($"Console.WriteLine(\"  {subDisplay.PadRight(30)} {subcommand.Description}\");");
                            }
                            else
                            {
                                cb.AppendLine($"Console.WriteLine(\"  {subDisplay}\");");
                            }
                        }

                        cb.AppendLine("Console.WriteLine();");
                    }

                    // Display actions
                    if (actionMethods.Count > 0)
                    {
                        cb.AppendLine("Console.WriteLine(\"Actions:\");");

                        foreach (var action in actionMethods)
                        {
                            var actionAttr = action.GetAttribute<ActionAttribute>();
                            if (actionAttr != null && actionAttr.ConstructorArguments.Length > 0)
                            {
                                string actionName = actionAttr.ConstructorArguments[0].Value?.ToString() ?? action.Name;
                                string? actionDesc = actionAttr.NamedArguments.FirstOrDefault(na => na.Key == "Description").Value.Value?.ToString();

                                // Extract action aliases
                                var actionAliases = new List<string>();
                                var actionAliasesArg = actionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Aliases");
                                if (!actionAliasesArg.Value.IsNull && actionAliasesArg.Value.Kind == TypedConstantKind.Array)
                                {
                                    foreach (var value in actionAliasesArg.Value.Values)
                                    {
                                        if (value.Value is string alias)
                                        {
                                            actionAliases.Add(alias);
                                        }
                                    }
                                }

                                // Build action display name with aliases
                                string actionDisplay = actionName;
                                if (actionAliases.Count > 0)
                                {
                                    var aliasesStr = string.Join(", ", actionAliases);
                                    actionDisplay = $"{actionName} ({aliasesStr})";
                                }

                                if (!string.IsNullOrEmpty(actionDesc))
                                {
                                    cb.AppendLine($"Console.WriteLine(\"  {actionDisplay.PadRight(30)} {actionDesc}\");");
                                }
                                else
                                {
                                    cb.AppendLine($"Console.WriteLine(\"  {actionDisplay}\");");
                                }
                            }
                        }

                        cb.AppendLine("Console.WriteLine();");
                    }

                    // Display options if there are any (check primary + all actions)
                    bool hasOptions = false;
                    foreach (var method in primaryMethods.Concat(actionMethods))
                    {
                        if (method.Parameters.Any(p => p.HasAttribute<OptionAttribute>()))
                        {
                            hasOptions = true;
                            break;
                        }
                    }

                    if (hasOptions)
                    {
                        cb.AppendLine("Console.WriteLine(\"Options:\");");
                        cb.AppendLine("Console.WriteLine(\"  --help, -h           Display this help message\");");
                        cb.AppendLine("Console.WriteLine();");
                    }

                    // Always show global options
                    cb.AppendLine("Console.WriteLine(\"Global Options:\");");
                    cb.AppendLine("Console.WriteLine(\"  --version            Display version information\");");
                    cb.AppendLine("Console.WriteLine();");
                }
            }
        }

        context.AddSource($"CommandDispatcher.Command.{classSymbol.Name}.Documentation.cs", SourceText.From(cb, Encoding.UTF8));

        // Recursively generate documentation for subcommands
        foreach (var subcommand in commandInfo.Subcommands)
        {
            GenerateCommandDocumentation(context, compilation, subcommand);
        }
    }

    private void GenerateCommandDocumentation(SourceProductionContext context, Compilation compilation, ClassDeclarationSyntax classDecl)
    {
        var model = compilation.GetSemanticModel(classDecl.SyntaxTree);
        if (model.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol)
        {
            return;
        }

        // Get command info
        var commandAttr = classSymbol.GetAttribute<CommandAttribute>();
        if (commandAttr == null || commandAttr.ConstructorArguments.Length == 0)
        {
            return;
        }

        string commandName = commandAttr.ConstructorArguments[0].Value?.ToString() ?? classDecl.Identifier.Text;
        string? commandDesc = commandAttr.NamedArguments.FirstOrDefault(na => na.Key == "Description").Value.Value?.ToString();

        // Extract command aliases
        var commandAliases = new List<string>();
        var aliasesArg = commandAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Aliases");
        if (!aliasesArg.Value.IsNull && aliasesArg.Value.Kind == TypedConstantKind.Array)
        {
            foreach (var value in aliasesArg.Value.Values)
            {
                if (value.Value is string alias)
                {
                    commandAliases.Add(alias);
                }
            }
        }

        CodeBuilder cb = new CodeBuilder("System", "System.Linq");

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                using (cb.AddBlock($"public static void DisplayCommand{classDecl.Identifier.Text}Help(string actionName = null)"))
                {
                    // Display command header with aliases
                    if (commandAliases.Count > 0)
                    {
                        var aliasesStr = string.Join(", ", commandAliases.Select(a => $"\"{a}\""));
                        cb.AppendLine($"Console.WriteLine(\"Command: {commandName} (aliases: {aliasesStr})\");");
                    }
                    else
                    {
                        cb.AppendLine($"Console.WriteLine(\"Command: {commandName}\");");
                    }
                    if (!string.IsNullOrEmpty(commandDesc))
                    {
                        cb.AppendLine($"Console.WriteLine(\"Description: {commandDesc}\");");
                    }
                    cb.AppendLine("Console.WriteLine();");

                    // Get all actions
                    var actionMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol, ActionAttribute>().ToList();
                    var primaryMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol, PrimaryAttribute>().ToList();

                    if (primaryMethods.Count > 0 || actionMethods.Count > 0)
                    {
                        cb.AppendLine("Console.WriteLine(\"Usage:\");");

                        // Primary action usage
                        if (primaryMethods.Count > 0)
                        {
                            var primaryMethod = primaryMethods.First();
                            string usagePattern = BuildUsagePattern(commandName, null, primaryMethod);
                            cb.AppendLine($"Console.WriteLine(\"  {usagePattern}\");");
                        }

                        // Named actions usage
                        foreach (var action in actionMethods)
                        {
                            var actionAttr = action.GetAttribute<ActionAttribute>();
                            if (actionAttr != null && actionAttr.ConstructorArguments.Length > 0)
                            {
                                string actionName = actionAttr.ConstructorArguments[0].Value?.ToString() ?? action.Name;
                                string usagePattern = BuildUsagePattern(commandName, actionName, action);
                                cb.AppendLine($"Console.WriteLine(\"  {usagePattern}\");");
                            }
                        }

                        cb.AppendLine("Console.WriteLine();");
                    }

                    // Display actions
                    if (actionMethods.Count > 0)
                    {
                        cb.AppendLine("Console.WriteLine(\"Actions:\");");

                        foreach (var action in actionMethods)
                        {
                            var actionAttr = action.GetAttribute<ActionAttribute>();
                            if (actionAttr != null && actionAttr.ConstructorArguments.Length > 0)
                            {
                                string actionName = actionAttr.ConstructorArguments[0].Value?.ToString() ?? action.Name;
                                string? actionDesc = actionAttr.NamedArguments.FirstOrDefault(na => na.Key == "Description").Value.Value?.ToString();

                                // Extract action aliases
                                var actionAliases = new List<string>();
                                var actionAliasesArg = actionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Aliases");
                                if (!actionAliasesArg.Value.IsNull && actionAliasesArg.Value.Kind == TypedConstantKind.Array)
                                {
                                    foreach (var value in actionAliasesArg.Value.Values)
                                    {
                                        if (value.Value is string alias)
                                        {
                                            actionAliases.Add(alias);
                                        }
                                    }
                                }

                                // Build action display name with aliases
                                string actionDisplay = actionName;
                                if (actionAliases.Count > 0)
                                {
                                    var aliasesStr = string.Join(", ", actionAliases);
                                    actionDisplay = $"{actionName} ({aliasesStr})";
                                }

                                if (!string.IsNullOrEmpty(actionDesc))
                                {
                                    cb.AppendLine($"Console.WriteLine(\"  {actionDisplay.PadRight(30)} {actionDesc}\");");
                                }
                                else
                                {
                                    cb.AppendLine($"Console.WriteLine(\"  {actionDisplay}\");");
                                }
                            }
                        }

                        cb.AppendLine("Console.WriteLine();");
                    }

                    // Display options if there are any (check primary + all actions)
                    bool hasOptions = false;
                    foreach (var method in primaryMethods.Concat(actionMethods))
                    {
                        if (method.Parameters.Any(p => p.HasAttribute<OptionAttribute>()))
                        {
                            hasOptions = true;
                            break;
                        }
                    }

                    if (hasOptions)
                    {
                        cb.AppendLine("Console.WriteLine(\"Options:\");");
                        cb.AppendLine("Console.WriteLine(\"  --help, -h           Display this help message\");");
                        cb.AppendLine("Console.WriteLine();");
                    }

                    // Always show global options
                    cb.AppendLine("Console.WriteLine(\"Global Options:\");");
                    cb.AppendLine("Console.WriteLine(\"  --version            Display version information\");");
                    cb.AppendLine("Console.WriteLine();");
                }
            }
        }

        context.AddSource($"CommandDispatcher.Command.{classDecl.Identifier.Text}.Documentation.cs", SourceText.From(cb, Encoding.UTF8));
    }

    private string BuildUsagePattern(string commandName, string? actionName, IMethodSymbol method)
    {
        StringBuilder usage = new StringBuilder();
        usage.Append(commandName);

        if (!string.IsNullOrEmpty(actionName))
        {
            usage.Append($" {actionName}");
        }

        // Add options
        var optionParams = method.Parameters.Where(p => p.HasAttribute<OptionAttribute>()).ToList();
        if (optionParams.Count > 0)
        {
            usage.Append(" [options]");
        }

        // Add arguments
        var argParams = method.Parameters.Where(p => p.HasAttribute<ArgumentAttribute>()).ToList();
        foreach (var arg in argParams)
        {
            string argName = arg.Name.ToUpper();
            if (arg.HasExplicitDefaultValue)
            {
                usage.Append($" [{argName}]");
            }
            else
            {
                usage.Append($" <{argName}>");
            }
        }

        return usage.ToString();
    }

    private string BuildUsagePatternFromInfo(CommandSourceInfo commandInfo, string? actionName, IMethodSymbol method)
    {
        StringBuilder usage = new StringBuilder();
        usage.Append(commandInfo.GetFullCommandPath());

        if (!string.IsNullOrEmpty(actionName))
        {
            usage.Append($" {actionName}");
        }

        // Add options
        var optionParams = method.Parameters.Where(p => p.HasAttribute<OptionAttribute>()).ToList();
        if (optionParams.Count > 0)
        {
            usage.Append(" [options]");
        }

        // Add arguments
        var argParams = method.Parameters.Where(p => p.HasAttribute<ArgumentAttribute>()).ToList();
        foreach (var arg in argParams)
        {
            string argName = arg.Name.ToUpper();
            if (arg.HasExplicitDefaultValue)
            {
                usage.Append($" [{argName}]");
            }
            else
            {
                usage.Append($" <{argName}>");
            }
        }

        return usage.ToString();
    }
}