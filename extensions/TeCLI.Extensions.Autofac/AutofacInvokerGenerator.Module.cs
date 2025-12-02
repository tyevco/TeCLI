using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace TeCLI.Extensions.Autofac.Generators;

public partial class AutofacInvokerGenerator
{
    private void GenerateContainerModule(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes)
    {
        var sb = new CodeBuilder("System", "Autofac");

        using (sb.AddBlock("namespace TeCLI"))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Autofac module that registers all TeCLI commands and the CommandDispatcher.");
            sb.AppendLine("/// </summary>");
            using (sb.AddBlock("public class CommandDispatcherModule : Module"))
            {
                using (sb.AddBlock("protected override void Load(ContainerBuilder builder)"))
                {
                    sb.AppendLine("builder.RegisterType<CommandDispatcher>().AsSelf().SingleInstance();");
                    foreach (var classDecl in classes)
                    {
                        var fullName = compilation.GetFullyQualifiedName(classDecl);
                        sb.AppendLine($"builder.RegisterType<{fullName}>().AsSelf().SingleInstance();");
                    }
                }
            }

            sb.AddBlankLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Extension methods for registering TeCLI commands with Autofac.");
            sb.AppendLine("/// </summary>");
            using (sb.AddBlock("public static class ContainerBuilderExtensions"))
            {
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// Registers the CommandDispatcher and all commands with the container builder.");
                sb.AppendLine("/// </summary>");
                using (sb.AddBlock("public static ContainerBuilder AddCommandDispatcher(this ContainerBuilder builder)"))
                {
                    sb.AppendLine("builder.RegisterModule<CommandDispatcherModule>();");
                    sb.AppendLine("return builder;");
                }
            }
        }

        context.AddSource("CommandDispatcher.Module.cs", SourceText.From(sb, Encoding.UTF8));
    }
}
