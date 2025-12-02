using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Validates that hook attributes reference types that implement the correct hook interface.
/// [BeforeExecute] must reference IBeforeExecuteHook, [AfterExecute] must reference IAfterExecuteHook, etc.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HookTypeValidationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor InvalidBeforeExecuteHookRule = new(
        "CLI036",
        "Invalid BeforeExecute hook type",
        "Type '{0}' does not implement IBeforeExecuteHook",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The type specified in [BeforeExecute] must implement the IBeforeExecuteHook interface.");

    private static readonly DiagnosticDescriptor InvalidAfterExecuteHookRule = new(
        "CLI036",
        "Invalid AfterExecute hook type",
        "Type '{0}' does not implement IAfterExecuteHook",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The type specified in [AfterExecute] must implement the IAfterExecuteHook interface.");

    private static readonly DiagnosticDescriptor InvalidOnErrorHookRule = new(
        "CLI036",
        "Invalid OnError hook type",
        "Type '{0}' does not implement IOnErrorHook",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The type specified in [OnError] must implement the IOnErrorHook interface.");

    private static readonly DiagnosticDescriptor AbstractHookRule = new(
        "CLI036",
        "Abstract hook type",
        "Hook type '{0}' is abstract and cannot be instantiated",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Hook types must be concrete types that can be instantiated.");

    private static readonly DiagnosticDescriptor NoParameterlessConstructorRule = new(
        "CLI036",
        "Hook type missing parameterless constructor",
        "Hook type '{0}' does not have a public parameterless constructor",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Hook types must have a public parameterless constructor to be instantiated by the framework.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            InvalidBeforeExecuteHookRule,
            InvalidAfterExecuteHookRule,
            InvalidOnErrorHookRule,
            AbstractHookRule,
            NoParameterlessConstructorRule);

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

        ValidateHookAttributes(context, classSymbol, classDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol methodSymbol)
            return;

        // Only analyze action methods
        if (!methodSymbol.HasAttribute(AttributeNames.ActionAttribute))
            return;

        ValidateHookAttributes(context, methodSymbol, methodDeclaration.Identifier.GetLocation());
    }

    private void ValidateHookAttributes(SyntaxNodeAnalysisContext context, ISymbol symbol, Location location)
    {
        foreach (var attr in symbol.GetAttributes())
        {
            var attrName = attr.AttributeClass?.Name;

            if (attrName == "BeforeExecuteAttribute" || attrName == "BeforeExecute")
            {
                ValidateHookType(context, attr, "IBeforeExecuteHook", InvalidBeforeExecuteHookRule, location);
            }
            else if (attrName == "AfterExecuteAttribute" || attrName == "AfterExecute")
            {
                ValidateHookType(context, attr, "IAfterExecuteHook", InvalidAfterExecuteHookRule, location);
            }
            else if (attrName == "OnErrorAttribute" || attrName == "OnError")
            {
                ValidateHookType(context, attr, "IOnErrorHook", InvalidOnErrorHookRule, location);
            }
        }
    }

    private void ValidateHookType(
        SyntaxNodeAnalysisContext context,
        AttributeData attr,
        string requiredInterfaceName,
        DiagnosticDescriptor interfaceRule,
        Location location)
    {
        // Get the hook type from the attribute constructor
        if (attr.ConstructorArguments.Length == 0 || attr.ConstructorArguments[0].IsNull)
            return;

        if (attr.ConstructorArguments[0].Value is not INamedTypeSymbol hookType)
            return;

        var hookTypeName = hookType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        // Check if the hook type is abstract
        if (hookType.IsAbstract)
        {
            var diagnostic = Diagnostic.Create(AbstractHookRule, location, hookTypeName);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Check for parameterless constructor
        var hasParameterlessConstructor = hookType.Constructors.Any(c =>
            c.DeclaredAccessibility == Accessibility.Public &&
            c.Parameters.Length == 0);

        if (!hasParameterlessConstructor)
        {
            var diagnostic = Diagnostic.Create(NoParameterlessConstructorRule, location, hookTypeName);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Check if the hook type implements the required interface
        var implementsInterface = hookType.AllInterfaces.Any(i => i.Name == requiredInterfaceName);

        if (!implementsInterface)
        {
            var diagnostic = Diagnostic.Create(interfaceRule, location, hookTypeName);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
