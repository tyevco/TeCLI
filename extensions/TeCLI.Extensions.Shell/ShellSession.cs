using System;
using System.Collections.Generic;

namespace TeCLI.Shell;

/// <summary>
/// Manages session state that persists between commands in shell mode.
/// </summary>
public class ShellSession
{
    private readonly Dictionary<string, object?> _variables;
    private readonly Dictionary<string, Func<object?>> _variableProviders;

    /// <summary>
    /// Gets the command history for this session.
    /// </summary>
    public CommandHistory History { get; }

    /// <summary>
    /// Gets or sets whether the session is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets the number of commands executed in this session.
    /// </summary>
    public int CommandCount { get; private set; }

    /// <summary>
    /// Gets the session start time.
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    /// Gets or sets the last command result.
    /// </summary>
    public object? LastResult { get; set; }

    /// <summary>
    /// Gets or sets the last exit code.
    /// </summary>
    public int LastExitCode { get; set; }

    /// <summary>
    /// Event raised when a command is about to execute.
    /// </summary>
    public event EventHandler<ShellCommandEventArgs>? CommandExecuting;

    /// <summary>
    /// Event raised when a command has completed.
    /// </summary>
    public event EventHandler<ShellCommandEventArgs>? CommandExecuted;

    /// <summary>
    /// Event raised when the session is about to end.
    /// </summary>
    public event EventHandler? SessionEnding;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellSession"/> class.
    /// </summary>
    /// <param name="history">The command history to use.</param>
    public ShellSession(CommandHistory? history = null)
    {
        History = history ?? new CommandHistory();
        _variables = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        _variableProviders = new Dictionary<string, Func<object?>>(StringComparer.OrdinalIgnoreCase);
        StartTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets a session variable.
    /// </summary>
    /// <typeparam name="T">The type of the variable.</typeparam>
    /// <param name="name">The variable name.</param>
    /// <param name="defaultValue">The default value if not found.</param>
    /// <returns>The variable value.</returns>
    public T? Get<T>(string name, T? defaultValue = default)
    {
        if (_variableProviders.TryGetValue(name, out var provider))
        {
            var result = provider();
            if (result is T typedResult)
                return typedResult;
        }

        if (_variables.TryGetValue(name, out var value) && value is T tValue)
        {
            return tValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// Sets a session variable.
    /// </summary>
    /// <typeparam name="T">The type of the variable.</typeparam>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The value to set.</param>
    public void Set<T>(string name, T? value)
    {
        _variables[name] = value;
    }

    /// <summary>
    /// Checks if a session variable exists.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>True if the variable exists.</returns>
    public bool Has(string name)
    {
        return _variables.ContainsKey(name) || _variableProviders.ContainsKey(name);
    }

    /// <summary>
    /// Removes a session variable.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>True if the variable was removed.</returns>
    public bool Remove(string name)
    {
        _variableProviders.Remove(name);
        return _variables.Remove(name);
    }

    /// <summary>
    /// Registers a variable provider that computes the value on access.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="provider">The value provider function.</param>
    public void RegisterProvider(string name, Func<object?> provider)
    {
        _variableProviders[name] = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>
    /// Gets all variable names.
    /// </summary>
    /// <returns>The variable names.</returns>
    public IEnumerable<string> GetVariableNames()
    {
        var names = new HashSet<string>(_variables.Keys, StringComparer.OrdinalIgnoreCase);
        foreach (var name in _variableProviders.Keys)
        {
            names.Add(name);
        }
        return names;
    }

    /// <summary>
    /// Records that a command is about to execute.
    /// </summary>
    /// <param name="commandLine">The command line.</param>
    internal void OnCommandExecuting(string commandLine)
    {
        CommandExecuting?.Invoke(this, new ShellCommandEventArgs(commandLine, CommandCount));
    }

    /// <summary>
    /// Records that a command has completed.
    /// </summary>
    /// <param name="commandLine">The command line.</param>
    /// <param name="exitCode">The exit code.</param>
    /// <param name="result">The command result.</param>
    internal void OnCommandExecuted(string commandLine, int exitCode, object? result = null)
    {
        CommandCount++;
        LastExitCode = exitCode;
        LastResult = result;
        CommandExecuted?.Invoke(this, new ShellCommandEventArgs(commandLine, CommandCount - 1, exitCode, result));
    }

    /// <summary>
    /// Signals that the session is ending.
    /// </summary>
    internal void OnSessionEnding()
    {
        IsActive = false;
        SessionEnding?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clears all session variables.
    /// </summary>
    public void ClearVariables()
    {
        _variables.Clear();
        _variableProviders.Clear();
    }
}

/// <summary>
/// Event arguments for shell command events.
/// </summary>
public class ShellCommandEventArgs : EventArgs
{
    /// <summary>
    /// Gets the command line.
    /// </summary>
    public string CommandLine { get; }

    /// <summary>
    /// Gets the command index.
    /// </summary>
    public int CommandIndex { get; }

    /// <summary>
    /// Gets the exit code (only valid for CommandExecuted).
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Gets the command result (only valid for CommandExecuted).
    /// </summary>
    public object? Result { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellCommandEventArgs"/> class.
    /// </summary>
    public ShellCommandEventArgs(string commandLine, int commandIndex, int exitCode = 0, object? result = null)
    {
        CommandLine = commandLine;
        CommandIndex = commandIndex;
        ExitCode = exitCode;
        Result = result;
    }
}
