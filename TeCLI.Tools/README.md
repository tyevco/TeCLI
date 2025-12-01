# TeCLI.Tools

Internal utilities and helpers for TeCLI source generators.

## Overview

TeCLI.Tools provides shared utilities used by the TeCLI source generator to analyze code and generate CLI parsing logic. This package is automatically referenced when you install the main [TeCLI](https://www.nuget.org/packages/TeCLI) package.

## Contents

This package includes internal utilities for:

- **Code Generation** - `CodeBuilder` and `RoslynSyntaxBuilder` for generating C# source code
- **Source Analysis** - Classes for analyzing command, action, and parameter definitions
- **Attribute Extensions** - Helpers for working with Roslyn symbol attributes
- **Constants** - Shared constants used across the generator

## Installation

This package is typically installed automatically as a dependency of the main TeCLI package:

```bash
dotnet add package TeCLI
```

## Note

This package contains internal implementation details of TeCLI. The APIs in this package are not intended for direct consumption and may change between versions.

## License

MIT License - see [LICENSE](https://github.com/tyevco/TeCLI/blob/main/LICENSE) for details.

## More Information

For full documentation and examples, see the [TeCLI repository](https://github.com/tyevco/TeCLI).
