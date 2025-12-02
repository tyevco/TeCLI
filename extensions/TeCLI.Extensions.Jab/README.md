# TeCLI.Extensions.Jab

Jab (AOT-friendly) dependency injection integration for TeCLI.

## Overview

TeCLI.Extensions.Jab is a Roslyn source generator for Jab integration. Jab is an AOT-friendly, zero-reflection dependency injection framework. This extension generates a `CommandServiceProvider` class with service resolution capabilities, optimized for ahead-of-time compilation scenarios.

## Installation

```bash
dotnet add package TeCLI.Extensions.Jab
```

## Quick Start

```csharp
// Jab generates a CommandServiceProvider class
var serviceProvider = new CommandServiceProvider();
var dispatcher = serviceProvider.GetService<CommandDispatcher>();

var exitCode = await dispatcher.DispatchAsync(args);

return exitCode;
```

## Features

- **AOT (Ahead-of-Time) Friendly** - No reflection required
- Automatic command scanning and registration
- Lightweight generated service provider
- Ideal for self-contained, trimmed deployments
- Smaller binary size and faster startup time
- Incremental code generation

## Why Jab?

Jab is perfect for:
- Native AOT compilation scenarios
- Self-contained single-file deployments
- Trimmed applications where reflection is limited
- Performance-critical CLI tools
- Minimal memory footprint requirements

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

This is a **Roslyn Source Generator** package optimized for AOT scenarios with zero runtime reflection.

## Documentation

For full documentation and examples, see the [TeCLI repository](https://github.com/tyevco/TeCLI).

## License

MIT License - see [LICENSE](https://github.com/tyevco/TeCLI/blob/main/LICENSE) for details.
