# TeCLI.Extensions.Shell

Interactive shell/REPL functionality for TeCLI applications.

## Overview

TeCLI.Extensions.Shell provides a comprehensive interactive shell experience with command history, readline-like line editing, session state management, and auto-completion support. Build database shells, REPLs, and interactive CLI tools with ease.

## Installation

```bash
dotnet add package TeCLI.Extensions.Shell
```

## Features

- Command history with up/down arrow navigation
- Readline-like line editing (Ctrl+A, Ctrl+E, Ctrl+U, Ctrl+K, Ctrl+W)
- Session state persistence between commands
- Auto-completion support
- Built-in commands (exit, help, history, clear)
- History persistence to file
- Fluent action-based shell builder

## Quick Start

```csharp
using TeCLI.Shell;

// Create an action-based shell
var shell = ShellExtensions.CreateActionShell(new ShellOptions
{
    Prompt = "db> ",
    WelcomeMessage = "Welcome to the Database Shell!",
    ExitMessage = "Goodbye!"
})
.WithAction("query", "Execute a SQL query", args =>
{
    var sql = string.Join(" ", args);
    Console.WriteLine($"Executing: {sql}");
    return 0;
})
.WithAction("tables", "List all tables", _ =>
{
    Console.WriteLine("users, orders, products");
    return 0;
})
.Build();

// Run the shell
return shell.Run();
```

## Key Classes

| Class | Purpose |
|-------|---------|
| `ShellHost` | Main REPL loop with built-in commands |
| `ShellSession` | Session state management between commands |
| `CommandHistory` | Command history with navigation and persistence |
| `LineEditor` | Readline-like input with editing support |
| `ShellAttribute` | Attribute to configure shell mode on commands |
| `ShellOptions` | Configuration options for the shell |
| `ActionShellBuilder` | Fluent API for building action-based shells |

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| Up/Down | Navigate command history |
| Left/Right | Move cursor |
| Ctrl+A / Home | Move to start of line |
| Ctrl+E / End | Move to end of line |
| Ctrl+U | Delete to start of line |
| Ctrl+K | Delete to end of line |
| Ctrl+W | Delete previous word |
| Ctrl+L | Clear screen |
| Tab | Auto-complete |
| Escape | Clear line |

## Built-in Commands

| Command | Description |
|---------|-------------|
| `exit` / `quit` | Exit the shell |
| `help` | Show available commands and shortcuts |
| `history` | Show command history |
| `clear` / `cls` | Clear the screen |

## Shell Attribute Usage

```csharp
[Command("db")]
[Shell(Prompt = "db> ", WelcomeMessage = "Database Shell v1.0")]
public class DatabaseCommand
{
    [Action("query")]
    public void Query([Argument] string sql) { }

    [Action("tables")]
    public void ListTables() { }
}
```

## Session State

```csharp
var host = new ShellHost();

// Store and retrieve session variables
host.Session.Set("currentDatabase", "mydb");
var db = host.Session.Get<string>("currentDatabase");

// Register computed providers
host.Session.RegisterProvider("uptime", () => DateTime.UtcNow - host.Session.StartTime);

// Subscribe to session events
host.Session.CommandExecuting += (s, e) => Console.WriteLine($"Running: {e.CommandLine}");
host.Session.CommandExecuted += (s, e) => Console.WriteLine($"Exit code: {e.ExitCode}");
```

## Custom Command Handler

```csharp
var host = new ShellHost(new ShellOptions { Prompt = "app> " });

host.CommandHandler += async (h, args) =>
{
    var command = args[0];
    var cmdArgs = args.Skip(1).ToArray();

    switch (command)
    {
        case "process":
            await ProcessAsync(cmdArgs);
            return 0;
        default:
            Console.Error.WriteLine($"Unknown command: {command}");
            return 1;
    }
};

host.AutoCompleteHandler += input =>
    new[] { "process", "status", "config" }
        .Where(c => c.StartsWith(input, StringComparison.OrdinalIgnoreCase));

await host.RunAsync();
```

## History Persistence

```csharp
var options = new ShellOptions
{
    HistoryFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".myapp_history"),
    MaxHistorySize = 1000
};

var shell = new ShellHost(options);
```

## Documentation

For full documentation and examples, see the [TeCLI repository](https://github.com/tyevco/TeCLI).

## License

MIT License - see [LICENSE](https://github.com/tyevco/TeCLI/blob/main/LICENSE) for details.
