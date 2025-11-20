using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace TeCLI.Generators
{
    public class HookInfo
    {
        public string? HookTypeName { get; set; }
        public int Order { get; set; }
    }

    public class ActionSourceInfo
    {
        public IMethodSymbol? Method { get;  set; }
        public string? InvokerMethodName { get;  set; }
        public string? ActionName { get; set; }
        public string? DisplayName { get; set; }
        public List<string> Aliases { get; set; } = new();

        // Hooks
        public List<HookInfo> BeforeExecuteHooks { get; set; } = new();
        public List<HookInfo> AfterExecuteHooks { get; set; } = new();
        public List<HookInfo> OnErrorHooks { get; set; } = new();
    }
}
