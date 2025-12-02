# TeCLI

TeCLI is a source-generated CLI parsing library for .NET that simplifies command-line interface development. Using Roslyn source generators and custom attributes, TeCLI automatically generates type-safe parsing and dispatching logic at compile time.

## Features

- **Source Generation** - Zero-runtime reflection, all code generated at compile time
- **Attribute-Based API** - Simple, declarative command and option definitions
- **Type-Safe Parsing** - Automatic parsing of all primitive types, enums, and collections
- **Help Generation** - Automatic `--help` and `--version` text generation
- **Roslyn Analyzers** - 32 analyzers providing real-time feedback and error detection
- **Dependency Injection** - Multiple DI container integrations (Microsoft DI, Autofac, SimpleInjector, Jab, PureDI)
- **Async Support** - First-class support for async actions with `Task` and `ValueTask`
- **Short/Long Options** - Support for both `-e` and `--environment` style flags
- **Container Parameters** - Group related options into complex types
- **Nested Commands** - Git-style hierarchical command structures with unlimited nesting
- **Command Aliases** - Multiple names for commands and actions
- **Validation** - Built-in validation attributes (`[Range]`, `[RegularExpression]`, `[FileExists]`, etc.)
- **Environment Variables** - Automatic fallback to environment variables for options
- **Global Options** - Shared options across all commands
- **Shell Completion** - Generate completion scripts for Bash, Zsh, PowerShell, and Fish
- **Exit Codes** - Structured exit code support with exception mapping
- **Middleware/Hooks** - Pre/post execution hooks for authentication, logging, etc.
- **Interactive Mode** - Prompt for missing arguments with optional secure input
- **Pipeline/Stream Support** - Unix-style stdin/stdout piping with `Stream`, `TextReader`, `TextWriter`
- **Testing Utilities** - `TeCLI.Extensions.Testing` package for easy CLI testing

## Installation

```bash
dotnet add package TeCLI
```

For dependency injection support:

```bash
dotnet add package TeCLI.Extensions.DependencyInjection
```

## Quick Start

### 1. Define a Command

```csharp
using TeCLI;

[Command("greet", Description = "Greets the user")]
public class GreetCommand
{
    [Primary(Description = "Say hello")]
    public void Hello([Argument(Description = "Name to greet")] string name)
    {
        Console.WriteLine($"Hello, {name}!");
    }

    [Action("goodbye", Description = "Say goodbye")]
    public void Goodbye([Argument] string name)
    {
        Console.WriteLine($"Goodbye, {name}!");
    }
}
```

### 2. Dispatch Commands

```csharp
public class Program
{
    public static async Task Main(string[] args)
    {
        await CommandDispatcher.DispatchAsync(args);
    }
}
```

### 3. Run Your CLI

```bash
# Run primary action
myapp greet John
# Output: Hello, John!

# Run named action
myapp greet goodbye John
# Output: Goodbye, John!

# Get help
myapp --help
myapp greet --help
```

## Attributes Reference

### Command Attributes

#### `[Command("name")]`
Marks a class as a CLI command.

- **Name** (required): Command name as it appears on the command line
- **Description** (optional): Description shown in help text
- **Aliases** (optional): Alternative names for the command

```csharp
[Command("deploy", Description = "Deploy the application", Aliases = new[] { "dpl" })]
public class DeployCommand { }
```

### Action Attributes

#### `[Action("name")]`
Marks a method as a named action (subcommand).

- **Name** (required): Action name
- **Description** (optional): Description shown in help text
- **Aliases** (optional): Alternative names for the action

```csharp
[Action("start", Description = "Start the service", Aliases = new[] { "run", "begin" })]
public void Start() { }
```

#### `[Primary]`
Marks a method as the default action when no action name is specified.

- Only one `[Primary]` action allowed per command
- Can be combined with `[Action]` for both default and named invocation

```csharp
[Primary(Description = "Run the default action")]
public void Execute() { }
```

### Parameter Attributes

#### `[Option("name")]`
Marks a parameter or property as a named option.

- **Name** (required): Long option name (used with `--`)
- **ShortName** (optional): Single-character short name (used with `-`)
- **Description** (optional): Description for help text
- **Required** (optional): Mark as required (`Required = true`)
- **EnvVar** (optional): Environment variable fallback name
- **Prompt** (optional): Interactive prompt message if not provided
- **SecurePrompt** (optional): Mask input for sensitive data
- **MutuallyExclusiveSet** (optional): Group mutually exclusive options

```csharp
[Option("environment", ShortName = 'e', Description = "Target environment")]
string environment

[Option("force", ShortName = 'f')] // Boolean switch
bool force

[Option("api-key", Required = true, EnvVar = "API_KEY")]
string apiKey
```

#### `[Argument]`
Marks a parameter or property as a positional argument.

- **Description** (optional): Description for help text
- **Prompt** (optional): Interactive prompt message if not provided
- **SecurePrompt** (optional): Mask input for sensitive data
- Arguments are positional and required by default
- Use default values to make arguments optional

```csharp
[Argument(Description = "Input file path")]
string inputFile

[Argument(Description = "Output file path")]
string outputFile = "output.txt" // Optional with default

[Argument(Prompt = "Enter password", SecurePrompt = true)]
string password
```

## Supported Types

All primitive .NET types and more are supported for options and arguments:

- **Boolean**: `bool` (switches when used as options)
- **Characters**: `char`
- **Integers**: `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`
- **Floating-point**: `float`, `double`, `decimal`
- **Strings**: `string`
- **Enums**: Any enum type with case-insensitive parsing, including `[Flags]` enums
- **Collections**: `T[]`, `List<T>`, `IEnumerable<T>`, `ICollection<T>`, `IList<T>`, `IReadOnlyCollection<T>`, `IReadOnlyList<T>`
- **Common Types**: `Uri`, `DateTime`, `DateTimeOffset`, `TimeSpan`, `Guid`, `FileInfo`, `DirectoryInfo`
- **Streams**: `Stream`, `TextReader`, `TextWriter`, `StreamReader`, `StreamWriter` (for pipeline support)
- **Custom Types**: Via `ITypeConverter<T>` interface and `[TypeConverter]` attribute

## Help Text Generation

TeCLI automatically generates comprehensive help text for your CLI.

### Reserved Switches

The `--help` and `-h` switches are **reserved** and cannot be used as user-defined option names. They are available at both application and command levels.

### Application-Level Help

```bash
myapp --help
```

Shows all available commands with descriptions.

### Command-Level Help

```bash
myapp deploy --help
```

Shows:
- Command description
- Usage patterns for all actions
- Available actions with descriptions
- Options (including `--help`)

## Dependency Injection

TeCLI supports multiple DI containers through extension packages:

- `TeCLI.Extensions.DependencyInjection` - Microsoft.Extensions.DependencyInjection
- `TeCLI.Extensions.Autofac` - Autofac container
- `TeCLI.Extensions.SimpleInjector` - SimpleInjector container
- `TeCLI.Extensions.Jab` - Jab source-generated container
- `TeCLI.Extensions.PureDI` - PureDI container

### Setup with Microsoft DI

```csharp
using Microsoft.Extensions.DependencyInjection;
using TeCLI.Extensions.DependencyInjection;

IServiceCollection services = new ServiceCollection();

// Register your services
services.AddSingleton<IMyService, MyService>();

// Add command dispatcher
services.AddCommandDispatcher();

// Build and dispatch
var serviceProvider = services.BuildServiceProvider();
var dispatcher = serviceProvider.GetRequiredService<CommandDispatcher>();
await dispatcher.DispatchAsync(args);
```

### Constructor Injection in Commands

```csharp
[Command("process")]
public class ProcessCommand
{
    private readonly IMyService _service;

    public ProcessCommand(IMyService service)
    {
        _service = service;
    }

    [Primary]
    public void Execute()
    {
        _service.DoWork();
    }
}
```

## Async Support

TeCLI fully supports asynchronous actions using `Task` and `ValueTask`:

```csharp
[Command("fetch")]
public class FetchCommand
{
    [Primary]
    public async Task FetchData(string url)
    {
        using var client = new HttpClient();
        var data = await client.GetStringAsync(url);
        Console.WriteLine(data);
    }

    [Action("multiple")]
    public async ValueTask FetchMultiple(string[] urls)
    {
        // Async implementation
    }
}
```

## Container Parameters

Group related options into complex types:

```csharp
public class DeploymentOptions
{
    [Option("environment", ShortName = 'e')]
    public string Environment { get; set; }

    [Option("region", ShortName = 'r')]
    public string Region { get; set; }

    [Option("verbose", ShortName = 'v')]
    public bool Verbose { get; set; }
}

[Command("deploy")]
public class DeployCommand
{
    [Primary]
    public void Execute(DeploymentOptions options)
    {
        Console.WriteLine($"Deploying to {options.Environment} in {options.Region}");
    }
}
```

Usage:
```bash
myapp deploy -e production -r us-west --verbose
```

## Compile-Time Analyzers

TeCLI includes 32 Roslyn analyzers that provide real-time feedback during development:

### Error-Level Analyzers
- **CLI001**: Options/arguments must use supported types
- **CLI002**: Option properties must have accessible setters
- **CLI003**: Only one `[Primary]` action allowed per command
- **CLI006**: Command/action/option names cannot be empty
- **CLI007**: Action names must be unique within a command
- **CLI008**: Option names must be unique within an action
- **CLI009**: Argument positions cannot conflict
- **CLI010**: Option short names must be unique within an action
- **CLI016**: Invalid validation attribute combinations
- **CLI018**: Duplicate command names across classes
- **CLI021**: Collection argument must be in last position
- **CLI022**: Option/Argument property without setter
- **CLI030**: Option uses empty enum type

### Warning-Level Analyzers
- **CLI004**: Command names should contain only letters, numbers, and hyphens
- **CLI005**: Option names should contain only letters, numbers, and hyphens
- **CLI011**: Async methods must return `Task` or `ValueTask`
- **CLI012**: Avoid async void in action methods
- **CLI013**: Optional argument before required argument
- **CLI015**: Action method in non-command class or inaccessible
- **CLI017**: Option name conflicts with reserved switch (`--help`, `-h`)
- **CLI020**: Boolean option marked as required
- **CLI024**: Command class without action methods
- **CLI028**: Hidden option marked as required
- **CLI031**: Multiple GlobalOptions classes

### Info-Level Analyzers
- **CLI014**: Consider using container parameter for 4+ options
- **CLI019**: Missing description on Command/Option/Argument
- **CLI023**: Async action without CancellationToken
- **CLI025**: Inconsistent naming convention
- **CLI026**: Single-character option name should use ShortName
- **CLI027**: Redundant name specification
- **CLI029**: Nullable option without explicit default
- **CLI032**: Sensitive option detected (security info)

### Diagnostic Suppressor
- **CLI900**: Suppresses CS8618 nullable warnings for properties with `[Option]`/`[Argument]` attributes (generator initializes them)

## Error Handling

TeCLI provides helpful error messages for common issues:

- Missing required parameters
- Invalid option values
- Unknown commands or actions
- Type conversion failures

All error messages include a suggestion to use `--help` for guidance.

## Examples

### Basic Command with Options

```csharp
[Command("build", Description = "Build the project")]
public class BuildCommand
{
    [Primary(Description = "Build the project")]
    public void Build(
        [Option("configuration", ShortName = 'c', Description = "Build configuration")]
        string configuration = "Debug",

        [Option("output", ShortName = 'o', Description = "Output directory")]
        string output = "./bin",

        [Option("verbose", ShortName = 'v', Description = "Verbose output")]
        bool verbose = false)
    {
        Console.WriteLine($"Building in {configuration} mode...");
        if (verbose)
        {
            Console.WriteLine($"Output: {output}");
        }
    }
}
```

Usage:
```bash
myapp build -c Release -o ./dist --verbose
myapp build --configuration Release --output ./dist -v
```

### Multiple Actions

```csharp
[Command("git")]
public class GitCommand
{
    [Action("commit", Description = "Commit changes")]
    public void Commit(
        [Option("message", ShortName = 'm')] string message,
        [Option("all", ShortName = 'a')] bool all = false)
    {
        Console.WriteLine($"Committing: {message}");
    }

    [Action("push", Description = "Push to remote")]
    public async Task Push(
        [Option("force", ShortName = 'f')] bool force = false)
    {
        await Task.Run(() => Console.WriteLine("Pushing..."));
    }
}
```

Usage:
```bash
myapp git commit -m "Initial commit" --all
myapp git push --force
```

## Framework Support

TeCLI supports .NET 6.0, 7.0, 8.0, 9.0, and 10.0.

**Core Library:** Targets netstandard2.0 for maximum compatibility with source generators.

**Test & Example Projects:** Support .NET 8.0, 9.0, and 10.0.

## Contributing

Contributions are welcome! Please fork the repository, make your changes, and submit a pull request.

## Additional Resources

- **Code Coverage**: See [COVERAGE.md](COVERAGE.md) for testing and coverage guidelines
- **Benchmarks**: See [TeCLI.Benchmarks/README.md](TeCLI.Benchmarks/README.md) for performance benchmarks
- **Integration Tests**: See [TeCLI.Tests/README.md](TeCLI.Tests/README.md) for test examples
