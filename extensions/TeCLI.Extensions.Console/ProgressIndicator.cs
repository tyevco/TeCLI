using System;

namespace TeCLI.Console;

/// <summary>
/// A progress indicator that displays a progress bar with percentage.
/// </summary>
public sealed class ProgressIndicator : IProgressIndicator
{
    private readonly StyledConsole _console;
    private string? _message;
    private double _progress;
    private bool _isDisposed;
    private bool _hasOutput;
    private int _lastBarWidth;

    /// <summary>
    /// Gets or sets the progress bar width in characters.
    /// </summary>
    public int BarWidth { get; set; } = 30;

    /// <summary>
    /// Gets or sets the character used for completed progress.
    /// </summary>
    public char FilledChar { get; set; } = '█';

    /// <summary>
    /// Gets or sets the character used for remaining progress.
    /// </summary>
    public char EmptyChar { get; set; } = '░';

    /// <summary>
    /// Creates a new progress indicator.
    /// </summary>
    /// <param name="console">The console to write to.</param>
    /// <param name="message">The initial message to display.</param>
    public ProgressIndicator(StyledConsole console, string? message = null)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _message = message;
        _progress = 0;
        _hasOutput = false;

        Render();
    }

    /// <inheritdoc />
    public double Progress
    {
        get => _progress;
        set
        {
            _progress = Math.Max(0, Math.Min(100, value));
            Render();
        }
    }

    /// <inheritdoc />
    public string? Message
    {
        get => _message;
        set
        {
            _message = value;
            Render();
        }
    }

    /// <inheritdoc />
    public void Report(double value)
    {
        Progress = value;
    }

    /// <inheritdoc />
    public void Report(double value, string? message)
    {
        _message = message;
        _progress = Math.Max(0, Math.Min(100, value));
        Render();
    }

    /// <inheritdoc />
    public void Complete(string? finalMessage = null)
    {
        _progress = 100;
        _message = finalMessage ?? _message;
        RenderFinal(ConsoleStyle.Success, "✓");
    }

    /// <inheritdoc />
    public void Fail(string? errorMessage = null)
    {
        _message = errorMessage ?? _message;
        RenderFinal(ConsoleStyle.Error, "✗");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (_hasOutput && _console.CanControlCursor)
        {
            _console.WriteRaw(Environment.NewLine);
        }
    }

    private void Render()
    {
        if (_isDisposed) return;

        var filled = (int)((_progress / 100) * BarWidth);
        var empty = BarWidth - filled;

        var bar = new string(FilledChar, filled) + new string(EmptyChar, empty);
        var percentage = $"{_progress,6:F1}%";

        var output = string.IsNullOrEmpty(_message)
            ? $"[{bar}] {percentage}"
            : $"{_message} [{bar}] {percentage}";

        if (_console.CanControlCursor)
        {
            // Clear line and write
            if (_hasOutput)
            {
                _console.WriteRaw("\r" + AnsiCodes.Erase.Line);
            }

            if (_console.SupportsAnsi)
            {
                // Color the filled portion
                var styledBar = AnsiCodes.Stylize(new string(FilledChar, filled), ConsoleStyle.Success) +
                               new string(EmptyChar, empty);

                output = string.IsNullOrEmpty(_message)
                    ? $"[{styledBar}] {percentage}"
                    : $"{_message} [{styledBar}] {percentage}";
            }

            _console.WriteRaw(output);
            _lastBarWidth = output.Length;
        }
        else
        {
            // Non-interactive: just write on new lines periodically
            if (!_hasOutput || Math.Abs(_progress - 100) < 0.1)
            {
                _console.WriteLine(output);
            }
        }

        _hasOutput = true;
    }

    private void RenderFinal(ConsoleStyle style, string symbol)
    {
        if (_isDisposed) return;
        _isDisposed = true;

        var output = string.IsNullOrEmpty(_message)
            ? $"{symbol} Complete ({_progress:F1}%)"
            : $"{symbol} {_message}";

        if (_console.CanControlCursor)
        {
            _console.WriteRaw("\r" + AnsiCodes.Erase.Line);
            _console.WriteLine(output, style);
        }
        else
        {
            _console.WriteLine(output, style);
        }
    }
}
