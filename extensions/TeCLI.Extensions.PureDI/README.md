# TeCLI.Extensions.PureDI

Pure.DI dependency injection integration for TeCLI.

## Overview

TeCLI.Extensions.PureDI is a Roslyn source generator for Pure.DI framework integration. It generates a `Composition` class with dependency resolution capabilities, optimized for type-safe, AOT-compatible dependency injection without reflection.

## Installation

```bash
dotnet add package TeCLI.Extensions.PureDI
```

## Quick Start

```csharp
// Pure.DI generates a Composition class with property access
var composition = new Composition();
var dispatcher = composition.CommandDispatcher;

var exitCode = await dispatcher.DispatchAsync(args);

return exitCode;
```

## Features

- **Pure.DI Framework Integration** - Type-safe composition
- **AOT Compatible** - No reflection required
- Type-safe property access (not method calls)
- Automatic command discovery and registration
- Optimized composition code generation
- Incremental generation for fast builds

## Why Pure.DI?

Pure.DI is ideal for:
- Type-safe dependency resolution
- Native AOT compilation
- Applications requiring pure composition approach
- Scenarios where reflection is not desired
- Compile-time dependency validation

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

This is a **Roslyn Source Generator** package optimized for type-safe, AOT-friendly dependency injection.

## Documentation

For full documentation and examples, see the [TeCLI repository](https://github.com/tyevco/TeCLI).

## License

MIT License - see [LICENSE](https://github.com/tyevco/TeCLI/blob/main/LICENSE) for details.
