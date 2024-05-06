using Microsoft.CodeAnalysis;

namespace TeCLI.Generators
{
    public class ActionSourceInfo
    {
        public IMethodSymbol? Method { get;  set; }
        public string? InvokerMethodName { get;  set; }
        public string? ActionName { get; set; }
        public string? DisplayName { get; set; }
    }
}
