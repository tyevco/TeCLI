using System;

namespace TeCLI.Console;

/// <summary>
/// Represents a spinner that shows ongoing activity without a specific progress value.
/// Dispose of the spinner when the operation is complete.
/// </summary>
public interface ISpinner : IDisposable
{
    /// <summary>
    /// Gets or sets the message displayed alongside the spinner.
    /// </summary>
    string? Message { get; set; }

    /// <summary>
    /// Gets a value indicating whether the spinner is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Starts the spinner animation.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the spinner animation.
    /// </summary>
    void Stop();

    /// <summary>
    /// Updates the message displayed alongside the spinner.
    /// </summary>
    /// <param name="message">The new message to display.</param>
    void Update(string? message);

    /// <summary>
    /// Marks the operation as successful and stops the spinner.
    /// </summary>
    /// <param name="finalMessage">Optional final message to display.</param>
    void Success(string? finalMessage = null);

    /// <summary>
    /// Marks the operation as failed and stops the spinner.
    /// </summary>
    /// <param name="errorMessage">Optional error message to display.</param>
    void Fail(string? errorMessage = null);

    /// <summary>
    /// Marks the operation as completed with a warning and stops the spinner.
    /// </summary>
    /// <param name="warningMessage">Optional warning message to display.</param>
    void Warn(string? warningMessage = null);

    /// <summary>
    /// Marks the operation as completed with info status and stops the spinner.
    /// </summary>
    /// <param name="infoMessage">Optional info message to display.</param>
    void Info(string? infoMessage = null);
}
