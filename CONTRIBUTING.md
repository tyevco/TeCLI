# Contributing to TeCLI

Thank you for your interest in contributing to TeCLI! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Project Structure](#project-structure)
- [Making Changes](#making-changes)
- [Testing](#testing)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Documentation](#documentation)
- [Issue Guidelines](#issue-guidelines)

## Code of Conduct

By participating in this project, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md). Please read it before contributing.

## Getting Started

### Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or later (10.0 recommended)
- [Git](https://git-scm.com/)
- An IDE or text editor (Visual Studio, VS Code, JetBrains Rider recommended)

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/TeCLI.git
   cd TeCLI
   ```
3. Add the upstream remote:
   ```bash
   git remote add upstream https://github.com/tyevco/TeCLI.git
   ```

## Development Setup

### Building the Project

```bash
# Restore dependencies
dotnet restore TeCLI.sln

# Build the solution
dotnet build TeCLI.sln --configuration Debug

# Build for release
dotnet build TeCLI.sln --configuration Release
```

### Running Tests

```bash
# Run all tests
dotnet test TeCLI.sln

# Run tests with coverage
dotnet test TeCLI.Tests/TeCLI.Tests.csproj --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/TeCLI.Extensions.Configuration.Tests/TeCLI.Extensions.Configuration.Tests.csproj
```

### Running Examples

```bash
# Run the simple example
dotnet run --project examples/TeCLI.Example.Simple -- greet John

# Run with help
dotnet run --project examples/TeCLI.Example.Simple -- --help
```

### Running Benchmarks

```bash
dotnet run --project TeCLI.Benchmarks -c Release
```

## Project Structure

```
TeCLI/
├── TeCLI/                    # Core library with source generators and analyzers
│   ├── Generators/           # Roslyn source generators
│   └── *.cs                  # Analyzers (CLI001-CLI032)
├── TeCLI.Tools/              # Shared utilities for code generation
├── extensions/               # Extension packages
│   ├── TeCLI.Extensions.Configuration/      # Config file support
│   ├── TeCLI.Extensions.Console/            # Console enhancements
│   ├── TeCLI.Extensions.DependencyInjection/# Microsoft DI
│   ├── TeCLI.Extensions.Localization/       # i18n support
│   ├── TeCLI.Extensions.Output/             # Output formatting
│   ├── TeCLI.Extensions.Shell/              # Interactive shell
│   └── TeCLI.Extensions.Testing/            # Testing utilities
├── examples/                 # Example applications
├── tests/                    # Test projects
├── docs/                     # Documentation and migration guides
└── .github/                  # GitHub configuration
```

### Key Components

| Component | Description |
|-----------|-------------|
| `TeCLI` | Core library with source generators and 32 Roslyn analyzers |
| `TeCLI.Tools` | Shared utilities for Roslyn code generation |
| `TeCLI.Extensions.*` | Optional feature extensions |
| `TeCLI.*.Analyzers` | Extension-specific analyzers |

## Making Changes

### Branch Naming

Use descriptive branch names with prefixes:

- `feature/` - New features (e.g., `feature/add-yaml-config`)
- `fix/` - Bug fixes (e.g., `fix/null-reference-parser`)
- `docs/` - Documentation changes (e.g., `docs/update-readme`)
- `refactor/` - Code refactoring (e.g., `refactor/simplify-generator`)
- `test/` - Test additions/changes (e.g., `test/add-shell-tests`)
- `chore/` - Maintenance tasks (e.g., `chore/update-dependencies`)

### Commit Messages

Follow conventional commit format:

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `refactor`: Code refactoring
- `test`: Adding/updating tests
- `chore`: Maintenance tasks
- `perf`: Performance improvements

Examples:
```
feat(config): add TOML configuration file support

fix(generator): resolve null reference in argument parsing

docs(readme): update installation instructions for .NET 8
```

## Testing

### Test Requirements

- All new features must include unit tests
- Bug fixes should include a regression test
- Maintain or improve code coverage
- All tests must pass before submitting a PR

### Writing Tests

```csharp
using Xunit;
using TeCLI.Testing;

public class MyFeatureTests
{
    [Fact]
    public void Feature_WhenCondition_ShouldBehavior()
    {
        // Arrange
        var sut = new MyFeature();

        // Act
        var result = sut.DoSomething();

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("input1", "expected1")]
    [InlineData("input2", "expected2")]
    public void Feature_WithVariousInputs_ShouldReturnExpected(string input, string expected)
    {
        // Test implementation
    }
}
```

### Using TeCLI.Extensions.Testing

```csharp
using TeCLI.Testing;

[Fact]
public async Task Command_WithValidArgs_ShouldExecute()
{
    var result = await TestCommandRunner.RunAsync<MyCommand>("action", "--option", "value");

    Assert.Equal(0, result.ExitCode);
    Assert.Contains("expected output", result.Output);
}
```

## Pull Request Process

1. **Update your fork** with the latest upstream changes:
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Create a feature branch**:
   ```bash
   git checkout -b feature/my-awesome-feature
   ```

3. **Make your changes** following the coding standards

4. **Write/update tests** for your changes

5. **Run tests locally**:
   ```bash
   dotnet test TeCLI.sln
   ```

6. **Push to your fork**:
   ```bash
   git push origin feature/my-awesome-feature
   ```

7. **Create a Pull Request** using the PR template

### PR Checklist

- [ ] Code follows project style guidelines
- [ ] Tests added/updated and passing
- [ ] Documentation updated if needed
- [ ] No compiler warnings
- [ ] Commit messages follow conventions
- [ ] PR description explains the changes

## Coding Standards

### C# Style Guidelines

- Use C# 12 features where appropriate
- Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful names for variables, methods, and classes
- Prefer `var` when the type is obvious
- Use expression-bodied members for simple operations
- Add XML documentation for public APIs

### Code Examples

```csharp
// Good: Clear naming, appropriate use of var
public async Task<CommandResult> ExecuteCommandAsync(string[] args)
{
    var parser = new ArgumentParser(args);
    var options = parser.Parse<CommandOptions>();

    return await ProcessOptionsAsync(options);
}

// Good: XML documentation for public API
/// <summary>
/// Parses command-line arguments into the specified type.
/// </summary>
/// <typeparam name="T">The type to parse arguments into.</typeparam>
/// <param name="args">The command-line arguments.</param>
/// <returns>The parsed options object.</returns>
public T Parse<T>(string[] args) where T : class, new()
{
    // Implementation
}
```

### Analyzer Compliance

TeCLI includes 32 analyzers. Ensure your code doesn't trigger any warnings:

```bash
# Build with all analyzers enabled
dotnet build --configuration Debug /p:TreatWarningsAsErrors=true
```

## Documentation

### When to Update Documentation

- Adding new features or APIs
- Changing existing behavior
- Fixing bugs that affect documented behavior
- Adding new examples

### Documentation Locations

| Type | Location |
|------|----------|
| Main documentation | `README.md` |
| API documentation | XML comments in source files |
| Migration guides | `docs/migration/` |
| Extension docs | `extensions/*/README.md` |
| Examples | `examples/*/README.md` |

### XML Documentation

All public APIs should have XML documentation:

```csharp
/// <summary>
/// Represents a CLI command with actions.
/// </summary>
/// <remarks>
/// Commands are the top-level entry points for CLI applications.
/// </remarks>
/// <example>
/// <code>
/// [Command("deploy")]
/// public class DeployCommand
/// {
///     [Primary]
///     public void Execute() { }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class)]
public class CommandAttribute : Attribute
{
    // Implementation
}
```

## Issue Guidelines

### Bug Reports

When reporting bugs, please include:

- TeCLI version
- .NET version
- Operating system
- Minimal reproduction code
- Expected vs actual behavior
- Stack trace (if applicable)

### Feature Requests

When requesting features:

- Describe the use case
- Explain how it would benefit users
- Consider potential implementation approaches
- Check if similar requests exist

### Questions

For questions:

- Check existing documentation first
- Search closed issues for similar questions
- Use GitHub Discussions for general questions

## Getting Help

- **Documentation**: Check the [README](README.md) and [docs/](docs/) folder
- **Issues**: Search existing issues or create a new one
- **Discussions**: Use GitHub Discussions for questions
- **Security**: Report vulnerabilities via [Security Advisories](https://github.com/tyevco/TeCLI/security/advisories)

## Recognition

Contributors are recognized in:
- Release notes via Release Drafter
- Git commit history
- Security Hall of Fame (for security researchers)

Thank you for contributing to TeCLI!
