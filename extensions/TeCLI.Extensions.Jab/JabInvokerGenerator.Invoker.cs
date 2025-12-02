using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace TeCLI.Extensions.Jab.Generators;

public partial class JabInvokerGenerator
{
    private void GenerateInvoker(SourceProductionContext context, Compilation compilation)
    {
        var cb = new CodeBuilder("System", "Jab");

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                cb.AppendLine("private CommandServiceProvider ServiceProvider { get; }");
                cb.AddBlankLine();
                using (cb.AddBlock("public CommandDispatcher(CommandServiceProvider serviceProvider)"))
                {
                    cb.AppendLine("ServiceProvider = serviceProvider;");
                }

                cb.AddBlankLine();
                using (cb.AddBlock("async Task InvokeCommandActionAsync<TCommand>(Func<TCommand, Task> parameterizedAction, System.Threading.CancellationToken cancellationToken = default)"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = ServiceProvider.GetService<TCommand>();");
                    cb.AppendLine("await parameterizedAction?.Invoke(command);");
                }

                cb.AddBlankLine();
                using (cb.AddBlock("void InvokeCommandAction<TCommand>(Action<TCommand> parameterizedAction, System.Threading.CancellationToken cancellationToken = default)"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = ServiceProvider.GetService<TCommand>();");
                    cb.AppendLine("parameterizedAction?.Invoke(command);");
                }
            }
        }

        context.AddSource("CommandDispatcher.Invoker.cs", SourceText.From(cb, Encoding.UTF8));
    }
}
