# TeCLI

The core source-generated CLI parsing library for .NET.

## Overview

TeCLI is the main package that provides source-generated command-line interface parsing. Using Roslyn source generators and custom attributes, TeCLI automatically generates type-safe parsing and dispatching logic at compile time with zero runtime reflection.

## Installation

```bash
dotnet add package TeCLI
```

## Quick Start

```csharp
using TeCLI;

[Command("greet", Description = "Greets the user")]
public class GreetCommand
{
    [Primary(Description = "Say hello")]
    public void Hello([Argument] string name)
    {
        Console.WriteLine($"Hello, {name}!");
    }
}

// In Program.cs
var exitCode = await CommandDispatcher.DispatchAsync(args);
return exitCode;
```

## Features

- **Source Generation** - All parsing code generated at compile time
- **32 Roslyn Analyzers** - Real-time feedback and error detection
- **Type-Safe Parsing** - Primitives, enums, collections, custom types
- **Help Generation** - Automatic `--help` and `--version` support
- **Shell Completion** - Generate scripts for Bash, Zsh, PowerShell, Fish
- **Validation** - Built-in validation attributes
- **Async Support** - First-class `Task` and `ValueTask` support

## Attributes

| Attribute | Purpose |
|-----------|---------|
| `[Command]` | Marks a class as a CLI command |
| `[Action]` | Marks a method as a named action |
| `[Primary]` | Marks the default action |
| `[Option]` | Named option (`--name` or `-n`) |
| `[Argument]` | Positional argument |
| `[GlobalOptions]` | Shared options across commands |

## Documentation

For full documentation, examples, and API reference, see the [TeCLI repository](https://github.com/tyevco/TeCLI).

## License

MIT License - see [LICENSE](https://github.com/tyevco/TeCLI/blob/main/LICENSE) for details.
