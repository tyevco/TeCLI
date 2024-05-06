using Microsoft.CodeAnalysis;
using System;

namespace TeCLI.Extensions.DependencyInjection.Generators
{
    [Generator]
    public partial class DependencyInjectionInvokerGenerator : ISourceGenerator
    {
        private GeneratorExecutionContext Context { get; set; }
        private TeCLIDependencyReceiver? Receiver { get; set; }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new TeCLIDependencyReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is TeCLIDependencyReceiver cliReceiver)
            {
                Receiver = cliReceiver;
                Context = context;

                var invokerLibrary = context.GetBuildProperty(Constants.BuildProperties.InvokerLibrary);
                if (string.Equals(invokerLibrary, GetType().Assembly.GetName().Name, StringComparison.OrdinalIgnoreCase))
                {
                    GenerateInvoker(context);
                    GenerateCommandRegistrations(context, cliReceiver.InvokerClasses);
                }
            }
        }
    }
}
