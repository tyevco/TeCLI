# TeCLI.Example.PureDI

Example demonstrating TeCLI with Pure.DI.

## Features Demonstrated

- Pure.DI source-generated composition
- Type-safe property access
- AOT (Ahead-of-Time) compilation friendly
- Zero-reflection dependency injection

## Setup

```csharp
using TeCLI;

// Pure.DI generates a Composition class with property access
var composition = new Composition();
var dispatcher = composition.CommandDispatcher;
var exitCode = await dispatcher.DispatchAsync(args);

return exitCode;
```

## Why Pure.DI?

Pure.DI is ideal for:
- Type-safe dependency resolution
- Native AOT compilation
- Pure composition approach
- Compile-time dependency validation
- No runtime reflection

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

- `Program.cs` - Pure.DI composition setup and command dispatch
- `GreetCommand.cs` - Greeting command with injected services

## More Information

See the [TeCLI.Extensions.PureDI](../../extensions/TeCLI.Extensions.PureDI/README.md) package documentation.
