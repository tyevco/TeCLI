# Migrating from System.CommandLine to TeCLI

This guide helps you migrate your CLI application from [System.CommandLine](https://github.com/dotnet/command-line-api) to TeCLI. While both libraries provide robust CLI parsing, TeCLI offers a simpler attribute-based API with compile-time source generation.

## Key Differences

| Aspect | System.CommandLine | TeCLI |
|--------|-------------------|-------|
| API Style | Fluent builder pattern | Attribute-based declarative |
| Code Generation | Runtime binding | Compile-time source generation |
| Type Safety | Runtime validation | Compile-time analyzers |
| Configuration | Programmatic setup | Attributes on classes/methods |
| Learning Curve | Steeper (many concepts) | Gentle (familiar patterns) |

## Concept Mapping

| System.CommandLine | TeCLI | Notes |
|-------------------|-------|-------|
| `RootCommand` | `[Command]` class | Entry point for your CLI |
| `Command` | `[Command]` (nested class) | Subcommands via nested classes |
| `Option<T>` | `[Option]` parameter | Named parameters like `--name` |
| `Argument<T>` | `[Argument]` parameter | Positional parameters |
| Handler delegate | `[Action]` / `[Primary]` method | Command execution logic |
| `IConsole` | Constructor injection | Via DI extensions |
| Middleware | `[BeforeExecute]` / `[AfterExecute]` | Pre/post execution hooks |
| `InvocationContext` | Method parameters | Direct parameter injection |

## Migration Examples

### Basic Command

**System.CommandLine:**
```csharp
var nameOption = new Option<string>(
    name: "--name",
    description: "The name to greet");

var rootCommand = new RootCommand("A greeting application");
rootCommand.AddOption(nameOption);

rootCommand.SetHandler((name) =>
{
    Console.WriteLine($"Hello, {name}!");
}, nameOption);

await rootCommand.InvokeAsync(args);
```

**TeCLI:**
```csharp
[Command("greet", Description = "A greeting application")]
public class GreetCommand
{
    [Primary]
    public void Execute(
        [Option("name", Description = "The name to greet")] string name)
    {
        Console.WriteLine($"Hello, {name}!");
    }
}

// Program.cs
var dispatcher = new CommandDispatcher();
await dispatcher.DispatchAsync(args);
```

### Options with Short Names and Defaults

**System.CommandLine:**
```csharp
var verboseOption = new Option<bool>(
    aliases: new[] { "--verbose", "-v" },
    description: "Enable verbose output",
    getDefaultValue: () => false);

var countOption = new Option<int>(
    aliases: new[] { "--count", "-c" },
    description: "Number of times to repeat",
    getDefaultValue: () => 1);

rootCommand.AddOption(verboseOption);
rootCommand.AddOption(countOption);
```

**TeCLI:**
```csharp
[Primary]
public void Execute(
    [Option("verbose", ShortName = 'v', Description = "Enable verbose output")]
    bool verbose = false,

    [Option("count", ShortName = 'c', Description = "Number of times to repeat")]
    int count = 1)
{
    // Implementation
}
```

### Positional Arguments

**System.CommandLine:**
```csharp
var fileArgument = new Argument<FileInfo>(
    name: "file",
    description: "The file to process");

var outputArgument = new Argument<DirectoryInfo>(
    name: "output",
    description: "Output directory",
    getDefaultValue: () => new DirectoryInfo("."));

rootCommand.AddArgument(fileArgument);
rootCommand.AddArgument(outputArgument);
```

**TeCLI:**
```csharp
[Primary]
public void Execute(
    [Argument(Description = "The file to process")] FileInfo file,
    [Argument(Description = "Output directory")] DirectoryInfo output = default!)
{
    output ??= new DirectoryInfo(".");
    // Implementation
}
```

### Subcommands

**System.CommandLine:**
```csharp
var rootCommand = new RootCommand("Git-like CLI");

var addCommand = new Command("add", "Add files to staging");
var fileArg = new Argument<string[]>("files");
addCommand.AddArgument(fileArg);
addCommand.SetHandler((files) =>
{
    foreach (var file in files)
        Console.WriteLine($"Adding {file}");
}, fileArg);

var commitCommand = new Command("commit", "Commit staged changes");
var messageOption = new Option<string>(new[] { "-m", "--message" }, "Commit message");
commitCommand.AddOption(messageOption);
commitCommand.SetHandler((message) =>
{
    Console.WriteLine($"Committing: {message}");
}, messageOption);

rootCommand.AddCommand(addCommand);
rootCommand.AddCommand(commitCommand);
```

**TeCLI:**
```csharp
[Command("git", Description = "Git-like CLI")]
public class GitCommand
{
    [Action("add", Description = "Add files to staging")]
    public void Add(
        [Argument(Description = "Files to add")] string[] files)
    {
        foreach (var file in files)
            Console.WriteLine($"Adding {file}");
    }

    [Action("commit", Description = "Commit staged changes")]
    public void Commit(
        [Option("message", ShortName = 'm', Description = "Commit message")]
        string message)
    {
        Console.WriteLine($"Committing: {message}");
    }
}
```

### Nested Commands (Git-style)

**System.CommandLine:**
```csharp
var rootCommand = new RootCommand();
var configCommand = new Command("config", "Configuration management");

var getCommand = new Command("get", "Get a config value");
var keyArg = new Argument<string>("key");
getCommand.AddArgument(keyArg);
getCommand.SetHandler((key) => Console.WriteLine($"Getting {key}"), keyArg);

var setCommand = new Command("set", "Set a config value");
var setKeyArg = new Argument<string>("key");
var valueArg = new Argument<string>("value");
setCommand.AddArgument(setKeyArg);
setCommand.AddArgument(valueArg);
setCommand.SetHandler((key, value) => Console.WriteLine($"Setting {key}={value}"),
    setKeyArg, valueArg);

configCommand.AddCommand(getCommand);
configCommand.AddCommand(setCommand);
rootCommand.AddCommand(configCommand);
```

**TeCLI:**
```csharp
[Command("app")]
public class AppCommand
{
    [Command("config", Description = "Configuration management")]
    public class ConfigCommand
    {
        [Action("get", Description = "Get a config value")]
        public void Get([Argument] string key)
        {
            Console.WriteLine($"Getting {key}");
        }

        [Action("set", Description = "Set a config value")]
        public void Set([Argument] string key, [Argument] string value)
        {
            Console.WriteLine($"Setting {key}={value}");
        }
    }
}
```

### Validation

**System.CommandLine:**
```csharp
var portOption = new Option<int>("--port");
portOption.AddValidator(result =>
{
    var value = result.GetValueForOption(portOption);
    if (value < 1 || value > 65535)
        result.ErrorMessage = "Port must be between 1 and 65535";
});

var emailOption = new Option<string>("--email");
emailOption.AddValidator(result =>
{
    var value = result.GetValueForOption(emailOption);
    if (!Regex.IsMatch(value ?? "", @"^[\w\.-]+@[\w\.-]+\.\w+$"))
        result.ErrorMessage = "Invalid email format";
});
```

**TeCLI:**
```csharp
[Primary]
public void Execute(
    [Option("port")]
    [Range(1, 65535)]
    int port,

    [Option("email")]
    [RegularExpression(@"^[\w\.-]+@[\w\.-]+\.\w+$", ErrorMessage = "Invalid email format")]
    string email)
{
    // Implementation
}
```

### Required Options

**System.CommandLine:**
```csharp
var apiKeyOption = new Option<string>("--api-key")
{
    IsRequired = true
};
```

**TeCLI:**
```csharp
[Option("api-key", Required = true)]
string apiKey
```

### Environment Variable Fallback

**System.CommandLine:**
```csharp
// Requires custom implementation or third-party binding
var apiKeyOption = new Option<string>("--api-key");
apiKeyOption.SetDefaultValueFactory(() =>
    Environment.GetEnvironmentVariable("API_KEY") ?? "");
```

**TeCLI:**
```csharp
[Option("api-key", EnvVar = "API_KEY")]
string apiKey
```

### Async Commands

**System.CommandLine:**
```csharp
rootCommand.SetHandler(async (url, output, ct) =>
{
    using var client = new HttpClient();
    var content = await client.GetStringAsync(url, ct);
    await File.WriteAllTextAsync(output, content, ct);
}, urlOption, outputOption, cancellationTokenBinder);
```

**TeCLI:**
```csharp
[Primary]
public async Task ExecuteAsync(
    [Option("url")] Uri url,
    [Option("output")] string output,
    CancellationToken cancellationToken)
{
    using var client = new HttpClient();
    var content = await client.GetStringAsync(url, cancellationToken);
    await File.WriteAllTextAsync(output, content, cancellationToken);
}
```

### Dependency Injection

**System.CommandLine:**
```csharp
var services = new ServiceCollection();
services.AddSingleton<IMyService, MyService>();
var serviceProvider = services.BuildServiceProvider();

rootCommand.SetHandler(async (context) =>
{
    var service = serviceProvider.GetRequiredService<IMyService>();
    await service.DoWorkAsync();
});
```

**TeCLI:**
```csharp
// Command class with constructor injection
[Command("work")]
public class WorkCommand
{
    private readonly IMyService _service;

    public WorkCommand(IMyService service)
    {
        _service = service;
    }

    [Primary]
    public async Task ExecuteAsync()
    {
        await _service.DoWorkAsync();
    }
}

// Program.cs
var services = new ServiceCollection();
services.AddSingleton<IMyService, MyService>();
services.AddCommandDispatcher(); // Generated extension method
var sp = services.BuildServiceProvider();
var dispatcher = sp.GetRequiredService<CommandDispatcher>();
await dispatcher.DispatchAsync(args);
```

## Step-by-Step Migration

1. **Install TeCLI:**
   ```bash
   dotnet add package TeCLI
   # Optional: Add DI extension if using dependency injection
   dotnet add package TeCLI.Extensions.DependencyInjection
   ```

2. **Remove System.CommandLine:**
   ```bash
   dotnet remove package System.CommandLine
   ```

3. **Create Command Classes:**
   - Convert `RootCommand` to a class with `[Command]` attribute
   - Convert `Command` objects to `[Action]` methods or nested command classes
   - Convert `Option<T>` to `[Option]` parameters
   - Convert `Argument<T>` to `[Argument]` parameters

4. **Migrate Handlers:**
   - Move handler logic into `[Action]` or `[Primary]` methods
   - Parameters are automatically injected

5. **Update Entry Point:**
   ```csharp
   // Replace InvokeAsync with DispatchAsync
   var dispatcher = new CommandDispatcher();
   return await dispatcher.DispatchAsync(args);
   ```

6. **Migrate Validation:**
   - Replace custom validators with `[Range]`, `[RegularExpression]`, `[FileExists]`, `[DirectoryExists]`
   - For complex validation, use `[BeforeExecute]` hooks

7. **Test Your Application:**
   - TeCLI analyzers will flag issues at compile time
   - Run your test suite to verify behavior

## Features Comparison

| Feature | System.CommandLine | TeCLI |
|---------|-------------------|-------|
| Options | ✅ | ✅ |
| Arguments | ✅ | ✅ |
| Subcommands | ✅ | ✅ |
| Nested commands | ✅ | ✅ |
| Aliases | ✅ | ✅ |
| Required options | ✅ | ✅ |
| Default values | ✅ | ✅ |
| Validation | ✅ Custom | ✅ Attribute-based |
| Environment variables | ⚠️ Manual | ✅ Built-in |
| Async support | ✅ | ✅ |
| Cancellation tokens | ✅ | ✅ |
| Dependency injection | ⚠️ Manual | ✅ Extensions |
| Shell completion | ✅ | ✅ |
| Help generation | ✅ | ✅ |
| Middleware | ✅ | ✅ Hooks |
| Source generation | ⚠️ Partial | ✅ Full |
| Compile-time validation | ❌ | ✅ 32 analyzers |
| Interactive prompts | ❌ | ✅ |
| Global options | ✅ | ✅ |
| Configuration files | ⚠️ Manual | ✅ Auto-discovery |
| Localization (i18n) | ❌ | ✅ Attribute-based |
| Interactive shell (REPL) | ❌ | ✅ Built-in |
| Progress UI | ❌ | ✅ Auto-injected |
| Structured output (JSON/XML/YAML/Table) | ❌ | ✅ Built-in |

## Benefits of Migration

1. **Simpler Code:** Attribute-based API reduces boilerplate significantly
2. **Compile-Time Safety:** 32 analyzers catch errors before runtime
3. **Better Performance:** Full source generation means zero reflection
4. **Built-in Features:** Environment variables, validation, and prompts included
5. **DI Integration:** First-class support for popular DI containers
6. **Familiar Patterns:** If you've used ASP.NET Core, the patterns feel natural

## Common Gotchas

1. **No Global `this` Binding:** In TeCLI, command logic lives in methods, not handlers
2. **Primary Action:** Use `[Primary]` for the default action when no subcommand is specified
3. **Nested Classes:** Subcommands in TeCLI use nested classes, not AddCommand()
4. **Return Types:** Actions can return `void`, `int`, `Task`, `Task<int>`, or `ValueTask` variants

## Advanced Features (TeCLI-Only)

These features have no direct equivalent in System.CommandLine:

### Configuration File Support

TeCLI can automatically load settings from configuration files:

```csharp
// Program.cs - merge config files with CLI args
var mergedArgs = args.WithConfiguration(appName: "myapp");
await dispatcher.DispatchAsync(mergedArgs);
```

Supports JSON, YAML, TOML, and INI formats with automatic discovery in standard locations (`~/.config/myapp/`, working directory, etc.).

### Localization (i18n)

Localize your CLI with attribute-based translations:

```csharp
[Command("greet")]
[LocalizedDescription("GreetCommand_Description")]
public class GreetCommand
{
    [Primary]
    [LocalizedDescription("GreetCommand_Hello_Description")]
    public void Hello(
        [Argument]
        [LocalizedDescription("GreetCommand_Name_Description")]
        string name)
    {
        Console.WriteLine(Localizer.GetString("Greeting_Hello", name));
    }
}

// Program.cs
Localizer.Initialize(new ResourceLocalizationProvider(typeof(Strings)), args);
```

### Interactive Shell (REPL)

Add an interactive shell mode to your CLI:

```csharp
[Command("db")]
[Shell(Prompt = "db> ", WelcomeMessage = "Database Shell", EnableHistory = true)]
public class DatabaseCommand
{
    [Action("query")]
    public void Query([Argument] string sql) { /* ... */ }

    [Action("tables")]
    public void Tables() { /* ... */ }
}
// Running `myapp db` without arguments enters interactive shell mode
```

### Progress UI with Auto-Injection

Display progress bars and spinners with auto-injected context:

```csharp
[Action("download")]
public async Task Download(
    [Argument] string url,
    IProgressContext progress)  // Auto-injected by framework
{
    using var bar = progress.CreateProgressBar("Downloading...", maxValue: 100);
    for (int i = 0; i <= 100; i += 10)
    {
        bar.Value = i;
        await Task.Delay(100);
    }
    bar.Complete("Download complete!");
}
```

### Structured Output Formatting

Format output as JSON, XML, YAML, or tables with a single attribute:

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
// myapp list users -o yaml
```

Or use the fluent API directly:

```csharp
OutputContext.Create()
    .WithFormat(OutputFormat.Json)
    .WriteTo(Console.Out)
    .Write(users);
