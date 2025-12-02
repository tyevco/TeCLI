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
- **Auto-injected `IProgressContext`** for action methods
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

## Auto-Injected Progress Context

When using `IProgressContext` as a parameter in your action methods, TeCLI automatically injects a ready-to-use progress context. This provides a clean way to add progress UI to your CLI commands without manual setup.

```csharp
using TeCLI.Attributes;
using TeCLI.Console;

[Command("file")]
public class FileCommand
{
    [Action("download")]
    public async Task Download(
        [Argument] string url,
        IProgressContext progress)  // Auto-injected by framework
    {
        using var bar = progress.CreateProgressBar("Downloading...", totalBytes);

        await foreach (var chunk in DownloadChunksAsync(url))
        {
            bar.Increment(chunk.Length);
        }
        bar.Complete("Download complete!");
    }

    [Action("process")]
    public async Task Process(
        [Argument] string[] files,
        IProgressContext progress)
    {
        using var spinner = progress.CreateSpinner("Processing...");

        foreach (var file in files)
        {
            spinner.Update($"Processing {file}...");
            await ProcessFileAsync(file);
        }
        spinner.Success("All files processed!");
    }
}
```

### IProgressContext Interface

| Method | Description |
|--------|-------------|
| `CreateProgressBar(message, maxValue)` | Creates a progress bar with absolute value support |
| `CreateSpinner(message)` | Creates an animated spinner for indeterminate operations |
| `CreateProgress(message)` | Creates a percentage-based progress indicator |
| `Console` | Gets the underlying `IConsoleOutput` for styled output |

### IProgressBar Interface

| Member | Description |
|--------|-------------|
| `Value` | Current progress value |
| `MaxValue` | Maximum value (100% complete) |
| `Message` | Status message |
| `Increment(amount)` | Add to the current value |
| `Report(value)` | Set absolute progress value |
| `Report(value, message)` | Set value and message |
| `Complete(message)` | Mark as successfully completed |
| `Fail(message)` | Mark as failed |

## Key Classes

| Class | Purpose |
|-------|---------|
| `StyledConsole` | Main console output with color and ANSI support |
| `IConsoleOutput` | Interface for console output operations |
| `ConsoleStyle` | Text styling (colors, bold, italic, underline) |
| `IProgressContext` | Auto-injectable progress context for actions |
| `IProgressBar` | Progress bar with increment support |
| `IProgressIndicator` | Percentage-based progress interface |
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
