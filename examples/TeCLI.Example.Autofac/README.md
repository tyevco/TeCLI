# TeCLI.Example.Autofac

Example demonstrating TeCLI with Autofac dependency injection.

## Features Demonstrated

- Autofac container integration
- Constructor injection in commands
- `AddCommandDispatcher()` generated extension method
- Service registration with Autofac's `ContainerBuilder`

## Setup

```csharp
using Autofac;
using TeCLI;

var builder = new ContainerBuilder();
builder.AddCommandDispatcher();  // Generated extension method

var container = builder.Build();
var dispatcher = container.Resolve<CommandDispatcher>();
var exitCode = await dispatcher.DispatchAsync(args);

return exitCode;
```

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

- `Program.cs` - Autofac container setup and command dispatch
- `GreetCommand.cs` - Greeting command with injected services

## More Information

See the [TeCLI.Extensions.Autofac](../../extensions/TeCLI.Extensions.Autofac/README.md) package documentation.
