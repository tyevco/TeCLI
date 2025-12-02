using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace TeCLI.Extensions.PureDI.Generators;

public partial class PureDIInvokerGenerator
{
    private void GenerateCompositionSetup(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes)
    {
        var sb = new CodeBuilder("System", "Pure.DI");

        using (sb.AddBlock("namespace TeCLI"))
        {
            using (sb.AddBlock("internal partial class Composition"))
            {
                using (sb.AddBlock("private static void Setup() => DI.Setup(nameof(Composition))"))
                {
                    sb.AppendLine(".Bind<CommandDispatcher>().As(Lifetime.Singleton).To<CommandDispatcher>()");
                    foreach (var classDecl in classes)
                    {
                        var fullName = compilation.GetFullyQualifiedName(classDecl);
                        sb.AppendLine($".Bind<{fullName}>().As(Lifetime.Singleton).To<{fullName}>()");
                    }
                    sb.AppendLine(".Root<CommandDispatcher>(\"CommandDispatcher\");");
                }
            }
        }

        context.AddSource("Composition.Setup.cs", SourceText.From(sb, Encoding.UTF8));
    }
}
