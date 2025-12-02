using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace TeCLI.Extensions.SimpleInjector.Generators;

public partial class SimpleInjectorInvokerGenerator
{
    private void GenerateContainerExtensions(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes)
    {
        var sb = new CodeBuilder("System", "SimpleInjector");

        using (sb.AddBlock("namespace TeCLI"))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Extension methods for registering TeCLI commands with SimpleInjector.");
            sb.AppendLine("/// </summary>");
            using (sb.AddBlock("public static class ContainerExtensions"))
            {
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// Registers the CommandDispatcher and all commands with the container.");
                sb.AppendLine("/// </summary>");
                using (sb.AddBlock("public static Container AddCommandDispatcher(this Container container)"))
                {
                    sb.AppendLine("container.RegisterSingleton<CommandDispatcher>();");
                    foreach (var classDecl in classes)
                    {
                        var fullName = compilation.GetFullyQualifiedName(classDecl);
                        sb.AppendLine($"container.RegisterSingleton<{fullName}>();");
                    }
                    sb.AppendLine("return container;");
                }
            }
        }

        context.AddSource("CommandDispatcher.Extensions.cs", SourceText.From(sb, Encoding.UTF8));
    }
}
