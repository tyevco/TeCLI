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
                // Store the exit code from the last executed action
                cb.AppendLine("private int _lastExitCode = 0;");
                cb.AddBlankLine();

                // Property to get the last exit code
                cb.AppendLine("/// <summary>");
                cb.AppendLine("/// Gets the exit code from the last executed action.");
                cb.AppendLine("/// Returns 0 for success, non-zero for errors or custom exit codes.");
                cb.AppendLine("/// </summary>");
                cb.AppendLine("public int LastExitCode => _lastExitCode;");
                cb.AddBlankLine();

                // Original void-returning invokers (for void/Task actions)
                using (cb.AddBlock("async Task InvokeCommandActionAsync<TCommand>(Func<TCommand, Task> parameterizedAction, System.Threading.CancellationToken cancellationToken = default)"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = Activator.CreateInstance<TCommand>();");
                    cb.AppendLine("await parameterizedAction?.Invoke(command)!;");
                    cb.AppendLine("_lastExitCode = 0;");
                }

                cb.AddBlankLine();
                using (cb.AddBlock("void InvokeCommandAction<TCommand>(Action<TCommand> parameterizedAction, System.Threading.CancellationToken cancellationToken = default)"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = Activator.CreateInstance<TCommand>();");
                    cb.AppendLine("parameterizedAction?.Invoke(command);");
                    cb.AppendLine("_lastExitCode = 0;");
                }

                cb.AddBlankLine();

                // New int-returning invokers (for int/enum/Task<int>/Task<enum> actions)
                using (cb.AddBlock("async Task<int> InvokeCommandActionWithResultAsync<TCommand>(Func<TCommand, Task<int>> parameterizedAction, System.Threading.CancellationToken cancellationToken = default)"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = Activator.CreateInstance<TCommand>();");
                    cb.AppendLine("_lastExitCode = await parameterizedAction?.Invoke(command)!;");
                    cb.AppendLine("return _lastExitCode;");
                }

                cb.AddBlankLine();
                using (cb.AddBlock("int InvokeCommandActionWithResult<TCommand>(Func<TCommand, int> parameterizedAction, System.Threading.CancellationToken cancellationToken = default)"))
                {
                    cb.AppendLine("cancellationToken.ThrowIfCancellationRequested();");
                    cb.AppendLine("var command = Activator.CreateInstance<TCommand>();");
                    cb.AppendLine("_lastExitCode = parameterizedAction?.Invoke(command) ?? 0;");
                    cb.AppendLine("return _lastExitCode;");
                }
            }
        }

        context.AddSource("CommandDispatcher.Invoker.cs", SourceText.From(cb, Encoding.UTF8));
    }
}
