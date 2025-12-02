# TeCLI.Extensions.Testing

Testing utilities for TeCLI applications.

## Overview

TeCLI.Extensions.Testing provides comprehensive testing utilities for TeCLI applications, enabling unit and integration testing of CLI commands with captured I/O, mock input support, and fluent assertion methods. Works with any test framework (xUnit, NUnit, MSTest).

## Installation

```bash
dotnet add package TeCLI.Extensions.Testing
```

## Features

- Command execution in isolated test environment
- Captured stdout/stderr output
- Mock console input for interactive commands
- Fluent assertion API
- Exit code verification
- Exception testing
- Execution time assertions

## Quick Start

```csharp
using TeCLI.Testing;

// Create test host
var host = new CommandTestHost<CommandDispatcher>();

// Execute command
var result = await host.ExecuteAsync(new[] { "greet", "--name", "Alice" });

// Assert results with fluent API
result.ShouldSucceed()
    .ShouldContainOutput("Hello Alice")
    .ShouldHaveNoError();
```

## ArgumentBuilder

Build command-line arguments fluently:

```csharp
var args = ArgumentBuilder.Command("deploy")
    .Action("staging")
    .Option("environment", "prod")
    .Flag("force")
    .Build();

var result = await host.ExecuteAsync(args);

// Or parse a command string
var parsed = ArgumentBuilder.Parse("deploy staging --environment prod");
```

## Mock Input

Test interactive commands with mock input:

```csharp
var result = await host.ExecuteWithInputAsync(
    new[] { "configure" },
    new[] { "admin", "password123" });
```

## Assertions

```csharp
// Success/failure
result.ShouldSucceed();
result.ShouldFail("Expected failure");

// Exit codes
result.ShouldHaveExitCode(0);
result.ShouldHaveExitCode(2);

// Output matching
result.ShouldContainOutput("success");
result.ShouldMatchOutput(@"Error: \d+");
result.ShouldHaveNoError();

// Exceptions
result.ShouldThrow<InvalidOperationException>();

// Timing
result.ShouldCompleteWithin(TimeSpan.FromSeconds(5));
```

## Key Classes

| Class | Purpose |
|-------|---------|
| `CommandTestHost<T>` | Test host for executing commands |
| `CommandResult` | Captured output, error, exit code, exception |
| `CommandResultAssertions` | Fluent assertion extension methods |
| `ArgumentBuilder` | Fluent argument construction |
| `TestConsole` | Mock console for I/O capture |

## Documentation

For full documentation and examples, see the [TeCLI repository](https://github.com/tyevco/TeCLI).

## License

MIT License - see [LICENSE](https://github.com/tyevco/TeCLI/blob/main/LICENSE) for details.
