using System;

namespace TeCLI.Console;

/// <summary>
/// Represents a progress indicator that displays completion percentage.
/// Dispose of the progress indicator when the operation is complete.
/// </summary>
public interface IProgressIndicator : IDisposable
{
    /// <summary>
    /// Gets or sets the current progress value (0-100).
    /// </summary>
    double Progress { get; set; }

    /// <summary>
    /// Gets or sets the message displayed alongside the progress.
    /// </summary>
    string? Message { get; set; }

    /// <summary>
    /// Reports progress update with a value between 0 and 100.
    /// </summary>
    /// <param name="value">The progress value (0-100).</param>
    void Report(double value);

    /// <summary>
    /// Reports progress update with a value and message.
    /// </summary>
    /// <param name="value">The progress value (0-100).</param>
    /// <param name="message">The message to display.</param>
    void Report(double value, string? message);

    /// <summary>
    /// Marks the progress as complete (100%) and finishes the indicator.
    /// </summary>
    /// <param name="finalMessage">Optional final message to display.</param>
    void Complete(string? finalMessage = null);

    /// <summary>
    /// Marks the progress as failed and finishes the indicator.
    /// </summary>
    /// <param name="errorMessage">Optional error message to display.</param>
    void Fail(string? errorMessage = null);
}
