using System;

namespace TeCLI.Shell;

/// <summary>
/// Marks a command class as supporting interactive shell mode.
/// When applied, the command can be invoked without arguments to enter a REPL loop.
/// </summary>
/// <example>
/// <code>
/// [Command("db")]
/// [Shell(Prompt = "db> ")]
/// public class DatabaseCommand
/// {
///     [Action("query")]
///     public void Query([Argument] string sql) { }
///
///     [Action("tables")]
///     public void ListTables() { }
/// }
///
/// // Usage:
/// // myapp db          -> enters shell mode
/// // db> query SELECT * FROM users
/// // db> tables
/// // db> exit
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ShellAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the prompt displayed in shell mode.
    /// Defaults to "&gt; " if not specified.
    /// </summary>
    public string Prompt { get; set; } = "> ";

    /// <summary>
    /// Gets or sets the welcome message displayed when entering shell mode.
    /// </summary>
    public string? WelcomeMessage { get; set; }

    /// <summary>
    /// Gets or sets the exit message displayed when leaving shell mode.
    /// </summary>
    public string? ExitMessage { get; set; }

    /// <summary>
    /// Gets or sets whether command history is enabled. Defaults to true.
    /// </summary>
    public bool EnableHistory { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of history entries to retain. Defaults to 100.
    /// </summary>
    public int MaxHistorySize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the file path to persist history between sessions.
    /// If null, history is not persisted.
    /// </summary>
    public string? HistoryFile { get; set; }

    /// <summary>
    /// Gets or sets whether to show help on startup. Defaults to false.
    /// </summary>
    public bool ShowHelpOnStart { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellAttribute"/> class.
    /// </summary>
    public ShellAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellAttribute"/> class with a custom prompt.
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    public ShellAttribute(string prompt)
    {
        Prompt = prompt ?? "> ";
    }
}
