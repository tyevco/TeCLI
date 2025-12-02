using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using TeCLI.Extensions;

namespace TeCLI.Analyzers;

/// <summary>
/// Validates environment variable names specified in the EnvVar property of options.
/// Environment variable names should follow platform conventions and avoid problematic characters.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InvalidEnvironmentVariableAnalyzer : DiagnosticAnalyzer
{
    // Environment variable naming: typically uppercase letters, digits, and underscores
    // Should not start with a digit
    private static readonly Regex ValidEnvVarPattern = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    private static readonly DiagnosticDescriptor InvalidEnvVarNameRule = new(
        "CLI034",
        "Invalid environment variable name",
        "Environment variable name '{0}' contains invalid characters. Use only letters, digits, and underscores, and don't start with a digit.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Environment variable names should contain only alphanumeric characters and underscores, and should not start with a digit for maximum portability across platforms.");

    private static readonly DiagnosticDescriptor EmptyEnvVarNameRule = new(
        "CLI034",
        "Empty environment variable name",
        "EnvVar property is set but the value is empty or whitespace",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "If EnvVar is specified, it must have a non-empty value.");

    private static readonly DiagnosticDescriptor LowercaseEnvVarRule = new(
        "CLI034",
        "Lowercase environment variable name",
        "Environment variable name '{0}' uses lowercase letters. Convention is to use UPPERCASE_WITH_UNDERSCORES.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "By convention, environment variable names should use uppercase letters with underscores as separators.");

    private static readonly DiagnosticDescriptor DuplicateEnvVarRule = new(
        "CLI034",
        "Duplicate environment variable mapping",
        "Environment variable '{0}' is mapped to multiple options",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Each environment variable should only be mapped to a single option to avoid unexpected behavior.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            InvalidEnvVarNameRule,
            EmptyEnvVarNameRule,
            LowercaseEnvVarRule,
            DuplicateEnvVarRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
        context.RegisterSymbolAction(AnalyzeCommandForDuplicates, SymbolKind.NamedType);
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(propertyDeclaration) is not IPropertySymbol propertySymbol)
            return;

        var optionAttr = propertySymbol.GetAttribute(AttributeNames.OptionAttribute);
        if (optionAttr == null)
            return;

        ValidateEnvVar(context, optionAttr, propertyDeclaration.Identifier.GetLocation());
    }

    private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameterSyntax = (ParameterSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
            return;

        var optionAttr = parameterSymbol.GetAttribute(AttributeNames.OptionAttribute);
        if (optionAttr == null)
            return;

        ValidateEnvVar(context, optionAttr, parameterSyntax.GetLocation());
    }

    private void ValidateEnvVar(SyntaxNodeAnalysisContext context, AttributeData optionAttr, Location location)
    {
        var envVarArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "EnvVar").Value;

        if (envVarArg.IsNull)
            return;

        var envVarName = envVarArg.Value?.ToString();

        // Check for empty value
        if (string.IsNullOrWhiteSpace(envVarName))
        {
            var diagnostic = Diagnostic.Create(EmptyEnvVarNameRule, location);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Check for invalid characters
        if (!ValidEnvVarPattern.IsMatch(envVarName))
        {
            var diagnostic = Diagnostic.Create(InvalidEnvVarNameRule, location, envVarName);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Check for lowercase usage (convention is uppercase)
        if (envVarName.Any(char.IsLower))
        {
            var diagnostic = Diagnostic.Create(LowercaseEnvVarRule, location, envVarName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private void AnalyzeCommandForDuplicates(SymbolAnalysisContext context)
    {
        var classSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze command classes
        if (!classSymbol.HasAttribute(AttributeNames.CommandAttribute))
            return;

        var envVarMappings = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Location>>(
            System.StringComparer.OrdinalIgnoreCase);

        // Collect all EnvVar mappings from properties
        foreach (var property in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            CollectEnvVarMapping(property, envVarMappings);
        }

        // Collect all EnvVar mappings from method parameters
        foreach (var method in classSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            if (!method.HasAttribute(AttributeNames.ActionAttribute))
                continue;

            foreach (var parameter in method.Parameters)
            {
                CollectEnvVarMapping(parameter, envVarMappings);
            }
        }

        // Report duplicates
        foreach (var kvp in envVarMappings.Where(x => x.Value.Count > 1))
        {
            foreach (var location in kvp.Value)
            {
                var diagnostic = Diagnostic.Create(DuplicateEnvVarRule, location, kvp.Key);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void CollectEnvVarMapping(ISymbol symbol, System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Location>> mappings)
    {
        var optionAttr = symbol.GetAttribute(AttributeNames.OptionAttribute);
        if (optionAttr == null)
            return;

        var envVarArg = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "EnvVar").Value;
        if (envVarArg.IsNull)
            return;

        var envVarName = envVarArg.Value?.ToString();
        if (string.IsNullOrWhiteSpace(envVarName))
            return;

        var location = symbol.Locations.FirstOrDefault() ?? Location.None;

        if (!mappings.TryGetValue(envVarName, out var locations))
        {
            locations = new System.Collections.Generic.List<Location>();
            mappings[envVarName] = locations;
        }

        locations.Add(location);
    }
}
