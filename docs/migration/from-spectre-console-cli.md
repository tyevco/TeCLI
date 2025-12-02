# Migrating from Spectre.Console.Cli to TeCLI

This guide helps you migrate your CLI application from [Spectre.Console.Cli](https://spectreconsole.net/cli/) to TeCLI. Both libraries use attribute-based configuration, but TeCLI offers compile-time source generation and a more streamlined API.

## Key Differences

| Aspect | Spectre.Console.Cli | TeCLI |
|--------|---------------------|-------|
| Command Pattern | Inherit from `Command<TSettings>` | Class with `[Command]` attribute |
| Settings | Separate `CommandSettings` class | Method parameters with attributes |
| Execution | Override `Execute()` method | `[Action]` / `[Primary]` methods |
| Configuration | `CommandApp` + `Configure()` | `CommandDispatcher` (auto-discovered) |
| Parsing | Runtime | Compile-time source generation |
| Console Output | Spectre.Console integration | Separate (use any library) |

## Concept Mapping

| Spectre.Console.Cli | TeCLI | Notes |
|--------------------|-------|-------|
| `Command<TSettings>` | `[Command]` class | Command definition |
| `CommandSettings` | Method parameters | Options and arguments |
| `[CommandOption]` | `[Option]` | Named parameters |
| `[CommandArgument]` | `[Argument]` | Positional parameters |
| `[Description]` | `Description` property | On attributes |
| `CommandApp` | `CommandDispatcher` | Entry point |
| `app.Configure()` | Auto-discovered | No manual registration |
| `ITypeResolver` | DI extensions | Dependency injection |
| `[DefaultValue]` | Parameter defaults | C# default values |

## Migration Examples

### Basic Command

**Spectre.Console.Cli:**
```csharp
public class GreetSettings : CommandSettings
{
    [CommandArgument(0, "<name>")]
    [Description("Name of the person to greet")]
    public string Name { get; set; } = "";

    [CommandOption("-c|--count")]
    [Description("Number of times to greet")]
    [DefaultValue(1)]
    public int Count { get; set; }
}

public class GreetCommand : Command<GreetSettings>
{
    public override int Execute(CommandContext context, GreetSettings settings)
    {
        for (int i = 0; i < settings.Count; i++)
        {
            AnsiConsole.WriteLine($"Hello, {settings.Name}!");
        }
        return 0;
    }
}

// Program.cs
var app = new CommandApp<GreetCommand>();
return app.Run(args);
```

**TeCLI:**
```csharp
[Command("greet", Description = "Greeting command")]
public class GreetCommand
{
    [Primary]
    public int Execute(
        [Argument(Description = "Name of the person to greet")] string name,
        [Option("count", ShortName = 'c', Description = "Number of times to greet")] int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            Console.WriteLine($"Hello, {name}!");
        }
        return 0;
    }
}

// Program.cs
var dispatcher = new CommandDispatcher();
return await dispatcher.DispatchAsync(args);
```

### Multiple Commands

**Spectre.Console.Cli:**
```csharp
public class AddSettings : CommandSettings
{
    [CommandArgument(0, "<file>")]
    public string File { get; set; } = "";
}

public class AddCommand : Command<AddSettings>
{
    public override int Execute(CommandContext context, AddSettings settings)
    {
        AnsiConsole.WriteLine($"Adding {settings.File}");
        return 0;
    }
}

public class CommitSettings : CommandSettings
{
    [CommandOption("-m|--message")]
    [Description("Commit message")]
    public string? Message { get; set; }
}

public class CommitCommand : Command<CommitSettings>
{
    public override int Execute(CommandContext context, CommitSettings settings)
    {
        AnsiConsole.WriteLine($"Committing: {settings.Message}");
        return 0;
    }
}

// Program.cs
var app = new CommandApp();
app.Configure(config =>
{
    config.AddCommand<AddCommand>("add")
          .WithDescription("Add file to staging");
    config.AddCommand<CommitCommand>("commit")
          .WithDescription("Commit changes");
});
return app.Run(args);
```

**TeCLI:**
```csharp
[Command("git", Description = "Git-like version control")]
public class GitCommand
{
    [Action("add", Description = "Add file to staging")]
    public int Add(
        [Argument(Description = "File to add")] string file)
    {
        Console.WriteLine($"Adding {file}");
        return 0;
    }

    [Action("commit", Description = "Commit changes")]
    public int Commit(
        [Option("message", ShortName = 'm', Description = "Commit message")] string? message)
    {
        Console.WriteLine($"Committing: {message}");
        return 0;
    }
}

// Program.cs
var dispatcher = new CommandDispatcher();
return await dispatcher.DispatchAsync(args);
```

### Nested Commands (Branches)

**Spectre.Console.Cli:**
```csharp
public class DatabaseSettings : CommandSettings { }

public class MigrateSettings : CommandSettings
{
    [CommandOption("--target")]
    public string? Target { get; set; }
}

public class MigrateCommand : Command<MigrateSettings>
{
    public override int Execute(CommandContext context, MigrateSettings settings)
    {
        AnsiConsole.WriteLine($"Migrating to {settings.Target ?? "latest"}");
        return 0;
    }
}

public class SeedSettings : CommandSettings
{
    [CommandOption("--count")]
    [DefaultValue(100)]
    public int Count { get; set; }
}

public class SeedCommand : Command<SeedSettings>
{
    public override int Execute(CommandContext context, SeedSettings settings)
    {
        AnsiConsole.WriteLine($"Seeding {settings.Count} records");
        return 0;
    }
}

// Program.cs
var app = new CommandApp();
app.Configure(config =>
{
    config.AddBranch("database", db =>
    {
        db.AddCommand<MigrateCommand>("migrate");
        db.AddCommand<SeedCommand>("seed");
    });
});
```

**TeCLI:**
```csharp
[Command("app")]
public class AppCommand
{
    [Command("database", Description = "Database operations")]
    public class DatabaseCommand
    {
        [Action("migrate", Description = "Run database migrations")]
        public int Migrate(
            [Option("target", Description = "Target migration")] string? target)
        {
            Console.WriteLine($"Migrating to {target ?? "latest"}");
            return 0;
        }

        [Action("seed", Description = "Seed database")]
        public int Seed(
            [Option("count", Description = "Number of records")] int count = 100)
        {
            Console.WriteLine($"Seeding {count} records");
            return 0;
        }
    }
}
```

### Required Arguments and Options

**Spectre.Console.Cli:**
```csharp
public class DeploySettings : CommandSettings
{
    [CommandArgument(0, "<environment>")]
    [Description("Target environment")]
    public string Environment { get; set; } = "";

    [CommandOption("--api-key")]
    [Description("API key for deployment")]
    public string ApiKey { get; set; } = "";  // Validated in Validate()

    public override ValidationResult Validate()
    {
        if (string.IsNullOrEmpty(ApiKey))
            return ValidationResult.Error("API key is required");
        return ValidationResult.Success();
    }
}
```

**TeCLI:**
```csharp
[Primary]
public int Execute(
    [Argument(Description = "Target environment")] string environment,
    [Option("api-key", Required = true, Description = "API key for deployment")] string apiKey)
{
    // apiKey is guaranteed to be provided
    return 0;
}
```

### Validation

**Spectre.Console.Cli:**
```csharp
public class ServerSettings : CommandSettings
{
    [CommandOption("-p|--port")]
    [DefaultValue(8080)]
    public int Port { get; set; }

    [CommandOption("-e|--email")]
    public string? Email { get; set; }

    public override ValidationResult Validate()
    {
        if (Port < 1 || Port > 65535)
            return ValidationResult.Error("Port must be between 1 and 65535");

        if (Email != null && !Email.Contains("@"))
            return ValidationResult.Error("Invalid email format");

        return ValidationResult.Success();
    }
}
```

**TeCLI:**
```csharp
[Primary]
public int Execute(
    [Option("port", ShortName = 'p', Description = "Server port")]
    [Range(1, 65535)]
    int port = 8080,

    [Option("email", ShortName = 'e', Description = "Contact email")]
    [RegularExpression(@"^[\w\.-]+@[\w\.-]+\.\w+$", ErrorMessage = "Invalid email format")]
    string? email = null)
{
    return 0;
}
```

### Async Commands

**Spectre.Console.Cli:**
```csharp
public class FetchCommand : AsyncCommand<FetchSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, FetchSettings settings)
    {
        using var client = new HttpClient();
        var content = await client.GetStringAsync(settings.Url);
        AnsiConsole.WriteLine(content);
        return 0;
    }
}
```

**TeCLI:**
```csharp
[Primary]
public async Task<int> ExecuteAsync(
    [Argument(Description = "URL to fetch")] Uri url,
    CancellationToken cancellationToken)
{
    using var client = new HttpClient();
    var content = await client.GetStringAsync(url, cancellationToken);
    Console.WriteLine(content);
    return 0;
}
```

### Dependency Injection

**Spectre.Console.Cli:**
```csharp
public class MyCommand : Command<MySettings>
{
    private readonly IMyService _service;

    public MyCommand(IMyService service)
    {
        _service = service;
    }

    public override int Execute(CommandContext context, MySettings settings)
    {
        _service.DoWork();
        return 0;
    }
}

// Program.cs
var services = new ServiceCollection();
services.AddSingleton<IMyService, MyService>();
var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.AddCommand<MyCommand>("my");
});
```

**TeCLI:**
```csharp
[Command("my", Description = "My command")]
public class MyCommand
{
    private readonly IMyService _service;

    public MyCommand(IMyService service)
    {
        _service = service;
    }

    [Primary]
    public int Execute()
    {
        _service.DoWork();
        return 0;
    }
}

// Program.cs
var services = new ServiceCollection();
services.AddSingleton<IMyService, MyService>();
services.AddCommandDispatcher();  // Generated extension method
var sp = services.BuildServiceProvider();
var dispatcher = sp.GetRequiredService<CommandDispatcher>();
return await dispatcher.DispatchAsync(args);
```

### Hidden Commands

**Spectre.Console.Cli:**
```csharp
app.Configure(config =>
{
    config.AddCommand<DebugCommand>("debug")
          .IsHidden();
});
```

**TeCLI:**
```csharp
[Command("debug", Hidden = true)]
public class DebugCommand
{
    [Primary]
    public void Execute() { }
}

// Or for hidden actions:
[Action("debug", Hidden = true)]
public void Debug() { }
```

### Command Aliases

**Spectre.Console.Cli:**
```csharp
app.Configure(config =>
{
    config.AddCommand<ListCommand>("list")
          .WithAlias("ls");
});
```

**TeCLI:**
```csharp
[Action("list", Description = "List items", Aliases = new[] { "ls" })]
public void List()
{
    // Implementation
}
```

### Default Command

**Spectre.Console.Cli:**
```csharp
var app = new CommandApp<DefaultCommand>();
// or
app.Configure(config =>
{
    config.SetDefaultCommand<RunCommand>();
});
```

**TeCLI:**
```csharp
[Command("app")]
public class AppCommand
{
    [Primary]  // This executes when no subcommand is specified
    public int Execute()
    {
        Console.WriteLine("Running default action");
        return 0;
    }

    [Action("other")]
    public int Other()
    {
        return 0;
    }
}
```

### Environment Variables

**Spectre.Console.Cli:**
```csharp
// Requires manual implementation in Validate() or Execute()
public class Settings : CommandSettings
{
    [CommandOption("--api-key")]
    public string? ApiKey { get; set; }

    public override ValidationResult Validate()
    {
        ApiKey ??= Environment.GetEnvironmentVariable("API_KEY");
        return ValidationResult.Success();
    }
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

## Step-by-Step Migration

1. **Install TeCLI:**
   ```bash
   dotnet add package TeCLI
   # Optional: Add DI extension if using dependency injection
   dotnet add package TeCLI.Extensions.DependencyInjection
   ```

2. **Remove Spectre.Console.Cli:**
   ```bash
   dotnet remove package Spectre.Console.Cli
   # Keep Spectre.Console if you want to continue using it for console output
   ```

3. **Convert Command Classes:**
   - Remove inheritance from `Command<TSettings>` or `AsyncCommand<TSettings>`
   - Add `[Command]` attribute to the class
   - Move logic from `Execute()` to a method with `[Primary]` or `[Action]`

4. **Convert Settings to Parameters:**
   - Move `[CommandArgument]` properties to `[Argument]` parameters
   - Move `[CommandOption]` properties to `[Option]` parameters
   - Use C# default values instead of `[DefaultValue]`
   - Use `Description` property instead of `[Description]` attribute

5. **Convert Validation:**
   - Replace `Validate()` with TeCLI validation attributes
   - Use `[Range]`, `[RegularExpression]`, `[FileExists]`, `[DirectoryExists]`
   - For complex validation, use `[BeforeExecute]` hooks

6. **Update Configuration:**
   - Remove `CommandApp` and `Configure()` calls
   - Create `CommandDispatcher` and call `DispatchAsync()`
   - Commands are auto-discovered, no registration needed

7. **Migrate DI (if using):**
   - Replace custom `TypeRegistrar` with TeCLI DI extension
   - Use `services.AddCommandDispatcher()` instead

8. **Update Branches to Nested Commands:**
   - Replace `AddBranch()` with nested classes using `[Command]`

## Features Comparison

| Feature | Spectre.Console.Cli | TeCLI |
|---------|---------------------|-------|
| Options | ✅ | ✅ |
| Arguments | ✅ | ✅ |
| Commands | ✅ | ✅ |
| Nested commands (branches) | ✅ | ✅ |
| Aliases | ✅ | ✅ |
| Required options | ✅ | ✅ |
| Default values | ✅ | ✅ |
| Validation | ✅ Override method | ✅ Attribute-based |
| Environment variables | ⚠️ Manual | ✅ Built-in |
| Async support | ✅ | ✅ |
| Cancellation tokens | ⚠️ Manual | ✅ Native |
| Dependency injection | ✅ TypeRegistrar | ✅ Extensions |
| Hidden commands | ✅ | ✅ |
| Help generation | ✅ | ✅ |
| Shell completion | ❌ | ✅ |
| Compile-time checks | ❌ | ✅ 32 analyzers |
| Source generation | ❌ | ✅ |
| Interactive prompts | ✅ Spectre.Console | ✅ Built-in |
| Pre/post hooks | ⚠️ Interceptors | ✅ Attributes |
| Console styling | ✅ Spectre.Console | ⚠️ Bring your own |
| Global options | ✅ | ✅ |
| Configuration files | ❌ | ✅ Auto-discovery |
| Localization (i18n) | ❌ | ✅ Attribute-based |
| Interactive shell (REPL) | ❌ | ✅ Built-in |
| Progress UI | ✅ Spectre.Console | ✅ Auto-injected |
| Structured output (JSON/XML/YAML/Table) | ❌ | ✅ Built-in |

## Benefits of Migration

1. **No Manual Registration:** Commands are auto-discovered, no `Configure()` needed
2. **Compile-Time Safety:** 32 analyzers catch errors before runtime
3. **Simpler API:** No separate Settings classes, parameters are inline
4. **Built-in Features:** Environment variables, validation attributes, prompts
5. **Better Performance:** Source generation means zero reflection overhead
6. **Native Async:** `CancellationToken` is automatically injected

## Console Output

TeCLI focuses on CLI parsing and doesn't include console styling. You can continue using Spectre.Console for rich output:

```csharp
using Spectre.Console;

[Command("demo")]
public class DemoCommand
{
    [Primary]
    public void Execute([Argument] string name)
    {
        // TeCLI handles parsing, Spectre.Console handles output
        AnsiConsole.MarkupLine($"Hello, [green]{name}[/]!");
    }
}
```

Or use TeCLI.Extensions.Console for basic styling:

```bash
dotnet add package TeCLI.Extensions.Console
```

## Common Gotchas

1. **No Settings Class:** Options/arguments are method parameters, not class properties
2. **Auto-Discovery:** No need to register commands manually
3. **Return Types:** Return `int` or `Task<int>` for exit codes; `void`/`Task` returns 0
4. **Nested Commands:** Use nested classes with `[Command]` instead of `AddBranch()`
5. **Interceptors → Hooks:** Use `[BeforeExecute]`/`[AfterExecute]` instead of interceptors
6. **ExceptionHandler:** Use `[OnError]` attribute for error handling

## Advanced Features (TeCLI-Only)

These features have no direct equivalent in Spectre.Console.Cli:

### Configuration File Support

Auto-discover and load configuration files:

```csharp
var mergedArgs = args.WithConfiguration(appName: "myapp");
await dispatcher.DispatchAsync(mergedArgs);
```

### Localization (i18n)

Attribute-based localization without external dependencies:

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

Built-in shell mode for interactive CLIs:

```csharp
[Command("db")]
[Shell(Prompt = "db> ", WelcomeMessage = "Database Shell", EnableHistory = true)]
public class DatabaseCommand
{
    [Action("query")]
    public void Query([Argument] string sql) { }
}
```

### Progress UI Comparison

Both Spectre.Console and TeCLI support progress UI, but with different approaches:

**Spectre.Console (manual setup):**
```csharp
public override int Execute(CommandContext context, Settings settings)
{
    AnsiConsole.Progress()
        .Start(ctx =>
        {
            var task = ctx.AddTask("Processing...");
            while (!ctx.IsFinished)
            {
                task.Increment(10);
                Thread.Sleep(100);
            }
        });
    return 0;
}
```

**TeCLI (auto-injected):**
```csharp
[Primary]
public async Task Execute(IProgressContext progress)  // Auto-injected!
{
    using var bar = progress.CreateProgressBar("Processing...", 100);
    for (int i = 0; i <= 100; i += 10)
    {
        bar.Value = i;
        await Task.Delay(100);
    }
    bar.Complete("Done!");
}
```

TeCLI's `IProgressContext` is automatically injected—no manual setup required.

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
// myapp list users -o yaml
```

The table formatter uses Spectre.Console for rich terminal output, so you get familiar formatting.
