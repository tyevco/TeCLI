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

    private void GenerateActionCode(CodeBuilder cb, ActionSourceInfo actionInfo, GlobalOptionsSourceInfo? globalOptions = null)
    {

        using (cb.AddBlock(
            actionInfo.Method.MapAsync(
                    () => $"private async Task Process{actionInfo.InvokerMethodName}Async(string[] args)",
                    () => $"private void Process{actionInfo.InvokerMethodName}(string[] args)")))
        {
            // parse all the remaining arguments.
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
