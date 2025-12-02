using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace TeCLI.Extensions.Autofac.Generators;

public partial class AutofacInvokerGenerator
{
    private void GenerateInvoker(SourceProductionContext context, Compilation compilation)
    {
        var cb = new CodeBuilder("System", "Autofac");

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                cb.AppendLine("private ILifetimeScope LifetimeScope { get; }");
                cb.AddBlankLine();
                using (cb.AddBlock("public CommandDispatcher(ILifetimeScope lifetimeScope)"))
                {
                    cb.AppendLine("LifetimeScope = lifetimeScope;");
                }

                cb.AddBlankLine();
                using (cb.AddBlock("async Task InvokeCommandActionAsync<TCommand>(Func<TCommand, Task> parameterizedAction, System.Threading.CancellationToken cancellationToken = default)"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = LifetimeScope.Resolve<TCommand>();");
                    cb.AppendLine("await parameterizedAction?.Invoke(command);");
                }

                cb.AddBlankLine();
                using (cb.AddBlock("void InvokeCommandAction<TCommand>(Action<TCommand> parameterizedAction, System.Threading.CancellationToken cancellationToken = default)"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = LifetimeScope.Resolve<TCommand>();");
                    cb.AppendLine("parameterizedAction?.Invoke(command);");
                }
            }
        }

        context.AddSource("CommandDispatcher.Invoker.cs", SourceText.From(cb, Encoding.UTF8));
    }
}
