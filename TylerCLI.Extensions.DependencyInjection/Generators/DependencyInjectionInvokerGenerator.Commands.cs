using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Text;

namespace TylerCLI.Extensions.DependencyInjection.Generators;

public partial class DependencyInjectionInvokerGenerator
{
    private void GenerateCommandRegistrations(GeneratorExecutionContext context, IEnumerable<ClassDeclarationSyntax> classes)
    {
        var sb = new CodeBuilder("System", "System.Linq", "Microsoft.Extensions.DependencyInjection");

        using (sb.AddBlock("namespace TylerCLI"))
        {
            using (sb.AddBlock("public static class CommandDispatcherExtensions"))
            {
                using (sb.AddBlock("public static IServiceCollection AddCommandDispatcher(this IServiceCollection services)"))
                {
                    sb.AppendLine("services.AddSingleton<CommandDispatcher>();");
                    foreach (var classDecl in classes)
                    {
                        sb.AppendLine($"services.AddSingleton<{context.GetFullyQualifiedName(classDecl)}>();");
                    }
                    sb.AppendLine("return services;");
                }
            }
        }

        context.AddSource("CommandDispatcher.Registrations.cs", SourceText.From(sb, Encoding.UTF8));
    }
}
