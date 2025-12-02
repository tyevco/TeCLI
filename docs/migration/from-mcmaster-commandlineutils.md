# Migrating from McMaster.Extensions.CommandLineUtils to TeCLI

This guide helps you migrate your CLI application from [McMaster.Extensions.CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils) to TeCLI. Both libraries use attribute-based APIs, making the migration straightforward. TeCLI offers compile-time source generation for better performance and type safety.

## Key Differences

| Aspect | McMaster.CommandLineUtils | TeCLI |
|--------|--------------------------|-------|
| Execution Model | Property-based with `OnExecute` | Method-based with `[Action]` |
| Type Binding | Runtime reflection | Compile-time source generation |
| Configuration | Attribute + `Configure()` | Pure attribute-based |
| Base Class | `CommandLineApplication` | Plain classes with `[Command]` |
| Subcommands | `[Subcommand]` attribute | Nested classes with `[Command]` |

## Concept Mapping

| McMaster.CommandLineUtils | TeCLI | Notes |
|--------------------------|-------|-------|
| `[Command]` | `[Command]` | Same attribute name |
| `[Option]` | `[Option]` | Same attribute name |
| `[Argument]` | `[Argument]` | Same attribute name |
| `[Subcommand]` | Nested `[Command]` class | Nested class approach |
| `OnExecute()` / `OnExecuteAsync()` | `[Primary]` / `[Action]` method | Execution entry point |
| `CommandLineApplication` | `CommandDispatcher` | Entry point |
| `IConsole` | Constructor injection | Via DI extensions |
| `[HelpOption]` | Auto-generated | Built-in `--help` |
| `[VersionOption]` | `[Action("version")]` | Define manually |

## Migration Examples

### Basic Command

**McMaster.CommandLineUtils:**
```csharp
[Command(Name = "greet", Description = "A greeting application")]
[HelpOption("-h|--help")]
public class GreetCommand
{
    [Argument(0, Description = "Name of the person to greet")]
    public string Name { get; set; } = "";

    [Option("-c|--count", Description = "Number of times to greet")]
    public int Count { get; set; } = 1;

    [Option("-v|--verbose", Description = "Enable verbose output")]
    public bool Verbose { get; set; }

    private void OnExecute()
    {
        for (int i = 0; i < Count; i++)
        {
            if (Verbose)
                Console.WriteLine($"Greeting #{i + 1}:");
            Console.WriteLine($"Hello, {Name}!");
        }
    }
}

// Program.cs
return CommandLineApplication.Execute<GreetCommand>(args);
```

**TeCLI:**
```csharp
[Command("greet", Description = "A greeting application")]
public class GreetCommand
{
    [Primary]
    public void Execute(
        [Argument(Description = "Name of the person to greet")] string name,
        [Option("count", ShortName = 'c', Description = "Number of times to greet")] int count = 1,
        [Option("verbose", ShortName = 'v', Description = "Enable verbose output")] bool verbose = false)
    {
        for (int i = 0; i < count; i++)
        {
            if (verbose)
                Console.WriteLine($"Greeting #{i + 1}:");
            Console.WriteLine($"Hello, {name}!");
        }
    }
}

// Program.cs
var dispatcher = new CommandDispatcher();
return await dispatcher.DispatchAsync(args);
```

### Multiple Arguments

**McMaster.CommandLineUtils:**
```csharp
[Command]
public class CopyCommand
{
    [Argument(0, Description = "Source file")]
    public string Source { get; set; } = "";

    [Argument(1, Description = "Destination file")]
    public string Destination { get; set; } = "";

    [Option("-f|--force", Description = "Overwrite existing files")]
    public bool Force { get; set; }

    private void OnExecute()
    {
        Console.WriteLine($"Copying {Source} to {Destination}");
    }
}
```

**TeCLI:**
```csharp
[Command("copy", Description = "Copy files")]
public class CopyCommand
{
    [Primary]
    public void Execute(
        [Argument(Description = "Source file")] string source,
        [Argument(Description = "Destination file")] string destination,
        [Option("force", ShortName = 'f', Description = "Overwrite existing files")] bool force = false)
    {
        Console.WriteLine($"Copying {source} to {destination}");
    }
}
```

### Subcommands

**McMaster.CommandLineUtils:**
```csharp
[Command]
[Subcommand(typeof(AddCommand), typeof(RemoveCommand), typeof(ListCommand))]
public class PackageCommand
{
    private int OnExecute(CommandLineApplication app)
    {
        app.ShowHelp();
        return 1;
    }
}

[Command("add", Description = "Add a package")]
public class AddCommand
{
    [Argument(0)]
    public string PackageName { get; set; } = "";

    [Option("-v|--version")]
    public string? Version { get; set; }

    private void OnExecute()
    {
        Console.WriteLine($"Adding {PackageName} {Version ?? "latest"}");
    }
}

[Command("remove", Description = "Remove a package")]
public class RemoveCommand
{
    [Argument(0)]
    public string PackageName { get; set; } = "";

    private void OnExecute()
    {
        Console.WriteLine($"Removing {PackageName}");
    }
}

[Command("list", Description = "List packages")]
public class ListCommand
{
    [Option("--outdated")]
    public bool Outdated { get; set; }

    private void OnExecute()
    {
        Console.WriteLine($"Listing packages{(Outdated ? " (outdated only)" : "")}");
    }
}
```

**TeCLI:**
```csharp
[Command("package", Description = "Package management")]
public class PackageCommand
{
    [Primary]
    public int ShowHelp()
    {
        Console.WriteLine("Usage: package <command>");
        return 1;
    }

    [Action("add", Description = "Add a package")]
    public void Add(
        [Argument(Description = "Package name")] string packageName,
        [Option("version", ShortName = 'v', Description = "Package version")] string? version = null)
    {
        Console.WriteLine($"Adding {packageName} {version ?? "latest"}");
    }

    [Action("remove", Description = "Remove a package")]
    public void Remove(
        [Argument(Description = "Package name")] string packageName)
    {
        Console.WriteLine($"Removing {packageName}");
    }

    [Action("list", Description = "List packages")]
    public void List(
        [Option("outdated", Description = "Show only outdated packages")] bool outdated = false)
    {
        Console.WriteLine($"Listing packages{(outdated ? " (outdated only)" : "")}");
    }
}
```

### Nested Subcommands

**McMaster.CommandLineUtils:**
```csharp
[Command]
[Subcommand(typeof(ConfigCommand))]
public class AppCommand { }

[Command("config")]
[Subcommand(typeof(GetCommand), typeof(SetCommand))]
public class ConfigCommand { }

[Command("get")]
public class GetCommand
{
    [Argument(0)]
    public string Key { get; set; } = "";

    private void OnExecute() => Console.WriteLine($"Getting {Key}");
}

[Command("set")]
public class SetCommand
{
    [Argument(0)]
    public string Key { get; set; } = "";

    [Argument(1)]
    public string Value { get; set; } = "";

    private void OnExecute() => Console.WriteLine($"Setting {Key}={Value}");
}
```

**TeCLI:**
```csharp
[Command("app")]
public class AppCommand
{
    [Command("config", Description = "Configuration management")]
    public class ConfigCommand
    {
        [Action("get", Description = "Get a configuration value")]
        public void Get([Argument(Description = "Config key")] string key)
        {
            Console.WriteLine($"Getting {key}");
        }

        [Action("set", Description = "Set a configuration value")]
        public void Set(
            [Argument(Description = "Config key")] string key,
            [Argument(Description = "Config value")] string value)
        {
            Console.WriteLine($"Setting {key}={value}");
        }
    }
}
```

### Required Options

**McMaster.CommandLineUtils:**
```csharp
[Command]
public class DeployCommand
{
    [Option("--api-key", Description = "API key for deployment")]
    [Required]
    public string ApiKey { get; set; } = "";

    [Option("-e|--environment", Description = "Target environment")]
    [Required]
    public string Environment { get; set; } = "";

    private void OnExecute()
    {
        Console.WriteLine($"Deploying to {Environment}");
    }
}
```

**TeCLI:**
```csharp
[Command("deploy", Description = "Deploy application")]
public class DeployCommand
{
    [Primary]
    public void Execute(
        [Option("api-key", Required = true, Description = "API key for deployment")]
        string apiKey,

        [Option("environment", ShortName = 'e', Required = true, Description = "Target environment")]
        string environment)
    {
        Console.WriteLine($"Deploying to {environment}");
    }
}
```

### Validation

**McMaster.CommandLineUtils:**
```csharp
[Command]
public class ServerCommand
{
    [Option("-p|--port")]
    [Range(1, 65535)]
    public int Port { get; set; } = 8080;

    [Option("-e|--email")]
    [EmailAddress]
    public string? Email { get; set; }

    [Option("-c|--config")]
    [FileExists]
    public string? ConfigFile { get; set; }

    private void OnExecute()
    {
        Console.WriteLine($"Starting server on port {Port}");
    }
}
```

**TeCLI:**
```csharp
[Command("server", Description = "Start server")]
public class ServerCommand
{
    [Primary]
    public void Execute(
        [Option("port", ShortName = 'p', Description = "Server port")]
        [Range(1, 65535)]
        int port = 8080,

        [Option("email", ShortName = 'e', Description = "Admin email")]
        [RegularExpression(@"^[\w\.-]+@[\w\.-]+\.\w+$", ErrorMessage = "Invalid email")]
        string? email = null,

        [Option("config", ShortName = 'c', Description = "Config file path")]
        [FileExists]
        string? configFile = null)
    {
        Console.WriteLine($"Starting server on port {port}");
    }
}
```

### Async Commands

**McMaster.CommandLineUtils:**
```csharp
[Command]
public class DownloadCommand
{
    [Argument(0)]
    public string Url { get; set; } = "";

    [Option("-o|--output")]
    public string? OutputFile { get; set; }

    private async Task<int> OnExecuteAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        var content = await client.GetStringAsync(Url, cancellationToken);

        if (OutputFile != null)
            await File.WriteAllTextAsync(OutputFile, content, cancellationToken);
        else
            Console.WriteLine(content);

        return 0;
    }
}
```

**TeCLI:**
```csharp
[Command("download", Description = "Download content from URL")]
public class DownloadCommand
{
    [Primary]
    public async Task<int> ExecuteAsync(
        [Argument(Description = "URL to download")] Uri url,
        [Option("output", ShortName = 'o', Description = "Output file")] string? outputFile,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        var content = await client.GetStringAsync(url, cancellationToken);

        if (outputFile != null)
            await File.WriteAllTextAsync(outputFile, content, cancellationToken);
        else
            Console.WriteLine(content);

        return 0;
    }
}
```

### Dependency Injection

**McMaster.CommandLineUtils:**
```csharp
[Command]
public class MyCommand
{
    private readonly IMyService _service;
    private readonly ILogger<MyCommand> _logger;

    public MyCommand(IMyService service, ILogger<MyCommand> logger)
    {
        _service = service;
        _logger = logger;
    }

    private void OnExecute()
    {
        _logger.LogInformation("Starting work");
        _service.DoWork();
    }
}

// Program.cs
var services = new ServiceCollection();
services.AddSingleton<IMyService, MyService>();
services.AddLogging();

var app = new CommandLineApplication<MyCommand>();
app.Conventions
    .UseDefaultConventions()
    .UseConstructorInjection(services.BuildServiceProvider());

return app.Execute(args);
```

**TeCLI:**
```csharp
[Command("my", Description = "My command")]
public class MyCommand
{
    private readonly IMyService _service;
    private readonly ILogger<MyCommand> _logger;

    public MyCommand(IMyService service, ILogger<MyCommand> logger)
    {
        _service = service;
        _logger = logger;
    }

    [Primary]
    public void Execute()
    {
        _logger.LogInformation("Starting work");
        _service.DoWork();
    }
}

// Program.cs
var services = new ServiceCollection();
services.AddSingleton<IMyService, MyService>();
services.AddLogging();
services.AddCommandDispatcher();  // Generated extension method

var sp = services.BuildServiceProvider();
var dispatcher = sp.GetRequiredService<CommandDispatcher>();
return await dispatcher.DispatchAsync(args);
```

### Remaining Arguments

**McMaster.CommandLineUtils:**
```csharp
[Command]
public class ExecCommand
{
    [Argument(0)]
    public string Program { get; set; } = "";

    [Option("--")]
    public string[] RemainingArguments { get; set; } = Array.Empty<string>();

    private void OnExecute()
    {
        Console.WriteLine($"Running: {Program} {string.Join(" ", RemainingArguments)}");
    }
}
```

**TeCLI:**
```csharp
[Command("exec", Description = "Execute a program")]
public class ExecCommand
{
    [Primary]
    public void Execute(
        [Argument(Description = "Program to run")] string program,
        [Argument(Description = "Program arguments")] string[] args)
    {
        Console.WriteLine($"Running: {program} {string.Join(" ", args)}");
    }
}
```

### Environment Variables

**McMaster.CommandLineUtils:**
```csharp
// Requires manual implementation or separate package
[Option("--api-key")]
public string? ApiKey { get; set; }

protected override void OnExecute()
{
    ApiKey ??= Environment.GetEnvironmentVariable("API_KEY");
}
```

**TeCLI:**
```csharp
[Primary]
public void Execute(
    [Option("api-key", EnvVar = "API_KEY", Description = "API key")]
    string? apiKey)
{
    // apiKey is automatically populated from API_KEY env var if not provided
}
```

### Option Inheritance (Parent Options)

**McMaster.CommandLineUtils:**
```csharp
[Command]
[Subcommand(typeof(ChildCommand))]
public class ParentCommand
{
    [Option("-v|--verbose")]
    public bool Verbose { get; set; }
}

[Command("child")]
public class ChildCommand
{
    private ParentCommand Parent { get; set; } = null!;

    private void OnExecute()
    {
        if (Parent.Verbose)
            Console.WriteLine("Verbose mode enabled");
    }
}
```

**TeCLI:**
```csharp
// Define global options class
[GlobalOptions]
public class GlobalOptions
{
    [Option("verbose", ShortName = 'v', Description = "Enable verbose output")]
    public bool Verbose { get; set; }
}

[Command("parent")]
public class ParentCommand
{
    [Command("child")]
    public class ChildCommand
    {
        [Primary]
        public void Execute(GlobalOptions globals)
        {
            if (globals.Verbose)
                Console.WriteLine("Verbose mode enabled");
        }
    }
}
```

### Allow Multiple Values

**McMaster.CommandLineUtils:**
```csharp
[Command]
public class TagCommand
{
    [Option("-t|--tag")]
    public string[] Tags { get; set; } = Array.Empty<string>();

    [Option("-p|--port")]
    public int[] Ports { get; set; } = Array.Empty<int>();

    private void OnExecute()
    {
        Console.WriteLine($"Tags: {string.Join(", ", Tags)}");
        Console.WriteLine($"Ports: {string.Join(", ", Ports)}");
    }
}
// Usage: --tag foo --tag bar --port 80 --port 443
```

**TeCLI:**
```csharp
[Command("tag", Description = "Tag management")]
public class TagCommand
{
    [Primary]
    public void Execute(
        [Option("tag", ShortName = 't', Description = "Tags")]
        string[] tags,

        [Option("port", ShortName = 'p', Description = "Ports")]
        int[] ports)
    {
        Console.WriteLine($"Tags: {string.Join(", ", tags)}");
        Console.WriteLine($"Ports: {string.Join(", ", ports)}");
    }
}
// Usage: --tag foo --tag bar --port 80 --port 443
// Or: --tag foo,bar --port 80,443
```

## Step-by-Step Migration

1. **Install TeCLI:**
   ```bash
   dotnet add package TeCLI
   # Optional: Add DI extension if using dependency injection
   dotnet add package TeCLI.Extensions.DependencyInjection
   ```

2. **Remove McMaster.Extensions.CommandLineUtils:**
   ```bash
   dotnet remove package McMaster.Extensions.CommandLineUtils
   ```

3. **Convert Command Classes:**
   - Remove inheritance/base class dependency
   - Keep `[Command]` attribute (update properties as needed)
   - Convert `[Option]` properties to method parameters
   - Convert `[Argument]` properties to method parameters
   - Replace `OnExecute()`/`OnExecuteAsync()` with `[Primary]` method

4. **Convert Subcommands:**
   - Remove `[Subcommand]` attributes
   - Create nested classes with `[Command]` attribute
   - Or convert to `[Action]` methods on parent

5. **Convert Validation:**
   - Keep `[Range]` and `[FileExists]` (same in TeCLI)
   - Replace `[EmailAddress]` with `[RegularExpression]`
   - Keep `[DirectoryExists]` (same in TeCLI)
   - Replace `[Required]` attribute with `Required = true` property

6. **Update Entry Point:**
   ```csharp
   // Replace
   return CommandLineApplication.Execute<MyCommand>(args);

   // With
   var dispatcher = new CommandDispatcher();
   return await dispatcher.DispatchAsync(args);
   ```

7. **Migrate DI (if using):**
   - Replace `UseConstructorInjection()` with TeCLI DI extension
   - Use `services.AddCommandDispatcher()` instead

## Attribute Comparison

| McMaster | TeCLI | Notes |
|----------|-------|-------|
| `[Command("name")]` | `[Command("name")]` | Same |
| `[Option("-s\|--long")]` | `[Option("long", ShortName = 's')]` | Different format |
| `[Argument(0)]` | `[Argument]` | Position is implicit |
| `[Required]` | `Required = true` | Property instead of attribute |
| `[FileExists]` | `[FileExists]` | Same |
| `[DirectoryExists]` | `[DirectoryExists]` | Same |
| `[Range(min, max)]` | `[Range(min, max)]` | Same |
| `[Subcommand(...)]` | Nested `[Command]` class | Different approach |
| `[HelpOption]` | Auto-generated | Built-in |
| `[VersionOption]` | Manual `[Action]` | Define yourself |

## Features Comparison

| Feature | McMaster.CommandLineUtils | TeCLI |
|---------|--------------------------|-------|
| Options | ✅ | ✅ |
| Arguments | ✅ | ✅ |
| Subcommands | ✅ | ✅ |
| Nested subcommands | ✅ | ✅ |
| Aliases | ✅ | ✅ |
| Required options | ✅ | ✅ |
| Default values | ✅ | ✅ |
| Validation | ✅ | ✅ |
| Collections | ✅ | ✅ |
| Enums | ✅ | ✅ |
| Environment variables | ⚠️ Manual | ✅ Built-in |
| Async support | ✅ | ✅ |
| Cancellation tokens | ✅ | ✅ |
| Dependency injection | ✅ | ✅ Extensions |
| Help generation | ✅ | ✅ |
| Version option | ✅ | ⚠️ Manual |
| Shell completion | ⚠️ Limited | ✅ Full |
| Compile-time checks | ❌ | ✅ 32 analyzers |
| Source generation | ❌ | ✅ |
| Interactive prompts | ⚠️ Prompt<T> | ✅ Built-in |
| Pre/post hooks | ❌ | ✅ |
| Global options | ✅ Parent props | ✅ `[GlobalOptions]` |
| Configuration files | ❌ | ✅ Auto-discovery |
| Localization (i18n) | ❌ | ✅ Attribute-based |
| Interactive shell (REPL) | ❌ | ✅ Built-in |
| Progress UI | ❌ | ✅ Auto-injected |
| Structured output (JSON/XML/YAML/Table) | ❌ | ✅ Built-in |

## Benefits of Migration

1. **Similar API:** Familiar attribute-based approach
2. **Simpler Structure:** No property-based options, direct parameters
3. **Compile-Time Safety:** 32 analyzers catch errors early
4. **Better Performance:** Source generation eliminates reflection
5. **Built-in Features:** Environment variables, validation, prompts
6. **First-Class DI:** Native support for popular containers
7. **Active Hooks:** Pre/post execution hooks built-in

## Common Gotchas

1. **Properties to Parameters:** Options/arguments move from properties to method parameters
2. **No `OnExecute()`:** Use `[Primary]` or `[Action]` methods instead
3. **Option Syntax:** Use `ShortName = 'x'` instead of `-x|--long`
4. **Required Syntax:** Use `Required = true` property, not `[Required]` attribute
5. **No Parent Property:** Use `[GlobalOptions]` for shared options across commands
6. **Subcommands:** Use nested classes, not `[Subcommand]` attribute
7. **Version Option:** Define manually as an `[Action("version")]`

## Advanced Features (TeCLI-Only)

These features have no equivalent in McMaster.Extensions.CommandLineUtils:

### Configuration File Support

Auto-discover and load configuration files:

```csharp
// Program.cs
var mergedArgs = args.WithConfiguration(appName: "myapp");
await dispatcher.DispatchAsync(mergedArgs);
```

Supports JSON, YAML, TOML, and INI formats.

### Localization (i18n)

Attribute-based internationalization:

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

Add shell mode to your CLI:

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
    for (int i = 0; i <= 100; i += 10)
    {
        bar.Value = i;
        await Task.Delay(100);
    }
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
