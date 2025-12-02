using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Detects when an option or argument uses an enum type that has no values,
/// which would make it impossible to provide valid input.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EmptyEnumTypeAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI030",
        "Option uses empty enum type",
        "{0} '{1}' uses enum type '{2}' which has no values. Users cannot provide valid input.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Options and arguments using enum types must have at least one enum value defined for users to provide valid input.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(propertyDeclaration) is not IPropertySymbol propertySymbol)
            return;

        string? attributeType = null;
        if (propertySymbol.HasAttribute(AttributeNames.OptionAttribute))
            attributeType = "Option";
        else if (propertySymbol.HasAttribute(AttributeNames.ArgumentAttribute))
            attributeType = "Argument";

        if (attributeType == null)
            return;

        CheckEnumType(context, propertySymbol.Type, propertySymbol.Name, attributeType, propertyDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameterSyntax = (ParameterSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
            return;

        string? attributeType = null;
        if (parameterSymbol.HasAttribute(AttributeNames.OptionAttribute))
            attributeType = "Option";
        else if (parameterSymbol.HasAttribute(AttributeNames.ArgumentAttribute))
            attributeType = "Argument";

        if (attributeType == null)
            return;

        CheckEnumType(context, parameterSymbol.Type, parameterSymbol.Name, attributeType, parameterSyntax.GetLocation());
    }

    private void CheckEnumType(SyntaxNodeAnalysisContext context, ITypeSymbol type, string memberName, string attributeType, Location location)
    {
        // Unwrap nullable
        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            type = namedType.TypeArguments[0];
        }

        // Check if it's an enum
        if (type.TypeKind != TypeKind.Enum)
            return;

        // Check if the enum has any values
        var enumMembers = type.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.HasConstantValue)
            .ToList();

        if (enumMembers.Count == 0)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                attributeType,
                memberName,
                type.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
