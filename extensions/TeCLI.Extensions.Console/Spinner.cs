using System;
using System.Threading;

namespace TeCLI.Console;

/// <summary>
/// A spinner that shows ongoing activity with an animated indicator.
/// </summary>
public sealed class Spinner : ISpinner
{
    private readonly StyledConsole _console;
    private string? _message;
    private bool _isRunning;
    private bool _isDisposed;
    private Thread? _animationThread;
    private int _currentFrame;

    private static readonly string[] DefaultFrames = new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
    private static readonly string[] AsciiFrames = new[] { "|", "/", "-", "\\" };

    private readonly string[] _frames;

    /// <summary>
    /// Gets or sets the interval between animation frames in milliseconds.
    /// </summary>
    public int Interval { get; set; } = 80;

    /// <summary>
    /// Creates a new spinner.
    /// </summary>
    /// <param name="console">The console to write to.</param>
    /// <param name="message">The message to display alongside the spinner.</param>
    public Spinner(StyledConsole console, string? message = null)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _message = message;

        // Use Unicode spinners if ANSI is supported, otherwise ASCII
        _frames = _console.SupportsAnsi ? DefaultFrames : AsciiFrames;

        Start();
    }

    /// <inheritdoc />
    public string? Message
    {
        get => _message;
        set
        {
            _message = value;
        }
    }

    /// <inheritdoc />
    public bool IsRunning => _isRunning;

    /// <inheritdoc />
    public void Start()
    {
        if (_isRunning || _isDisposed) return;

        _isRunning = true;

        if (_console.CanControlCursor)
        {
            // Hide cursor while spinning
            _console.WriteRaw(AnsiCodes.Cursor.Hide);

            _animationThread = new Thread(AnimationLoop)
            {
                IsBackground = true,
                Name = "SpinnerAnimation"
            };
            _animationThread.Start();
        }
        else
        {
            // Non-interactive: just show the message once
            Render();
        }
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _animationThread?.Join(Interval * 2);
        _animationThread = null;

        if (_console.CanControlCursor)
        {
            _console.WriteRaw(AnsiCodes.Cursor.Show);
        }
    }

    /// <inheritdoc />
    public void Update(string? message)
    {
        _message = message;
    }

    /// <inheritdoc />
    public void Success(string? finalMessage = null)
    {
        Stop();
        FinishWith("✓", finalMessage ?? _message, ConsoleStyle.Success);
    }

    /// <inheritdoc />
    public void Fail(string? errorMessage = null)
    {
        Stop();
        FinishWith("✗", errorMessage ?? _message, ConsoleStyle.Error);
    }

    /// <inheritdoc />
    public void Warn(string? warningMessage = null)
    {
        Stop();
        FinishWith("⚠", warningMessage ?? _message, ConsoleStyle.Warning);
    }

    /// <inheritdoc />
    public void Info(string? infoMessage = null)
    {
        Stop();
        FinishWith("ℹ", infoMessage ?? _message, ConsoleStyle.Info);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed) return;

        Stop();
        _isDisposed = true;

        if (_console.CanControlCursor)
        {
            _console.WriteRaw(AnsiCodes.Cursor.Show + Environment.NewLine);
        }
    }

    private void AnimationLoop()
    {
        while (_isRunning)
        {
            Render();
            _currentFrame = (_currentFrame + 1) % _frames.Length;
            Thread.Sleep(Interval);
        }
    }

    private void Render()
    {
        var frame = _frames[_currentFrame];
        var output = string.IsNullOrEmpty(_message) ? frame : $"{frame} {_message}";

        if (_console.CanControlCursor)
        {
            _console.WriteRaw("\r" + AnsiCodes.Erase.Line);

            if (_console.SupportsAnsi)
            {
                var coloredFrame = AnsiCodes.Stylize(frame, ConsoleStyle.Info);
                output = string.IsNullOrEmpty(_message) ? coloredFrame : $"{coloredFrame} {_message}";
            }

            _console.WriteRaw(output);
        }
    }

    private void FinishWith(string symbol, string? message, ConsoleStyle style)
    {
        if (_isDisposed) return;
        _isDisposed = true;

        var output = string.IsNullOrEmpty(message) ? symbol : $"{symbol} {message}";

        if (_console.CanControlCursor)
        {
            _console.WriteRaw("\r" + AnsiCodes.Erase.Line);
        }

        _console.WriteLine(output, style);
    }
}
