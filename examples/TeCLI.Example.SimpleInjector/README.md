# TeCLI.Example.SimpleInjector

Example demonstrating TeCLI with SimpleInjector dependency injection.

## Features Demonstrated

- SimpleInjector container integration
- Constructor injection in commands
- `AddCommandDispatcher()` generated extension method
- Container verification

## Setup

```csharp
using SimpleInjector;
using TeCLI;

var container = new Container();
container.AddCommandDispatcher();  // Generated extension method
container.Verify();  // SimpleInjector best practice

var dispatcher = container.GetInstance<CommandDispatcher>();
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

- `Program.cs` - SimpleInjector container setup and command dispatch
- `GreetCommand.cs` - Greeting command with injected services

## More Information

See the [TeCLI.Extensions.SimpleInjector](../../extensions/TeCLI.Extensions.SimpleInjector/README.md) package documentation.
