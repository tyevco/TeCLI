# TeCLI.Example.Jab

Example demonstrating TeCLI with Jab (AOT-friendly DI).

## Features Demonstrated

- Jab source-generated DI integration
- Zero-reflection dependency injection
- AOT (Ahead-of-Time) compilation friendly
- Constructor injection in commands

## Setup

```csharp
using TeCLI;

// Jab generates a CommandServiceProvider class
var serviceProvider = new CommandServiceProvider();
var dispatcher = serviceProvider.GetService<CommandDispatcher>();
var exitCode = await dispatcher.DispatchAsync(args);

return exitCode;
```

## Why Jab?

Jab is ideal for:
- Native AOT compilation scenarios
- Self-contained single-file deployments
- Trimmed applications
- Minimal memory footprint
- Fast startup times

## Commands

### Greet Command

```bash
# Say hello
dotnet run -- greet hello World
dotnet run -- greet hello World --excited

# Say goodbye
dotnet run -- greet goodbye Friend
```

## Code Structure

- `Program.cs` - Jab service provider setup and command dispatch
- `GreetCommand.cs` - Greeting command with injected services

## More Information

See the [TeCLI.Extensions.Jab](../../extensions/TeCLI.Extensions.Jab/README.md) package documentation.
