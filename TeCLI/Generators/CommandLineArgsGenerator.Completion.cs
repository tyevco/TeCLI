using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TeCLI.Generators;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TeCLI.Generators;

/// <summary>
/// Generates shell completion scripts for various shells (Bash, Zsh, PowerShell, Fish)
/// </summary>
public partial class CommandLineArgsGenerator
{
    /// <summary>
    /// Generates completion support method declarations using Roslyn syntax.
    /// </summary>
    private IEnumerable<MemberDeclarationSyntax> GenerateCompletionSupportMembers(List<CommandSourceInfo> commandHierarchies, GlobalOptionsSourceInfo? globalOptions)
    {
        var members = new List<MemberDeclarationSyntax>();

        // Generate GenerateCompletion method
        members.Add(GenerateCompletionMethod());

        // Generate shell-specific methods
        members.Add(GenerateBashCompletionMethodSyntax(commandHierarchies, globalOptions));
        members.Add(GenerateZshCompletionMethodSyntax(commandHierarchies, globalOptions));
        members.Add(GeneratePowerShellCompletionMethodSyntax(commandHierarchies, globalOptions));
        members.Add(GenerateFishCompletionMethodSyntax(commandHierarchies, globalOptions));

        return members;
    }

    private MethodDeclarationSyntax GenerateCompletionMethod()
    {
        var statements = new List<StatementSyntax>
        {
            // shell = shell.ToLower();
            ExpressionStatement(
                AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName("shell"),
                    InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("shell"),
                            IdentifierName("ToLower"))))),

            // switch (shell)
            SwitchStatement(IdentifierName("shell"))
                .WithSections(List(new[]
                {
                    SwitchSection()
                        .WithLabels(SingletonList<SwitchLabelSyntax>(CaseSwitchLabel(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("bash")))))
                        .WithStatements(List(new StatementSyntax[]
                        {
                            ExpressionStatement(InvocationExpression(IdentifierName("GenerateBashCompletion"))),
                            BreakStatement()
                        })),
                    SwitchSection()
                        .WithLabels(SingletonList<SwitchLabelSyntax>(CaseSwitchLabel(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("zsh")))))
                        .WithStatements(List(new StatementSyntax[]
                        {
                            ExpressionStatement(InvocationExpression(IdentifierName("GenerateZshCompletion"))),
                            BreakStatement()
                        })),
                    SwitchSection()
                        .WithLabels(SingletonList<SwitchLabelSyntax>(CaseSwitchLabel(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("powershell")))))
                        .WithStatements(List(new StatementSyntax[]
                        {
                            ExpressionStatement(InvocationExpression(IdentifierName("GeneratePowerShellCompletion"))),
                            BreakStatement()
                        })),
                    SwitchSection()
                        .WithLabels(SingletonList<SwitchLabelSyntax>(CaseSwitchLabel(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("fish")))))
                        .WithStatements(List(new StatementSyntax[]
                        {
                            ExpressionStatement(InvocationExpression(IdentifierName("GenerateFishCompletion"))),
                            BreakStatement()
                        })),
                    SwitchSection()
                        .WithLabels(SingletonList<SwitchLabelSyntax>(DefaultSwitchLabel()))
                        .WithStatements(List(new StatementSyntax[]
                        {
                            ExpressionStatement(
                                InvocationExpression(
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("Console"),
                                            IdentifierName("Error")),
                                        IdentifierName("WriteLine")))
                                .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                    Argument(InterpolatedStringExpression(Token(SyntaxKind.InterpolatedStringStartToken))
                                        .WithContents(List(new InterpolatedStringContentSyntax[]
                                        {
                                            InterpolatedStringText()
                                                .WithTextToken(Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, "Unknown shell: ", "Unknown shell: ", TriviaList())),
                                            Interpolation(IdentifierName("shell"))
                                        }))))))),
                            ExpressionStatement(
                                InvocationExpression(
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("Console"),
                                            IdentifierName("Error")),
                                        IdentifierName("WriteLine")))
                                .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("Supported shells: bash, zsh, powershell, fish"))))))),
                            BreakStatement()
                        }))
                }))
        };

        return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("GenerateCompletion"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(ParameterList(SingletonSeparatedList(
                Parameter(Identifier("shell")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword))))))
            .WithBody(Block(statements));
    }

    private MethodDeclarationSyntax GenerateBashCompletionMethodSyntax(List<CommandSourceInfo> commandHierarchies, GlobalOptionsSourceInfo? globalOptions)
    {
        // Build the bash script as a string with placeholder for appName
        var scriptParts = new List<string>
        {
            "# Bash completion script for {APP_NAME}",
            "# Generated by TeCLI",
            "",
            "_{APP_NAME}_completions()",
            "{",
            "    local cur prev opts",
            "    COMPREPLY=()",
            "    cur=\"\"${COMP_WORDS[COMP_CWORD]}\"\"",
            "    prev=\"\"${COMP_WORDS[COMP_CWORD-1]}\"\"",
            ""
        };

        // Global options
        var globalOpts = "--help -h --version";
        if (globalOptions != null && globalOptions.Options.Count > 0)
        {
            foreach (var option in globalOptions.Options)
            {
                globalOpts += $" --{option.Name}";
                if (option.ShortName != '\0')
                {
                    globalOpts += $" -{option.ShortName}";
                }
            }
        }
        scriptParts.Add($"    local global_opts=\"\"{globalOpts}\"\"");
        scriptParts.Add("");

        // Commands
        var commandNames = string.Join(" ", commandHierarchies.Select(c => c.CommandName));
        scriptParts.Add($"    local commands=\"\"{commandNames}\"\"");
        scriptParts.Add("");

        // Command-specific completions
        scriptParts.Add("    # Get the command (first non-option argument)");
        scriptParts.Add("    local command=\"\"\"\"");
        scriptParts.Add("    for ((i=1; i<COMP_CWORD; i++)); do");
        scriptParts.Add("        if [[ ${COMP_WORDS[i]} != -* ]]; then");
        scriptParts.Add("            command=${COMP_WORDS[i]}");
        scriptParts.Add("            break");
        scriptParts.Add("        fi");
        scriptParts.Add("    done");
        scriptParts.Add("");
        scriptParts.Add("    if [[ -z $command ]]; then");
        scriptParts.Add("        COMPREPLY=( $(compgen -W \"\"${commands} ${global_opts}\"\" -- ${cur}) )");
        scriptParts.Add("        return 0");
        scriptParts.Add("    fi");
        scriptParts.Add("");
        scriptParts.Add("    case ${command} in");

        foreach (var command in commandHierarchies)
        {
            scriptParts.AddRange(GenerateBashCommandCompletionLines(command, 2));
        }

        scriptParts.Add("    esac");
        scriptParts.Add("}");
        scriptParts.Add("");
        scriptParts.Add("complete -F _{APP_NAME}_completions {APP_NAME}");

        var scriptContent = string.Join("\n", scriptParts);

        var statements = new List<StatementSyntax>
        {
            ParseStatement("var appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? \"app\";"),
            ParseStatement($"var script = @\"\n{scriptContent}\n\".Replace(\"{{APP_NAME}}\", appName);"),
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Console"),
                        IdentifierName("WriteLine")))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("script"))))))
        };

        return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("GenerateBashCompletion"))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
            .WithBody(Block(statements));
    }

    private IEnumerable<string> GenerateBashCommandCompletionLines(CommandSourceInfo command, int indentLevel)
    {
        var lines = new List<string>();
        var indent = new string(' ', indentLevel * 4);

        lines.Add($"{indent}{command.CommandName})");

        // Collect all options from actions
        var allOptions = new HashSet<string>();
        foreach (var action in command.Actions)
        {
            if (action.Method != null)
            {
                foreach (var param in action.Method.Parameters)
                {
                    var optionAttr = param.GetAttributes().FirstOrDefault(a =>
                        a.AttributeClass?.Name == "OptionAttribute" ||
                        a.AttributeClass?.ToDisplayString() == "TeCLI.Attributes.OptionAttribute");

                    if (optionAttr != null)
                    {
                        var longName = optionAttr.ConstructorArguments.FirstOrDefault().Value?.ToString();
                        if (!string.IsNullOrEmpty(longName))
                        {
                            allOptions.Add($"--{longName}");
                        }

                        var shortNameArg = optionAttr.NamedArguments.FirstOrDefault(na => na.Key == "ShortName");
                        if (shortNameArg.Value.Value is char shortChar && shortChar != '\0')
                        {
                            allOptions.Add($"-{shortChar}");
                        }
                    }
                }
            }
        }

        // Subcommands and actions
        var subItems = command.Subcommands.Select(s => s.CommandName!).Concat(command.Actions.Select(a => a.ActionName!)).ToList();

        if (subItems.Count > 0 || allOptions.Count > 0)
        {
            var completionItems = new List<string>();
            if (subItems.Count > 0) completionItems.AddRange(subItems);
            if (allOptions.Count > 0) completionItems.AddRange(allOptions);
            // Use doubled quotes for verbatim string escaping
            lines.Add($"{indent}    COMPREPLY=( $(compgen -W \"\"{string.Join(" ", completionItems)}\"\" -- ${{cur}}) )");
        }

        lines.Add($"{indent}    ;;");

        return lines;
    }

    private MethodDeclarationSyntax GenerateZshCompletionMethodSyntax(List<CommandSourceInfo> commandHierarchies, GlobalOptionsSourceInfo? globalOptions)
    {
        var scriptParts = new List<string>
        {
            "#compdef {APP_NAME}",
            "# Zsh completion script for {APP_NAME}",
            "# Generated by TeCLI",
            "",
            "_{APP_NAME}() {",
            "    local line state",
            "",
            "    _arguments -C \\",
            "        '(-h --help)'{-h,--help}'[Show help information]' \\",
            "        '--version[Show version information]' \\"
        };

        if (globalOptions != null && globalOptions.Options.Count > 0)
        {
            foreach (var option in globalOptions.Options)
            {
                var shortPart = option.ShortName != '\0' ? $"-{option.ShortName}," : "";
                var desc = option.Description.IsNull ? (option.Name ?? "") : (option.Description.Value?.ToString() ?? "");
                scriptParts.Add($"        '({shortPart}--{option.Name})'{{{shortPart}--{option.Name}}}'[{desc}]' \\");
            }
        }

        scriptParts.Add("        '1: :->command' \\");
        scriptParts.Add("        '*::arg:->args'");
        scriptParts.Add("");
        scriptParts.Add("    case $state in");
        scriptParts.Add("        command)");
        scriptParts.Add("            local commands=(");

        foreach (var command in commandHierarchies)
        {
            var desc = command.Description ?? command.CommandName ?? "";
            scriptParts.Add($"                '{command.CommandName}:{desc}'");
        }

        scriptParts.Add("            )");
        scriptParts.Add("            _describe 'command' commands");
        scriptParts.Add("            ;;");
        scriptParts.Add("        args)");
        scriptParts.Add("            case $line[1] in");

        foreach (var command in commandHierarchies)
        {
            scriptParts.AddRange(GenerateZshCommandCompletionLines(command, 4));
        }

        scriptParts.Add("            esac");
        scriptParts.Add("            ;;");
        scriptParts.Add("    esac");
        scriptParts.Add("}");
        scriptParts.Add("");
        scriptParts.Add("_{APP_NAME}");

        var scriptContent = string.Join("\n", scriptParts);

        var statements = new List<StatementSyntax>
        {
            ParseStatement("var appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? \"app\";"),
            ParseStatement($"var script = @\"\n{scriptContent}\n\".Replace(\"{{APP_NAME}}\", appName);"),
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Console"),
                        IdentifierName("WriteLine")))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("script"))))))
        };

        return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("GenerateZshCompletion"))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
            .WithBody(Block(statements));
    }

    private IEnumerable<string> GenerateZshCommandCompletionLines(CommandSourceInfo command, int indentLevel)
    {
        var lines = new List<string>();
        var indent = new string(' ', indentLevel * 4);

        lines.Add($"{indent}{command.CommandName})");

        if (command.Subcommands.Count > 0 || command.Actions.Count > 0)
        {
            lines.Add($"{indent}    local subcommands=(");

            foreach (var sub in command.Subcommands)
            {
                var desc = sub.Description ?? sub.CommandName ?? "";
                lines.Add($"{indent}        '{sub.CommandName}:{desc}'");
            }

            foreach (var action in command.Actions)
            {
                var desc = action.DisplayName ?? action.ActionName ?? "";
                lines.Add($"{indent}        '{action.ActionName}:{desc}'");
            }

            lines.Add($"{indent}    )");
            lines.Add($"{indent}    _describe 'subcommand' subcommands");
        }

        lines.Add($"{indent}    ;;");

        return lines;
    }

    private MethodDeclarationSyntax GeneratePowerShellCompletionMethodSyntax(List<CommandSourceInfo> commandHierarchies, GlobalOptionsSourceInfo? globalOptions)
    {
        var scriptParts = new List<string>
        {
            "# PowerShell completion script for {APP_NAME}",
            "# Generated by TeCLI",
            "",
            "Register-ArgumentCompleter -Native -CommandName {APP_NAME} -ScriptBlock {",
            "    param($wordToComplete, $commandAst, $cursorPosition)",
            "",
            "    $commands = @("
        };

        foreach (var command in commandHierarchies)
        {
            var desc = command.Description ?? "";
            scriptParts.Add($"        @{{ Name = '{command.CommandName}'; Description = '{desc}' }}");
        }

        scriptParts.Add("    )");
        scriptParts.Add("");
        scriptParts.Add("    $globalOpts = @(");
        scriptParts.Add("        @{ Name = '--help'; Description = 'Show help information' }");
        scriptParts.Add("        @{ Name = '-h'; Description = 'Show help information' }");
        scriptParts.Add("        @{ Name = '--version'; Description = 'Show version information' }");

        if (globalOptions != null && globalOptions.Options.Count > 0)
        {
            foreach (var option in globalOptions.Options)
            {
                var desc = option.Description.IsNull ? "" : (option.Description.Value?.ToString() ?? "");
                scriptParts.Add($"        @{{ Name = '--{option.Name}'; Description = '{desc}' }}");
                if (option.ShortName != '\0')
                {
                    scriptParts.Add($"        @{{ Name = '-{option.ShortName}'; Description = '{desc}' }}");
                }
            }
        }

        scriptParts.Add("    )");
        scriptParts.Add("");
        scriptParts.Add("    # Parse the command line to determine context");
        scriptParts.Add("    $tokens = $commandAst.ToString().Split(' ', [StringSplitOptions]::RemoveEmptyEntries)");
        scriptParts.Add("");
        scriptParts.Add("    # If we're at the first argument, show commands and global options");
        scriptParts.Add("    if ($tokens.Count -eq 1 -or ($tokens.Count -eq 2 -and $wordToComplete)) {");
        // Use doubled quotes for verbatim string
        scriptParts.Add("        $commands + $globalOpts | Where-Object { $_.Name -like \"\"$wordToComplete*\"\" } | ForEach-Object {");
        scriptParts.Add("            [System.Management.Automation.CompletionResult]::new($_.Name, $_.Name, 'ParameterValue', $_.Description)");
        scriptParts.Add("        }");
        scriptParts.Add("    }");
        scriptParts.Add("}");

        var scriptContent = string.Join("\n", scriptParts);

        var statements = new List<StatementSyntax>
        {
            ParseStatement("var appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? \"app\";"),
            ParseStatement($"var script = @\"\n{scriptContent}\n\".Replace(\"{{APP_NAME}}\", appName);"),
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Console"),
                        IdentifierName("WriteLine")))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("script"))))))
        };

        return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("GeneratePowerShellCompletion"))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
            .WithBody(Block(statements));
    }

    private MethodDeclarationSyntax GenerateFishCompletionMethodSyntax(List<CommandSourceInfo> commandHierarchies, GlobalOptionsSourceInfo? globalOptions)
    {
        var scriptParts = new List<string>
        {
            "# Fish completion script for {APP_NAME}",
            "# Generated by TeCLI",
            "",
            "# Global options",
            "complete -c {APP_NAME} -s h -l help -d 'Show help information'",
            "complete -c {APP_NAME} -l version -d 'Show version information'"
        };

        if (globalOptions != null && globalOptions.Options.Count > 0)
        {
            scriptParts.Add("");
            foreach (var option in globalOptions.Options)
            {
                var desc = option.Description.IsNull ? "" : (option.Description.Value?.ToString() ?? "");
                var shortPart = option.ShortName != '\0' ? $" -s {option.ShortName}" : "";
                scriptParts.Add($"complete -c {{APP_NAME}}{shortPart} -l {option.Name} -d '{desc}'");
            }
        }

        scriptParts.Add("");
        scriptParts.Add("# Commands");

        foreach (var command in commandHierarchies)
        {
            scriptParts.AddRange(GenerateFishCommandCompletionLines(command));
        }

        var scriptContent = string.Join("\n", scriptParts);

        var statements = new List<StatementSyntax>
        {
            ParseStatement("var appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? \"app\";"),
            ParseStatement($"var script = @\"\n{scriptContent}\n\".Replace(\"{{APP_NAME}}\", appName);"),
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Console"),
                        IdentifierName("WriteLine")))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("script"))))))
        };

        return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("GenerateFishCompletion"))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
            .WithBody(Block(statements));
    }

    private IEnumerable<string> GenerateFishCommandCompletionLines(CommandSourceInfo command)
    {
        var lines = new List<string>();
        var desc = command.Description ?? "";
        lines.Add($"complete -c {{APP_NAME}} -n '__fish_use_subcommand' -a '{command.CommandName}' -d '{desc}'");

        // Subcommands
        foreach (var sub in command.Subcommands)
        {
            var subDesc = sub.Description ?? "";
            lines.Add($"complete -c {{APP_NAME}} -n '__fish_seen_subcommand_from {command.CommandName}' -a '{sub.CommandName}' -d '{subDesc}'");
        }

        // Actions
        foreach (var action in command.Actions)
        {
            var actionDesc = action.DisplayName ?? "";
            lines.Add($"complete -c {{APP_NAME}} -n '__fish_seen_subcommand_from {command.CommandName}' -a '{action.ActionName}' -d '{actionDesc}'");
        }

        // Options from actions
        foreach (var action in command.Actions)
        {
            if (action.Method != null)
            {
                foreach (var param in action.Method.Parameters)
                {
                    var optionAttr = param.GetAttributes().FirstOrDefault(a =>
                        a.AttributeClass?.Name == "OptionAttribute" ||
                        a.AttributeClass?.ToDisplayString() == "TeCLI.Attributes.OptionAttribute");

                    if (optionAttr != null)
                    {
                        var longName = optionAttr.ConstructorArguments.FirstOrDefault().Value?.ToString();
                        var shortNameArg = optionAttr.NamedArguments.FirstOrDefault(na => na.Key == "ShortName");
                        var shortChar = shortNameArg.Value.Value is char c && c != '\0' ? c.ToString() : "";

                        if (!string.IsNullOrEmpty(longName))
                        {
                            var shortPart = !string.IsNullOrEmpty(shortChar) ? $" -s {shortChar}" : "";
                            lines.Add($"complete -c {{APP_NAME}} -n '__fish_seen_subcommand_from {command.CommandName}'{shortPart} -l {longName}");
                        }
                    }
                }
            }
        }

        return lines;
    }

    /// <summary>
    /// Generates completion support methods in the CommandDispatcher (legacy CodeBuilder version)
    /// </summary>
    private void GenerateCompletionSupport(CodeBuilder cb, List<CommandSourceInfo> commandHierarchies, GlobalOptionsSourceInfo? globalOptions)
    {
        using (cb.AddBlock("public void GenerateCompletion(string shell)"))
        {
            cb.AppendLine("shell = shell.ToLower();");
            cb.AddBlankLine();

            using (cb.AddBlock("switch (shell)"))
            {
                using (cb.AddBlock("case \"bash\":"))
                {
                    cb.AppendLine("GenerateBashCompletion();");
                    cb.AppendLine("break;");
                }
                cb.AddBlankLine();

                using (cb.AddBlock("case \"zsh\":"))
                {
                    cb.AppendLine("GenerateZshCompletion();");
                    cb.AppendLine("break;");
                }
                cb.AddBlankLine();

                using (cb.AddBlock("case \"powershell\":"))
                {
                    cb.AppendLine("GeneratePowerShellCompletion();");
                    cb.AppendLine("break;");
                }
                cb.AddBlankLine();

                using (cb.AddBlock("case \"fish\":"))
                {
                    cb.AppendLine("GenerateFishCompletion();");
                    cb.AppendLine("break;");
                }
                cb.AddBlankLine();

                using (cb.AddBlock("default:"))
                {
                    cb.AppendLine("Console.Error.WriteLine($\"Unknown shell: {shell}\");");
                    cb.AppendLine("Console.Error.WriteLine(\"Supported shells: bash, zsh, powershell, fish\");");
                    cb.AppendLine("break;");
                }
            }
        }

        cb.AddBlankLine();

        // Generate Bash completion
        GenerateBashCompletionMethod(cb, commandHierarchies, globalOptions);
        cb.AddBlankLine();

        // Generate Zsh completion
        GenerateZshCompletionMethod(cb, commandHierarchies, globalOptions);
        cb.AddBlankLine();

        // Generate PowerShell completion
        GeneratePowerShellCompletionMethod(cb, commandHierarchies, globalOptions);
        cb.AddBlankLine();

        // Generate Fish completion
        GenerateFishCompletionMethod(cb, commandHierarchies, globalOptions);
    }

    private void GenerateBashCompletionMethod(CodeBuilder cb, List<CommandSourceInfo> commandHierarchies, GlobalOptionsSourceInfo? globalOptions)
    {
        using (cb.AddBlock("private void GenerateBashCompletion()"))
        {
            cb.AppendLine("var appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? \"app\";");
            cb.AppendLine("var script = @\"");

            // Bash completion script template
            cb.AppendLine("# Bash completion script for \" + appName + @\"");
            cb.AppendLine("# Generated by TeCLI");
            cb.AppendLine("");
            cb.AppendLine("_\" + appName + @\"_completions()");
            cb.AppendLine("{");
            cb.AppendLine("    local cur prev opts");
            cb.AppendLine("    COMPREPLY=()");
            cb.AppendLine("    cur=\\\"${COMP_WORDS[COMP_CWORD]}\\\"");
            cb.AppendLine("    prev=\\\"${COMP_WORDS[COMP_CWORD-1]}\\\"");
            cb.AppendLine("");

            // Global options
            cb.Append("    local global_opts=\\\"--help -h --version");
            if (globalOptions != null && globalOptions.Options.Count > 0)
            {
                foreach (var option in globalOptions.Options)
                {
                    cb.Append($" --{option.Name}");
                    if (!(option.ShortName == '\0'))
                    {
                        cb.Append($" -{option.ShortName}");
                    }
                }
            }
            cb.AppendLine("\\\"");
            cb.AppendLine("");

            // Commands
            var commandNames = commandHierarchies.Select(c => c.CommandName).ToList();
            cb.AppendLine($"    local commands=\\\"{string.Join(" ", commandNames)}\\\"");
            cb.AppendLine("");

            // Command-specific completions
            cb.AppendLine("    # Get the command (first non-option argument)");
            cb.AppendLine("    local command=\\\"\\\"");
            cb.AppendLine("    for ((i=1; i<COMP_CWORD; i++)); do");
            cb.AppendLine("        if [[ ${COMP_WORDS[i]} != -* ]]; then");
            cb.AppendLine("            command=${COMP_WORDS[i]}");
            cb.AppendLine("            break");
            cb.AppendLine("        fi");
            cb.AppendLine("    done");
            cb.AppendLine("");

            // If no command yet, complete with commands and global options
            cb.AppendLine("    if [[ -z $command ]]; then");
            cb.AppendLine("        COMPREPLY=( $(compgen -W \\\"${commands} ${global_opts}\\\" -- ${cur}) )");
            cb.AppendLine("        return 0");
            cb.AppendLine("    fi");
            cb.AppendLine("");

            // Command-specific completions
            cb.AppendLine("    case ${command} in");
            foreach (var command in commandHierarchies)
            {
                GenerateBashCommandCompletion(cb, command, 2);
            }
            cb.AppendLine("    esac");
            cb.AppendLine("}");
            cb.AppendLine("");
            cb.AppendLine("complete -F _\" + appName + @\"_completions \" + appName + @\"");
            cb.AppendLine("\";");
            cb.AddBlankLine();
            cb.AppendLine("Console.WriteLine(script);");
        }
    }

    private void GenerateBashCommandCompletion(CodeBuilder cb, CommandSourceInfo command, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);

        cb.AppendLine($"{indent}{command.CommandName})");

        // Collect all options from actions
        var allOptions = new HashSet<string>();
        foreach (var action in command.Actions)
        {
            if (action.Method != null)
            {
                foreach (var param in action.Method.Parameters)
                {
                    var optionAttr = param.GetAttributes().FirstOrDefault(a =>
                        a.AttributeClass?.Name == "OptionAttribute" ||
                        a.AttributeClass?.ToDisplayString() == "TeCLI.Attributes.OptionAttribute");

                    if (optionAttr != null)
                    {
                        var longName = optionAttr.ConstructorArguments.FirstOrDefault().Value?.ToString();
                        if (!string.IsNullOrEmpty(longName))
                        {
                            allOptions.Add($"--{longName}");
                        }

                        var shortNameArg = optionAttr.NamedArguments.FirstOrDefault(na => na.Key == "ShortName");
                        if (shortNameArg.Value.Value is char shortChar && shortChar != '\0')
                        {
                            allOptions.Add($"-{shortChar}");
                        }
                    }
                }
            }
        }

        // Subcommands and actions
        var subItems = command.Subcommands.Select(s => s.CommandName!).Concat(command.Actions.Select(a => a.ActionName!)).ToList();

        if (subItems.Count > 0 || allOptions.Count > 0)
        {
            cb.Append($"{indent}    COMPREPLY=( $(compgen -W \\\"");
            if (subItems.Count > 0)
            {
                cb.Append(string.Join(" ", subItems));
            }
            if (allOptions.Count > 0)
            {
                if (subItems.Count > 0) cb.Append(" ");
                cb.Append(string.Join(" ", allOptions));
            }
            cb.AppendLine("\\\" -- ${cur}) )");
        }

        cb.AppendLine($"{indent}    ;;");
    }

    private void GenerateZshCompletionMethod(CodeBuilder cb, List<CommandSourceInfo> commandHierarchies, GlobalOptionsSourceInfo? globalOptions)
    {
        using (cb.AddBlock("private void GenerateZshCompletion()"))
        {
            cb.AppendLine("var appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? \"app\";");
            cb.AppendLine("var script = @\"");

            cb.AppendLine("#compdef \" + appName + @\"");
            cb.AppendLine("# Zsh completion script for \" + appName + @\"");
            cb.AppendLine("# Generated by TeCLI");
            cb.AppendLine("");
            cb.AppendLine("_\" + appName + @\"() {");
            cb.AppendLine("    local line state");
            cb.AppendLine("");
            cb.AppendLine("    _arguments -C \\\\");

            // Global options
            cb.AppendLine("        '(-h --help)'{-h,--help}'[Show help information]' \\\\");
            cb.AppendLine("        '--version[Show version information]' \\\\");

            if (globalOptions != null && globalOptions.Options.Count > 0)
            {
                foreach (var option in globalOptions.Options)
                {
                    var shortPart = !(option.ShortName == '\0') ? $"-{option.ShortName}," : "";
                    var desc = option.Description.IsNull ? (option.Name ?? "") : (option.Description.Value?.ToString() ?? "");
                    cb.AppendLine($"        '({shortPart}--{option.Name})'{{{shortPart}--{option.Name}}}'[{desc}]' \\\\");
                }
            }

            cb.AppendLine("        '1: :->command' \\\\");
            cb.AppendLine("        '*::arg:->args'");
            cb.AppendLine("");
            cb.AppendLine("    case $state in");
            cb.AppendLine("        command)");
            cb.AppendLine("            local commands=(");

            foreach (var command in commandHierarchies)
            {
                var desc = command.Description ?? command.CommandName ?? "";
                cb.AppendLine($"                '{command.CommandName}:{desc}'");
            }

            cb.AppendLine("            )");
            cb.AppendLine("            _describe 'command' commands");
            cb.AppendLine("            ;;");
            cb.AppendLine("        args)");
            cb.AppendLine("            case $line[1] in");

            foreach (var command in commandHierarchies)
            {
                GenerateZshCommandCompletion(cb, command, 4);
            }

            cb.AppendLine("            esac");
            cb.AppendLine("            ;;");
            cb.AppendLine("    esac");
            cb.AppendLine("}");
            cb.AppendLine("");
            cb.AppendLine("_\" + appName + @\"");
            cb.AppendLine("\";");
            cb.AddBlankLine();
            cb.AppendLine("Console.WriteLine(script);");
        }
    }

    private void GenerateZshCommandCompletion(CodeBuilder cb, CommandSourceInfo command, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);

        cb.AppendLine($"{indent}{command.CommandName})");

        if (command.Subcommands.Count > 0 || command.Actions.Count > 0)
        {
            cb.AppendLine($"{indent}    local subcommands=(");

            foreach (var sub in command.Subcommands)
            {
                var desc = sub.Description ?? sub.CommandName ?? "";
                cb.AppendLine($"{indent}        '{sub.CommandName}:{desc}'");
            }

            foreach (var action in command.Actions)
            {
                var desc = action.DisplayName ?? action.ActionName ?? "";
                cb.AppendLine($"{indent}        '{action.ActionName}:{desc}'");
            }

            cb.AppendLine($"{indent}    )");
            cb.AppendLine($"{indent}    _describe 'subcommand' subcommands");
        }

        cb.AppendLine($"{indent}    ;;");
    }

    private void GeneratePowerShellCompletionMethod(CodeBuilder cb, List<CommandSourceInfo> commandHierarchies, GlobalOptionsSourceInfo? globalOptions)
    {
        using (cb.AddBlock("private void GeneratePowerShellCompletion()"))
        {
            cb.AppendLine("var appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? \"app\";");
            cb.AppendLine("var script = @\"");

            cb.AppendLine("# PowerShell completion script for \" + appName + @\"");
            cb.AppendLine("# Generated by TeCLI");
            cb.AppendLine("");
            cb.AppendLine("Register-ArgumentCompleter -Native -CommandName \" + appName + @\" -ScriptBlock {");
            cb.AppendLine("    param($wordToComplete, $commandAst, $cursorPosition)");
            cb.AppendLine("");
            cb.AppendLine("    $commands = @(");

            foreach (var command in commandHierarchies)
            {
                var desc = command.Description ?? "";
                cb.AppendLine($"        @{{ Name = '{command.CommandName}'; Description = '{desc}' }}");
            }

            cb.AppendLine("    )");
            cb.AppendLine("");
            cb.AppendLine("    $globalOpts = @(");
            cb.AppendLine("        @{ Name = '--help'; Description = 'Show help information' }");
            cb.AppendLine("        @{ Name = '-h'; Description = 'Show help information' }");
            cb.AppendLine("        @{ Name = '--version'; Description = 'Show version information' }");

            if (globalOptions != null && globalOptions.Options.Count > 0)
            {
                foreach (var option in globalOptions.Options)
                {
                    var desc = option.Description.IsNull ? "" : (option.Description.Value?.ToString() ?? "");
                    cb.AppendLine($"        @{{ Name = '--{option.Name}'; Description = '{desc}' }}");
                    if (!(option.ShortName == '\0'))
                    {
                        cb.AppendLine($"        @{{ Name = '-{option.ShortName}'; Description = '{desc}' }}");
                    }
                }
            }

            cb.AppendLine("    )");
            cb.AppendLine("");
            cb.AppendLine("    # Parse the command line to determine context");
            cb.AppendLine("    $tokens = $commandAst.ToString().Split(' ', [StringSplitOptions]::RemoveEmptyEntries)");
            cb.AppendLine("");
            cb.AppendLine("    # If we're at the first argument, show commands and global options");
            cb.AppendLine("    if ($tokens.Count -eq 1 -or ($tokens.Count -eq 2 -and $wordToComplete)) {");
            cb.AppendLine("        $commands + $globalOpts | Where-Object { $_.Name -like \\\"$wordToComplete*\\\" } | ForEach-Object {");
            cb.AppendLine("            [System.Management.Automation.CompletionResult]::new($_.Name, $_.Name, 'ParameterValue', $_.Description)");
            cb.AppendLine("        }");
            cb.AppendLine("    }");
            cb.AppendLine("}");
            cb.AppendLine("\";");
            cb.AddBlankLine();
            cb.AppendLine("Console.WriteLine(script);");
        }
    }

    private void GenerateFishCompletionMethod(CodeBuilder cb, List<CommandSourceInfo> commandHierarchies, GlobalOptionsSourceInfo? globalOptions)
    {
        using (cb.AddBlock("private void GenerateFishCompletion()"))
        {
            cb.AppendLine("var appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? \"app\";");
            cb.AppendLine("var script = @\"");

            cb.AppendLine("# Fish completion script for \" + appName + @\"");
            cb.AppendLine("# Generated by TeCLI");
            cb.AppendLine("");

            // Global options
            cb.AppendLine("# Global options");
            cb.AppendLine("complete -c \" + appName + @\" -s h -l help -d 'Show help information'");
            cb.AppendLine("complete -c \" + appName + @\" -l version -d 'Show version information'");

            if (globalOptions != null && globalOptions.Options.Count > 0)
            {
                cb.AppendLine("");
                foreach (var option in globalOptions.Options)
                {
                    var desc = option.Description.IsNull ? "" : (option.Description.Value?.ToString() ?? "");
                    var shortPart = !(option.ShortName == '\0') ? $" -s {option.ShortName}" : "";
                    cb.AppendLine($"complete -c \" + appName + @\"{shortPart} -l {option.Name} -d '{desc}'");
                }
            }

            cb.AppendLine("");
            cb.AppendLine("# Commands");

            foreach (var command in commandHierarchies)
            {
                GenerateFishCommandCompletion(cb, command);
            }

            cb.AppendLine("\";");
            cb.AddBlankLine();
            cb.AppendLine("Console.WriteLine(script);");
        }
    }

    private void GenerateFishCommandCompletion(CodeBuilder cb, CommandSourceInfo command)
    {
        var desc = command.Description ?? "";
        cb.AppendLine($"complete -c \" + appName + @\" -n '__fish_use_subcommand' -a '{command.CommandName}' -d '{desc}'");

        // Subcommands
        foreach (var sub in command.Subcommands)
        {
            var subDesc = sub.Description ?? "";
            cb.AppendLine($"complete -c \" + appName + @\" -n '__fish_seen_subcommand_from {command.CommandName}' -a '{sub.CommandName}' -d '{subDesc}'");
        }

        // Actions
        foreach (var action in command.Actions)
        {
            var actionDesc = action.DisplayName ?? "";
            cb.AppendLine($"complete -c \" + appName + @\" -n '__fish_seen_subcommand_from {command.CommandName}' -a '{action.ActionName}' -d '{actionDesc}'");
        }

        // Options from actions
        foreach (var action in command.Actions)
        {
            if (action.Method != null)
            {
                foreach (var param in action.Method.Parameters)
                {
                    var optionAttr = param.GetAttributes().FirstOrDefault(a =>
                        a.AttributeClass?.Name == "OptionAttribute" ||
                        a.AttributeClass?.ToDisplayString() == "TeCLI.Attributes.OptionAttribute");

                    if (optionAttr != null)
                    {
                        var longName = optionAttr.ConstructorArguments.FirstOrDefault().Value?.ToString();
                        var shortNameArg = optionAttr.NamedArguments.FirstOrDefault(na => na.Key == "ShortName");
                        var shortChar = shortNameArg.Value.Value is char c && c != '\0' ? c.ToString() : "";

                        if (!string.IsNullOrEmpty(longName))
                        {
                            var shortPart = !string.IsNullOrEmpty(shortChar) ? $" -s {shortChar}" : "";
                            cb.AppendLine($"complete -c \" + appName + @\" -n '__fish_seen_subcommand_from {command.CommandName}'{shortPart} -l {longName}");
                        }
                    }
                }
            }
        }
    }
}
