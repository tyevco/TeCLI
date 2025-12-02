using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Validates MapExitCode attribute usage to detect conflicting or problematic exception-to-exit-code mappings.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExitCodeMappingAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor DuplicateExceptionMappingRule = new(
        "CLI037",
        "Duplicate exception mapping",
        "Exception type '{0}' is mapped multiple times with different exit codes",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Each exception type should only be mapped once to avoid ambiguity.");

    private static readonly DiagnosticDescriptor NotExceptionTypeRule = new(
        "CLI037",
        "Invalid exception type in MapExitCode",
        "Type '{0}' is not an exception type. MapExitCode requires a type that derives from System.Exception.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The first parameter of [MapExitCode] must be an exception type (deriving from System.Exception).");

    private static readonly DiagnosticDescriptor OverlappingExitCodeRule = new(
        "CLI037",
        "Overlapping exit code mapping",
        "Exit code {0} is used for multiple exception types: {1}",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Multiple exception types are mapped to the same exit code. This may be intentional but could make it harder to diagnose issues.");

    private static readonly DiagnosticDescriptor GenericExceptionMappingRule = new(
        "CLI037",
        "Generic exception mapping",
        "Mapping System.Exception to an exit code will catch all exceptions. Consider mapping more specific exception types.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Mapping the base System.Exception type will catch all exceptions, which may hide unexpected errors.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DuplicateExceptionMappingRule,
            NotExceptionTypeRule,
            OverlappingExitCodeRule,
            GenericExceptionMappingRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return;

        // Only analyze command classes
        if (!classSymbol.HasAttribute(AttributeNames.CommandAttribute))
            return;

        AnalyzeMapExitCodeAttributes(context, classSymbol, classDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol methodSymbol)
            return;

        // Only analyze action methods
        if (!methodSymbol.HasAttribute(AttributeNames.ActionAttribute))
            return;

        AnalyzeMapExitCodeAttributes(context, methodSymbol, methodDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeMapExitCodeAttributes(SyntaxNodeAnalysisContext context, ISymbol symbol, Location location)
    {
        var mappings = new List<ExitCodeMapping>();

        foreach (var attr in symbol.GetAttributes())
        {
            if (attr.AttributeClass?.Name != "MapExitCodeAttribute" && attr.AttributeClass?.Name != "MapExitCode")
                continue;

            if (attr.ConstructorArguments.Length < 2)
                continue;

            var exceptionTypeArg = attr.ConstructorArguments[0];
            var exitCodeArg = attr.ConstructorArguments[1];

            if (exceptionTypeArg.Value is not INamedTypeSymbol exceptionType)
                continue;

            // Validate that the type is actually an exception
            if (!IsExceptionType(exceptionType))
            {
                var diagnostic = Diagnostic.Create(
                    NotExceptionTypeRule,
                    location,
                    exceptionType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                context.ReportDiagnostic(diagnostic);
                continue;
            }

            // Check for System.Exception being mapped (too generic)
            if (exceptionType.ToDisplayString() == "System.Exception")
            {
                var diagnostic = Diagnostic.Create(GenericExceptionMappingRule, location);
                context.ReportDiagnostic(diagnostic);
            }

            // Get exit code value
            int exitCode = 0;
            if (exitCodeArg.Value is int intValue)
            {
                exitCode = intValue;
            }
            else if (exitCodeArg.Value != null)
            {
                // Try to get the underlying value from the enum
                exitCode = (int)exitCodeArg.Value;
            }

            mappings.Add(new ExitCodeMapping
            {
                ExceptionType = exceptionType,
                ExitCode = exitCode
            });
        }

        // Check for duplicate exception mappings
        var exceptionGroups = mappings
            .GroupBy(m => m.ExceptionType, SymbolEqualityComparer.Default)
            .Where(g => g.Count() > 1);

        foreach (var group in exceptionGroups)
        {
            var exitCodes = group.Select(m => m.ExitCode).Distinct().ToList();
            if (exitCodes.Count > 1)
            {
                var diagnostic = Diagnostic.Create(
                    DuplicateExceptionMappingRule,
                    location,
                    ((INamedTypeSymbol)group.Key!).ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                context.ReportDiagnostic(diagnostic);
            }
        }

        // Check for overlapping exit codes (informational)
        var exitCodeGroups = mappings
            .GroupBy(m => m.ExitCode)
            .Where(g => g.Count() > 1);

        foreach (var group in exitCodeGroups)
        {
            var exceptionNames = string.Join(", ", group
                .Select(m => m.ExceptionType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));

            var diagnostic = Diagnostic.Create(
                OverlappingExitCodeRule,
                location,
                group.Key,
                exceptionNames);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsExceptionType(INamedTypeSymbol type)
    {
        var currentType = type;
        while (currentType != null)
        {
            if (currentType.ToDisplayString() == "System.Exception")
                return true;

            currentType = currentType.BaseType;
        }

        return false;
    }

    private class ExitCodeMapping
    {
        public INamedTypeSymbol ExceptionType { get; set; } = null!;
        public int ExitCode { get; set; }
    }
}
