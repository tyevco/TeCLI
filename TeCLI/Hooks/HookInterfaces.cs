using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif

namespace TeCLI.Hooks
{
    /// <summary>
    /// Context information passed to hooks during command execution
    /// </summary>
    public class HookContext
    {
        /// <summary>
        /// The name of the command being executed
        /// </summary>
        public string CommandName { get; init; } = string.Empty;

        /// <summary>
        /// The name of the action being executed
        /// </summary>
        public string ActionName { get; init; } = string.Empty;

        /// <summary>
        /// The arguments passed to the command
        /// </summary>
        public string[] Arguments { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Additional context data that can be shared between hooks
        /// </summary>
        public Dictionary<string, object> Data { get; init; } = new();

        /// <summary>
        /// Whether execution should be cancelled
        /// </summary>
        public bool IsCancelled { get; set; }

        /// <summary>
        /// Optional cancellation message
        /// </summary>
        public string? CancellationMessage { get; set; }

        /// <summary>
        /// Cancellation token for cooperative cancellation of async operations
        /// </summary>
        public CancellationToken CancellationToken { get; init; } = CancellationToken.None;
    }

    /// <summary>
    /// Hook that executes before an action
    /// </summary>
    public interface IBeforeExecuteHook
    {
        /// <summary>
        /// Executes before the action. Can cancel execution by setting context.IsCancelled = true
        /// </summary>
        Task BeforeExecuteAsync(HookContext context);
    }

    /// <summary>
    /// Hook that executes after an action completes successfully
    /// </summary>
    public interface IAfterExecuteHook
    {
        /// <summary>
        /// Executes after the action completes successfully
        /// </summary>
        /// <param name="context">The execution context</param>
        /// <param name="result">The result returned by the action (if any)</param>
        Task AfterExecuteAsync(HookContext context, object? result);
    }

    /// <summary>
    /// Hook that executes when an error occurs during action execution
    /// </summary>
    public interface IOnErrorHook
    {
        /// <summary>
        /// Executes when an exception occurs during action execution
        /// </summary>
        /// <param name="context">The execution context</param>
        /// <param name="exception">The exception that occurred</param>
        /// <returns>True to suppress the exception, false to rethrow</returns>
        Task<bool> OnErrorAsync(HookContext context, Exception exception);
    }
}
