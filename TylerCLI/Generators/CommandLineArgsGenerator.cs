using Microsoft.CodeAnalysis;
using System;
using TylerCLI.Extensions;

namespace TylerCLI.Generators;

[Generator]
public partial class CommandLineArgsGenerator : ISourceGenerator
{
    private GeneratorExecutionContext Context { get; set; }
    private TylerCLIReceiver? Receiver { get; set; }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new TylerCLIReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is TylerCLIReceiver cliReceiver)
        {
            Receiver = cliReceiver;
            Context = context;

            GenerateCommandDispatcher(context, cliReceiver.CommandClasses);
        }
    }
}