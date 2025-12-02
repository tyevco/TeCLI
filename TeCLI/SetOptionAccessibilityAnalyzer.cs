using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SetOptionAccessibilityAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor SetOptionAccessibilityRule = new(
        id: "CLI002",
        title: "Property lacks a setter accessible by the generated code",
        messageFormat: "Property '{0}' is used as a command-line argument or option but lacks an accessible setter",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All properties used as command-line arguments or options must have an accessible setter."
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(SetOptionAccessibilityRule);

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
            (propertySymbol.HasAttribute(AttributeNames.OptionAttribute)
             || propertySymbol.HasAttribute(AttributeNames.ArgumentAttribute)))
        {
            // Check if the property has an accessible setter
            if (propertySymbol.SetMethod == null || propertySymbol.SetMethod.DeclaredAccessibility == Accessibility.Private)
            {
                var diagnostic = Diagnostic.Create(SetOptionAccessibilityRule, propertyDeclaration.GetLocation(), propertySymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}