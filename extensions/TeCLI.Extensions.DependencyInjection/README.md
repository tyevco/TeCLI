# TeCLI.Extensions.DependencyInjection

Microsoft.Extensions.DependencyInjection integration for TeCLI.

## Overview

TeCLI.Extensions.DependencyInjection is a Roslyn source generator that automatically generates command dispatcher and registration code for Microsoft.Extensions.DependencyInjection (MEDI) integration. It eliminates boilerplate by scanning for `[Command]` attributes and generating initialization code at compile time.

## Installation

```bash
dotnet add package TeCLI.Extensions.DependencyInjection
```

## Quick Start

```csharp
using Microsoft.Extensions.DependencyInjection;

// Create service collection
IServiceCollection services = new ServiceCollection();

// Register your services
services.AddSingleton<IMyService, MyService>();

// Add command dispatcher (generated extension method)
services.AddCommandDispatcher();

// Build and dispatch
var sp = services.BuildServiceProvider();
var dispatcher = sp.GetRequiredService<CommandDispatcher>();
var exitCode = await dispatcher.DispatchAsync(args);

return exitCode;
```

## Features

- Automatic command discovery via `[Command]` attribute scanning
- Generated `AddCommandDispatcher()` extension method
- Full dependency injection support for command constructors
- Incremental generation for fast builds
- Compatible with Microsoft.Extensions.DependencyInjection 2.1.1+

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

This is a **Roslyn Source Generator** package. It generates code at compile time and has no runtime dependencies beyond Microsoft.Extensions.DependencyInjection.

## Documentation

For full documentation and examples, see the [TeCLI repository](https://github.com/tyevco/TeCLI).

## License

MIT License - see [LICENSE](https://github.com/tyevco/TeCLI/blob/main/LICENSE) for details.
