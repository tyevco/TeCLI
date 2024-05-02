using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using TylerCLI.Attributes;
using TylerCLI.Extensions;

namespace TylerCLI.Generators;

public partial class CommandLineArgsGenerator
{
    private IEnumerable<ActionSourceInfo> GetActionInfo(GeneratorExecutionContext context, ClassDeclarationSyntax classDecl)
    {
        List<ActionSourceInfo> actions = new List<ActionSourceInfo>();

        var model = context.Compilation.GetSemanticModel(classDecl.SyntaxTree);

        if (model.GetDeclaredSymbol(classDecl) is INamedTypeSymbol classSymbol)
        {
            var actionMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol, ActionAttribute>();

            foreach (var actionMethod in actionMethods)
            {
                ActionSourceInfo asi = new()
                {
                    Method = actionMethod,
                };

                var actionAttribute = actionMethod.GetAttribute<ActionAttribute>();
                asi.DisplayName = actionAttribute!.ConstructorArguments[0]!.Value!.ToString();
                asi.ActionName = actionMethod.Name;
                asi.InvokerMethodName = $"{classDecl.Identifier.Text}{actionMethod.Name}";

                if (asi != null)
                {
                    actions.Add(asi);
                }
            }
        }

        return actions;
    }

    private void GenerateCommandActions(GeneratorExecutionContext context, CodeBuilder codeBuilder, ClassDeclarationSyntax classDecl, ActionSourceInfo actionInfo)
    {
        using (codeBuilder.AddBlock($"case \"{actionInfo.DisplayName}\":"))
        {
            codeBuilder.AppendLine(
                actionInfo.Method.MapAsync(
                        () => $"await Process{actionInfo.InvokerMethodName}Async(remainingArgs);",
                        () => $"Process{actionInfo.InvokerMethodName}(remainingArgs);"));
            codeBuilder.AppendLine("break;");
        }
    }

    private void GenerateActionCode(CodeBuilder cb, ActionSourceInfo actionInfo)
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
                    () => $"InvokeCommandAction<{actionInfo.Method.ContainingSymbol.Name}>"));
        }
    }
}
