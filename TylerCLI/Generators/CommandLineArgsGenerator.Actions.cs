using TylerCLI.Attributes;
using TylerCLI.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace TylerCLI.Generators;

public partial class CommandLineArgsGenerator
{
    private Dictionary<string, IMethodSymbol> GenerateCommandActions(GeneratorExecutionContext context, CodeBuilder codeBuilder, ClassDeclarationSyntax classDecl)
    {
        var map = new Dictionary<string, IMethodSymbol>();

        var model = context.Compilation.GetSemanticModel(classDecl.SyntaxTree);

        if (model.GetDeclaredSymbol(classDecl) is INamedTypeSymbol classSymbol)
        {
            var actionMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol, ActionAttribute>();

            foreach (var actionMethod in actionMethods)
            {
                var actionAttribute = actionMethod.GetAttribute<ActionAttribute>();

                string actionName = actionAttribute!.ConstructorArguments[0]!.Value!.ToString();

                var actionInvokeMethodName = $"{classDecl.Identifier.Text}{actionMethod.Name}";

                using (codeBuilder.AddBlock($"case \"{actionName}\":"))
                {
                    codeBuilder.AppendLine($"Process{actionInvokeMethodName}(remainingArgs);");
                    codeBuilder.AppendLine("break;");
                }

                map.Add(actionInvokeMethodName, actionMethod);
            }
        }

        return map;
    }

    private void GenerateActionCode(CodeBuilder cb, string methodName, IMethodSymbol methodSymbol)
    {
        using (cb.AddBlock($"private void Process{methodName}(string[] args)"))
        {
            // parse all the remaining arguments.
            GenerateParameterCode(cb, methodSymbol, $"InvokeCommandAction<{methodSymbol.ContainingSymbol.Name}>");
        }
    }
}
