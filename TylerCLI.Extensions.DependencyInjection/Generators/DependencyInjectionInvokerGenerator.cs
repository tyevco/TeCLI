using Microsoft.CodeAnalysis;
using System;

namespace TylerCLI.Extensions.DependencyInjection.Generators
{
    [Generator]
    public partial class DependencyInjectionInvokerGenerator : ISourceGenerator
    {
        private GeneratorExecutionContext Context { get; set; }
        private TylerCLIDependencyReceiver? Receiver { get; set; }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new TylerCLIDependencyReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is TylerCLIDependencyReceiver cliReceiver)
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
