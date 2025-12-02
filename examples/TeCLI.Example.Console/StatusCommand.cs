using TeCLI.Attributes;
using TeCLI.Console;

namespace TeCLI.Example.Console;

/// <summary>
/// Demonstrates colored console output using TeCLI.Extensions.Console.
/// Shows how to use WriteSuccess, WriteWarning, WriteError, and other styled output methods.
/// </summary>
[Command("status", Description = "Display status messages with colors")]
public class StatusCommand
{
    private readonly IConsoleOutput _console;

    public StatusCommand()
    {
        _console = new StyledConsole();
    }

    /// <summary>
    /// Show all types of status messages
    /// </summary>
    [Primary]
    [Action("show", Description = "Display various status message types")]
    public void Show()
    {
        _console.WriteLine("=== Status Message Examples ===", ConsoleStyle.BoldStyle);
        _console.WriteLine();

        _console.WriteSuccess("Operation completed successfully");
        _console.WriteWarning("Cache is stale - consider refreshing");
        _console.WriteErrorMessage("Connection failed - check network settings");
        _console.WriteInfo("Processing 42 items...");
        _console.WriteDebug("Debug: Memory usage at 45%");

        _console.WriteLine();
        _console.WriteLine("=== Custom Styled Output ===", ConsoleStyle.BoldStyle);
        _console.WriteLine();

        // Custom styles
        _console.WriteLine("Bold text", ConsoleStyle.BoldStyle);
        _console.WriteLine("Underlined text", ConsoleStyle.UnderlineStyle);
        _console.WriteLine("Cyan info text", ConsoleStyle.Info);

        // Combined styles
        var boldGreen = ConsoleStyle.Success.WithBold();
        _console.WriteLine("Bold green success!", boldGreen);

        var underlineYellow = ConsoleStyle.Warning.WithUnderline();
        _console.WriteLine("Underlined warning!", underlineYellow);
    }

    /// <summary>
    /// Demonstrate inline string styling
    /// </summary>
    [Action("inline", Description = "Show inline string styling")]
    public void InlineStyles()
    {
        _console.WriteLine("=== Inline String Styling ===", ConsoleStyle.BoldStyle);
        _console.WriteLine();

        // Using extension methods on strings
        System.Console.WriteLine("This is " + "red".Red() + " text");
        System.Console.WriteLine("This is " + "green".Green() + " and " + "blue".Blue() + " text");
        System.Console.WriteLine("This is " + "bold".Bold() + " and " + "underlined".Underline());
        System.Console.WriteLine("Status: " + "OK".Green().Bold());
        System.Console.WriteLine("Warning: " + "Deprecated API".Yellow());
        System.Console.WriteLine("Error: " + "File not found".Red().Bold());
    }

    /// <summary>
    /// Show all available colors
    /// </summary>
    [Action("colors", Description = "Display all available colors")]
    public void ShowColors()
    {
        _console.WriteLine("=== Available Colors ===", ConsoleStyle.BoldStyle);
        _console.WriteLine();

        var colors = new[]
        {
            (ConsoleColor.Black, "Black"),
            (ConsoleColor.DarkBlue, "DarkBlue"),
            (ConsoleColor.DarkGreen, "DarkGreen"),
            (ConsoleColor.DarkCyan, "DarkCyan"),
            (ConsoleColor.DarkRed, "DarkRed"),
            (ConsoleColor.DarkMagenta, "DarkMagenta"),
            (ConsoleColor.DarkYellow, "DarkYellow"),
            (ConsoleColor.Gray, "Gray"),
            (ConsoleColor.DarkGray, "DarkGray"),
            (ConsoleColor.Blue, "Blue"),
            (ConsoleColor.Green, "Green"),
            (ConsoleColor.Cyan, "Cyan"),
            (ConsoleColor.Red, "Red"),
            (ConsoleColor.Magenta, "Magenta"),
            (ConsoleColor.Yellow, "Yellow"),
            (ConsoleColor.White, "White"),
        };

        foreach (var (color, name) in colors)
        {
            _console.WriteLine($"  {name}", ConsoleStyle.Color(color));
        }
    }

    /// <summary>
    /// Check terminal capabilities
    /// </summary>
    [Action("capabilities", Description = "Show terminal color capabilities")]
    public void ShowCapabilities()
    {
        _console.WriteLine("=== Terminal Capabilities ===", ConsoleStyle.BoldStyle);
        _console.WriteLine();

        var colorSupport = TerminalCapabilities.SupportsColor ? "Yes".Green() : "No".Red();
        var ansiSupport = TerminalCapabilities.SupportsAnsi ? "Yes".Green() : "No".Red();

        System.Console.WriteLine($"  Supports Color: {colorSupport}");
        System.Console.WriteLine($"  Supports ANSI:  {ansiSupport}");

        _console.WriteLine();

        // Check environment variables
        var noColor = Environment.GetEnvironmentVariable("NO_COLOR");
        var forceColor = Environment.GetEnvironmentVariable("FORCE_COLOR");
        var term = Environment.GetEnvironmentVariable("TERM");

        _console.WriteDebug($"  NO_COLOR:    {noColor ?? "(not set)"}");
        _console.WriteDebug($"  FORCE_COLOR: {forceColor ?? "(not set)"}");
        _console.WriteDebug($"  TERM:        {term ?? "(not set)"}");
    }
}
