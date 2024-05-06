using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace TeCLI.Generators;

public partial class CommandLineArgsGenerator
{
    private void GenerateApplicationDocumentation(GeneratorExecutionContext context)
    {
        CodeBuilder cb = new CodeBuilder("System");

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                using (cb.AddBlock("public static void DisplayApplicationHelp()"))
                {
                    cb.AppendLine("Console.WriteLine(\"Please provide more details...\");");
                }
            }
        }

        context.AddSource("CommandDispatcher.Documentation.cs", SourceText.From(cb, Encoding.UTF8));
    }

    private void GenerateCommandDocumentation(GeneratorExecutionContext context, ClassDeclarationSyntax classDecl)
    {
        CodeBuilder cb = new CodeBuilder("System");
        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                using (cb.AddBlock($"public static void DisplayCommand{classDecl.Identifier.Text}Help(string actionName = null)"))
                {
                    cb.AppendLine("Console.WriteLine(\"Please provide more details...\");");
                }
            }
        }

        context.AddSource($"CommandDispatcher.Command.{classDecl.Identifier.Text}.Documentation.cs", SourceText.From(cb, Encoding.UTF8));
    }
}