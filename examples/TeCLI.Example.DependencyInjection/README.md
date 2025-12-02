# TeCLI.Example.DependencyInjection

Example demonstrating TeCLI with Microsoft.Extensions.DependencyInjection.

## Features Demonstrated

- Microsoft.Extensions.DependencyInjection integration
- Constructor injection in commands
- Service registration and resolution
- `AddCommandDispatcher()` generated extension method

## Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using TeCLI;

IServiceCollection services = new ServiceCollection();
services.AddCommandDispatcher();  // Generated extension method

var sp = services.BuildServiceProvider();
var dispatcher = sp.GetRequiredService<CommandDispatcher>();
var exitCode = await dispatcher.DispatchAsync(args);

return exitCode;
```

## Commands

### Configuration Command

```bash
dotnet run -- configuration show
dotnet run -- configuration set --key "app.name" --value "MyApp"
```

### Authorization Command

```bash
dotnet run -- authorization check
dotnet run -- authorization login
```

## Code Structure

- `Program.cs` - DI setup and command dispatch
- `ConfigurationCommand.cs` - Configuration management example
- `AuthorizationCommand.cs` - Authorization example with injected services
- `CommandLineOptions.cs` - Shared options

## More Information

See the [TeCLI.Extensions.DependencyInjection](../../extensions/TeCLI.Extensions.DependencyInjection/README.md) package documentation.
