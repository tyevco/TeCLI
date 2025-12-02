# Migration Guides

This directory contains guides to help you migrate your CLI application from other popular .NET CLI libraries to TeCLI.

## Available Guides

| Library | Guide | Difficulty |
|---------|-------|------------|
| [System.CommandLine](https://github.com/dotnet/command-line-api) | [Migration Guide](./from-system-commandline.md) | Moderate |
| [CommandLineParser](https://github.com/commandlineparser/commandline) | [Migration Guide](./from-commandlineparser.md) | Easy |
| [Spectre.Console.Cli](https://spectreconsole.net/cli/) | [Migration Guide](./from-spectre-console-cli.md) | Moderate |
| [McMaster.Extensions.CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils) | [Migration Guide](./from-mcmaster-commandlineutils.md) | Easy |

## Why Migrate to TeCLI?

### Compile-Time Safety
TeCLI uses Roslyn source generators and includes 32 analyzers that catch errors at compile time, not runtime. This means fewer surprises when your users run your CLI.

### Zero Reflection
Unlike most CLI libraries that use runtime reflection, TeCLI generates all parsing code at compile time. This results in faster startup, smaller trimmed binaries, and better AOT compatibility.

### Simple, Declarative API
TeCLI's attribute-based API is intuitive and requires minimal boilerplate:

```csharp
[Command("greet", Description = "Greeting application")]
public class GreetCommand
{
    [Primary]
    public void Execute(
        [Argument(Description = "Name to greet")] string name,
        [Option("excited", ShortName = 'e')] bool excited = false)
    {
        var greeting = excited ? $"Hello, {name}!!!" : $"Hello, {name}.";
        Console.WriteLine(greeting);
    }
}
```

### Built-In Features
Many features that require manual implementation or third-party packages in other libraries are built into TeCLI:

- **Environment variable fallback**: `[Option("key", EnvVar = "API_KEY")]`
- **Validation attributes**: `[Range]`, `[RegularExpression]`, `[FileExists]`, `[DirectoryExists]`
- **Interactive prompts**: `[Option("password", Prompt = "Enter password:", SecurePrompt = true)]`
- **Shell completion**: Bash, Zsh, PowerShell, and Fish scripts
- **Pre/post execution hooks**: `[BeforeExecute]`, `[AfterExecute]`, `[OnError]`
- **Global options**: `[GlobalOptions]` for options shared across commands
- **Configuration files**: Auto-discovery of JSON, YAML, TOML, INI configs
- **Localization (i18n)**: `[LocalizedDescription]` with pluggable providers
- **Interactive shell (REPL)**: `[Shell]` attribute for shell mode
- **Progress UI**: Auto-injected `IProgressContext` for progress bars and spinners

### First-Class DI Support
TeCLI provides extensions for popular dependency injection containers:

- Microsoft.Extensions.DependencyInjection
- Autofac
- SimpleInjector
- Jab (source-generated DI)
- PureDI

## Quick Comparison

| Feature | System.CommandLine | CommandLineParser | Spectre.Console.Cli | McMaster | TeCLI |
|---------|-------------------|-------------------|---------------------|----------|-------|
| API Style | Fluent builder | Attributes | Class inheritance | Attributes | Attributes |
| Source generation | Partial | No | No | No | Full |
| Compile-time checks | No | No | No | No | 32 analyzers |
| Environment variables | Manual | No | Manual | Manual | Built-in |
| Validation | Custom | Manual | Override method | Attributes | Attributes |
| Shell completion | Yes | No | No | Limited | Yes |
| Interactive prompts | No | No | Via Spectre.Console | Limited | Built-in |
| DI extensions | Manual | No | TypeRegistrar | Constructor | Extensions |
| Pre/post hooks | Middleware | No | Interceptors | No | Attributes |
| Configuration files | Manual | No | No | No | Auto-discovery |
| Localization (i18n) | No | No | No | No | Attribute-based |
| Interactive shell | No | No | No | No | Built-in |
| Progress UI | No | No | Spectre.Console | No | Auto-injected |

## General Migration Steps

Regardless of which library you're migrating from, the general steps are:

1. **Install TeCLI:**
   ```bash
   dotnet add package TeCLI
   ```

2. **Optional - Install DI extension:**
   ```bash
   dotnet add package TeCLI.Extensions.DependencyInjection
   ```

3. **Remove the old library:**
   ```bash
   dotnet remove package <old-library>
   ```

4. **Convert your commands** following the specific migration guide

5. **Update your entry point:**
   ```csharp
   var dispatcher = new CommandDispatcher();
   return await dispatcher.DispatchAsync(args);
   ```

6. **Fix any analyzer warnings** - TeCLI's analyzers will help catch issues

7. **Test your application** to verify behavior matches

## Concept Mapping Overview

| Concept | System.CommandLine | CommandLineParser | Spectre.Console.Cli | McMaster | TeCLI |
|---------|-------------------|-------------------|---------------------|----------|-------|
| Command | `Command` | `[Verb]` | `Command<T>` | `[Command]` | `[Command]` |
| Subcommand | `AddCommand()` | Multiple verbs | `AddBranch()` | `[Subcommand]` | Nested class |
| Option | `Option<T>` | `[Option]` | `[CommandOption]` | `[Option]` | `[Option]` |
| Argument | `Argument<T>` | `[Value]` | `[CommandArgument]` | `[Argument]` | `[Argument]` |
| Handler | `SetHandler()` | `WithParsed()` | `Execute()` | `OnExecute()` | `[Primary]`/`[Action]` |
| Required | `IsRequired` | `Required=true` | `Validate()` | `[Required]` | `Required=true` |

## Need Help?

If you encounter issues during migration or have questions:

1. Check the specific migration guide for your library
2. Review the [main TeCLI documentation](../../README.md)
3. Look at the [example projects](../../examples/) for reference implementations
4. Open an issue on GitHub

## Contributing

Found a missing pattern or improvement for these guides? Contributions are welcome! Please submit a pull request or open an issue.
