using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace TeCLI.Shell.Analyzers;

/// <summary>
/// Validates the HistoryFile path in [Shell] attribute for potential issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ShellHistoryPathAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor AbsolutePathRule = new(
        "TCLSHELL002",
        "Absolute path in HistoryFile",
        "HistoryFile uses absolute path '{0}'. Consider using a relative path or environment variable for better portability.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Absolute paths in HistoryFile may not work across different systems or user environments.");

    private static readonly DiagnosticDescriptor SensitivePathRule = new(
        "TCLSHELL002",
        "Sensitive location for HistoryFile",
        "HistoryFile path '{0}' may expose command history in a sensitive location. Consider using a user-specific directory.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Storing command history in shared locations could expose sensitive information to other users.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(AbsolutePathRule, SensitivePathRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        // Check if this is a Shell attribute
        var symbolInfo = context.SemanticModel.GetSymbolInfo(attributeSyntax);
        if (symbolInfo.Symbol is not IMethodSymbol attributeConstructor)
            return;

        var attributeClass = attributeConstructor.ContainingType;
        if (attributeClass.Name != "ShellAttribute" && attributeClass.Name != "Shell")
            return;

        // Look for HistoryFile property
        if (attributeSyntax.ArgumentList == null)
            return;

        foreach (var arg in attributeSyntax.ArgumentList.Arguments)
        {
            if (arg.NameEquals?.Name.ToString() != "HistoryFile")
                continue;

            var constantValue = context.SemanticModel.GetConstantValue(arg.Expression);
            if (!constantValue.HasValue || constantValue.Value is not string historyPath)
                continue;

            if (string.IsNullOrEmpty(historyPath))
                continue;

            // Check for absolute paths (Windows or Unix style)
            if (IsAbsolutePath(historyPath))
            {
                var diagnostic = Diagnostic.Create(AbsolutePathRule, arg.GetLocation(), historyPath);
                context.ReportDiagnostic(diagnostic);
            }

            // Check for sensitive locations
            if (IsSensitiveLocation(historyPath))
            {
                var diagnostic = Diagnostic.Create(SensitivePathRule, arg.GetLocation(), historyPath);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsAbsolutePath(string path)
    {
        // Windows absolute paths: C:\, D:\, \\server, etc.
        // Unix absolute paths: /path/to/file
        if (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':')
            return true;

        if (path.StartsWith("/") || path.StartsWith("\\\\"))
            return true;

        return false;
    }

    private static bool IsSensitiveLocation(string path)
    {
        var lowerPath = path.ToLowerInvariant();

        // Common shared/sensitive locations
        var sensitivePatterns = new[]
        {
            "/tmp/",
            "\\temp\\",
            "/var/",
            "/etc/",
            "c:\\windows\\",
            "c:\\program files",
            "/usr/",
            "/opt/"
        };

        return sensitivePatterns.Any(pattern => lowerPath.Contains(pattern));
    }
}
