using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace TeCLI.Extensions.Jab.Generators;

public partial class JabInvokerGenerator
{
    private void GenerateServiceProvider(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes)
    {
        var sb = new CodeBuilder("System", "Jab");

        using (sb.AddBlock("namespace TeCLI"))
        {
            // Generate the ServiceProvider attribute with all singleton registrations
            sb.AppendLine("[ServiceProvider]");
            sb.AppendLine("[Singleton(typeof(CommandDispatcher))]");

            foreach (var classDecl in classes)
            {
                var fullName = compilation.GetFullyQualifiedName(classDecl);
                sb.AppendLine($"[Singleton(typeof({fullName}))]");
            }

            using (sb.AddBlock("internal partial class CommandServiceProvider"))
            {
                // Jab generates the implementation
            }
        }

        context.AddSource("CommandServiceProvider.cs", SourceText.From(sb, Encoding.UTF8));
    }
}
