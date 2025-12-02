using System;

namespace TeCLI.Console;

/// <summary>
/// Default implementation of <see cref="IProgressContext"/> that provides progress UI capabilities.
/// </summary>
public sealed class ProgressContext : IProgressContext
{
    private readonly IConsoleOutput _console;

    /// <summary>
    /// Creates a new progress context with the default styled console.
    /// </summary>
    public ProgressContext()
        : this(new StyledConsole())
    {
    }

    /// <summary>
    /// Creates a new progress context with the specified console output.
    /// </summary>
    /// <param name="console">The console output to use for progress rendering.</param>
    public ProgressContext(IConsoleOutput console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    /// <inheritdoc />
    public IConsoleOutput Console => _console;

    /// <inheritdoc />
    public IProgressBar CreateProgressBar(string? message = null, double maxValue = 100)
    {
        return new ProgressBar(_console, message, maxValue);
    }

    /// <inheritdoc />
    public ISpinner CreateSpinner(string? message = null)
    {
        return _console.CreateSpinner(message);
    }

    /// <inheritdoc />
    public IProgressIndicator CreateProgress(string? message = null)
    {
        return _console.CreateProgress(message);
    }
}

/// <summary>
/// A progress bar implementation that wraps <see cref="IProgressIndicator"/> with support for
/// absolute values and increments.
/// </summary>
public sealed class ProgressBar : IProgressBar
{
    private readonly IProgressIndicator _indicator;
    private double _value;
    private double _maxValue;
    private string? _message;

    /// <summary>
    /// Creates a new progress bar.
    /// </summary>
    /// <param name="console">The console to write to.</param>
    /// <param name="message">The initial message to display.</param>
    /// <param name="maxValue">The maximum value representing 100% completion.</param>
    public ProgressBar(IConsoleOutput console, string? message = null, double maxValue = 100)
    {
        if (console == null) throw new ArgumentNullException(nameof(console));

        _maxValue = maxValue > 0 ? maxValue : 100;
        _value = 0;
        _message = message;
        _indicator = console.CreateProgress(message);
    }

    /// <inheritdoc />
    public double Value
    {
        get => _value;
        set
        {
            _value = Math.Max(0, Math.Min(_maxValue, value));
            UpdateIndicator();
        }
    }

    /// <inheritdoc />
    public double MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value > 0 ? value : 100;
            UpdateIndicator();
        }
    }

    /// <inheritdoc />
    public string? Message
    {
        get => _message;
        set
        {
            _message = value;
            _indicator.Message = value;
        }
    }

    /// <inheritdoc />
    public void Increment(double amount = 1)
    {
        Value += amount;
    }

    /// <inheritdoc />
    public void Report(double value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public void Report(double value, string? message)
    {
        _value = Math.Max(0, Math.Min(_maxValue, value));
        _message = message;
        _indicator.Report(CalculatePercentage(), message);
    }

    /// <inheritdoc />
    public void Complete(string? finalMessage = null)
    {
        _value = _maxValue;
        _indicator.Complete(finalMessage);
    }

    /// <inheritdoc />
    public void Fail(string? errorMessage = null)
    {
        _indicator.Fail(errorMessage);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _indicator.Dispose();
    }

    private double CalculatePercentage()
    {
        return _maxValue > 0 ? (_value / _maxValue) * 100 : 0;
    }

    private void UpdateIndicator()
    {
        _indicator.Report(CalculatePercentage());
    }
}
