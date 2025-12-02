using System;
using System.IO;

namespace TeCLI.Console;

/// <summary>
/// A styled console output implementation that supports colors and ANSI formatting.
/// This is the main implementation of <see cref="IConsoleOutput"/> for terminal output.
/// </summary>
public class StyledConsole : IConsoleOutput
{
    private readonly TextWriter _out;
    private readonly TextWriter _error;
    private readonly bool _supportsColor;
    private readonly bool _supportsAnsi;

    /// <summary>
    /// Creates a new styled console with default System.Console output.
    /// </summary>
    public StyledConsole()
        : this(System.Console.Out, System.Console.Error)
    {
    }

    /// <summary>
    /// Creates a new styled console with custom output writers.
    /// </summary>
    /// <param name="output">The output writer for stdout.</param>
    /// <param name="error">The output writer for stderr.</param>
    public StyledConsole(TextWriter output, TextWriter error)
        : this(output, error, TerminalCapabilities.SupportsColor, TerminalCapabilities.SupportsAnsi)
    {
    }

    /// <summary>
    /// Creates a new styled console with custom output writers and color support settings.
    /// </summary>
    /// <param name="output">The output writer for stdout.</param>
    /// <param name="error">The output writer for stderr.</param>
    /// <param name="supportsColor">Whether color output is enabled.</param>
    /// <param name="supportsAnsi">Whether ANSI escape codes are supported.</param>
    public StyledConsole(TextWriter output, TextWriter error, bool supportsColor, bool supportsAnsi)
    {
        _out = output ?? throw new ArgumentNullException(nameof(output));
        _error = error ?? throw new ArgumentNullException(nameof(error));
        _supportsColor = supportsColor;
        _supportsAnsi = supportsAnsi;
    }

    /// <inheritdoc />
    public bool SupportsColor => _supportsColor;

    /// <inheritdoc />
    public bool SupportsAnsi => _supportsAnsi;

    /// <inheritdoc />
    public void Write(string? text)
    {
        if (text != null)
            _out.Write(text);
    }

    /// <inheritdoc />
    public void WriteLine(string? text)
    {
        _out.WriteLine(text);
    }

    /// <inheritdoc />
    public void WriteLine()
    {
        _out.WriteLine();
    }

    /// <inheritdoc />
    public void WriteError(string? text)
    {
        if (text != null)
            _error.Write(text);
    }

    /// <inheritdoc />
    public void WriteErrorLine(string? text)
    {
        _error.WriteLine(text);
    }

    /// <inheritdoc />
    public void Write(string? text, ConsoleStyle style)
    {
        if (text == null) return;

        if (_supportsAnsi && _supportsColor)
        {
            _out.Write(AnsiCodes.Stylize(text, style));
        }
        else if (_supportsColor && style.Foreground.HasValue)
        {
            WriteWithConsoleColor(text, style.Foreground.Value, false);
        }
        else
        {
            _out.Write(text);
        }
    }

    /// <inheritdoc />
    public void WriteLine(string? text, ConsoleStyle style)
    {
        if (_supportsAnsi && _supportsColor)
        {
            _out.WriteLine(text != null ? AnsiCodes.Stylize(text, style) : null);
        }
        else if (_supportsColor && style.Foreground.HasValue && text != null)
        {
            WriteWithConsoleColor(text, style.Foreground.Value, true);
        }
        else
        {
            _out.WriteLine(text);
        }
    }

    /// <inheritdoc />
    public void WriteSuccess(string? text)
    {
        WriteLine(text, ConsoleStyle.Success);
    }

    /// <inheritdoc />
    public void WriteWarning(string? text)
    {
        WriteLine(text, ConsoleStyle.Warning);
    }

    /// <inheritdoc />
    public void WriteErrorMessage(string? text)
    {
        if (_supportsAnsi && _supportsColor)
        {
            _error.WriteLine(text != null ? AnsiCodes.Stylize(text, ConsoleStyle.Error) : null);
        }
        else if (_supportsColor && text != null)
        {
            var originalColor = System.Console.ForegroundColor;
            try
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                _error.WriteLine(text);
            }
            finally
            {
                System.Console.ForegroundColor = originalColor;
            }
        }
        else
        {
            _error.WriteLine(text);
        }
    }

    /// <inheritdoc />
    public void WriteInfo(string? text)
    {
        WriteLine(text, ConsoleStyle.Info);
    }

    /// <inheritdoc />
    public void WriteDebug(string? text)
    {
        WriteLine(text, ConsoleStyle.Debug);
    }

    /// <inheritdoc />
    public IProgressIndicator CreateProgress(string? message = null)
    {
        return new ProgressIndicator(this, message);
    }

    /// <inheritdoc />
    public ISpinner CreateSpinner(string? message = null)
    {
        return new Spinner(this, message);
    }

    private void WriteWithConsoleColor(string text, ConsoleColor color, bool newLine)
    {
        var originalColor = System.Console.ForegroundColor;
        try
        {
            System.Console.ForegroundColor = color;
            if (newLine)
                _out.WriteLine(text);
            else
                _out.Write(text);
        }
        finally
        {
            System.Console.ForegroundColor = originalColor;
        }
    }

    /// <summary>
    /// Writes raw text without any color transformation (for internal use with ANSI codes).
    /// </summary>
    internal void WriteRaw(string? text)
    {
        if (text != null)
            _out.Write(text);
    }

    /// <summary>
    /// Writes raw text to stderr without any color transformation.
    /// </summary>
    internal void WriteErrorRaw(string? text)
    {
        if (text != null)
            _error.Write(text);
    }

    /// <summary>
    /// Gets a value indicating whether the cursor can be controlled.
    /// </summary>
    internal bool CanControlCursor => _supportsAnsi && !System.Console.IsOutputRedirected;
}
