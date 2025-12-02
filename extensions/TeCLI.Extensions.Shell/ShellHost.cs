using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TeCLI.Shell;

/// <summary>
/// Hosts an interactive shell session with command history and line editing.
/// </summary>
public class ShellHost
{
    private readonly ShellSession _session;
    private readonly LineEditor _lineEditor;
    private readonly ShellOptions _options;
    private readonly Dictionary<string, ShellCommand> _builtInCommands;

    /// <summary>
    /// Gets the current session.
    /// </summary>
    public ShellSession Session => _session;

    /// <summary>
    /// Gets the shell options.
    /// </summary>
    public ShellOptions Options => _options;

    /// <summary>
    /// Event raised to execute a command.
    /// </summary>
    public event Func<ShellHost, string[], Task<int>>? CommandHandler;

    /// <summary>
    /// Event raised to provide auto-complete suggestions.
    /// </summary>
    public event Func<string, IEnumerable<string>>? AutoCompleteHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellHost"/> class.
    /// </summary>
    /// <param name="options">The shell options.</param>
    public ShellHost(ShellOptions? options = null)
    {
        _options = options ?? new ShellOptions();
        _session = new ShellSession(new CommandHistory(_options.MaxHistorySize, _options.HistoryFile));
        _lineEditor = new LineEditor(_session.History, GetAutoCompletions)
        {
            Prompt = _options.Prompt
        };
        _builtInCommands = CreateBuiltInCommands();

        // Register built-in session variables
        _session.RegisterProvider("commandCount", () => _session.CommandCount);
        _session.RegisterProvider("lastExitCode", () => _session.LastExitCode);
        _session.RegisterProvider("sessionTime", () => DateTime.UtcNow - _session.StartTime);
    }

    /// <summary>
    /// Runs the shell loop.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The exit code.</returns>
    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_options.WelcomeMessage))
        {
            Console.WriteLine(_options.WelcomeMessage);
        }

        if (_options.ShowHelpOnStart)
        {
            ShowHelp();
        }

        while (_session.IsActive && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var line = _lineEditor.ReadLine();

                if (line == null)
                {
                    // EOF
                    break;
                }

                line = line.Trim();

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                _session.History.Add(line);
                await ExecuteCommandAsync(line, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }

        _session.OnSessionEnding();

        if (!string.IsNullOrEmpty(_options.ExitMessage))
        {
            Console.WriteLine(_options.ExitMessage);
        }

        return _session.LastExitCode;
    }

    /// <summary>
    /// Runs the shell loop synchronously.
    /// </summary>
    /// <returns>The exit code.</returns>
    public int Run()
    {
        return RunAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes a command line.
    /// </summary>
    /// <param name="commandLine">The command line to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The exit code.</returns>
    public async Task<int> ExecuteCommandAsync(string commandLine, CancellationToken cancellationToken = default)
    {
        var args = ParseCommandLine(commandLine);
        if (args.Length == 0)
            return 0;

        _session.OnCommandExecuting(commandLine);

        int exitCode;
        try
        {
            // Check for built-in commands first
            if (_builtInCommands.TryGetValue(args[0].ToLowerInvariant(), out var builtIn))
            {
                exitCode = await builtIn.ExecuteAsync(this, args.Skip(1).ToArray());
            }
            else if (CommandHandler != null)
            {
                exitCode = await CommandHandler(this, args);
            }
            else
            {
                Console.Error.WriteLine($"Unknown command: {args[0]}");
                Console.Error.WriteLine("Type 'help' for available commands.");
                exitCode = 1;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error executing command: {ex.Message}");
            exitCode = 1;
        }

        _session.OnCommandExecuted(commandLine, exitCode);
        return exitCode;
    }

    /// <summary>
    /// Registers a built-in command.
    /// </summary>
    /// <param name="name">The command name.</param>
    /// <param name="description">The command description.</param>
    /// <param name="handler">The command handler.</param>
    public void RegisterCommand(string name, string description, Func<ShellHost, string[], Task<int>> handler)
    {
        _builtInCommands[name.ToLowerInvariant()] = new ShellCommand(name, description, handler);
    }

    /// <summary>
    /// Registers a built-in command (synchronous).
    /// </summary>
    /// <param name="name">The command name.</param>
    /// <param name="description">The command description.</param>
    /// <param name="handler">The command handler.</param>
    public void RegisterCommand(string name, string description, Func<ShellHost, string[], int> handler)
    {
        _builtInCommands[name.ToLowerInvariant()] = new ShellCommand(
            name,
            description,
            (host, args) => Task.FromResult(handler(host, args)));
    }

    private Dictionary<string, ShellCommand> CreateBuiltInCommands()
    {
        return new Dictionary<string, ShellCommand>(StringComparer.OrdinalIgnoreCase)
        {
            ["exit"] = new ShellCommand("exit", "Exit the shell", (host, _) =>
            {
                host._session.IsActive = false;
                return Task.FromResult(0);
            }),
            ["quit"] = new ShellCommand("quit", "Exit the shell (alias for exit)", (host, _) =>
            {
                host._session.IsActive = false;
                return Task.FromResult(0);
            }),
            ["help"] = new ShellCommand("help", "Show available commands", (host, _) =>
            {
                host.ShowHelp();
                return Task.FromResult(0);
            }),
            ["history"] = new ShellCommand("history", "Show command history", (host, args) =>
            {
                host.ShowHistory(args);
                return Task.FromResult(0);
            }),
            ["clear"] = new ShellCommand("clear", "Clear the screen", (_, _) =>
            {
                Console.Clear();
                return Task.FromResult(0);
            }),
            ["cls"] = new ShellCommand("cls", "Clear the screen (alias for clear)", (_, _) =>
            {
                Console.Clear();
                return Task.FromResult(0);
            }),
        };
    }

    private void ShowHelp()
    {
        Console.WriteLine("Built-in commands:");
        foreach (var cmd in _builtInCommands.Values.GroupBy(c => c.Description).Select(g => g.First()))
        {
            var aliases = _builtInCommands.Where(kvp => kvp.Value.Description == cmd.Description)
                .Select(kvp => kvp.Key)
                .ToList();
            Console.WriteLine($"  {string.Join(", ", aliases),-20} {cmd.Description}");
        }
        Console.WriteLine();
        Console.WriteLine("Keyboard shortcuts:");
        Console.WriteLine("  Up/Down            Navigate command history");
        Console.WriteLine("  Left/Right         Move cursor");
        Console.WriteLine("  Ctrl+A / Home      Move to start of line");
        Console.WriteLine("  Ctrl+E / End       Move to end of line");
        Console.WriteLine("  Ctrl+U             Delete to start of line");
        Console.WriteLine("  Ctrl+K             Delete to end of line");
        Console.WriteLine("  Ctrl+W             Delete previous word");
        Console.WriteLine("  Ctrl+L             Clear screen");
        Console.WriteLine("  Tab                Auto-complete");
        Console.WriteLine("  Escape             Clear line");
        Console.WriteLine();
    }

    private void ShowHistory(string[] args)
    {
        var entries = _session.History.Entries;
        var count = entries.Count;

        if (args.Length > 0 && int.TryParse(args[0], out var limit))
        {
            entries = entries.Skip(Math.Max(0, count - limit)).ToList();
        }

        var startIndex = count - entries.Count;
        for (int i = 0; i < entries.Count; i++)
        {
            Console.WriteLine($"  {startIndex + i + 1,4}  {entries[i]}");
        }
    }

    private IEnumerable<string> GetAutoCompletions(string input)
    {
        var completions = new List<string>();

        // Split input to get the current word being typed
        var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var currentWord = parts.Length > 0 ? parts[parts.Length - 1] : string.Empty;
        var isFirstWord = parts.Length <= 1 && !input.EndsWith(" ");

        if (isFirstWord)
        {
            // Complete command names
            completions.AddRange(_builtInCommands.Keys
                .Where(k => k.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase)));
        }

        // Get additional completions from handler
        if (AutoCompleteHandler != null)
        {
            completions.AddRange(AutoCompleteHandler(input));
        }

        return completions.Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static string[] ParseCommandLine(string commandLine)
    {
        var args = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;
        var escapeNext = false;
        char quoteChar = '"';

        foreach (var c in commandLine)
        {
            if (escapeNext)
            {
                current.Append(c);
                escapeNext = false;
                continue;
            }

            if (c == '\\')
            {
                escapeNext = true;
                continue;
            }

            if ((c == '"' || c == '\'') && (!inQuotes || c == quoteChar))
            {
                if (inQuotes)
                {
                    inQuotes = false;
                }
                else
                {
                    inQuotes = true;
                    quoteChar = c;
                }
                continue;
            }

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    args.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
        {
            args.Add(current.ToString());
        }

        return args.ToArray();
    }
}

/// <summary>
/// Configuration options for the shell.
/// </summary>
public class ShellOptions
{
    /// <summary>
    /// Gets or sets the prompt to display.
    /// </summary>
    public string Prompt { get; set; } = "> ";

    /// <summary>
    /// Gets or sets the welcome message.
    /// </summary>
    public string? WelcomeMessage { get; set; }

    /// <summary>
    /// Gets or sets the exit message.
    /// </summary>
    public string? ExitMessage { get; set; }

    /// <summary>
    /// Gets or sets the maximum history size.
    /// </summary>
    public int MaxHistorySize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the history file path.
    /// </summary>
    public string? HistoryFile { get; set; }

    /// <summary>
    /// Gets or sets whether to show help on start.
    /// </summary>
    public bool ShowHelpOnStart { get; set; }

    /// <summary>
    /// Creates options from a <see cref="ShellAttribute"/>.
    /// </summary>
    public static ShellOptions FromAttribute(ShellAttribute attribute)
    {
        return new ShellOptions
        {
            Prompt = attribute.Prompt,
            WelcomeMessage = attribute.WelcomeMessage,
            ExitMessage = attribute.ExitMessage,
            MaxHistorySize = attribute.MaxHistorySize,
            HistoryFile = attribute.HistoryFile,
            ShowHelpOnStart = attribute.ShowHelpOnStart
        };
    }
}

/// <summary>
/// Represents a built-in shell command.
/// </summary>
internal class ShellCommand
{
    public string Name { get; }
    public string Description { get; }
    public Func<ShellHost, string[], Task<int>> Handler { get; }

    public ShellCommand(string name, string description, Func<ShellHost, string[], Task<int>> handler)
    {
        Name = name;
        Description = description;
        Handler = handler;
    }

    public Task<int> ExecuteAsync(ShellHost host, string[] args)
    {
        return Handler(host, args);
    }
}
