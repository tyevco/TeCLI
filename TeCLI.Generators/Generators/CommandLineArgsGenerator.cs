using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Attributes;
using TeCLI.Extensions;

namespace TeCLI.Generators;

[Generator]
public partial class CommandLineArgsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register embedded attribute definitions
        RegisterEmbeddedAttributes(context);

        // Create a pipeline for classes with CommandAttribute
        // Only include top-level commands (not nested in another command class)
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

                if (!hasCommandAttribute)
                    return null;

                // Skip nested command classes - they will be processed when their parent is processed
                var containingType = classSymbol.ContainingType;
                if (containingType != null)
                {
                    var parentHasCommandAttribute = containingType.GetAttributes().Any(attr =>
                    {
                        var attrClass = attr.AttributeClass;
                        return attrClass?.Name == "CommandAttribute"
                            || attrClass?.ToDisplayString() == "TeCLI.Attributes.CommandAttribute";
                    });
                    if (parentHasCommandAttribute)
                        return null;
                }

                return classDecl;
            })
            .Where(static c => c is not null)
            .Select(static (c, _) => c!);

        // Create a pipeline for classes with GlobalOptionsAttribute
        var globalOptionsClassesProvider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
            transform: static (ctx, _) =>
            {
                var classDecl = (ClassDeclarationSyntax)ctx.Node;
                var model = ctx.SemanticModel;

                // Get the symbol for the class
                if (model.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol)
                    return null;

                // Check if it has GlobalOptionsAttribute
                var hasGlobalOptionsAttribute = classSymbol.GetAttributes().Any(attr =>
                {
                    var attrClass = attr.AttributeClass;
                    return attrClass?.Name == "GlobalOptionsAttribute"
                        || attrClass?.ToDisplayString() == "TeCLI.Attributes.GlobalOptionsAttribute";
                });

                return hasGlobalOptionsAttribute ? classDecl : null;
            })
            .Where(static c => c is not null)
            .Select(static (c, _) => c!);

        // Collect all command classes and global options classes
        var commandClassesCollected = commandClassesProvider.Collect();
        var globalOptionsClassesCollected = globalOptionsClassesProvider.Collect();
        var compilationProvider = context.CompilationProvider;

        // Combine everything
        var combined = compilationProvider
            .Combine(commandClassesCollected)
            .Combine(globalOptionsClassesCollected);

        // Register source output
        context.RegisterSourceOutput(combined, (spc, source) =>
        {
            var ((compilation, commandClasses), globalOptionsClasses) = source;

            if (commandClasses.Length > 0)
            {
                GenerateCommandDispatcher(spc, compilation, commandClasses, globalOptionsClasses);
            }
        });
    }
}