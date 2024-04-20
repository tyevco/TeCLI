using TylerCLI.Attributes;
using TylerCLI.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace TylerCLI.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PrimitiveTypeArgumentAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI001",
        "Property type is not supported",
        "Property '{0}' is used as a command-line argument or option but is not a primitive type or string",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All properties used as command-line arguments or options must be primitive types or strings.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(propertyDeclaration) is IPropertySymbol propertySymbol &&
            (propertySymbol.HasAttribute<OptionAttribute>() || propertySymbol.HasAttribute<ArgumentAttribute>()) &&
            !propertySymbol.Type.IsValidOptionType())
        {
            var diagnostic = Diagnostic.Create(Rule, propertyDeclaration.GetLocation(), propertySymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}