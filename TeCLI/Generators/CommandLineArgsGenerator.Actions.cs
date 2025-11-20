using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using TeCLI.Attributes;
using TeCLI.Extensions;

namespace TeCLI.Generators;

public partial class CommandLineArgsGenerator
{
    private IEnumerable<ActionSourceInfo> GetActionInfo(Compilation compilation, ClassDeclarationSyntax classDecl)
    {
        List<ActionSourceInfo> actions = new List<ActionSourceInfo>();

        var model = compilation.GetSemanticModel(classDecl.SyntaxTree);

        if (model.GetDeclaredSymbol(classDecl) is INamedTypeSymbol classSymbol)
        {
            var actionMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol, ActionAttribute>();

            foreach (var actionMethod in actionMethods)
            {
                var actionAttribute = actionMethod.GetAttribute<ActionAttribute>();
                if (actionAttribute == null || actionAttribute.ConstructorArguments.Length == 0)
                {
                    continue; // Skip actions without valid ActionAttribute
                }

                ActionSourceInfo asi = new()
                {
                    Method = actionMethod,
                    DisplayName = actionAttribute.ConstructorArguments[0].Value?.ToString() ?? actionMethod.Name,
                    ActionName = actionMethod.Name,
                    InvokerMethodName = $"{classDecl.Identifier.Text}{actionMethod.Name}"
                };

                // Extract aliases from the ActionAttribute
                var aliasesArg = actionAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Aliases");
                if (!aliasesArg.Value.IsNull && aliasesArg.Value.Kind == TypedConstantKind.Array)
                {
                    foreach (var value in aliasesArg.Value.Values)
                    {
                        if (value.Value is string alias)
                        {
                            asi.Aliases.Add(alias);
                        }
                    }
                }

                actions.Add(asi);
            }
        }

        return actions;
    }

    private void GenerateCommandActions(Compilation compilation, CodeBuilder codeBuilder, ClassDeclarationSyntax classDecl, ActionSourceInfo actionInfo)
    {
        // Generate case for primary name
        using (codeBuilder.AddBlock($"case \"{actionInfo.DisplayName}\":"))
        {
            codeBuilder.AppendLine(
                actionInfo.Method.MapAsync(
                        () => $"await Process{actionInfo.InvokerMethodName}Async(remainingArgs);",
                        () => $"Process{actionInfo.InvokerMethodName}(remainingArgs);"));
            codeBuilder.AppendLine("break;");
        }

        codeBuilder.AddBlankLine();

        // Generate cases for aliases
        foreach (var alias in actionInfo.Aliases)
        {
            using (codeBuilder.AddBlock($"case \"{alias}\":"))
            {
                codeBuilder.AppendLine(
                    actionInfo.Method.MapAsync(
                            () => $"await Process{actionInfo.InvokerMethodName}Async(remainingArgs);",
                            () => $"Process{actionInfo.InvokerMethodName}(remainingArgs);"));
                codeBuilder.AppendLine("break;");
            }

            codeBuilder.AddBlankLine();
        }
    }

    private bool ActionHasHooks(ActionSourceInfo actionInfo, CommandSourceInfo? commandInfo = null)
    {
        if (actionInfo.BeforeExecuteHooks.Count > 0 || actionInfo.AfterExecuteHooks.Count > 0 || actionInfo.OnErrorHooks.Count > 0)
            return true;

        if (commandInfo != null &&
            (commandInfo.BeforeExecuteHooks.Count > 0 || commandInfo.AfterExecuteHooks.Count > 0 || commandInfo.OnErrorHooks.Count > 0))
            return true;

        return false;
    }

    private void GenerateActionCode(CodeBuilder cb, ActionSourceInfo actionInfo, CommandSourceInfo? commandInfo = null, GlobalOptionsSourceInfo? globalOptions = null)
    {
        // Combine command-level hooks with action-level hooks
        var allBeforeHooks = new List<HookInfo>();
        var allAfterHooks = new List<HookInfo>();
        var allErrorHooks = new List<HookInfo>();

        if (commandInfo != null)
        {
            allBeforeHooks.AddRange(commandInfo.BeforeExecuteHooks);
            allAfterHooks.AddRange(commandInfo.AfterExecuteHooks);
            allErrorHooks.AddRange(commandInfo.OnErrorHooks);
        }

        allBeforeHooks.AddRange(actionInfo.BeforeExecuteHooks);
        allAfterHooks.AddRange(actionInfo.AfterExecuteHooks);
        allErrorHooks.AddRange(actionInfo.OnErrorHooks);

        bool hasAnyHooks = allBeforeHooks.Count > 0 || allAfterHooks.Count > 0 || allErrorHooks.Count > 0;

        // Always generate async methods when hooks are present since hooks are async
        string methodSignature = hasAnyHooks
            ? $"private async System.Threading.Tasks.Task Process{actionInfo.InvokerMethodName}Async(string[] args)"
            : actionInfo.Method.MapAsync(
                () => $"private async System.Threading.Tasks.Task Process{actionInfo.InvokerMethodName}Async(string[] args)",
                () => $"private void Process{actionInfo.InvokerMethodName}(string[] args)");

        using (cb.AddBlock(methodSignature))
        {
            if (hasAnyHooks)
            {
                // Generate hook context creation
                cb.AppendLine("// Create hook context");
                cb.AppendLine("var hookContext = new TeCLI.Core.Hooks.HookContext");
                using (cb.AddBlock("{", "};"))
                {
                    cb.AppendLine($"CommandName = \"{commandInfo?.CommandName ?? ""}\",");
                    cb.AppendLine($"ActionName = \"{actionInfo.DisplayName}\",");
                    cb.AppendLine("Arguments = args");
                }
                cb.AddBlankLine();

                // Generate before execute hooks
                if (allBeforeHooks.Count > 0)
                {
                    cb.AppendLine("// Before execute hooks");
                    cb.AppendLine("var beforeHooks = new System.Collections.Generic.List<TeCLI.Core.Hooks.IBeforeExecuteHook>();");
                    foreach (var hook in allBeforeHooks)
                    {
                        cb.AppendLine($"beforeHooks.Add(new {hook.HookTypeName}());");
                    }
                    cb.AddBlankLine();

                    using (cb.AddBlock("foreach (var hook in beforeHooks)"))
                    {
                        cb.AppendLine("await hook.BeforeExecuteAsync(hookContext);");
                        using (cb.AddBlock("if (hookContext.IsCancelled)"))
                        {
                            using (cb.AddBlock("if (!string.IsNullOrEmpty(hookContext.CancellationMessage))"))
                            {
                                cb.AppendLine("System.Console.WriteLine(hookContext.CancellationMessage);");
                            }
                            cb.AppendLine("return;");
                        }
                    }
                    cb.AddBlankLine();
                }

                // Wrap action execution in try-catch if there are after or error hooks
                if (allAfterHooks.Count > 0 || allErrorHooks.Count > 0)
                {
                    using (cb.AddBlock("try"))
                    {
                        // Generate parameter parsing and action invocation
                        GenerateParameterCode(
                            cb,
                            actionInfo.Method,
                            actionInfo.Method.MapAsync(
                                () => $"InvokeCommandActionAsync<{actionInfo.Method.ContainingSymbol.Name}>",
                                () => $"InvokeCommandAction<{actionInfo.Method.ContainingSymbol.Name}>"),
                            globalOptions);

                        // Generate after execute hooks
                        if (allAfterHooks.Count > 0)
                        {
                            cb.AddBlankLine();
                            cb.AppendLine("// After execute hooks");
                            cb.AppendLine("var afterHooks = new System.Collections.Generic.List<TeCLI.Core.Hooks.IAfterExecuteHook>();");
                            foreach (var hook in allAfterHooks)
                            {
                                cb.AppendLine($"afterHooks.Add(new {hook.HookTypeName}());");
                            }
                            cb.AddBlankLine();

                            using (cb.AddBlock("foreach (var hook in afterHooks)"))
                            {
                                cb.AppendLine("await hook.AfterExecuteAsync(hookContext, null);");
                            }
                        }
                    }

                    // Generate error hooks
                    if (allErrorHooks.Count > 0)
                    {
                        using (cb.AddBlock("catch (System.Exception ex)"))
                        {
                            cb.AppendLine("// Error hooks");
                            cb.AppendLine("var errorHooks = new System.Collections.Generic.List<TeCLI.Core.Hooks.IOnErrorHook>();");
                            foreach (var hook in allErrorHooks)
                            {
                                cb.AppendLine($"errorHooks.Add(new {hook.HookTypeName}());");
                            }
                            cb.AddBlankLine();

                            cb.AppendLine("bool handled = false;");
                            using (cb.AddBlock("foreach (var hook in errorHooks)"))
                            {
                                using (cb.AddBlock("if (await hook.OnErrorAsync(hookContext, ex))"))
                                {
                                    cb.AppendLine("handled = true;");
                                    cb.AppendLine("break;");
                                }
                            }
                            cb.AddBlankLine();

                            using (cb.AddBlock("if (!handled)"))
                            {
                                cb.AppendLine("throw;");
                            }
                        }
                    }
                }
                else
                {
                    // No after or error hooks, just generate the action code directly
                    GenerateParameterCode(
                        cb,
                        actionInfo.Method,
                        actionInfo.Method.MapAsync(
                            () => $"InvokeCommandActionAsync<{actionInfo.Method.ContainingSymbol.Name}>",
                            () => $"InvokeCommandAction<{actionInfo.Method.ContainingSymbol.Name}>"),
                        globalOptions);
                }
            }
            else
            {
                // No hooks, generate normal code
                GenerateParameterCode(
                    cb,
                    actionInfo.Method,
                    actionInfo.Method.MapAsync(
                        () => $"InvokeCommandActionAsync<{actionInfo.Method.ContainingSymbol.Name}>",
                        () => $"InvokeCommandAction<{actionInfo.Method.ContainingSymbol.Name}>"),
                    globalOptions);
            }
        }
    }
}
