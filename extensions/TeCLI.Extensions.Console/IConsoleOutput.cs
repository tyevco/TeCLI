using System;

namespace TeCLI.Console;

/// <summary>
/// Provides an abstraction for console output operations with support for styled text.
/// Use this interface to enable testable console output and custom rendering.
/// </summary>
public interface IConsoleOutput
{
    /// <summary>
    /// Gets a value indicating whether color output is supported and enabled.
    /// </summary>
    bool SupportsColor { get; }

    /// <summary>
    /// Gets a value indicating whether ANSI escape codes are supported.
    /// </summary>
    bool SupportsAnsi { get; }

    /// <summary>
    /// Writes text to the console without a trailing newline.
    /// </summary>
    /// <param name="text">The text to write.</param>
    void Write(string? text);

    /// <summary>
    /// Writes text to the console followed by a newline.
    /// </summary>
    /// <param name="text">The text to write.</param>
    void WriteLine(string? text);

    /// <summary>
    /// Writes a newline to the console.
    /// </summary>
    void WriteLine();

    /// <summary>
    /// Writes text to the error stream without a trailing newline.
    /// </summary>
    /// <param name="text">The text to write.</param>
    void WriteError(string? text);

    /// <summary>
    /// Writes text to the error stream followed by a newline.
    /// </summary>
    /// <param name="text">The text to write.</param>
    void WriteErrorLine(string? text);

    /// <summary>
    /// Writes styled text to the console without a trailing newline.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="style">The style to apply.</param>
    void Write(string? text, ConsoleStyle style);

    /// <summary>
    /// Writes styled text to the console followed by a newline.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="style">The style to apply.</param>
    void WriteLine(string? text, ConsoleStyle style);

    /// <summary>
    /// Writes a success message (green text by default).
    /// </summary>
    /// <param name="text">The message to write.</param>
    void WriteSuccess(string? text);

    /// <summary>
    /// Writes a warning message (yellow text by default).
    /// </summary>
    /// <param name="text">The message to write.</param>
    void WriteWarning(string? text);

    /// <summary>
    /// Writes an error message to stderr (red text by default).
    /// </summary>
    /// <param name="text">The message to write.</param>
    void WriteErrorMessage(string? text);

    /// <summary>
    /// Writes an info message (cyan text by default).
    /// </summary>
    /// <param name="text">The message to write.</param>
    void WriteInfo(string? text);

    /// <summary>
    /// Writes a debug/dim message (gray text by default).
    /// </summary>
    /// <param name="text">The message to write.</param>
    void WriteDebug(string? text);

    /// <summary>
    /// Creates a progress indicator that updates in place.
    /// </summary>
    /// <param name="message">The initial message to display.</param>
    /// <returns>A progress indicator that can be updated and disposed.</returns>
    IProgressIndicator CreateProgress(string? message = null);

    /// <summary>
    /// Creates a spinner that shows activity.
    /// </summary>
    /// <param name="message">The message to display alongside the spinner.</param>
    /// <returns>A spinner that can be updated and disposed.</returns>
    ISpinner CreateSpinner(string? message = null);
}
