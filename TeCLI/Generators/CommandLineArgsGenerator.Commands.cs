using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using TeCLI.Attributes;
using TeCLI.Extensions;
using static TeCLI.Constants;

namespace TeCLI.Generators;

/// <summary>
/// Generates the CommandDispatcher class and command routing logic.
/// This partial class handles the top-level command dispatch infrastructure.
/// </summary>
public partial class CommandLineArgsGenerator
{
    private void GenerateCommandDispatcher(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> commandClasses, ImmutableArray<ClassDeclarationSyntax> globalOptionsClasses)
    {
        // Extract global options information
        GlobalOptionsSourceInfo? globalOptions = null;
        if (globalOptionsClasses.Length > 0)
        {
            // Only support one global options class for now
            var globalOptionsClass = globalOptionsClasses[0];
            globalOptions = ExtractGlobalOptionsInfo(compilation, globalOptionsClass);
        }

        // Build command hierarchies
        var commandHierarchies = BuildCommandHierarchies(compilation, commandClasses);

        var cb = new CodeBuilder("System", "System.Linq");

        // Add namespace for global options if present
        if (globalOptions != null && !string.IsNullOrEmpty(globalOptions.Namespace))
        {
            cb.AddUsing(globalOptions.Namespace);
        }

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                // Add global options field if present
                if (globalOptions != null)
                {
                    cb.AppendLine($"private {globalOptions.FullTypeName} _globalOptions = new {globalOptions.FullTypeName}();");
                    cb.AddBlankLine();
                }

                // make the partial method for the invoker
                using (cb.AddBlock("public async Task DispatchAsync(string[] args)"))
                {
                    using (cb.AddBlock("if (args.Length == 0)"))
                    {
                        cb.AppendLine("DisplayApplicationHelp();");
                        cb.AppendLine("return;");
                    }

                    cb.AddBlankLine();

                    // Parse global options first if present
                    if (globalOptions != null && globalOptions.Options.Count > 0)
                    {
                        cb.AppendLine("// Parse global options");
                        cb.AppendLine("var globalOptionsParsed = new System.Collections.Generic.HashSet<int>();");
                        GenerateGlobalOptionsParsingCode(cb, globalOptions);
                        cb.AddBlankLine();
                        cb.AppendLine("// Remove parsed global options from args");
                        cb.AppendLine("var commandArgs = new System.Collections.Generic.List<string>();");
                        using (cb.AddBlock("for (int i = 0; i < args.Length; i++)"))
                        {
                            using (cb.AddBlock("if (!globalOptionsParsed.Contains(i))"))
                            {
                                cb.AppendLine("commandArgs.Add(args[i]);");
                            }
                        }
                        cb.AppendLine("args = commandArgs.ToArray();");
                        cb.AddBlankLine();
                    }

                    using (cb.AddBlock("if (args.Length == 0)"))
                    {
                        cb.AppendLine("DisplayApplicationHelp();");
                        cb.AppendLine("return;");
                    }

                    cb.AddBlankLine();

                    // Check for version flag
                    using (cb.AddBlock("if (args.Contains(\"--version\"))"))
                    {
                        cb.AppendLine("DisplayVersion();");
                        cb.AppendLine("return;");
                    }

                    cb.AddBlankLine();

                    // Check for help flag
                    using (cb.AddBlock("if (args.Contains(\"--help\") || args.Contains(\"-h\"))"))
                    {
                        cb.AppendLine("DisplayApplicationHelp();");
                        cb.AppendLine("return;");
                    }

                    cb.AddBlankLine();

                    // Check for generate-completion flag
                    using (cb.AddBlock("if (args[0] == \"--generate-completion\" && args.Length >= 2)"))
                    {
                        cb.AppendLine("GenerateCompletion(args[1]);");
                        cb.AppendLine("return;");
                    }

                    cb.AddBlankLine();

                    cb.AppendLine("string command = args[0].ToLower();");
                    cb.AppendLine("string[] remainingArgs = args.Skip(1).ToArray();");

                    cb.AddBlankLine();

                    using (cb.AddBlock("switch (command)"))
                    {
                        foreach (var commandInfo in commandHierarchies)
                        {
                            var methodName = $"Dispatch{commandInfo.TypeSymbol!.Name}Async";

                            // Generate case for primary name
                            using (cb.AddBlock($"case \"{commandInfo.CommandName!.ToLower()}\":"))
                            {
                                cb.AppendLine($"await {methodName}(remainingArgs);");
                                cb.AppendLine("break;");
                            }

                            cb.AddBlankLine();

                            // Generate cases for aliases
                            foreach (var alias in commandInfo.Aliases)
                            {
                                using (cb.AddBlock($"case \"{alias.ToLower()}\":"))
                                {
                                    cb.AppendLine($"await {methodName}(remainingArgs);");
                                    cb.AppendLine("break;");
                                }

                                cb.AddBlankLine();
                            }
                        }

                        using (cb.AddBlock("default:"))
                        {
                            // Build list of available commands (including aliases) for suggestions
                            cb.AppendLine("var availableCommands = new[] {");
                            bool first = true;
                            foreach (var commandInfo in commandHierarchies)
                            {
                                if (!first) cb.Append(", ");
                                cb.Append($"\"{commandInfo.CommandName!.ToLower()}\"");
                                first = false;

                                // Add aliases to the suggestion list
                                foreach (var alias in commandInfo.Aliases)
                                {
                                    cb.Append($", \"{alias.ToLower()}\"");
                                }
                            }
                            cb.AppendLine(" };");

                            cb.AppendLine("var suggestion = TeCLI.StringSimilarity.FindMostSimilar(command, availableCommands);");
                            using (cb.AddBlock("if (suggestion != null)"))
                            {
                                cb.AppendLine($"""Console.WriteLine(string.Format("{ErrorMessages.UnknownCommandWithSuggestion}", args[0], suggestion));""");
                            }
                            using (cb.AddBlock("else"))
                            {
                                cb.AppendLine($"""Console.WriteLine(string.Format("{ErrorMessages.UnknownCommand}", args[0]));""");
                            }
                            cb.AppendLine("DisplayApplicationHelp();");
                            cb.AppendLine("break;");
                        }
                    }
                }

                cb.AddBlankLine();

                // Generate completion support methods
                GenerateCompletionSupport(cb, commandHierarchies, globalOptions);
                }
            }
        }

        context.AddSource("CommandDispatcher.cs", SourceText.From(cb, Encoding.UTF8));

        // Generate dispatch methods for all commands in the hierarchies
        foreach (var commandInfo in commandHierarchies)
        {
            GenerateCommandSourceFileHierarchical(context, compilation, commandInfo, globalOptions);
            GenerateCommandDocumentation(context, compilation, commandInfo);
        }

        GenerateApplicationDocumentation(context, compilation);
    }

    private void GenerateCommandSourceFile(SourceProductionContext context, Compilation compilation, List<string> methodNames, ClassDeclarationSyntax classDecl)
    {
        var cb = new CodeBuilder("System", "System.Linq", "TeCLI", "TeCLI.Attributes");

        var actionMap = GetActionInfo(compilation, classDecl);

        cb.AddUsing(compilation.GetNamespace(classDecl)!);

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                foreach (var methodName in methodNames)
                {
                    using (cb.AddBlock($"private async Task {methodName}(string[] args)"))
                    {
                        // Check for help flag first
                        using (cb.AddBlock("if (args.Contains(\"--help\") || args.Contains(\"-h\"))"))
                        {
                            cb.AppendLine($"DisplayCommand{classDecl.Identifier.Text}Help();");
                            cb.AppendLine("return;");
                        }

                        cb.AddBlankLine();

                        using (cb.AddBlock("if (args.Length == 0)"))
                        {
                            GeneratePrimaryMethodInvocation(cb, compilation, classDecl, throwOnNoPrimary: true);
                        }
                        using (cb.AddBlock("else"))
                        {
                            cb.AppendLine("string action = args[0].ToLower();");
                            cb.AppendLine("string[] remainingArgs = args.Skip(1).ToArray();");

                            using (cb.AddBlock("switch (action)"))
                            {
                                foreach (var action in actionMap)
                                {
                                    GenerateCommandActions(compilation, cb, classDecl, action);
                                }

                                using (cb.AddBlock("default:"))
                                {
                                    GeneratePrimaryMethodInvocation(cb, compilation, classDecl, throwOnNoPrimary: false);

                                    // Build list of available actions (including aliases) for suggestions
                                    cb.AppendLine("var availableActions = new[] {");
                                    bool first = true;
                                    foreach (var action in actionMap)
                                    {
                                        if (!first) cb.Append(", ");
                                        cb.Append($"\"{action.ActionName.ToLower()}\"");
                                        first = false;

                                        // Add aliases to the suggestion list
                                        foreach (var alias in action.Aliases)
                                        {
                                            cb.Append($", \"{alias.ToLower()}\"");
                                        }
                                    }
                                    cb.AppendLine(" };");

                                    cb.AppendLine("var suggestion = TeCLI.StringSimilarity.FindMostSimilar(action, availableActions);");
                                    using (cb.AddBlock("if (suggestion != null)"))
                                    {
                                        cb.AppendLine($"""Console.WriteLine(string.Format("{ErrorMessages.UnknownActionWithSuggestion}", action, suggestion));""");
                                    }
                                    using (cb.AddBlock("else"))
                                    {
                                        cb.AppendLine($"""Console.WriteLine(string.Format("{ErrorMessages.UnknownAction}", action));""");
                                    }
                                    cb.AppendLine("break;");
                                }
                            }
                        }
                    }

                    cb.AddBlankLine();
                }

                // generator process action methods
                foreach (var entry in actionMap)
                {
                    cb.AddBlankLine();
                    GenerateActionCode(cb, entry);
                }

                cb.AddBlankLine();
            }
        }

        context.AddSource($"CommandDispatcher.Command.{classDecl.Identifier.Text}.cs", SourceText.From(cb, Encoding.UTF8));
    }

    /// <summary>
    /// Generates dispatch methods for a command hierarchy (including nested subcommands)
    /// </summary>
    private void GenerateCommandSourceFileHierarchical(SourceProductionContext context, Compilation compilation, CommandSourceInfo commandInfo, GlobalOptionsSourceInfo? globalOptions = null)
    {
        var cb = new CodeBuilder("System", "System.Linq", "TeCLI", "TeCLI.Attributes");

        // Add namespace for the command type
        var namespaceSymbol = commandInfo.TypeSymbol!.ContainingNamespace;
        if (namespaceSymbol != null && !namespaceSymbol.IsGlobalNamespace)
        {
            cb.AddUsing(namespaceSymbol.ToDisplayString());
        }

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                // Generate dispatch method for this command
                GenerateCommandDispatchMethod(cb, compilation, commandInfo);

                // Generate action processing methods for this command's actions
                foreach (var action in commandInfo.Actions)
                {
                    cb.AddBlankLine();
                    GenerateActionCode(cb, action, commandInfo, globalOptions);
                }

                cb.AddBlankLine();
            }
        }

        context.AddSource($"CommandDispatcher.Command.{commandInfo.TypeSymbol.Name}.cs", SourceText.From(cb, Encoding.UTF8));

        // Recursively generate dispatch methods for subcommands
        foreach (var subcommand in commandInfo.Subcommands)
        {
            GenerateCommandSourceFileHierarchical(context, compilation, subcommand, globalOptions);
        }
    }

    /// <summary>
    /// Generates the dispatch method for a single command (which may have subcommands and/or actions)
    /// </summary>
    private void GenerateCommandDispatchMethod(CodeBuilder cb, Compilation compilation, CommandSourceInfo commandInfo)
    {
        var methodName = $"Dispatch{commandInfo.TypeSymbol!.Name}Async";

        using (cb.AddBlock($"private async Task {methodName}(string[] args)"))
        {
            // Check for help flag first
            using (cb.AddBlock("if (args.Contains(\"--help\") || args.Contains(\"-h\"))"))
            {
                cb.AppendLine($"DisplayCommand{commandInfo.TypeSymbol.Name}Help();");
                cb.AppendLine("return;");
            }

            cb.AddBlankLine();

            using (cb.AddBlock("if (args.Length == 0)"))
            {
                // If no args, try to invoke primary action
                GeneratePrimaryMethodInvocationFromInfo(cb, compilation, commandInfo, throwOnNoPrimary: true);
            }
            using (cb.AddBlock("else"))
            {
                cb.AppendLine("string subcommandOrAction = args[0].ToLower();");
                cb.AppendLine("string[] remainingArgs = args.Skip(1).ToArray();");

                cb.AddBlankLine();

                using (cb.AddBlock("switch (subcommandOrAction)"))
                {
                    // Generate cases for subcommands first (they take precedence)
                    foreach (var subcommand in commandInfo.Subcommands)
                    {
                        var subMethodName = $"Dispatch{subcommand.TypeSymbol!.Name}Async";

                        // Primary subcommand name
                        using (cb.AddBlock($"case \"{subcommand.CommandName!.ToLower()}\":"))
                        {
                            cb.AppendLine($"await {subMethodName}(remainingArgs);");
                            cb.AppendLine("return;");
                        }

                        cb.AddBlankLine();

                        // Subcommand aliases
                        foreach (var alias in subcommand.Aliases)
                        {
                            using (cb.AddBlock($"case \"{alias.ToLower()}\":"))
                            {
                                cb.AppendLine($"await {subMethodName}(remainingArgs);");
                                cb.AppendLine("return;");
                            }

                            cb.AddBlankLine();
                        }
                    }

                    // Generate cases for actions
                    foreach (var action in commandInfo.Actions)
                    {
                        var actionInvokeMethodName = $"{commandInfo.TypeSymbol.Name}{action.Method!.Name}";

                        // Check if action has hooks - if so, always use async version
                        bool hasHooks = ActionHasHooks(action, commandInfo);
                        string actionCall = hasHooks
                            ? $"await Process{actionInvokeMethodName}Async(remainingArgs);"
                            : action.Method.MapAsync(
                                () => $"await Process{actionInvokeMethodName}Async(remainingArgs);",
                                () => $"Process{actionInvokeMethodName}(remainingArgs);");

                        // Primary action name
                        using (cb.AddBlock($"case \"{action.ActionName!.ToLower()}\":"))
                        {
                            cb.AppendLine(actionCall);
                            cb.AppendLine("break;");
                        }

                        cb.AddBlankLine();

                        // Action aliases
                        foreach (var alias in action.Aliases)
                        {
                            using (cb.AddBlock($"case \"{alias.ToLower()}\":"))
                            {
                                cb.AppendLine(actionCall);
                                cb.AppendLine("break;");
                            }

                            cb.AddBlankLine();
                        }
                    }

                    // Default case: try primary action or show error
                    using (cb.AddBlock("default:"))
                    {
                        GeneratePrimaryMethodInvocationFromInfo(cb, compilation, commandInfo, throwOnNoPrimary: false);

                        // Build list of available subcommands and actions for suggestions
                        cb.AppendLine("var availableOptions = new List<string>();");

                        // Add subcommands
                        foreach (var subcommand in commandInfo.Subcommands)
                        {
                            cb.AppendLine($"availableOptions.Add(\"{subcommand.CommandName!.ToLower()}\");");
                            foreach (var alias in subcommand.Aliases)
                            {
                                cb.AppendLine($"availableOptions.Add(\"{alias.ToLower()}\");");
                            }
                        }

                        // Add actions
                        foreach (var action in commandInfo.Actions)
                        {
                            cb.AppendLine($"availableOptions.Add(\"{action.ActionName!.ToLower()}\");");
                            foreach (var alias in action.Aliases)
                            {
                                cb.AppendLine($"availableOptions.Add(\"{alias.ToLower()}\");");
                            }
                        }

                        cb.AppendLine("var suggestion = TeCLI.StringSimilarity.FindMostSimilar(subcommandOrAction, availableOptions.ToArray());");
                        using (cb.AddBlock("if (suggestion != null)"))
                        {
                            cb.AppendLine($"""Console.WriteLine(string.Format("{ErrorMessages.UnknownActionWithSuggestion}", subcommandOrAction, suggestion));""");
                        }
                        using (cb.AddBlock("else"))
                        {
                            cb.AppendLine($"""Console.WriteLine(string.Format("{ErrorMessages.UnknownAction}", subcommandOrAction));""");
                        }
                        cb.AppendLine("break;");
                    }
                }
            }
        }

        cb.AddBlankLine();
    }

    /// <summary>
    /// Generates primary method invocation from CommandSourceInfo
    /// </summary>
    private void GeneratePrimaryMethodInvocationFromInfo(CodeBuilder cb, Compilation compilation, CommandSourceInfo commandInfo, bool throwOnNoPrimary)
    {
        var primaryMethods = commandInfo.TypeSymbol!.GetMembersWithAttribute<IMethodSymbol, PrimaryAttribute>();

        int count = 0;
        if (primaryMethods != null)
        {
            foreach (var primaryMethod in primaryMethods)
            {
                if (count++ > 0)
                {
                    // Multiple primary attributes defined - use the first one
                    break;
                }
                else
                {
                    // Use this method as the primary action
                    var actionInvokeMethodName = $"{commandInfo.TypeSymbol.Name}{primaryMethod.Name}";
                    cb.AppendLine(primaryMethod.MapAsync(
                            () => $"await Process{actionInvokeMethodName}Async(args);",
                            () => $"Process{actionInvokeMethodName}(args);"));
                }
            }
        }

        if (count == 0 && throwOnNoPrimary)
        {
            cb.AppendLine($"""throw new InvalidOperationException(string.Format("{ErrorMessages.NoPrimaryActionDefined}", "{commandInfo.CommandName}"));""");
        }
    }

    private string? GetCommandName(ClassDeclarationSyntax classDecl)
    {
        // Logic to extract command name from attributes
        var commandAttribute = classDecl.GetAttribute<CommandAttribute>();

        if (commandAttribute?.ArgumentList?.Arguments.Count > 0)
        {
            return commandAttribute.ArgumentList.Arguments.First().ToString().Trim('"');
        }
        return null;
    }

    private List<string> GetCommandAliases(Compilation compilation, ClassDeclarationSyntax classDecl)
    {
        var aliases = new List<string>();
        var model = compilation.GetSemanticModel(classDecl.SyntaxTree);

        if (model.GetDeclaredSymbol(classDecl) is INamedTypeSymbol classSymbol)
        {
            var commandAttr = classSymbol.GetAttribute<CommandAttribute>();
            if (commandAttr != null)
            {
                var aliasesArg = commandAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Aliases");
                if (!aliasesArg.Value.IsNull && aliasesArg.Value.Kind == TypedConstantKind.Array)
                {
                    foreach (var value in aliasesArg.Value.Values)
                    {
                        if (value.Value is string alias)
                        {
                            aliases.Add(alias);
                        }
                    }
                }
            }
        }

        return aliases;
    }

    private void GeneratePrimaryMethodInvocation(CodeBuilder cb, Compilation compilation, ClassDeclarationSyntax classDecl, bool throwOnNoPrimary)
    {
        var model = compilation.GetSemanticModel(classDecl.SyntaxTree);

        if (model.GetDeclaredSymbol(classDecl) is INamedTypeSymbol classSymbol)
        {
            var primaryMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol, PrimaryAttribute>();

            int count = 0;
            if (primaryMethods != null)
            {
                foreach (var primaryMethod in primaryMethods)
                {
                    if (count++ > 0)
                    {
                        // Multiple primary attributes defined - this should be reported as a diagnostic
                        // For now, we silently ignore additional primary attributes and use the first one
                        break;
                    }
                    else
                    {
                        // Use this method as the primary action
                        var actionInvokeMethodName = $"{classDecl.Identifier.Text}{primaryMethod.Name}";
                        cb.AppendLine(primaryMethod.MapAsync(
                                () => $"await Process{actionInvokeMethodName}Async(args);",
                                () => $"Process{actionInvokeMethodName}(args);"));
                    }
                }
            }

            if (count == 0 && throwOnNoPrimary)
            {
                cb.AppendLine($"""throw new InvalidOperationException(string.Format("{ErrorMessages.NoPrimaryActionDefined}", "{GetCommandName(classDecl)}"));""");
            }
        }
    }

    /// <summary>
    /// Recursively extracts command information including nested subcommands
    /// </summary>
    private CommandSourceInfo ExtractCommandInfo(Compilation compilation, INamedTypeSymbol typeSymbol, CommandSourceInfo? parent = null, int level = 0)
    {
        var commandInfo = new CommandSourceInfo
        {
            TypeSymbol = typeSymbol,
            Parent = parent,
            Level = level
        };

        // Extract command name and metadata from CommandAttribute
        var commandAttr = typeSymbol.GetAttribute<CommandAttribute>();
        if (commandAttr != null)
        {
            // Get command name from first constructor argument
            if (commandAttr.ConstructorArguments.Length > 0)
            {
                commandInfo.CommandName = commandAttr.ConstructorArguments[0].Value?.ToString();
            }

            // Get description from named argument
            var descArg = commandAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Description");
            if (!descArg.Value.IsNull)
            {
                commandInfo.Description = descArg.Value.Value?.ToString();
            }

            // Get aliases from named argument
            var aliasesArg = commandAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Aliases");
            if (!aliasesArg.Value.IsNull && aliasesArg.Value.Kind == TypedConstantKind.Array)
            {
                foreach (var value in aliasesArg.Value.Values)
                {
                    if (value.Value is string alias)
                    {
                        commandInfo.Aliases.Add(alias);
                    }
                }
            }
        }

        // Extract command-level hooks
        ExtractHooksFromSymbol(typeSymbol, commandInfo.BeforeExecuteHooks, commandInfo.AfterExecuteHooks, commandInfo.OnErrorHooks);

        // Extract actions from methods with ActionAttribute
        var actionMethods = typeSymbol.GetMembersWithAttribute<IMethodSymbol, ActionAttribute>();
        if (actionMethods != null)
        {
            foreach (var method in actionMethods)
            {
                var actionInfo = new ActionSourceInfo
                {
                    Method = method
                };

                var actionAttr = method.GetAttribute<ActionAttribute>();
                if (actionAttr != null)
                {
                    // Get action name from first constructor argument
                    if (actionAttr.ConstructorArguments.Length > 0)
                    {
                        actionInfo.ActionName = actionAttr.ConstructorArguments[0].Value?.ToString();
                    }

                    // Get display name
                    var displayNameArg = actionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "DisplayName");
                    if (!displayNameArg.Value.IsNull)
                    {
                        actionInfo.DisplayName = displayNameArg.Value.Value?.ToString();
                    }

                    // Get aliases
                    var aliasesArg = actionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Aliases");
                    if (!aliasesArg.Value.IsNull && aliasesArg.Value.Kind == TypedConstantKind.Array)
                    {
                        foreach (var value in aliasesArg.Value.Values)
                        {
                            if (value.Value is string alias)
                            {
                                actionInfo.Aliases.Add(alias);
                            }
                        }
                    }

                    // Set invoker method name
                    actionInfo.InvokerMethodName = $"{typeSymbol.Name}{method.Name}";
                }

                // Extract action-level hooks
                ExtractHooksFromSymbol(method, actionInfo.BeforeExecuteHooks, actionInfo.AfterExecuteHooks, actionInfo.OnErrorHooks);

                commandInfo.Actions.Add(actionInfo);
            }
        }

        // Recursively extract nested commands (nested classes with CommandAttribute)
        var nestedTypes = typeSymbol.GetTypeMembers();
        foreach (var nestedType in nestedTypes)
        {
            if (nestedType.GetAttribute<CommandAttribute>() != null)
            {
                var nestedCommandInfo = ExtractCommandInfo(compilation, nestedType, commandInfo, level + 1);
                commandInfo.Subcommands.Add(nestedCommandInfo);
            }
        }

        return commandInfo;
    }

    /// <summary>
    /// Builds a flat list of all commands in the hierarchy for easy iteration
    /// </summary>
    private List<CommandSourceInfo> FlattenCommandHierarchy(CommandSourceInfo rootCommand)
    {
        var result = new List<CommandSourceInfo> { rootCommand };

        foreach (var subcommand in rootCommand.Subcommands)
        {
            result.AddRange(FlattenCommandHierarchy(subcommand));
        }

        return result;
    }

    /// <summary>
    /// Builds command hierarchies from all top-level command classes
    /// </summary>
    private List<CommandSourceInfo> BuildCommandHierarchies(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> commandClasses)
    {
        var hierarchies = new List<CommandSourceInfo>();

        foreach (var commandClass in commandClasses)
        {
            var model = compilation.GetSemanticModel(commandClass.SyntaxTree);
            if (model.GetDeclaredSymbol(commandClass) is INamedTypeSymbol typeSymbol)
            {
                var commandInfo = ExtractCommandInfo(compilation, typeSymbol);
                hierarchies.Add(commandInfo);
            }
        }

        return hierarchies;
    }

    /// <summary>
    /// Extracts global options information from a class marked with [GlobalOptions]
    /// </summary>
    private GlobalOptionsSourceInfo ExtractGlobalOptionsInfo(Compilation compilation, ClassDeclarationSyntax classDecl)
    {
        var model = compilation.GetSemanticModel(classDecl.SyntaxTree);
        var typeSymbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

        var globalOptions = new GlobalOptionsSourceInfo
        {
            TypeSymbol = typeSymbol,
            TypeName = typeSymbol?.Name,
            FullTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            Namespace = typeSymbol?.ContainingNamespace?.ToDisplayString()
        };

        if (typeSymbol == null)
            return globalOptions;

        // Extract all properties with [Option] attribute
        var properties = typeSymbol.GetMembers().OfType<IPropertySymbol>();
        foreach (var property in properties)
        {
            var optionAttr = property.GetAttribute<OptionAttribute>();
            if (optionAttr != null)
            {
                var paramInfo = new ParameterSourceInfo
                {
                    Name = property.Name,
                    Type = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    Optional = property.NullableAnnotation == NullableAnnotation.Annotated || property.Type.IsValueType == false,
                    ParameterType = ParameterType.Option
                };

                // Detect collection types
                paramInfo.DetectCollectionType(property.Type);

                // Detect enum types
                paramInfo.DetectEnumType(property.Type);

                // Detect common built-in types
                paramInfo.DetectCommonTypes(property.Type);

                // Extract option name, short name, required, envvar from attribute
                var optionName = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
                if (optionName.IsNull && optionAttr.ConstructorArguments.Length > 0)
                {
                    optionName = optionAttr.ConstructorArguments[0];
                }
                paramInfo.Name = optionName.IsNull ? property.Name : optionName.Value?.ToString() ?? property.Name;

                var shortName = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "ShortName").Value;
                paramInfo.ShortName = !shortName.IsNull && shortName.Value is char ch ? ch : '\0';

                var required = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Required").Value;
                if (!required.IsNull && required.Value is bool isRequired)
                {
                    paramInfo.Required = isRequired;
                }

                var envVar = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "EnvVar").Value;
                if (!envVar.IsNull && envVar.Value is string envVarName)
                {
                    paramInfo.EnvVar = envVarName;
                }

                bool isBoolean = property.Type.SpecialType == SpecialType.System_Boolean;
                if (isBoolean)
                {
                    paramInfo.IsSwitch = true;
                }

                // Extract validation and custom converter info
                ParameterInfoExtractor.ExtractValidationInfo(paramInfo, property);
                ParameterInfoExtractor.ExtractCustomConverterInfo(paramInfo, property);

                globalOptions.Options.Add(paramInfo);
            }
        }

        return globalOptions;
    }

    /// <summary>
    /// Generates code to parse global options from command line arguments
    /// </summary>
    private void GenerateGlobalOptionsParsingCode(CodeBuilder cb, GlobalOptionsSourceInfo globalOptions)
    {
        foreach (var option in globalOptions.Options)
        {
            // Generate parsing code for each global option using the same logic as regular options
            var tempVarName = $"_globalOpt_{option.Name}";
            var setFlagName = $"{tempVarName}Set";

            cb.AppendLine($"// Parse global option: {option.Name}");

            // Look for the option in args
            cb.AppendLine($"for (int i = 0; i < args.Length; i++)");
            using (cb.AddBlock())
            {
                cb.AppendLine($"var arg = args[i];");

                // Build condition to match option name or short name
                var conditions = new List<string>();
                conditions.Add($"arg == \"--{option.Name}\"");
                if (option.ShortName != '\0')
                {
                    conditions.Add($"arg == \"-{option.ShortName}\"");
                }

                var condition = string.Join(" || ", conditions);

                using (cb.AddBlock($"if ({condition})"))
                {
                    cb.AppendLine("globalOptionsParsed.Add(i);");

                    if (option.IsSwitch)
                    {
                        // Boolean switch - just set to true
                        cb.AppendLine($"_globalOptions.{option.Name} = true;");
                    }
                    else
                    {
                        // Regular option - parse the next argument
                        using (cb.AddBlock("if (i + 1 < args.Length)"))
                        {
                            cb.AppendLine("globalOptionsParsed.Add(i + 1);");

                            // Generate parsing code based on type
                            if (option.IsEnum)
                            {
                                cb.AppendLine($"_globalOptions.{option.Name} = ({option.DisplayType})System.Enum.Parse(typeof({option.DisplayType}), args[i + 1], ignoreCase: true);");
                            }
                            else if (option.HasCustomConverter && !string.IsNullOrEmpty(option.CustomConverterType))
                            {
                                cb.AppendLine($"var converter_{option.Name} = new {option.CustomConverterType}();");
                                cb.AppendLine($"_globalOptions.{option.Name} = converter_{option.Name}.Convert(args[i + 1]);");
                            }
                            else if (option.IsCommonType && !string.IsNullOrEmpty(option.CommonTypeParser))
                            {
                                cb.AppendLine($"_globalOptions.{option.Name} = {option.CommonTypeParser}(args[i + 1]);");
                            }
                            else if (option.Type == "string" || option.Type == "global::System.String")
                            {
                                cb.AppendLine($"_globalOptions.{option.Name} = args[i + 1];");
                            }
                            else
                            {
                                // Default parsing using the type's Parse method
                                cb.AppendLine($"_globalOptions.{option.Name} = {option.DisplayType}.Parse(args[i + 1]);");
                            }

                            // Generate validation if present
                            if (option.Validations.Count > 0)
                            {
                                foreach (var validation in option.Validations)
                                {
                                    var validationCode = string.Format(validation.ValidationCode, $"_globalOptions.{option.Name}");
                                    cb.AppendLine($"{validationCode};");
                                }
                            }
                        }
                    }

                    cb.AppendLine("break;");
                }
            }

            cb.AddBlankLine();
        }
    }

    /// <summary>
    /// Extracts hook attributes from a symbol (command class or action method)
    /// </summary>
    private void ExtractHooksFromSymbol(
        ISymbol symbol,
        List<HookInfo> beforeExecuteHooks,
        List<HookInfo> afterExecuteHooks,
        List<HookInfo> onErrorHooks)
    {
        var attributes = symbol.GetAttributes();

        // Extract BeforeExecute hooks
        foreach (var attr in attributes.Where(a => a.AttributeClass?.Name == "BeforeExecuteAttribute"))
        {
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is INamedTypeSymbol hookType)
            {
                var hookInfo = new HookInfo
                {
                    HookTypeName = hookType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                };

                // Get Order property if specified
                var orderArg = attr.NamedArguments.FirstOrDefault(arg => arg.Key == "Order");
                if (!orderArg.Value.IsNull && orderArg.Value.Value is int order)
                {
                    hookInfo.Order = order;
                }

                beforeExecuteHooks.Add(hookInfo);
            }
        }

        // Extract AfterExecute hooks
        foreach (var attr in attributes.Where(a => a.AttributeClass?.Name == "AfterExecuteAttribute"))
        {
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is INamedTypeSymbol hookType)
            {
                var hookInfo = new HookInfo
                {
                    HookTypeName = hookType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                };

                // Get Order property if specified
                var orderArg = attr.NamedArguments.FirstOrDefault(arg => arg.Key == "Order");
                if (!orderArg.Value.IsNull && orderArg.Value.Value is int order)
                {
                    hookInfo.Order = order;
                }

                afterExecuteHooks.Add(hookInfo);
            }
        }

        // Extract OnError hooks
        foreach (var attr in attributes.Where(a => a.AttributeClass?.Name == "OnErrorAttribute"))
        {
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is INamedTypeSymbol hookType)
            {
                var hookInfo = new HookInfo
                {
                    HookTypeName = hookType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                };

                // Get Order property if specified
                var orderArg = attr.NamedArguments.FirstOrDefault(arg => arg.Key == "Order");
                if (!orderArg.Value.IsNull && orderArg.Value.Value is int order)
                {
                    hookInfo.Order = order;
                }

                onErrorHooks.Add(hookInfo);
            }
        }

        // Sort hooks by order
        beforeExecuteHooks.Sort((a, b) => a.Order.CompareTo(b.Order));
        afterExecuteHooks.Sort((a, b) => a.Order.CompareTo(b.Order));
        onErrorHooks.Sort((a, b) => a.Order.CompareTo(b.Order));
    }

}
