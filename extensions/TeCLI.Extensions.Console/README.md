# TeCLI.Extensions.Console

Console styling and output utilities for TeCLI applications.

## Overview

TeCLI.Extensions.Console provides comprehensive console styling capabilities including colored text output, progress indicators, spinners, and terminal capability detection. Supports both ANSI escape codes and legacy console color APIs with automatic capability detection.

## Installation

```bash
dotnet add package TeCLI.Extensions.Console
```

## Features

- Colored text output with ANSI and Console color support
- Progress indicators with customizable appearance
- Animated spinners for indeterminate operations
- Terminal capability detection (color support, ANSI support)
- Inline string styling extensions
- NO_COLOR environment variable support

## Quick Start

```csharp
using TeCLI.Console;

var console = new StyledConsole();

// Colored output
console.WriteSuccess("Operation completed successfully");
console.WriteWarning("Cache is stale");
console.WriteErrorMessage("Connection failed");
console.WriteInfo("Processing items...");

// Progress indicator
using var progress = console.CreateProgress("Downloading");
for (int i = 0; i <= 100; i += 10)
{
    progress.Report(i, $"Progress: {i}%");
    await Task.Delay(100);
}
progress.Complete("Download complete!");

// Spinner for indeterminate operations
using var spinner = console.CreateSpinner("Processing data");
await DoWork();
spinner.Success("Operation completed");
```

## Key Classes

| Class | Purpose |
|-------|---------|
| `StyledConsole` | Main console output with color and ANSI support |
| `IConsoleOutput` | Interface for console output operations |
| `ConsoleStyle` | Text styling (colors, bold, italic, underline) |
| `IProgressIndicator` | Progress bar interface |
| `ISpinner` | Animated spinner interface |
| `TerminalCapabilities` | Detects terminal color and ANSI support |

## Inline Styling

```csharp
using TeCLI.Console;

// Use extension methods for inline styling
Console.WriteLine("Status: " + "OK".Green().Bold());
Console.WriteLine("Error: " + "Failed".Red().Underline());
```

## Documentation

For full documentation and examples, see the [TeCLI repository](https://github.com/tyevco/TeCLI).

## License

MIT License - see [LICENSE](https://github.com/tyevco/TeCLI/blob/main/LICENSE) for details.
