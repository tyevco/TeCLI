using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace TeCLI.Generators;

public partial class CommandLineArgsGenerator
{
    private static void GenerateDefaultInvoker(SourceProductionContext context)
    {
        var cb = new CodeBuilder("System");

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                using (cb.AddBlock("async Task InvokeCommandActionAsync<TCommand>(Func<TCommand, Task> parameterizedAction, System.Threading.CancellationToken cancellationToken = default)"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = Activator.CreateInstance<TCommand>();");
                    cb.AppendLine("await parameterizedAction?.Invoke(command)!;");
                }

                cb.AddBlankLine();
                using (cb.AddBlock("void InvokeCommandAction<TCommand>(Action<TCommand> parameterizedAction, System.Threading.CancellationToken cancellationToken = default)"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = Activator.CreateInstance<TCommand>();");
                    cb.AppendLine("parameterizedAction?.Invoke(command);");
                }
            }
        }

        context.AddSource("CommandDispatcher.Invoker.cs", SourceText.From(cb, Encoding.UTF8));
    }
}
