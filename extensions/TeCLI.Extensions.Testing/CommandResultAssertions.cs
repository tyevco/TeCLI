using System;
using System.Text.RegularExpressions;

namespace TeCLI.Testing;

/// <summary>
/// Provides assertion methods for validating CommandResult in tests.
/// These methods throw exceptions when assertions fail, making them compatible
/// with any test framework (xUnit, NUnit, MSTest, etc.).
/// </summary>
public static class CommandResultAssertions
{
    /// <summary>
    /// Asserts that the command executed successfully (ExitCode == 0 and no exception).
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldSucceed(this CommandResult result, string? message = null)
    {
        if (!result.IsSuccess)
        {
            var details = result.Exception != null
                ? $"Exception: {result.Exception.GetType().Name}: {result.Exception.Message}"
                : $"Exit code: {result.ExitCode}";

            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected command to succeed, but it failed.\n{details}\nOutput: {result.Output}\nError: {result.Error}");
        }
        return result;
    }

    /// <summary>
    /// Asserts that the command failed (ExitCode != 0 or exception was thrown).
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldFail(this CommandResult result, string? message = null)
    {
        if (result.IsSuccess)
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected command to fail, but it succeeded.\nOutput: {result.Output}");
        }
        return result;
    }

    /// <summary>
    /// Asserts that the command exited with the specified exit code.
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="expectedExitCode">The expected exit code.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldHaveExitCode(this CommandResult result, int expectedExitCode, string? message = null)
    {
        if (result.ExitCode != expectedExitCode)
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected exit code {expectedExitCode}, but was {result.ExitCode}.\nOutput: {result.Output}\nError: {result.Error}");
        }
        return result;
    }

    /// <summary>
    /// Asserts that the standard output contains the specified text.
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="expected">The text expected to be in the output.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldContainOutput(this CommandResult result, string expected, string? message = null)
    {
        if (!result.OutputContains(expected))
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected output to contain \"{expected}\", but it did not.\nActual output: {result.Output}");
        }
        return result;
    }

    /// <summary>
    /// Asserts that the standard output contains the specified text (case-insensitive).
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="expected">The text expected to be in the output.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldContainOutputIgnoreCase(this CommandResult result, string expected, string? message = null)
    {
        if (!result.OutputContains(expected, StringComparison.OrdinalIgnoreCase))
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected output to contain \"{expected}\" (case-insensitive), but it did not.\nActual output: {result.Output}");
        }
        return result;
    }

    /// <summary>
    /// Asserts that the standard output does not contain the specified text.
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="unexpected">The text that should not be in the output.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldNotContainOutput(this CommandResult result, string unexpected, string? message = null)
    {
        if (result.OutputContains(unexpected))
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected output to NOT contain \"{unexpected}\", but it did.\nActual output: {result.Output}");
        }
        return result;
    }

    /// <summary>
    /// Asserts that the standard output matches the specified regex pattern.
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="pattern">The regex pattern to match.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldMatchOutput(this CommandResult result, string pattern, string? message = null)
    {
        if (!Regex.IsMatch(result.Output, pattern))
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected output to match pattern \"{pattern}\", but it did not.\nActual output: {result.Output}");
        }
        return result;
    }

    /// <summary>
    /// Asserts that the standard error output contains the specified text.
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="expected">The text expected to be in the error output.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldContainError(this CommandResult result, string expected, string? message = null)
    {
        if (!result.ErrorContains(expected))
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected error output to contain \"{expected}\", but it did not.\nActual error: {result.Error}");
        }
        return result;
    }

    /// <summary>
    /// Asserts that the standard error output does not contain the specified text.
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="unexpected">The text that should not be in the error output.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldNotContainError(this CommandResult result, string unexpected, string? message = null)
    {
        if (result.ErrorContains(unexpected))
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected error output to NOT contain \"{unexpected}\", but it did.\nActual error: {result.Error}");
        }
        return result;
    }

    /// <summary>
    /// Asserts that no error output was written.
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldHaveNoError(this CommandResult result, string? message = null)
    {
        if (result.HasError)
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected no error output, but found: {result.Error}");
        }
        return result;
    }

    /// <summary>
    /// Asserts that no standard output was written.
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldHaveNoOutput(this CommandResult result, string? message = null)
    {
        if (result.HasOutput)
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected no output, but found: {result.Output}");
        }
        return result;
    }

    /// <summary>
    /// Asserts that an exception of the specified type was thrown.
    /// </summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="result">The command result to verify.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The exception for further assertions.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static TException ShouldThrow<TException>(this CommandResult result, string? message = null)
        where TException : Exception
    {
        if (result.Exception == null)
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected exception of type {typeof(TException).Name}, but no exception was thrown.");
        }

        if (result.Exception is not TException typedException)
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected exception of type {typeof(TException).Name}, but got {result.Exception.GetType().Name}: {result.Exception.Message}");
        }

        return typedException;
    }

    /// <summary>
    /// Asserts that no exception was thrown during command execution.
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldNotThrow(this CommandResult result, string? message = null)
    {
        if (result.Exception != null)
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected no exception, but {result.Exception.GetType().Name} was thrown: {result.Exception.Message}");
        }
        return result;
    }

    /// <summary>
    /// Asserts that the output equals the expected string exactly.
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="expected">The expected output.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldHaveOutput(this CommandResult result, string expected, string? message = null)
    {
        var normalizedExpected = expected.Replace("\r\n", "\n").TrimEnd('\n');
        var normalizedActual = result.Output.Replace("\r\n", "\n").TrimEnd('\n');

        if (normalizedActual != normalizedExpected)
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected output:\n{expected}\nActual output:\n{result.Output}");
        }
        return result;
    }

    /// <summary>
    /// Asserts that the command completed within the specified time.
    /// </summary>
    /// <param name="result">The command result to verify.</param>
    /// <param name="maxDuration">The maximum allowed duration.</param>
    /// <param name="message">Optional custom message to include on failure.</param>
    /// <returns>The result for method chaining.</returns>
    /// <exception cref="CommandAssertionException">Thrown when the assertion fails.</exception>
    public static CommandResult ShouldCompleteWithin(this CommandResult result, TimeSpan maxDuration, string? message = null)
    {
        if (result.ElapsedTime > maxDuration)
        {
            var customMessage = message != null ? $"{message}\n" : "";
            throw new CommandAssertionException(
                $"{customMessage}Expected command to complete within {maxDuration.TotalMilliseconds}ms, but it took {result.ElapsedTime.TotalMilliseconds}ms.");
        }
        return result;
    }
}

/// <summary>
/// Exception thrown when a command assertion fails.
/// </summary>
public class CommandAssertionException : Exception
{
    /// <summary>
    /// Creates a new CommandAssertionException with the specified message.
    /// </summary>
    /// <param name="message">The assertion failure message.</param>
    public CommandAssertionException(string message) : base(message)
    {
    }
}
