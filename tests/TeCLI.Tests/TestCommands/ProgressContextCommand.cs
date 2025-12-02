using TeCLI.Attributes;
using TeCLI.Console;

namespace TeCLI.Tests.TestCommands;

/// <summary>
/// Test command for progress context auto-injection support
/// </summary>
[Command("progress", Description = "Test command with progress context")]
public class ProgressContextCommand
{
    public static bool WasCalled { get; private set; }
    public static IProgressContext? CapturedProgressContext { get; private set; }
    public static IProgressBar? CapturedProgressBar { get; private set; }
    public static ISpinner? CapturedSpinner { get; private set; }
    public static string? CapturedMessage { get; private set; }

    [Action("basic")]
    public void TestBasicProgressContext(IProgressContext progress)
    {
        WasCalled = true;
        CapturedProgressContext = progress;
    }

    [Action("bar")]
    public void TestProgressBar(IProgressContext progress)
    {
        WasCalled = true;
        CapturedProgressContext = progress;

        using var bar = progress.CreateProgressBar("Downloading...", 100);
        CapturedProgressBar = bar;
        bar.Increment(50);
        bar.Complete("Done!");
    }

    [Action("spinner")]
    public void TestSpinner(IProgressContext progress)
    {
        WasCalled = true;
        CapturedProgressContext = progress;

        using var spinner = progress.CreateSpinner("Processing...");
        CapturedSpinner = spinner;
        spinner.Update("Still processing...");
        spinner.Success("Completed!");
    }

    [Action("with-args")]
    public void TestWithOtherArguments(
        [Argument] string filename,
        IProgressContext progress,
        [Option("verbose", ShortName = 'v')] bool verbose = false)
    {
        WasCalled = true;
        CapturedProgressContext = progress;
        CapturedMessage = $"Processing {filename}, verbose={verbose}";

        using var bar = progress.CreateProgressBar($"Processing {filename}...");
        CapturedProgressBar = bar;
        bar.Report(100);
        bar.Complete();
    }

    [Action("indicator")]
    public void TestProgressIndicator(IProgressContext progress)
    {
        WasCalled = true;
        CapturedProgressContext = progress;

        using var indicator = progress.CreateProgress("Loading...");
        indicator.Report(50, "Halfway there...");
        indicator.Complete("All done!");
    }

    public static void Reset()
    {
        WasCalled = false;
        CapturedProgressContext = null;
        CapturedProgressBar = null;
        CapturedSpinner = null;
        CapturedMessage = null;
    }
}
