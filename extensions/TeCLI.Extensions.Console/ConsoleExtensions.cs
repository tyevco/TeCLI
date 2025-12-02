using System;

namespace TeCLI.Console;

/// <summary>
/// Extension methods for console output operations.
/// </summary>
public static class ConsoleExtensions
{
    private static StyledConsole? _default;

    /// <summary>
    /// Gets the default styled console instance.
    /// </summary>
    public static StyledConsole Default => _default ??= new StyledConsole();

    /// <summary>
    /// Sets the default styled console instance.
    /// </summary>
    /// <param name="console">The console to use as default.</param>
    public static void SetDefault(StyledConsole console)
    {
        _default = console ?? throw new ArgumentNullException(nameof(console));
    }

    /// <summary>
    /// Writes a success message to the default console.
    /// </summary>
    /// <param name="text">The message to write.</param>
    public static void WriteSuccess(string? text) => Default.WriteSuccess(text);

    /// <summary>
    /// Writes a warning message to the default console.
    /// </summary>
    /// <param name="text">The message to write.</param>
    public static void WriteWarning(string? text) => Default.WriteWarning(text);

    /// <summary>
    /// Writes an error message to the default console.
    /// </summary>
    /// <param name="text">The message to write.</param>
    public static void WriteError(string? text) => Default.WriteErrorMessage(text);

    /// <summary>
    /// Writes an info message to the default console.
    /// </summary>
    /// <param name="text">The message to write.</param>
    public static void WriteInfo(string? text) => Default.WriteInfo(text);

    /// <summary>
    /// Writes a debug message to the default console.
    /// </summary>
    /// <param name="text">The message to write.</param>
    public static void WriteDebug(string? text) => Default.WriteDebug(text);

    /// <summary>
    /// Creates a progress indicator using the default console.
    /// </summary>
    /// <param name="message">The initial message to display.</param>
    /// <returns>A progress indicator.</returns>
    public static IProgressIndicator CreateProgress(string? message = null) => Default.CreateProgress(message);

    /// <summary>
    /// Creates a spinner using the default console.
    /// </summary>
    /// <param name="message">The message to display alongside the spinner.</param>
    /// <returns>A spinner.</returns>
    public static ISpinner CreateSpinner(string? message = null) => Default.CreateSpinner(message);
}

/// <summary>
/// Extension methods for styled text building.
/// </summary>
public static class StyledTextExtensions
{
    /// <summary>
    /// Creates a styled text from a string with the specified color.
    /// </summary>
    /// <param name="text">The text to style.</param>
    /// <param name="color">The foreground color.</param>
    /// <returns>The styled text.</returns>
    public static string Colorize(this string text, ConsoleColor color)
    {
        if (!TerminalCapabilities.SupportsAnsi)
            return text;

        return AnsiCodes.Stylize(text, ConsoleStyle.Color(color));
    }

    /// <summary>
    /// Creates a styled text from a string with the specified style.
    /// </summary>
    /// <param name="text">The text to style.</param>
    /// <param name="style">The style to apply.</param>
    /// <returns>The styled text.</returns>
    public static string Stylize(this string text, ConsoleStyle style)
    {
        if (!TerminalCapabilities.SupportsAnsi)
            return text;

        return AnsiCodes.Stylize(text, style);
    }

    /// <summary>
    /// Makes the text bold.
    /// </summary>
    /// <param name="text">The text to style.</param>
    /// <returns>The styled text.</returns>
    public static string Bold(this string text)
    {
        if (!TerminalCapabilities.SupportsAnsi)
            return text;

        return AnsiCodes.Stylize(text, ConsoleStyle.BoldStyle);
    }

    /// <summary>
    /// Underlines the text.
    /// </summary>
    /// <param name="text">The text to style.</param>
    /// <returns>The styled text.</returns>
    public static string Underline(this string text)
    {
        if (!TerminalCapabilities.SupportsAnsi)
            return text;

        return AnsiCodes.Stylize(text, ConsoleStyle.UnderlineStyle);
    }

    /// <summary>
    /// Colors the text red (error style).
    /// </summary>
    /// <param name="text">The text to style.</param>
    /// <returns>The styled text.</returns>
    public static string Red(this string text) => text.Colorize(ConsoleColor.Red);

    /// <summary>
    /// Colors the text green (success style).
    /// </summary>
    /// <param name="text">The text to style.</param>
    /// <returns>The styled text.</returns>
    public static string Green(this string text) => text.Colorize(ConsoleColor.Green);

    /// <summary>
    /// Colors the text yellow (warning style).
    /// </summary>
    /// <param name="text">The text to style.</param>
    /// <returns>The styled text.</returns>
    public static string Yellow(this string text) => text.Colorize(ConsoleColor.Yellow);

    /// <summary>
    /// Colors the text cyan (info style).
    /// </summary>
    /// <param name="text">The text to style.</param>
    /// <returns>The styled text.</returns>
    public static string Cyan(this string text) => text.Colorize(ConsoleColor.Cyan);

    /// <summary>
    /// Colors the text dark gray (debug style).
    /// </summary>
    /// <param name="text">The text to style.</param>
    /// <returns>The styled text.</returns>
    public static string Gray(this string text) => text.Colorize(ConsoleColor.DarkGray);

    /// <summary>
    /// Colors the text blue.
    /// </summary>
    /// <param name="text">The text to style.</param>
    /// <returns>The styled text.</returns>
    public static string Blue(this string text) => text.Colorize(ConsoleColor.Blue);

    /// <summary>
    /// Colors the text magenta.
    /// </summary>
    /// <param name="text">The text to style.</param>
    /// <returns>The styled text.</returns>
    public static string Magenta(this string text) => text.Colorize(ConsoleColor.Magenta);

    /// <summary>
    /// Colors the text white.
    /// </summary>
    /// <param name="text">The text to style.</param>
    /// <returns>The styled text.</returns>
    public static string White(this string text) => text.Colorize(ConsoleColor.White);
}
