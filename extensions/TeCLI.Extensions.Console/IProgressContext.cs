using System;

namespace TeCLI.Console;

/// <summary>
/// Provides a context for creating progress indicators and spinners within CLI actions.
/// This interface can be auto-injected into action methods to provide rich progress UI support.
/// </summary>
/// <remarks>
/// <para>
/// When used as a parameter in an action method, the framework will automatically inject
/// an instance that is pre-configured for the current terminal capabilities.
/// </para>
/// <example>
/// <code>
/// [Action("download")]
/// public async Task Download(
///     [Argument] string url,
///     IProgressContext progress)  // Auto-injected by framework
/// {
///     using var bar = progress.CreateProgressBar("Downloading...");
///
///     await foreach (var chunk in DownloadChunksAsync(url))
///     {
///         bar.Increment(chunk.Length);
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public interface IProgressContext
{
    /// <summary>
    /// Gets the underlying console output instance.
    /// </summary>
    IConsoleOutput Console { get; }

    /// <summary>
    /// Creates a progress bar indicator.
    /// </summary>
    /// <param name="message">The initial message to display alongside the progress bar.</param>
    /// <param name="maxValue">The maximum value representing 100% completion. Defaults to 100.</param>
    /// <returns>A progress bar that updates in place.</returns>
    /// <remarks>
    /// The progress bar should be disposed when the operation is complete.
    /// Use the <see cref="IProgressBar.Increment"/> method to update progress.
    /// </remarks>
    IProgressBar CreateProgressBar(string? message = null, double maxValue = 100);

    /// <summary>
    /// Creates a spinner for indeterminate progress.
    /// </summary>
    /// <param name="message">The message to display alongside the spinner.</param>
    /// <returns>A spinner that animates while a task is running.</returns>
    /// <remarks>
    /// The spinner should be disposed when the operation is complete.
    /// Use the <see cref="ISpinner.Update"/> method to change the status message.
    /// </remarks>
    ISpinner CreateSpinner(string? message = null);

    /// <summary>
    /// Creates a progress indicator that displays completion percentage.
    /// </summary>
    /// <param name="message">The initial message to display.</param>
    /// <returns>A progress indicator that can be updated with percentage values.</returns>
    IProgressIndicator CreateProgress(string? message = null);
}

/// <summary>
/// Represents a progress bar with support for incrementing and setting values.
/// </summary>
public interface IProgressBar : IDisposable
{
    /// <summary>
    /// Gets or sets the current value of the progress bar.
    /// </summary>
    double Value { get; set; }

    /// <summary>
    /// Gets or sets the maximum value representing 100% completion.
    /// </summary>
    double MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the message displayed alongside the progress bar.
    /// </summary>
    string? Message { get; set; }

    /// <summary>
    /// Increments the progress bar value by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to increment. Defaults to 1.</param>
    void Increment(double amount = 1);

    /// <summary>
    /// Reports progress as an absolute value.
    /// </summary>
    /// <param name="value">The current progress value.</param>
    void Report(double value);

    /// <summary>
    /// Reports progress with a message.
    /// </summary>
    /// <param name="value">The current progress value.</param>
    /// <param name="message">The message to display.</param>
    void Report(double value, string? message);

    /// <summary>
    /// Marks the progress bar as complete (100%).
    /// </summary>
    /// <param name="finalMessage">Optional final message to display.</param>
    void Complete(string? finalMessage = null);

    /// <summary>
    /// Marks the progress bar as failed.
    /// </summary>
    /// <param name="errorMessage">Optional error message to display.</param>
    void Fail(string? errorMessage = null);
}
