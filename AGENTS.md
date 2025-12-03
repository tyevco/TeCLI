# AI Agent Instructions for TeCLI

This document provides instructions for AI coding agents (Claude, Copilot, etc.) working on the TeCLI codebase.

## Project Overview

TeCLI is a **source-generated CLI parsing library for .NET**. It uses Roslyn source generators and custom attributes to automatically generate type-safe command-line parsing logic at compile time.

### Key Characteristics

- **Zero runtime reflection** - All code is generated at compile time
- **Source generators** - Uses Roslyn incremental generators in `TeCLI/Generators/`
- **32 Roslyn analyzers** - Compile-time error detection (CLI001-CLI032)
- **Multiple DI containers** - Microsoft DI, Autofac, SimpleInjector, Jab, PureDI
- **Extension architecture** - Core + pluggable extensions in `extensions/`
- **Target frameworks** - netstandard2.0 (core), net8.0/net9.0/net10.0 (tests/examples)

## Repository Structure

```
TeCLI/
├── TeCLI/                    # Core library (source generators + analyzers)
│   ├── Generators/           # Roslyn incremental source generators
│   └── *Analyzer.cs          # 32 Roslyn analyzers (CLI001-CLI032)
├── TeCLI.Tools/              # Shared Roslyn utilities
├── extensions/               # Extension packages
│   ├── TeCLI.Extensions.Configuration/   # JSON/YAML/TOML/INI config
│   ├── TeCLI.Extensions.Console/         # Console UI enhancements
│   ├── TeCLI.Extensions.DependencyInjection/ # Microsoft DI
│   ├── TeCLI.Extensions.Localization/    # i18n support
│   ├── TeCLI.Extensions.Output/          # JSON/XML/YAML/table output
│   ├── TeCLI.Extensions.Shell/           # Interactive REPL
│   ├── TeCLI.Extensions.Testing/         # Testing utilities
│   └── *.Analyzers/                      # Extension-specific analyzers
├── examples/                 # 12 example applications
├── tests/                    # Test projects
├── docs/                     # Documentation and migration guides
└── .github/                  # GitHub workflows and templates
```

## Development Commands

### Build

```bash
# Full solution
dotnet build TeCLI.sln

# Core library only
dotnet build TeCLI/TeCLI.csproj

# Specific extension
dotnet build extensions/TeCLI.Extensions.Configuration/TeCLI.Extensions.Configuration.csproj
```

### Test

```bash
# All tests
dotnet test TeCLI.sln

# Main test project
dotnet test TeCLI.Tests/TeCLI.Tests.csproj

# With coverage
dotnet test TeCLI.Tests/TeCLI.Tests.csproj --collect:"XPlat Code Coverage"

# Specific test file
dotnet test TeCLI.Tests/TeCLI.Tests.csproj --filter "FullyQualifiedName~CommandParsingTests"
```

### Run Examples

```bash
# Simple example
dotnet run --project examples/TeCLI.Example.Simple -- greet John

# With DI
dotnet run --project examples/TeCLI.Example.DependencyInjection -- --help

# Configuration example
dotnet run --project examples/TeCLI.Example.Configuration -- --help
```

### Benchmarks

```bash
dotnet run --project TeCLI.Benchmarks -c Release
```

## Code Patterns

### Source Generator Pattern

Source generators in TeCLI follow the incremental generator pattern:

```csharp
[Generator]
public class MyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Create a syntax provider to find relevant syntax nodes
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => GetSemanticModel(ctx))
            .Where(static m => m is not null);

        // 2. Combine with compilation
        var compilation = context.CompilationProvider.Combine(provider.Collect());

        // 3. Register source output
        context.RegisterSourceOutput(compilation, Execute);
    }
}
```

### Analyzer Pattern

Analyzers follow a consistent structure:

```csharp
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MyAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CLI0XX";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Title",
        messageFormat: "Message with {0} parameters",
        category: "TeCLI",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }
}
```

### Command Pattern

Commands use attribute-based declarations:

```csharp
[Command("deploy", Description = "Deploy the application")]
public class DeployCommand
{
    [Primary(Description = "Deploy to environment")]
    public async Task Execute(
        [Option("environment", ShortName = 'e', Required = true)] string env,
        [Option("force", ShortName = 'f')] bool force = false,
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }

    [Action("rollback", Description = "Rollback deployment")]
    public void Rollback([Argument] string version)
    {
        // Implementation
    }
}
```

## Coding Standards

### Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Analyzers | `{Feature}Analyzer` | `DuplicateNameAnalyzer` |
| Generators | `{Feature}Generator` | `CommandDispatcherGenerator` |
| Extensions | `TeCLI.Extensions.{Feature}` | `TeCLI.Extensions.Configuration` |
| Test classes | `{Feature}Tests` | `CommandParsingTests` |
| Diagnostic IDs | `CLI0XX` | `CLI001`, `CLI032` |

### Analyzer Severity Levels

- **Error (CLI001-CLI010, CLI016, CLI018, CLI021-022, CLI030)**: Code won't work correctly
- **Warning (CLI004-005, CLI011-013, CLI015, CLI017, CLI020, CLI024, CLI028, CLI031)**: Potential issues
- **Info (CLI014, CLI019, CLI023, CLI025-027, CLI029, CLI032)**: Suggestions and best practices

### Required Practices

1. **Always run tests before committing**:
   ```bash
   dotnet test TeCLI.sln
   ```

2. **Add XML documentation** for public APIs

3. **Follow existing patterns** - Look at similar code in the codebase

4. **Maintain backward compatibility** - Don't break existing APIs without major version bump

5. **Include CancellationToken** in async methods where appropriate

## Testing Guidelines

### Unit Test Structure

```csharp
public class FeatureTests
{
    [Fact]
    public void Method_WhenCondition_ShouldBehavior()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        var result = sut.Method();

        // Assert
        Assert.NotNull(result);
    }
}
```

### Analyzer Test Pattern

```csharp
[Fact]
public async Task Analyzer_WhenViolation_ShouldReportDiagnostic()
{
    var source = @"
        [Command("""")]  // Empty name - should trigger CLI006
        public class TestCommand { }
    ";

    await VerifyCS.VerifyAnalyzerAsync(source,
        VerifyCS.Diagnostic(EmptyNameAnalyzer.DiagnosticId)
            .WithLocation(2, 10));
}
```

### Generator Test Pattern

```csharp
[Fact]
public void Generator_WithValidCommand_ShouldGenerateDispatcher()
{
    var source = @"
        using TeCLI;

        [Command(""test"")]
        public class TestCommand
        {
            [Primary]
            public void Execute() { }
        }
    ";

    var compilation = CreateCompilation(source);
    var generator = new CommandDispatcherGenerator();

    GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
    driver = driver.RunGenerators(compilation);

    var results = driver.GetRunResult();
    Assert.Single(results.GeneratedTrees);
}
```

## Common Tasks

### Adding a New Analyzer

1. Create `{Name}Analyzer.cs` in `TeCLI/`
2. Assign next available diagnostic ID (CLI0XX)
3. Implement `DiagnosticAnalyzer` base class
4. Add tests in `TeCLI.Tests/Analyzers/`
5. Update `README.md` analyzer documentation

### Adding a New Extension

1. Create project in `extensions/TeCLI.Extensions.{Name}/`
2. Reference `TeCLI` and `TeCLI.Tools`
3. Add `README.md` with usage examples
4. Create example project in `examples/TeCLI.Example.{Name}/`
5. Add test project in `tests/TeCLI.Extensions.{Name}.Tests/`
6. Update solution file

### Adding a New Generator Feature

1. Modify generator in `TeCLI/Generators/`
2. Update `TeCLI.Tools` if new utilities needed
3. Add comprehensive tests
4. Update `README.md` documentation

## Important Files

| File | Purpose |
|------|---------|
| `TeCLI.sln` | Solution file with all projects |
| `Directory.Build.props` | Global MSBuild properties |
| `NuGet.config` | NuGet package source configuration |
| `ROADMAP.md` | Feature roadmap and status |
| `.github/workflows/dotnet.yml` | CI/CD pipeline |

## Version Management

TeCLI uses [GitVersion](https://gitversion.net/) for semantic versioning:

- Version is determined from Git tags and commits
- No manual version management required
- Release workflow publishes to NuGet automatically

## Troubleshooting

### Common Issues

1. **Generator not running**: Clean and rebuild the solution
   ```bash
   dotnet clean TeCLI.sln && dotnet build TeCLI.sln
   ```

2. **Analyzer warnings in tests**: Tests intentionally trigger analyzers - this is expected

3. **Missing dependencies**: Restore NuGet packages
   ```bash
   dotnet restore TeCLI.sln
   ```

4. **Build order issues**: The solution has proper project references, but if building fails:
   ```bash
   dotnet build TeCLI.Tools/TeCLI.Tools.csproj
   dotnet build TeCLI/TeCLI.csproj
   dotnet build TeCLI.sln
   ```

## Additional Resources

- [README.md](README.md) - Main project documentation
- [CONTRIBUTING.md](CONTRIBUTING.md) - Contribution guidelines
- [ROADMAP.md](ROADMAP.md) - Feature roadmap
- [docs/migration/](docs/migration/) - Migration guides from other CLI libraries
- [.github/SECURITY.md](.github/SECURITY.md) - Security policy
