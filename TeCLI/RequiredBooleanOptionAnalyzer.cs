using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Warns when a boolean option is marked as required, since boolean flags
/// should typically be optional (false by default, true when specified).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RequiredBooleanOptionAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "CLI020",
        "Boolean option marked as required",
        "Boolean option '{0}' is marked as required. Boolean flags should typically be optional (false by default).",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Boolean options are typically used as flags that default to false and become true when specified. Marking them as required is unusual and may indicate a design issue.");

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

        var optionAttr = propertySymbol.GetAttribute(AttributeNames.OptionAttribute);
        if (optionAttr == null)
            return;

        if (IsBooleanType(propertySymbol.Type) && IsMarkedRequired(optionAttr))
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                propertyDeclaration.Identifier.GetLocation(),
                propertySymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameterSyntax = (ParameterSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
            return;

        var optionAttr = parameterSymbol.GetAttribute(AttributeNames.OptionAttribute);
        if (optionAttr == null)
            return;

        if (IsBooleanType(parameterSymbol.Type) && IsMarkedRequired(optionAttr))
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                parameterSyntax.GetLocation(),
                parameterSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsBooleanType(ITypeSymbol type)
    {
        // Handle nullable bool
        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            type = namedType.TypeArguments[0];
        }

        return type.SpecialType == SpecialType.System_Boolean;
    }

    private static bool IsMarkedRequired(AttributeData optionAttr)
    {
        var requiredArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Required").Value;
        return !requiredArg.IsNull && requiredArg.Value is true;
    }
}
