using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using TeCLI.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TeCLI.Generators;

public partial class CommandLineArgsGenerator
{
    private IEnumerable<ActionSourceInfo> GetActionInfo(Compilation compilation, ClassDeclarationSyntax classDecl)
    {
        List<ActionSourceInfo> actions = new List<ActionSourceInfo>();

        var model = compilation.GetSemanticModel(classDecl.SyntaxTree);

        if (model.GetDeclaredSymbol(classDecl) is INamedTypeSymbol classSymbol)
        {
            var actionMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol>(AttributeNames.ActionAttribute);

            foreach (var actionMethod in actionMethods)
            {
                var actionAttribute = actionMethod.GetAttribute(AttributeNames.ActionAttribute);
                if (actionAttribute == null || actionAttribute.ConstructorArguments.Length == 0)
                {
                    continue; // Skip actions without valid ActionAttribute
                }

                ActionSourceInfo asi = new()
                {
                    Method = actionMethod,
                    DisplayName = actionAttribute.ConstructorArguments[0].Value?.ToString() ?? actionMethod.Name,
                    ActionName = actionMethod.Name,
                    InvokerMethodName = $"{GetUniqueTypeName(classSymbol)}{actionMethod.Name}"
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

    /// <summary>
    /// Generates switch sections for command actions (Roslyn syntax version)
    /// </summary>
    private IEnumerable<SwitchSectionSyntax> GenerateCommandActionSwitchSections(ActionSourceInfo actionInfo)
    {
        var sections = new List<SwitchSectionSyntax>();

        var invokeStatement = actionInfo.Method!.MapAsync(
            () => $"await Process{actionInfo.InvokerMethodName}Async(remainingArgs);",
            () => $"Process{actionInfo.InvokerMethodName}(remainingArgs);");

        // Generate case for primary name
        sections.Add(SwitchSection()
            .WithLabels(SingletonList<SwitchLabelSyntax>(
                CaseSwitchLabel(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(actionInfo.DisplayName!)))))
            .WithStatements(List(new StatementSyntax[]
            {
                ParseStatement(invokeStatement),
                BreakStatement()
            })));

        // Generate cases for aliases
        foreach (var alias in actionInfo.Aliases)
        {
            sections.Add(SwitchSection()
                .WithLabels(SingletonList<SwitchLabelSyntax>(
                    CaseSwitchLabel(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(alias)))))
                .WithStatements(List(new StatementSyntax[]
                {
                    ParseStatement(invokeStatement),
                    BreakStatement()
                })));
        }

        return sections;
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

    /// <summary>
    /// Generates action processing method (Roslyn syntax version)
    /// </summary>
    private MethodDeclarationSyntax GenerateActionMethod(ActionSourceInfo actionInfo, CommandSourceInfo? commandInfo = null, GlobalOptionsSourceInfo? globalOptions = null)
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

        // Determine method signature
        bool isAsync = hasAnyHooks || actionInfo.Method!.IsAsync;
        string methodName = isAsync
            ? $"Process{actionInfo.InvokerMethodName}Async"
            : $"Process{actionInfo.InvokerMethodName}";

        var statements = new List<StatementSyntax>();

        if (hasAnyHooks)
        {
            GenerateHookStatements(statements, actionInfo, commandInfo, allBeforeHooks, allAfterHooks, allErrorHooks, globalOptions);
        }
        else
        {
            // No hooks, generate normal code
            GenerateParameterStatements(statements, actionInfo.Method!,
                actionInfo.Method!.MapAsync(
                    () => $"InvokeCommandActionAsync<{actionInfo.Method!.ContainingSymbol.Name}>",
                    () => $"InvokeCommandAction<{actionInfo.Method!.ContainingSymbol.Name}>"),
                globalOptions);
        }

        // Build method declaration
        var returnType = isAsync
            ? ParseTypeName("System.Threading.Tasks.Task")
            : PredefinedType(Token(SyntaxKind.VoidKeyword));

        var modifiers = isAsync
            ? TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.AsyncKeyword))
            : TokenList(Token(SyntaxKind.PrivateKeyword));

        return MethodDeclaration(returnType, Identifier(methodName))
            .WithModifiers(modifiers)
            .WithParameterList(ParameterList(SeparatedList(new[]
            {
                Parameter(Identifier("args"))
                    .WithType(ArrayType(PredefinedType(Token(SyntaxKind.StringKeyword)))
                        .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))))),
                Parameter(Identifier("cancellationToken"))
                    .WithType(ParseTypeName("System.Threading.CancellationToken"))
                    .WithDefault(EqualsValueClause(DefaultExpression(ParseTypeName("System.Threading.CancellationToken"))))
            })))
            .WithBody(Block(statements));
    }

    private void GenerateHookStatements(
        List<StatementSyntax> statements,
        ActionSourceInfo actionInfo,
        CommandSourceInfo? commandInfo,
        List<HookInfo> allBeforeHooks,
        List<HookInfo> allAfterHooks,
        List<HookInfo> allErrorHooks,
        GlobalOptionsSourceInfo? globalOptions)
    {
        // Generate hook context creation
        statements.Add(ParseStatement("// Create hook context"));
        statements.Add(ParseStatement($@"var hookContext = new TeCLI.Hooks.HookContext
{{
    CommandName = ""{commandInfo?.CommandName ?? ""}"",
    ActionName = ""{actionInfo.DisplayName}"",
    Arguments = args,
    CancellationToken = cancellationToken
}};"));

        // Generate before execute hooks
        if (allBeforeHooks.Count > 0)
        {
            statements.Add(ParseStatement("// Before execute hooks"));
            statements.Add(ParseStatement("var beforeHooks = new System.Collections.Generic.List<TeCLI.Hooks.IBeforeExecuteHook>();"));
            foreach (var hook in allBeforeHooks)
            {
                statements.Add(ParseStatement($"beforeHooks.Add(new {hook.HookTypeName}());"));
            }

            statements.Add(ParseStatement(@"foreach (var hook in beforeHooks)
{
    await hook.BeforeExecuteAsync(hookContext);
    if (hookContext.IsCancelled)
    {
        if (!string.IsNullOrEmpty(hookContext.CancellationMessage))
        {
            System.Console.WriteLine(hookContext.CancellationMessage);
        }
        return;
    }
}"));
        }

        // Wrap action execution in try-catch if there are after or error hooks
        if (allAfterHooks.Count > 0 || allErrorHooks.Count > 0)
        {
            var tryStatements = new List<StatementSyntax>();

            // Generate parameter parsing and action invocation
            GenerateParameterStatements(tryStatements, actionInfo.Method!,
                actionInfo.Method!.MapAsync(
                    () => $"InvokeCommandActionAsync<{actionInfo.Method!.ContainingSymbol.Name}>",
                    () => $"InvokeCommandAction<{actionInfo.Method!.ContainingSymbol.Name}>"),
                globalOptions);

            // Generate after execute hooks
            if (allAfterHooks.Count > 0)
            {
                tryStatements.Add(ParseStatement("// After execute hooks"));
                tryStatements.Add(ParseStatement("var afterHooks = new System.Collections.Generic.List<TeCLI.Hooks.IAfterExecuteHook>();"));
                foreach (var hook in allAfterHooks)
                {
                    tryStatements.Add(ParseStatement($"afterHooks.Add(new {hook.HookTypeName}());"));
                }
                tryStatements.Add(ParseStatement(@"foreach (var hook in afterHooks)
{
    await hook.AfterExecuteAsync(hookContext, null);
}"));
            }

            // Build try block
            var tryBlock = Block(tryStatements);

            // Build catch block for error hooks
            if (allErrorHooks.Count > 0)
            {
                var catchStatements = new List<StatementSyntax>();
                catchStatements.Add(ParseStatement("// Error hooks"));
                catchStatements.Add(ParseStatement("var errorHooks = new System.Collections.Generic.List<TeCLI.Hooks.IOnErrorHook>();"));
                foreach (var hook in allErrorHooks)
                {
                    catchStatements.Add(ParseStatement($"errorHooks.Add(new {hook.HookTypeName}());"));
                }
                catchStatements.Add(ParseStatement(@"bool handled = false;
foreach (var hook in errorHooks)
{
    if (await hook.OnErrorAsync(hookContext, ex))
    {
        handled = true;
        break;
    }
}

if (!handled)
{
    throw;
}"));

                var catchClause = CatchClause()
                    .WithDeclaration(CatchDeclaration(ParseTypeName("System.Exception"), Identifier("ex")))
                    .WithBlock(Block(catchStatements));

                statements.Add(TryStatement()
                    .WithBlock(tryBlock)
                    .WithCatches(SingletonList(catchClause)));
            }
            else
            {
                // No error hooks, just use try without catch
                statements.Add(TryStatement()
                    .WithBlock(tryBlock));
            }
        }
        else
        {
            // No after or error hooks, just generate the action code directly
            GenerateParameterStatements(statements, actionInfo.Method!,
                actionInfo.Method!.MapAsync(
                    () => $"InvokeCommandActionAsync<{actionInfo.Method!.ContainingSymbol.Name}>",
                    () => $"InvokeCommandAction<{actionInfo.Method!.ContainingSymbol.Name}>"),
                globalOptions);
        }
    }

    // Legacy CodeBuilder methods still used by Commands.cs

    private void GenerateCommandActions(Compilation compilation, CodeBuilder codeBuilder, ClassDeclarationSyntax classDecl, ActionSourceInfo actionInfo)
    {
        // Generate case for primary name
        using (codeBuilder.AddBlock($"case \"{actionInfo.DisplayName}\":"))
        {
            codeBuilder.AppendLine(
                actionInfo.Method!.MapAsync(
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
                    actionInfo.Method!.MapAsync(
                            () => $"await Process{actionInfo.InvokerMethodName}Async(remainingArgs);",
                            () => $"Process{actionInfo.InvokerMethodName}(remainingArgs);"));
                codeBuilder.AppendLine("break;");
            }

            codeBuilder.AddBlankLine();
        }
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
            ? $"private async System.Threading.Tasks.Task Process{actionInfo.InvokerMethodName}Async(string[] args, System.Threading.CancellationToken cancellationToken = default)"
            : actionInfo.Method!.MapAsync(
                () => $"private async System.Threading.Tasks.Task Process{actionInfo.InvokerMethodName}Async(string[] args, System.Threading.CancellationToken cancellationToken = default)",
                () => $"private void Process{actionInfo.InvokerMethodName}(string[] args, System.Threading.CancellationToken cancellationToken = default)");

        using (cb.AddBlock(methodSignature))
        {
            if (hasAnyHooks)
            {
                // Generate hook context creation
                cb.AppendLine("// Create hook context");
                cb.AppendLine("var hookContext = new TeCLI.Hooks.HookContext");
                using (cb.AddBlock("", 4, "{", "};"))
                {
                    cb.AppendLine($"CommandName = \"{commandInfo?.CommandName ?? ""}\",");
                    cb.AppendLine($"ActionName = \"{actionInfo.DisplayName}\",");
                    cb.AppendLine("Arguments = args,");
                    cb.AppendLine("CancellationToken = cancellationToken");
                }
                cb.AddBlankLine();

                // Generate before execute hooks
                if (allBeforeHooks.Count > 0)
                {
                    cb.AppendLine("// Before execute hooks");
                    cb.AppendLine("var beforeHooks = new System.Collections.Generic.List<TeCLI.Hooks.IBeforeExecuteHook>();");
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
                            actionInfo.Method!,
                            actionInfo.Method!.MapAsync(
                                () => $"InvokeCommandActionAsync<{actionInfo.Method!.ContainingSymbol.Name}>",
                                () => $"InvokeCommandAction<{actionInfo.Method!.ContainingSymbol.Name}>"),
                            globalOptions);

                        // Generate after execute hooks
                        if (allAfterHooks.Count > 0)
                        {
                            cb.AddBlankLine();
                            cb.AppendLine("// After execute hooks");
                            cb.AppendLine("var afterHooks = new System.Collections.Generic.List<TeCLI.Hooks.IAfterExecuteHook>();");
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
                            cb.AppendLine("var errorHooks = new System.Collections.Generic.List<TeCLI.Hooks.IOnErrorHook>();");
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
                        actionInfo.Method!,
                        actionInfo.Method!.MapAsync(
                            () => $"InvokeCommandActionAsync<{actionInfo.Method!.ContainingSymbol.Name}>",
                            () => $"InvokeCommandAction<{actionInfo.Method!.ContainingSymbol.Name}>"),
                        globalOptions);
                }
            }
            else
            {
                // No hooks, generate normal code
                GenerateParameterCode(
                    cb,
                    actionInfo.Method!,
                    actionInfo.Method!.MapAsync(
                        () => $"InvokeCommandActionAsync<{actionInfo.Method!.ContainingSymbol.Name}>",
                        () => $"InvokeCommandAction<{actionInfo.Method!.ContainingSymbol.Name}>"),
                    globalOptions);
            }
        }
    }
}
