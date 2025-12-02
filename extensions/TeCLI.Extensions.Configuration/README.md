# TeCLI.Extensions.Configuration

Configuration file support for TeCLI - load CLI options from JSON, YAML, TOML, and INI files with profile support.

## Installation

```bash
dotnet add package TeCLI.Extensions.Configuration
```

## Features

- **Multiple Format Support** - JSON, YAML, TOML, and INI files
- **Configuration File Discovery** - Automatic discovery in standard locations
- **Merge Strategy** - File < Environment < CLI arguments
- **Per-Command Configuration** - Command-specific option sections
- **Configuration Profiles** - Named profiles with inheritance
- **Environment Variable Overrides** - Optional prefix-based env var support

## Quick Start

### 1. Create a Configuration File

Create a `tecli.json` (or `.teclirc`, `tecli.yaml`, etc.) in your project:

```json
{
  "environment": "development",
  "region": "us-east",
  "verbose": false,
  "timeout": 300,

  "profiles": {
    "dev": {
      "environment": "development",
      "verbose": true
    },
    "prod": {
      "environment": "production",
      "region": "us-west",
      "verbose": false
    }
  }
}
```

### 2. Use Configuration in Your CLI

```csharp
using TeCLI.Configuration;

// Load and merge configuration with CLI arguments
var mergedArgs = args.WithConfiguration(appName: "myapp");
await CommandDispatcher.DispatchAsync(mergedArgs);
```

### 3. Run Your CLI

```bash
# Uses config file defaults
myapp deploy

# CLI arguments override config
myapp deploy --environment production

# Use a specific profile
myapp deploy --profile prod
```

## Configuration File Discovery

Configuration files are discovered in this order (lowest to highest precedence):

1. **Global** - `/etc/myapp/`, `%ProgramData%\myapp\`
2. **User** - `~/.config/myapp/`, `%AppData%\myapp\`
3. **Working Directory Tree** - Walking up from current directory
4. **Explicit Path** - Via `ConfigurationOptions.ExplicitConfigPath`

### Supported File Names

Without extension:
- `.teclirc`, `tecli`, `.tecli`, `tecliconfig`, `.tecliconfig`
- `.{appname}rc`, `{appname}`, `.{appname}`, `{appname}config`

With extensions:
- `.json`, `.yaml`, `.yml`, `.toml`, `.ini`, `.cfg`, `.conf`

## Configuration Formats

### JSON

```json
{
  "environment": "production",
  "verbose": true,
  "timeout": 300,
  "tags": ["deploy", "production"]
}
```

Features: Comments (`//`), trailing commas, nested objects, arrays.

### YAML

```yaml
environment: production
verbose: true
timeout: 300
tags:
  - deploy
  - production
```

Features: Indentation-based nesting, inline arrays, boolean variants (`yes`/`no`/`on`/`off`).

### TOML

```toml
environment = "production"
verbose = true
timeout = 300
tags = ["deploy", "production"]

[deploy]
timeout = 600
```

Features: Sections, inline tables, arrays, typed values.

### INI

```ini
environment = production
verbose = yes
timeout = 300

[deploy]
timeout = 600
```

Features: Sections, boolean variants, comma-separated lists.

## Per-Command Configuration

Configure default options for specific commands:

```json
{
  "verbose": false,
  "timeout": 30,

  "deploy": {
    "timeout": 600,
    "verbose": true
  },

  "commands": {
    "build": {
      "output": "./dist"
    }
  }
}
```

Access command-specific configuration:

```csharp
var loader = new ConfigurationLoader();
var config = loader.Load("myapp");
var deployConfig = loader.GetCommandConfiguration(config, "deploy");
// deployConfig["timeout"] = 600, deployConfig["verbose"] = true
```

## Configuration Profiles

Define named configuration profiles with inheritance:

```json
{
  "profiles": {
    "base": {
      "timeout": 30,
      "retries": 3
    },
    "dev": {
      "inherits": "base",
      "environment": "development",
      "verbose": true
    },
    "staging": {
      "inherits": "dev",
      "environment": "staging"
    },
    "prod": {
      "environment": "production",
      "timeout": 300,
      "verbose": false
    }
  }
}
```

Use a profile:

```csharp
// Via extension method
var args = args.WithProfile("prod", appName: "myapp");

// Via options
var options = ConfigurationOptions.WithProfile("staging");
var loader = new ConfigurationLoader(options);
var config = loader.Load("myapp");
```

## Environment Variable Overrides

Configure environment variable prefix for overrides:

```csharp
var options = new ConfigurationOptions
{
    EnvironmentVariablePrefix = "MYAPP_",
    EnvironmentOverridesConfig = true,
    EnvironmentVariableNestingSeparator = "__"
};
```

Then environment variables like `MYAPP_VERBOSE=true` or `MYAPP_DEPLOY__TIMEOUT=600` will override config values.

## Merge Strategy

Values are merged in this order (later sources override earlier ones):

1. **Configuration Files** (lowest) - In discovery order
2. **Environment Variables** - With optional prefix
3. **CLI Arguments** (highest) - Always take precedence

This ensures users can set defaults in config files but always override via CLI.

## API Reference

### ConfigurationLoader

Main entry point for loading configuration:

```csharp
var loader = new ConfigurationLoader(options);

// Load all discovered configuration files
var config = loader.Load("myapp");

// Load specific file
var fileConfig = loader.LoadFile("/path/to/config.json");

// Get typed values
var timeout = loader.GetValue<int>(config, "timeout", defaultValue: 30);
var region = loader.GetValue<string>(config, "deploy.region");

// Check for values
if (loader.HasValue(config, "api-key"))
{
    // ...
}
```

### ConfigurationOptions

```csharp
var options = new ConfigurationOptions
{
    SearchGlobalConfig = true,           // Search /etc, %ProgramData%
    SearchUserConfig = true,             // Search ~/.config, %AppData%
    SearchWorkingDirectory = true,       // Search current directory
    SearchWorkingDirectoryTree = true,   // Walk up directory tree
    ExplicitConfigPath = null,           // Explicit file path
    ProfileName = null,                  // Profile to use
    EnvironmentOverridesConfig = true,   // Apply env var overrides
    EnvironmentVariablePrefix = null,    // Prefix for env vars
    CaseInsensitiveKeys = true           // Case-insensitive key matching
};
```

### Extension Methods

```csharp
// Merge configuration with CLI arguments
var mergedArgs = args.WithConfiguration(options, appName);

// Use specific profile
var profileArgs = args.WithProfile("prod", appName);

// Use specific config file
var fileArgs = args.WithConfigFile("/path/to/config.json");
```

## Custom Parsers

Add support for additional configuration formats:

```csharp
public class MyCustomParser : IConfigurationParser
{
    public IEnumerable<string> SupportedExtensions => new[] { ".myext" };

    public bool CanParse(string extension) =>
        SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);

    public IDictionary<string, object?> Parse(string content)
    {
        // Parse content and return dictionary
    }

    public IDictionary<string, object?> Parse(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }
}

// Register custom parser
var loader = new ConfigurationLoader();
loader.AddParser(new MyCustomParser());
```

## Example

See the `TeCLI.Example.Configuration` project for a complete working example.

```bash
cd examples/TeCLI.Example.Configuration
dotnet run -- deploy                    # Uses config defaults
dotnet run -- deploy --profile prod     # Uses production profile
dotnet run -- deploy --verbose          # CLI overrides config
dotnet run -- config show               # Show current configuration
dotnet run -- config profiles           # List available profiles
```
