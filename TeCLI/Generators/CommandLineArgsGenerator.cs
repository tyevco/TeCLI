using Microsoft.CodeAnalysis;
using System;
using TeCLI.Extensions;

namespace TeCLI.Generators;

[Generator]
public partial class CommandLineArgsGenerator : ISourceGenerator
{
    private GeneratorExecutionContext Context { get; set; }
    private TeCLIReceiver? Receiver { get; set; }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new TeCLIReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is TeCLIReceiver cliReceiver)
        {
            Receiver = cliReceiver;
            Context = context;

            GenerateCommandDispatcher(context, cliReceiver.CommandClasses);
        }
    }
}