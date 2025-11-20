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
    private void GenerateCommandDispatcher(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> commandClasses)
    {
        Dictionary<ClassDeclarationSyntax, List<string>> dispatchMap = [];

        var cb = new CodeBuilder("System", "System.Linq");

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                // make the partial method for the invoker
                using (cb.AddBlock("public async Task DispatchAsync(string[] args)"))
                {
                    using (cb.AddBlock("if (args.Length == 0)"))
                    {
                        cb.AppendLine("DisplayApplicationHelp();");
                    }
                    using (cb.AddBlock("else"))
                    {
                        cb.AddBlankLine();

                        // Check for help flag
                        using (cb.AddBlock("if (args.Contains(\"--help\") || args.Contains(\"-h\"))"))
                        {
                            cb.AppendLine("DisplayApplicationHelp();");
                            cb.AppendLine("return;");
                        }

                        cb.AddBlankLine();

                        cb.AppendLine("string command = args[0].ToLower();");
                        cb.AppendLine("string[] remainingArgs = args.Skip(1).ToArray();");

                        cb.AddBlankLine();

                        using (cb.AddBlock("switch (command)"))
                        {
                            foreach (var commandClass in commandClasses)
                            {
                                var commandName = GetCommandName(commandClass);
                                var methodName = $"Dispatch{commandClass.Identifier.Text}Async";

                                using (cb.AddBlock($"case \"{commandName!.ToLower()}\":"))
                                {
                                    cb.AppendLine($"await {methodName}(remainingArgs);");
                                    cb.AppendLine("break;");
                                }

                                cb.AddBlankLine();

                                if (!dispatchMap.TryGetValue(commandClass, out var dispatchs))
                                {
                                    dispatchs = [];
                                }
                                dispatchs.Add(methodName);
                                dispatchMap[commandClass] = dispatchs;
                            }

                            using (cb.AddBlock("default:"))
                            {
                                cb.AppendLine("Console.WriteLine($\"Unknown command: {args[0]}\");");
                                cb.AppendLine("DisplayApplicationHelp();");
                                cb.AppendLine("break;");
                            }
                        }
                    }
                }
            }
        }

        context.AddSource("CommandDispatcher.cs", SourceText.From(cb, Encoding.UTF8));

        foreach (var entry in dispatchMap)
        {
            GenerateCommandSourceFile(context, compilation, entry.Value, entry.Key);
            GenerateCommandDocumentation(context, compilation, entry.Key);
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
                                    cb.AppendLine("Console.WriteLine($\"Unknown action: {action}\");");
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

}
