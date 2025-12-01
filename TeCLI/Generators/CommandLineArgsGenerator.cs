using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
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
            .Where(static c => c is not null);

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

            // DIAGNOSTIC: Always output debug info to verify generator runs
            var debugInfo = $@"// TeCLI Generator Diagnostic
// Generated at: {DateTime.Now}
// Command classes found: {commandClasses.Length}
// Global options classes found: {globalOptionsClasses.Length}
// Assembly name: {compilation.AssemblyName}
";
            spc.AddSource("TeCLI.Diagnostic.cs", Microsoft.CodeAnalysis.Text.SourceText.From(debugInfo, System.Text.Encoding.UTF8));

            if (commandClasses.Length > 0)
            {
                GenerateCommandDispatcher(spc, compilation, commandClasses!, globalOptionsClasses);
            }
        });
    }
}