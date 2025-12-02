# Migrating from CommandLineParser to TeCLI

This guide helps you migrate your CLI application from [CommandLineParser](https://github.com/commandlineparser/commandline) to TeCLI. Both libraries use an attribute-based approach, making the migration relatively straightforward.

## Key Differences

| Aspect | CommandLineParser | TeCLI |
|--------|------------------|-------|
| Parsing | Runtime reflection | Compile-time source generation |
| Options Classes | Separate from commands | Combined in command classes |
| Verbs | Separate classes | `[Action]` methods or nested classes |
| Error Handling | `NotParsed<T>` | Exception-based with `[OnError]` hooks |
| Result Pattern | `ParserResult<T>` | Direct method invocation |

## Concept Mapping

| CommandLineParser | TeCLI | Notes |
|-------------------|-------|-------|
| `[Verb]` | `[Command]` / `[Action]` | Commands and subcommands |
| `[Option]` | `[Option]` | Named parameters |
| `[Value]` | `[Argument]` | Positional parameters |
| Options class | Command class | Combined definition |
| `Parser.Default.ParseArguments` | `CommandDispatcher.DispatchAsync` | Entry point |
| `HelpText` | Auto-generated | Built-in help system |
| `[Required]` | `Required = true` | Property on `[Option]` |
| `SetName` (mutually exclusive) | `[MutuallyExclusive]` | Attribute-based groups |

## Migration Examples

### Basic Options Class

**CommandLineParser:**
```csharp
public class Options
{
    [Option('v', "verbose", Required = false, HelpText = "Enable verbose output.")]
    public bool Verbose { get; set; }

    [Option('n', "name", Required = true, HelpText = "Your name.")]
    public string Name { get; set; } = "";

    [Option('c', "count", Default = 1, HelpText = "Repeat count.")]
    public int Count { get; set; }
}

// Usage
Parser.Default.ParseArguments<Options>(args)
    .WithParsed(opts =>
    {
        for (int i = 0; i < opts.Count; i++)
            Console.WriteLine($"Hello, {opts.Name}!");
    });
```

**TeCLI:**
```csharp
[Command("greet", Description = "Greeting application")]
public class GreetCommand
{
    [Primary]
    public void Execute(
        [Option("verbose", ShortName = 'v', Description = "Enable verbose output.")]
        bool verbose = false,

        [Option("name", ShortName = 'n', Required = true, Description = "Your name.")]
        string name = default!,

        [Option("count", ShortName = 'c', Description = "Repeat count.")]
        int count = 1)
    {
        for (int i = 0; i < count; i++)
            Console.WriteLine($"Hello, {name}!");
    }
}

// Usage
var dispatcher = new CommandDispatcher();
await dispatcher.DispatchAsync(args);
```

### Positional Values (Arguments)

**CommandLineParser:**
```csharp
public class Options
{
    [Value(0, MetaName = "input", Required = true, HelpText = "Input file path.")]
    public string InputFile { get; set; } = "";

    [Value(1, MetaName = "output", Required = false, HelpText = "Output file path.")]
    public string? OutputFile { get; set; }
}
```

**TeCLI:**
```csharp
[Primary]
public void Execute(
    [Argument(Description = "Input file path.")] string inputFile,
    [Argument(Description = "Output file path.")] string? outputFile = null)
{
    // Implementation
}
```

### Verbs (Subcommands)

**CommandLineParser:**
```csharp
[Verb("add", HelpText = "Add file contents to the index.")]
public class AddOptions
{
    [Value(0, MetaName = "files", Required = true)]
    public IEnumerable<string> Files { get; set; } = Array.Empty<string>();

    [Option('f', "force", HelpText = "Allow adding ignored files.")]
    public bool Force { get; set; }
}

[Verb("commit", HelpText = "Record changes to the repository.")]
public class CommitOptions
{
    [Option('m', "message", Required = true, HelpText = "Commit message.")]
    public string Message { get; set; } = "";

    [Option('a', "all", HelpText = "Commit all changed files.")]
    public bool All { get; set; }
}

// Usage
Parser.Default.ParseArguments<AddOptions, CommitOptions>(args)
    .WithParsed<AddOptions>(opts => RunAdd(opts))
    .WithParsed<CommitOptions>(opts => RunCommit(opts));
```

**TeCLI:**
```csharp
[Command("git", Description = "Git-like version control")]
public class GitCommand
{
    [Action("add", Description = "Add file contents to the index.")]
    public void Add(
        [Argument(Description = "Files to add")] string[] files,
        [Option("force", ShortName = 'f', Description = "Allow adding ignored files.")] bool force = false)
    {
        foreach (var file in files)
            Console.WriteLine($"Adding {file}" + (force ? " (forced)" : ""));
    }

    [Action("commit", Description = "Record changes to the repository.")]
    public void Commit(
        [Option("message", ShortName = 'm', Required = true, Description = "Commit message.")]
        string message,
        [Option("all", ShortName = 'a', Description = "Commit all changed files.")]
        bool all = false)
    {
        Console.WriteLine($"Committing{(all ? " all" : "")}: {message}");
    }
}
```

### Collections and Sequences

**CommandLineParser:**
```csharp
public class Options
{
    [Option('t', "tags", Separator = ',', HelpText = "Tags for the item.")]
    public IEnumerable<string> Tags { get; set; } = Array.Empty<string>();

    [Option('p', "ports", HelpText = "Port numbers.")]
    public IEnumerable<int> Ports { get; set; } = Array.Empty<int>();
}

// Usage: --tags tag1,tag2,tag3 --ports 80 --ports 443
```

**TeCLI:**
```csharp
[Primary]
public void Execute(
    [Option("tags", ShortName = 't', Description = "Tags for the item.")]
    string[] tags,

    [Option("ports", ShortName = 'p', Description = "Port numbers.")]
    int[] ports)
{
    // TeCLI supports both repeated options and comma-separated values
    // Usage: --tags tag1 --tags tag2 OR --tags tag1,tag2,tag3
}
```

### Mutually Exclusive Options

**CommandLineParser:**
```csharp
public class Options
{
    [Option('f', "file", SetName = "source", HelpText = "Read from file.")]
    public string? File { get; set; }

    [Option('u', "url", SetName = "source", HelpText = "Read from URL.")]
    public string? Url { get; set; }

    [Option('s', "stdin", SetName = "source", HelpText = "Read from stdin.")]
    public bool Stdin { get; set; }
}
```

**TeCLI:**
```csharp
[Primary]
public void Execute(
    [Option("file", ShortName = 'f', Description = "Read from file.")]
    [MutuallyExclusive("source")]
    string? file,

    [Option("url", ShortName = 'u', Description = "Read from URL.")]
    [MutuallyExclusive("source")]
    string? url,

    [Option("stdin", ShortName = 's', Description = "Read from stdin.")]
    [MutuallyExclusive("source")]
    bool stdin = false)
{
    // Only one of file, url, or stdin can be specified
}
```

### Enum Options

**CommandLineParser:**
```csharp
public enum LogLevel { Debug, Info, Warning, Error }

public class Options
{
    [Option('l', "log-level", Default = LogLevel.Info, HelpText = "Logging level.")]
    public LogLevel LogLevel { get; set; }
}
```

**TeCLI:**
```csharp
public enum LogLevel { Debug, Info, Warning, Error }

[Primary]
public void Execute(
    [Option("log-level", ShortName = 'l', Description = "Logging level.")]
    LogLevel logLevel = LogLevel.Info)
{
    // Enum parsing is automatic and case-insensitive
}
```

### Hidden Options

**CommandLineParser:**
```csharp
public class Options
{
    [Option("debug-mode", Hidden = true)]
    public bool DebugMode { get; set; }
}
```

**TeCLI:**
```csharp
[Primary]
public void Execute(
    [Option("debug-mode", Hidden = true)]
    bool debugMode = false)
{
    // Hidden options work but won't appear in help text
}
```

### Error Handling

**CommandLineParser:**
```csharp
Parser.Default.ParseArguments<Options>(args)
    .WithParsed(opts => Run(opts))
    .WithNotParsed(errors =>
    {
        foreach (var error in errors)
        {
            Console.WriteLine($"Error: {error}");
        }
        Environment.Exit(1);
    });
```

**TeCLI:**
```csharp
[Command("app")]
public class AppCommand
{
    [OnError]
    public int HandleError(Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return 1;
    }

    [Primary]
    public void Execute([Option("value", Required = true)] string value)
    {
        // Implementation
    }
}
```

### Default Verb

**CommandLineParser:**
```csharp
[Verb("run", isDefault: true, HelpText = "Run the application.")]
public class RunOptions
{
    // ...
}
```

**TeCLI:**
```csharp
[Command("app")]
public class AppCommand
{
    [Primary]  // This is the default action when no verb is specified
    [Action("run", Description = "Run the application.")]
    public void Run()
    {
        // Implementation
    }
}
```

### Custom Help Text

**CommandLineParser:**
```csharp
public class Options
{
    [Option('h', "help")]
    public bool Help { get; set; }
}

// Manual help text generation
var helpText = HelpText.AutoBuild(result);
Console.WriteLine(helpText);
```

**TeCLI:**
```csharp
// Help is automatically generated and available via --help or -h
// Customize descriptions through attribute properties:

[Command("app", Description = "My awesome application")]
public class AppCommand
{
    [Action("process", Description = "Process input data")]
    public void Process(
        [Option("input", ShortName = 'i', Description = "Input file path")]
        string input,

        [Option("verbose", ShortName = 'v', Description = "Enable verbose output")]
        bool verbose = false)
    {
        // Help text is auto-generated from these descriptions
    }
}
```

## Step-by-Step Migration

1. **Install TeCLI:**
   ```bash
   dotnet add package TeCLI
   ```

2. **Remove CommandLineParser:**
   ```bash
   dotnet remove package CommandLineParser
   ```

3. **Convert Options Classes:**
   - Create a new class with `[Command]` attribute
   - Move options from properties to method parameters with `[Option]`
   - Move values from properties to method parameters with `[Argument]`
   - Add `[Primary]` or `[Action]` to the method

4. **Convert Verbs to Actions:**
   - Each `[Verb]` class becomes either:
     - An `[Action]` method on the main command, or
     - A nested class with `[Command]` attribute

5. **Update Entry Point:**
   ```csharp
   // Replace ParseArguments with DispatchAsync
   var dispatcher = new CommandDispatcher();
   return await dispatcher.DispatchAsync(args);
   ```

6. **Migrate Error Handling:**
   - Replace `WithNotParsed` with `[OnError]` hooks
   - TeCLI throws exceptions for validation errors

7. **Update Return Types:**
   - Actions can return `void`, `int`, `Task`, `Task<int>`, or `ValueTask` variants
   - Return value becomes the exit code

## Attribute Comparison

| CommandLineParser | TeCLI | Example |
|-------------------|-------|---------|
| `[Verb("name")]` | `[Command("name")]` or `[Action("name")]` | Subcommands |
| `[Option('s', "long")]` | `[Option("long", ShortName = 's')]` | Named options |
| `[Value(0)]` | `[Argument]` | Positional args |
| `Required = true` | `Required = true` | Same property |
| `Default = value` | Parameter default | `string x = "default"` |
| `HelpText = "..."` | `Description = "..."` | Help text |
| `Hidden = true` | `Hidden = true` | Same property |
| `SetName = "group"` | `[MutuallyExclusive("group")]` | Exclusive options |
| `Separator = ','` | Automatic | Built-in support |

## Features Comparison

| Feature | CommandLineParser | TeCLI |
|---------|------------------|-------|
| Options | ✅ | ✅ |
| Positional arguments | ✅ | ✅ |
| Verbs/Subcommands | ✅ | ✅ |
| Nested commands | ⚠️ Limited | ✅ Full support |
| Short options | ✅ | ✅ |
| Required options | ✅ | ✅ |
| Default values | ✅ | ✅ |
| Collections | ✅ | ✅ |
| Enums | ✅ | ✅ |
| Flags enums | ✅ | ✅ |
| Mutually exclusive | ✅ | ✅ |
| Hidden options | ✅ | ✅ |
| Help generation | ✅ | ✅ Auto-generated |
| Environment variables | ❌ | ✅ Built-in |
| Validation | ⚠️ Manual | ✅ Attribute-based |
| Compile-time checks | ❌ | ✅ 32 analyzers |
| Source generation | ❌ | ✅ Zero reflection |
| Interactive prompts | ❌ | ✅ |
| Dependency injection | ❌ | ✅ Extensions |
| Shell completion | ❌ | ✅ |
| Async support | ⚠️ Manual | ✅ Native |
| Pre/post hooks | ❌ | ✅ |
| Configuration files | ❌ | ✅ Auto-discovery |
| Localization (i18n) | ❌ | ✅ Attribute-based |
| Interactive shell (REPL) | ❌ | ✅ Built-in |
| Progress UI | ❌ | ✅ Auto-injected |
| Structured output (JSON/XML/YAML/Table) | ❌ | ✅ Built-in |

## Benefits of Migration

1. **Similar Attribute Style:** Both use attributes, minimal learning curve
2. **No Reflection:** TeCLI uses source generation for better performance
3. **Compile-Time Safety:** Catch errors before running your app
4. **Built-in Validation:** `[Range]`, `[RegularExpression]`, `[FileExists]`, etc.
5. **Environment Variables:** Native support without custom code
6. **DI Integration:** First-class support for popular containers
7. **Better Nested Commands:** Full git-style command hierarchies

## Common Gotchas

1. **Properties to Parameters:** Options move from class properties to method parameters
2. **Type Inference:** Default values determine optionality, not `Required = false`
3. **No Separator Property:** TeCLI automatically handles both repeated options and comma-separated values
4. **Verb → Action:** Simple verbs become `[Action]` methods; complex ones become nested `[Command]` classes
5. **Exit Codes:** Return `int` from actions to set exit codes

## Advanced Features (TeCLI-Only)

These features have no equivalent in CommandLineParser:

### Configuration File Support

Load defaults from configuration files automatically:

```csharp
// Program.cs - merge config files with CLI args
var mergedArgs = args.WithConfiguration(appName: "myapp");
await dispatcher.DispatchAsync(mergedArgs);
```

Supports JSON, YAML, TOML, and INI formats with automatic discovery.

### Localization (i18n)

Localize descriptions and messages:

```csharp
[Command("greet")]
[LocalizedDescription("GreetCommand_Description")]
public class GreetCommand
{
    [Primary]
    public void Hello(
        [Argument]
        [LocalizedDescription("Name_Description")]
        string name)
    {
        Console.WriteLine(Localizer.GetString("Greeting", name));
    }
}
```

### Interactive Shell (REPL)

Add shell mode to commands:

```csharp
[Command("db")]
[Shell(Prompt = "db> ", EnableHistory = true)]
public class DatabaseCommand
{
    [Action("query")]
    public void Query([Argument] string sql) { }
}
```

### Progress UI

Auto-injected progress context:

```csharp
[Primary]
public async Task Process(IProgressContext progress)
{
    using var bar = progress.CreateProgressBar("Processing...", 100);
    // Update bar.Value as work progresses
}
```

### Structured Output Formatting

Format output as JSON, XML, YAML, or tables:

```csharp
using TeCLI.Output;

[Command("list")]
public class ListCommand
{
    [Action("users")]
    [OutputFormat]  // Enables --output json|xml|table|yaml
    public IEnumerable<User> ListUsers()
    {
        return _userService.GetAll();
    }
}

// Usage:
// myapp list users --output json
// myapp list users --output table
