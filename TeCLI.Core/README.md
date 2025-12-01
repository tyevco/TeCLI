# TeCLI.Core

Core attributes and interfaces for TeCLI, a source-generated CLI parsing library for .NET.

## Overview

TeCLI.Core provides the fundamental attributes and interfaces used to define CLI commands, actions, options, and arguments. This package is automatically referenced when you install the main [TeCLI](https://www.nuget.org/packages/TeCLI) package.

## Included Attributes

### Command Definition
- `[Command]` - Marks a class as a CLI command
- `[Action]` - Marks a method as a named subcommand
- `[Primary]` - Marks a method as the default action
- `[GlobalOptions]` - Defines options available across all commands

### Parameter Definition
- `[Option]` - Marks a parameter as a named option (e.g., `--name` or `-n`)
- `[Argument]` - Marks a parameter as a positional argument

### Validation
- `[Range]` - Validates numeric values within a range
- `[FileExists]` - Validates that a file path exists
- `[DirectoryExists]` - Validates that a directory path exists
- `[RegularExpression]` - Validates string values against a regex pattern

### Type Conversion
- `[TypeConverter]` - Specifies a custom type converter
- `ITypeConverter<T>` - Interface for implementing custom converters

### Hooks
- `[PreAction]` / `[PostAction]` - Execute code before/after actions
- `IPreActionHook` / `IPostActionHook` - Interfaces for hook implementations

## Installation

This package is typically installed automatically as a dependency of the main TeCLI package:

```bash
dotnet add package TeCLI
```

## License

MIT License - see [LICENSE](https://github.com/tyevco/TeCLI/blob/main/LICENSE) for details.

## More Information

For full documentation and examples, see the [TeCLI repository](https://github.com/tyevco/TeCLI).
