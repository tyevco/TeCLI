# TeCLI.Extensions.Autofac

Autofac dependency injection integration for TeCLI.

## Overview

TeCLI.Extensions.Autofac is a Roslyn source generator for Autofac dependency injection integration. It automatically generates command registration and container module code for Autofac-based applications, handling command discovery and registration via source generation.

## Installation

```bash
dotnet add package TeCLI.Extensions.Autofac
```

## Quick Start

```csharp
using Autofac;

// Create container builder
var builder = new ContainerBuilder();

// Register your services
builder.RegisterType<MyService>().As<IMyService>();

// Add command dispatcher (generated extension method)
builder.AddCommandDispatcher();

// Build and dispatch
var container = builder.Build();
var dispatcher = container.Resolve<CommandDispatcher>();
var exitCode = await dispatcher.DispatchAsync(args);

return exitCode;
```

## Features

- Automatic scanning of `[Command]` decorated classes
- Generated Autofac module for registrations
- Full dependency injection support for command constructors
- Incremental code generation for fast builds
- Compatible with Autofac 6.5.0+

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

This is a **Roslyn Source Generator** package. It generates code at compile time with Autofac as a runtime dependency.

## Documentation

For full documentation and examples, see the [TeCLI repository](https://github.com/tyevco/TeCLI).

## License

MIT License - see [LICENSE](https://github.com/tyevco/TeCLI/blob/main/LICENSE) for details.
