# TeCLI.Extensions.SimpleInjector

SimpleInjector dependency injection integration for TeCLI.

## Overview

TeCLI.Extensions.SimpleInjector is a Roslyn source generator for SimpleInjector dependency injection framework integration. It automatically generates command registration code and extension methods to simplify SimpleInjector setup for TeCLI applications.

## Installation

```bash
dotnet add package TeCLI.Extensions.SimpleInjector
```

## Quick Start

```csharp
using SimpleInjector;

// Create container
var container = new Container();

// Register your services
container.Register<IMyService, MyService>(Lifestyle.Singleton);

// Add command dispatcher (generated extension method)
container.AddCommandDispatcher();

// Verify container (SimpleInjector best practice)
container.Verify();

// Dispatch commands
var dispatcher = container.GetInstance<CommandDispatcher>();
var exitCode = await dispatcher.DispatchAsync(args);

return exitCode;
```

## Features

- Automatic command discovery via `[Command]` attributes
- Generated extension methods for `Container`
- Full dependency injection support for command constructors
- Incremental code generation for fast builds
- Compatible with SimpleInjector 5.4.6+

## Constructor Injection

Commands can receive dependencies via constructor injection:

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

## Package Type

This is a **Roslyn Source Generator** package. It generates code at compile time with SimpleInjector as a runtime dependency.

## Documentation

For full documentation and examples, see the [TeCLI repository](https://github.com/tyevco/TeCLI).

## License

MIT License - see [LICENSE](https://github.com/tyevco/TeCLI/blob/main/LICENSE) for details.
