using System;

namespace TeCLI.Testing;

/// <summary>
/// Represents the result of executing a CLI command during testing.
/// Contains captured output, error messages, exit code, and any thrown exceptions.
/// </summary>
public sealed class CommandResult
{
    /// <summary>
    /// Gets the captured standard output.
    /// </summary>
    public string Output { get; }

    /// <summary>
    /// Gets the captured standard error output.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Gets the exit code of the command execution.
    /// 0 indicates success, non-zero indicates an error.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Gets the exception that was thrown during command execution, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets a value indicating whether the command executed successfully (ExitCode == 0 and no exception).
    /// </summary>
    public bool IsSuccess => ExitCode == 0 && Exception == null;

    /// <summary>
    /// Gets a value indicating whether the command failed (ExitCode != 0 or exception was thrown).
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the captured output split into individual lines.
    /// </summary>
    public string[] OutputLines => SplitLines(Output);

    /// <summary>
    /// Gets the captured error output split into individual lines.
    /// </summary>
    public string[] ErrorLines => SplitLines(Error);

    /// <summary>
    /// Gets a value indicating whether any output was written to stdout.
    /// </summary>
    public bool HasOutput => !string.IsNullOrEmpty(Output);

    /// <summary>
    /// Gets a value indicating whether any output was written to stderr.
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(Error);

    /// <summary>
    /// Gets a value indicating whether an exception was thrown during execution.
    /// </summary>
    public bool HasException => Exception != null;

    /// <summary>
    /// Gets the elapsed time of the command execution.
    /// </summary>
    public TimeSpan ElapsedTime { get; }

    /// <summary>
    /// Creates a new CommandResult instance.
    /// </summary>
    internal CommandResult(string output, string error, int exitCode, Exception? exception, TimeSpan elapsedTime)
    {
        Output = output ?? string.Empty;
        Error = error ?? string.Empty;
        ExitCode = exitCode;
        Exception = exception;
        ElapsedTime = elapsedTime;
    }

    /// <summary>
    /// Checks if the output contains the specified text.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <returns>True if the output contains the text; otherwise, false.</returns>
    public bool OutputContains(string text) =>
        Output.Contains(text);

    /// <summary>
    /// Checks if the output contains the specified text, using the specified comparison type.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    /// <returns>True if the output contains the text; otherwise, false.</returns>
    public bool OutputContains(string text, StringComparison comparison) =>
        Output.IndexOf(text, comparison) >= 0;

    /// <summary>
    /// Checks if the error output contains the specified text.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <returns>True if the error output contains the text; otherwise, false.</returns>
    public bool ErrorContains(string text) =>
        Error.Contains(text);

    /// <summary>
    /// Checks if the error output contains the specified text, using the specified comparison type.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    /// <returns>True if the error output contains the text; otherwise, false.</returns>
    public bool ErrorContains(string text, StringComparison comparison) =>
        Error.IndexOf(text, comparison) >= 0;

    /// <summary>
    /// Returns a string representation of the command result.
    /// </summary>
    public override string ToString()
    {
        var status = IsSuccess ? "Success" : $"Failed (ExitCode: {ExitCode})";
        var exceptionInfo = Exception != null ? $", Exception: {Exception.GetType().Name}" : "";
        return $"CommandResult: {status}{exceptionInfo}, Output: {Output.Length} chars, Error: {Error.Length} chars";
    }

    private static string[] SplitLines(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<string>();

        return text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    }

    /// <summary>
    /// Creates a successful result with the given output.
    /// </summary>
    public static CommandResult Success(string output = "", string error = "", TimeSpan elapsed = default) =>
        new CommandResult(output, error, 0, null, elapsed);

    /// <summary>
    /// Creates a failed result with the given exit code.
    /// </summary>
    public static CommandResult Failure(int exitCode, string output = "", string error = "", TimeSpan elapsed = default) =>
        new CommandResult(output, error, exitCode, null, elapsed);

    /// <summary>
    /// Creates a failed result with the given exception.
    /// </summary>
    public static CommandResult FromException(Exception exception, string output = "", string error = "", TimeSpan elapsed = default) =>
        new CommandResult(output, error, 1, exception, elapsed);
}
