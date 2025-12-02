# Code Coverage Guide

This document explains how to measure and analyze code coverage for TeCLI.

## Quick Start

```bash
./run-coverage.sh
```

This will:
1. Run all tests with coverage collection
2. Generate a Cobertura XML coverage report
3. Optionally generate an HTML report for browser viewing

## Manual Coverage Collection

### Using .NET CLI

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Results will be in: TeCLI.Tests/TestResults/{guid}/coverage.cobertura.xml
```

### Generate HTML Report

```bash
# Install reportgenerator tool (once)
dotnet tool install --global dotnet-reportgenerator-globaltool

# Generate report
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coverage/html" \
  -reporttypes:Html

# Open in browser
open coverage/html/index.html  # macOS
xdg-open coverage/html/index.html  # Linux
```

## Coverage Targets

We aim for:
- **Project Coverage**: 70% minimum
- **New Code (Patch)**: 70% minimum

### What's Covered

- ✅ TeCLI (source generators, analyzers, and embedded attributes)
- ✅ TeCLI.Tools (shared utilities)
- ✅ TeCLI.Extensions.DependencyInjection
- ✅ TeCLI.Extensions.Testing
- ✅ TeCLI.Extensions.Console
- ✅ TeCLI.Extensions.Autofac
- ✅ TeCLI.Extensions.SimpleInjector
- ✅ TeCLI.Extensions.Jab
- ✅ TeCLI.Extensions.PureDI

### What's Excluded

- ❌ TeCLI.Example.* (sample code)
- ❌ TeCLI.Benchmarks (performance tests)
- ❌ TeCLI.Tests, TeCLI.Extensions.*.Tests (test code itself)
- ❌ Generated files (*.g.cs, *.Designer.cs)

## CI/CD Integration

The coverage configuration in `codecov.yml` is set up for automatic uploads to codecov.io during CI builds.

### GitHub Actions Example

```yaml
- name: Test with coverage
  run: dotnet test --collect:"XPlat Code Coverage"

- name: Upload coverage to Codecov
  uses: codecov/codecov-action@v3
  with:
    files: ./coverage/coverage.cobertura.xml
```

## Interpreting Coverage Results

### Good Coverage Indicators
- ✅ All public APIs have tests
- ✅ Error handling paths are tested
- ✅ Edge cases are covered

### Areas Typically Needing Coverage
- ⚠️ Source generator logic (harder to test)
- ⚠️ Analyzer diagnostic rules
- ⚠️ Complex conditional logic

## Improving Coverage

1. **Identify Gaps**: Use the HTML report to find uncovered lines
2. **Write Tests**: Add integration tests for uncovered scenarios
3. **Verify**: Re-run coverage to confirm improvement

## Tools

- **dotnet-coverage**: Built-in .NET code coverage tool
- **ReportGenerator**: Converts coverage to human-readable HTML
- **Codecov.io**: Online coverage tracking and PR integration
