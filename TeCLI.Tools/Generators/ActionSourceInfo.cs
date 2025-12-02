using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace TeCLI.Generators
{
    public class HookInfo
    {
        public string? HookTypeName { get; set; }
        public int Order { get; set; }
    }

    /// <summary>
    /// Information about an exception-to-exit-code mapping from MapExitCodeAttribute
    /// </summary>
    public class ExitCodeMappingInfo
    {
        /// <summary>
        /// The fully qualified exception type name
        /// </summary>
        public string? ExceptionTypeName { get; set; }

        /// <summary>
        /// The exit code to use when this exception is thrown
        /// </summary>
        public int ExitCode { get; set; }
    }

    public class ActionSourceInfo
    {
        public IMethodSymbol? Method { get;  set; }
        public string? InvokerMethodName { get;  set; }
        public string? ActionName { get; set; }
        public string? DisplayName { get; set; }
        public List<string> Aliases { get; set; } = new();

        // Return type information for exit code support
        /// <summary>
        /// Whether the action returns an exit code (int, Task&lt;int&gt;, enum, or Task&lt;enum&gt;)
        /// </summary>
        public bool ReturnsExitCode { get; set; }

        /// <summary>
        /// Whether the return type is an enum (that will be cast to int for exit code)
        /// </summary>
        public bool ReturnTypeIsEnum { get; set; }

        /// <summary>
        /// The return type display string for code generation
        /// </summary>
        public string? ReturnTypeName { get; set; }

        /// <summary>
        /// The underlying type for Task&lt;T&gt; or ValueTask&lt;T&gt; returns (e.g., "int" or "ExitCode")
        /// </summary>
        public string? UnwrappedReturnTypeName { get; set; }

        /// <summary>
        /// Whether the return type is wrapped in Task or ValueTask
        /// </summary>
        public bool IsAsyncWithResult { get; set; }

        // Exception-to-exit-code mappings from MapExitCodeAttribute
        public List<ExitCodeMappingInfo> ExitCodeMappings { get; set; } = new();

        // Hooks
        public List<HookInfo> BeforeExecuteHooks { get; set; } = new();
        public List<HookInfo> AfterExecuteHooks { get; set; } = new();
        public List<HookInfo> OnErrorHooks { get; set; } = new();
    }
}
