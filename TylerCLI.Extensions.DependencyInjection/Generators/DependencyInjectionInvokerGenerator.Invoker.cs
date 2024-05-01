using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Text;

namespace TylerCLI.Extensions.DependencyInjection.Generators;

public partial class DependencyInjectionInvokerGenerator
{
    private void GenerateInvoker(GeneratorExecutionContext context)
    {
        Dictionary<string, ClassDeclarationSyntax> dispatchMap = [];

        var sb = new CodeBuilder("System", "System.Linq", "Microsoft.Extensions.DependencyInjection");

        using (sb.AddBlock("namespace TylerCLI"))
        {
            using (sb.AddBlock("public partial class CommandDispatcher"))
            {
                sb.AppendLine("private IServiceProvider ServiceProvider { get; }");
                sb.AddBlankLine();
                using (sb.AddBlock("public CommandDispatcher(IServiceProvider serviceProvider)"))
                {
                    sb.AppendLine("ServiceProvider = serviceProvider;");
                }

                sb.AddBlankLine();

                using (sb.AddBlock("partial void InvokeCommandAction<TCommand>(Action<TCommand> parameterizedAction)"))
                {
                    sb.AppendLine("var command = ServiceProvider.GetRequiredService<TCommand>();");
                    sb.AppendLine("parameterizedAction?.Invoke(command);");
                }
            }
        }

        context.AddSource("CommandDispatcher.Invoker.cs", SourceText.From(sb, Encoding.UTF8));
    }
}