using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace TeCLI.Extensions.SimpleInjector.Generators;

public partial class SimpleInjectorInvokerGenerator
{
    private void GenerateInvoker(SourceProductionContext context, Compilation compilation)
    {
        var cb = new CodeBuilder("System", "SimpleInjector");

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                cb.AppendLine("private Container Container { get; }");
                cb.AddBlankLine();
                using (cb.AddBlock("public CommandDispatcher(Container container)"))
                {
                    cb.AppendLine("Container = container;");
                }

                cb.AddBlankLine();
                using (cb.AddBlock("async Task InvokeCommandActionAsync<TCommand>(Func<TCommand, Task> parameterizedAction, System.Threading.CancellationToken cancellationToken = default) where TCommand : class"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = Container.GetInstance<TCommand>();");
                    cb.AppendLine("await parameterizedAction?.Invoke(command);");
                }

                cb.AddBlankLine();
                using (cb.AddBlock("void InvokeCommandAction<TCommand>(Action<TCommand> parameterizedAction, System.Threading.CancellationToken cancellationToken = default) where TCommand : class"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = Container.GetInstance<TCommand>();");
                    cb.AppendLine("parameterizedAction?.Invoke(command);");
                }
            }
        }

        context.AddSource("CommandDispatcher.Invoker.cs", SourceText.From(cb, Encoding.UTF8));
    }
}
