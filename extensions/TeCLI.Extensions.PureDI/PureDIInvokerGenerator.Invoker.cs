using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace TeCLI.Extensions.PureDI.Generators;

public partial class PureDIInvokerGenerator
{
    private void GenerateInvoker(SourceProductionContext context, Compilation compilation)
    {
        var cb = new CodeBuilder("System", "Pure.DI");

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                cb.AppendLine("private Composition Composition { get; }");
                cb.AddBlankLine();
                using (cb.AddBlock("public CommandDispatcher(Composition composition)"))
                {
                    cb.AppendLine("Composition = composition;");
                }

                cb.AddBlankLine();
                using (cb.AddBlock("async Task InvokeCommandActionAsync<TCommand>(Func<TCommand, Task> parameterizedAction, System.Threading.CancellationToken cancellationToken = default)"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = Composition.Resolve<TCommand>();");
                    cb.AppendLine("await parameterizedAction?.Invoke(command);");
                }

                cb.AddBlankLine();
                using (cb.AddBlock("void InvokeCommandAction<TCommand>(Action<TCommand> parameterizedAction, System.Threading.CancellationToken cancellationToken = default)"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = Composition.Resolve<TCommand>();");
                    cb.AppendLine("parameterizedAction?.Invoke(command);");
                }
            }
        }

        context.AddSource("CommandDispatcher.Invoker.cs", SourceText.From(cb, Encoding.UTF8));
    }
}
