using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Text;

namespace TeCLI.Extensions.DependencyInjection.Generators;

public partial class DependencyInjectionInvokerGenerator
{
    private void GenerateInvoker(GeneratorExecutionContext context)
    {
        Dictionary<string, ClassDeclarationSyntax> dispatchMap = [];

        var cb = new CodeBuilder("System", "System.Linq", "Microsoft.Extensions.DependencyInjection");

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                cb.AppendLine("private IServiceProvider ServiceProvider { get; }");
                cb.AddBlankLine();
                using (cb.AddBlock("public CommandDispatcher(IServiceProvider serviceProvider)"))
                {
                    cb.AppendLine("ServiceProvider = serviceProvider;");
                }

                cb.AddBlankLine();
                using (cb.AddBlock("async Task InvokeCommandActionAsync<TCommand>(Func<TCommand, Task> parameterizedAction)"))
                {
                    cb.AppendLine("var command = ServiceProvider.GetRequiredService<TCommand>();");
                    cb.AppendLine("await parameterizedAction?.Invoke(command);");
                }

                cb.AddBlankLine();
                using (cb.AddBlock($"void InvokeCommandAction<TCommand>(Action<TCommand> parameterizedAction)"))
                {
                    cb.AppendLine("var command = ServiceProvider.GetRequiredService<TCommand>();");
                    cb.AppendLine("parameterizedAction?.Invoke(command);");
                }
            }
        }

        context.AddSource("CommandDispatcher.Invoker.cs", SourceText.From(cb, Encoding.UTF8));
    }
}