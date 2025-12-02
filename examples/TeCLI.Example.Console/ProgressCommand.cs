using TeCLI.Attributes;
using TeCLI.Console;

namespace TeCLI.Example.Console;

/// <summary>
/// Demonstrates progress indicators and spinners using TeCLI.Extensions.Console.
/// Shows how to display progress bars and activity spinners for long-running operations.
/// </summary>
[Command("progress", Description = "Demonstrate progress indicators and spinners")]
public class ProgressCommand
{
    private readonly IConsoleOutput _console;

    public ProgressCommand()
    {
        _console = new StyledConsole();
    }

    /// <summary>
    /// Show a progress bar
    /// </summary>
    [Primary]
    [Action("bar", Description = "Display a progress bar")]
    public async Task ProgressBar(
        [Option("duration", ShortName = 'd', Description = "Duration in seconds")] int duration = 3)
    {
        _console.WriteLine("Starting download simulation...", ConsoleStyle.Info);
        _console.WriteLine();

        using var progress = _console.CreateProgress("Downloading");

        var steps = 20;
        var delay = (duration * 1000) / steps;

        for (int i = 0; i <= steps; i++)
        {
            var percentage = (i * 100.0) / steps;
            progress.Report(percentage, $"Downloading file.zip ({i * 5}MB / {steps * 5}MB)");
            await Task.Delay(delay);
        }

        progress.Complete("Download complete!");

        _console.WriteLine();
        _console.WriteSuccess("File saved successfully.");
    }

    /// <summary>
    /// Show a spinner
    /// </summary>
    [Action("spinner", Description = "Display a spinner")]
    public async Task ShowSpinner(
        [Option("duration", ShortName = 'd', Description = "Duration in seconds")] int duration = 3,
        [Option("fail", ShortName = 'f', Description = "Simulate a failure")] bool fail = false)
    {
        _console.WriteLine("Starting operation...", ConsoleStyle.Info);
        _console.WriteLine();

        using var spinner = _console.CreateSpinner("Processing data");

        // Simulate work with status updates
        var phases = new[] { "Reading input", "Parsing data", "Validating", "Processing", "Finalizing" };
        var phaseDelay = (duration * 1000) / phases.Length;

        foreach (var phase in phases)
        {
            spinner.Update(phase);
            await Task.Delay(phaseDelay);
        }

        if (fail)
        {
            spinner.Fail("Operation failed: Connection timeout");
            _console.WriteLine();
            _console.WriteErrorMessage("Please check your network connection and try again.");
        }
        else
        {
            spinner.Success("Operation completed successfully");
            _console.WriteLine();
            _console.WriteSuccess("All data processed!");
        }
    }

    /// <summary>
    /// Show multiple progress indicators
    /// </summary>
    [Action("multi", Description = "Multiple progress operations")]
    public async Task MultipleProgress()
    {
        _console.WriteLine("Processing multiple items...", ConsoleStyle.Info);
        _console.WriteLine();

        var items = new[] { "config.json", "data.csv", "image.png", "report.pdf" };

        foreach (var item in items)
        {
            using var spinner = _console.CreateSpinner($"Processing {item}");
            await Task.Delay(500 + Random.Shared.Next(500));

            if (item == "image.png")
            {
                spinner.Warn($"Skipped {item} (already up to date)");
            }
            else
            {
                spinner.Success($"Processed {item}");
            }
        }

        _console.WriteLine();
        _console.WriteSuccess($"Completed processing {items.Length} items.");
    }

    /// <summary>
    /// Demonstrate different spinner outcomes
    /// </summary>
    [Action("outcomes", Description = "Show different spinner outcomes")]
    public async Task SpinnerOutcomes()
    {
        _console.WriteLine("Demonstrating spinner outcomes...", ConsoleStyle.Info);
        _console.WriteLine();

        // Success
        using (var spinner = _console.CreateSpinner("Task 1: Connecting"))
        {
            await Task.Delay(800);
            spinner.Success("Connected to server");
        }

        // Warning
        using (var spinner = _console.CreateSpinner("Task 2: Loading cache"))
        {
            await Task.Delay(800);
            spinner.Warn("Using stale cache (server unavailable)");
        }

        // Info
        using (var spinner = _console.CreateSpinner("Task 3: Checking updates"))
        {
            await Task.Delay(800);
            spinner.Info("No updates available");
        }

        // Failure
        using (var spinner = _console.CreateSpinner("Task 4: Validating license"))
        {
            await Task.Delay(800);
            spinner.Fail("License expired");
        }

        _console.WriteLine();
        _console.WriteLine("All tasks completed.", ConsoleStyle.BoldStyle);
    }

    /// <summary>
    /// Demonstrate auto-injected IProgressContext
    /// </summary>
    [Action("context", Description = "Demonstrate auto-injected IProgressContext")]
    public async Task ProgressContext(
        IProgressContext progress,
        [Option("steps", ShortName = 's', Description = "Number of steps")] int steps = 10)
    {
        // IProgressContext is automatically injected by the framework
        // No manual setup required!
        progress.Console.WriteLine("Demonstrating auto-injected IProgressContext...", ConsoleStyle.Info);
        progress.Console.WriteLine();

        // Progress bar with increment support
        progress.Console.WriteLine("Progress bar with Increment():", ConsoleStyle.Debug);
        using (var bar = progress.CreateProgressBar("Processing items", steps))
        {
            for (int i = 0; i < steps; i++)
            {
                bar.Message = $"Processing item {i + 1}/{steps}";
                bar.Increment();
                await Task.Delay(200);
            }
            bar.Complete("All items processed!");
        }

        progress.Console.WriteLine();

        // Spinner for indeterminate progress
        progress.Console.WriteLine("Spinner for async operations:", ConsoleStyle.Debug);
        using (var spinner = progress.CreateSpinner("Finalizing"))
        {
            await Task.Delay(1000);
            spinner.Update("Saving results...");
            await Task.Delay(800);
            spinner.Success("Results saved!");
        }

        progress.Console.WriteLine();
        progress.Console.WriteSuccess("IProgressContext demo complete!");
    }

    /// <summary>
    /// Custom progress bar configuration
    /// </summary>
    [Action("custom", Description = "Show custom progress bar styles")]
    public async Task CustomProgress()
    {
        _console.WriteLine("Custom progress bar styles:", ConsoleStyle.Info);
        _console.WriteLine();

        // Standard style
        _console.WriteLine("Standard:", ConsoleStyle.Debug);
        using (var progress = (ProgressIndicator)_console.CreateProgress())
        {
            for (int i = 0; i <= 100; i += 10)
            {
                progress.Report(i);
                await Task.Delay(50);
            }
            progress.Complete();
        }

        // Hash style
        _console.WriteLine("Hash style:", ConsoleStyle.Debug);
        using (var progress = (ProgressIndicator)_console.CreateProgress())
        {
            progress.FilledChar = '#';
            progress.EmptyChar = '-';
            for (int i = 0; i <= 100; i += 10)
            {
                progress.Report(i);
                await Task.Delay(50);
            }
            progress.Complete();
        }

        // Wide bar
        _console.WriteLine("Wide bar (50 chars):", ConsoleStyle.Debug);
        using (var progress = (ProgressIndicator)_console.CreateProgress())
        {
            progress.BarWidth = 50;
            for (int i = 0; i <= 100; i += 10)
            {
                progress.Report(i);
                await Task.Delay(50);
            }
            progress.Complete();
        }

        _console.WriteLine();
        _console.WriteSuccess("All styles demonstrated!");
    }
}
