using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace TeCLI.Extensions.SimpleInjector.Generators
{
    [Generator]
    public partial class SimpleInjectorInvokerGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Create a pipeline for classes with CommandAttribute
            var commandClassesProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) =>
                {
                    var classDecl = (ClassDeclarationSyntax)ctx.Node;
                    var model = ctx.SemanticModel;

                    // Get the symbol for the class
                    if (model.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol)
                        return null;

                    // Check if it has CommandAttribute
                    var hasCommandAttribute = classSymbol.GetAttributes().Any(attr =>
                    {
                        var attrClass = attr.AttributeClass;
                        return attrClass?.Name == "CommandAttribute"
                            || attrClass?.ToDisplayString() == "TeCLI.Attributes.CommandAttribute";
                    });

                    return hasCommandAttribute ? classDecl : null;
                })
                .Where(static c => c is not null);

            // Collect all command classes
            var commandClassesCollected = commandClassesProvider.Collect();
            var compilationAndAnalyzerConfigProvider = context.CompilationProvider
                .Combine(context.AnalyzerConfigOptionsProvider);

            // Combine with command classes
            var combined = compilationAndAnalyzerConfigProvider.Combine(commandClassesCollected);

            // Register source output
            context.RegisterSourceOutput(combined, (spc, source) =>
            {
                var ((compilation, analyzerConfig), commandClasses) = source;

                // Check if this is the correct invoker library
                var invokerLibrary = analyzerConfig.GlobalOptions.TryGetValue(
                    $"build_property.{Constants.BuildProperties.InvokerLibrary}",
                    out var value) ? value : null;

                if (string.Equals(invokerLibrary, GetType().Assembly.GetName().Name, StringComparison.OrdinalIgnoreCase))
                {
                    GenerateInvoker(spc, compilation);
                    if (commandClasses.Length > 0)
                    {
                        GenerateContainerExtensions(spc, compilation, commandClasses!);
                    }
                }
            });
        }
    }
}
