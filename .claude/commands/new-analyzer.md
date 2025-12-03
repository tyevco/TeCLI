Guide for creating a new Roslyn analyzer in TeCLI.

Steps to create a new analyzer:

1. **Determine the diagnostic ID**: Find the next available CLI0XX number by checking existing analyzers in `TeCLI/`

2. **Create the analyzer file**: Create `TeCLI/{Name}Analyzer.cs` following this template:

```csharp
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TeCLI;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class {Name}Analyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CLI0XX";

    private static readonly LocalizableString Title = "{Title}";
    private static readonly LocalizableString MessageFormat = "{Message with {0} placeholders}";
    private static readonly LocalizableString Description = "{Description}";
    private const string Category = "TeCLI";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning, // or Error, Info
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register appropriate action
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        // Analysis logic here
    }
}
```

3. **Add tests**: Create `TeCLI.Tests/Analyzers/{Name}AnalyzerTests.cs`

4. **Update README.md**: Document the new analyzer in the analyzers section

5. **Build and test**:
```bash
dotnet build TeCLI/TeCLI.csproj
dotnet test TeCLI.Tests/TeCLI.Tests.csproj
```

What analyzer would you like to create? Please provide:
- The feature it analyzes
- The condition that triggers it
- The severity level (Error/Warning/Info)
