# TeCLI Roadmap

This document outlines potential features and improvements for TeCLI. Items are organized by category and priority.

## Legend

- 🎯 **High Priority** - Core features that would significantly enhance the library
- 📊 **Medium Priority** - Important improvements that add value
- 💡 **Nice to Have** - Features that would be beneficial but not critical
- 🔬 **Research Needed** - Ideas that require investigation before implementation

---

## Core Feature Enhancements

### 🎯 Array and Collection Support
**Status:** ✅ Completed
**Priority:** High

TeCLI now supports collection types for both options and arguments! This enables scenarios like:
```csharp
[Action("process")]
public void Process(
    [Option("files", ShortName = 'f')] string[] files,
    [Option("tags")] List<string> tags)
{
    // myapp process --files file1.txt --files file2.txt --tags tag1 --tags tag2
    // OR: myapp process --files file1.txt,file2.txt --tags tag1,tag2
}
```

**Implemented Features:**
- ✅ Support for arrays (`T[]`)
- ✅ Support for `List<T>`, `IEnumerable<T>`, `ICollection<T>`, `IList<T>`, `IReadOnlyCollection<T>`, `IReadOnlyList<T>`
- ✅ Repeatable options syntax (`--file a.txt --file b.txt`)
- ✅ Comma-separated values syntax (`--files a.txt,b.txt,c.txt`)
- ✅ Mixed syntax support (repeatable + comma-separated)
- ✅ Collection support for both options and arguments
- ✅ Comprehensive test coverage

**Files Changed:**
- `TeCLI.Tools/Generators/ParameterSourceInfo.cs` - Added collection tracking properties
- `TeCLI.Tools/Extensions.cs` - Added collection type detection and validation
- `TeCLI/Generators/ParameterInfoExtractor.cs` - Added collection type detection logic
- `TeCLI/Generators/ParameterCodeGenerator.cs` - Implemented collection parsing
- `TeCLI.Tests/TestCommands/CollectionCommand.cs` - Test command for collections
- `TeCLI.Tests/CollectionSupportTests.cs` - Comprehensive integration tests

---

### 🎯 Enum Support
**Status:** ✅ Completed
**Priority:** High

TeCLI now supports enum types with automatic validation! This enables scenarios like:
```csharp
public enum LogLevel { Debug, Info, Warning, Error }

[Action("run")]
public void Run([Option("log-level")] LogLevel level = LogLevel.Info)
{
    // myapp run --log-level Debug
    // myapp run --log-level debug  (case-insensitive)
}
```

**Implemented Features:**
- ✅ Case-insensitive enum parsing
- ✅ Automatic validation with helpful error messages showing valid enum values
- ✅ Support for `[Flags]` enums with comma-separated values
- ✅ Enum collections (arrays and lists of enums)
- ✅ Enum support for both options and arguments
- ✅ Comprehensive test coverage

**Files Changed:**
- `TeCLI.Tools/Generators/ParameterSourceInfo.cs` - Added enum tracking properties
- `TeCLI.Tools/Extensions.cs` - Added enum type detection methods
- `TeCLI/Generators/ParameterInfoExtractor.cs` - Added enum detection logic
- `TeCLI/Generators/ParameterCodeGenerator.cs` - Implemented enum parsing with Enum.Parse
- `TeCLI.Tests/TestCommands/EnumCommand.cs` - Test command for enums
- `TeCLI.Tests/EnumSupportTests.cs` - Comprehensive integration tests

---

### 🎯 Required Options
**Status:** ✅ Completed
**Priority:** High

TeCLI now supports marking options as required! This enables scenarios like:
```csharp
[Action("deploy")]
public void Deploy(
    [Option("environment", Required = true)] string environment,
    [Option("region")] string region = "us-west")
{
    // myapp deploy --environment production --region us-east
}
```

**Implemented Features:**
- ✅ `Required` property added to `OptionAttribute`
- ✅ Validation during parsing with clear error messages
- ✅ Support for required options with short names
- ✅ Support for required collection options
- ✅ Comprehensive test coverage

**Files Changed:**
- `TeCLI.Core/OptionAttribute.cs` - Added `Required` property
- `TeCLI/Generators/ParameterInfoExtractor.cs` - Extract `Required` from attribute
- `TeCLI/Generators/ParameterCodeGenerator.cs` - Validation logic (already existed)
- `TeCLI.Tools/Constants.cs` - Error message (already existed)
- `TeCLI.Tests/TestCommands/RequiredOptionsCommand.cs` - Test command
- `TeCLI.Tests/RequiredOptionTests.cs` - Comprehensive integration tests

---

### 📊 Custom Type Converters
**Status:** ✅ Completed
**Priority:** Medium

TeCLI now supports both built-in and custom type converters! This enables parsing of any custom type through the `ITypeConverter<T>` interface.

```csharp
// Built-in types work automatically
[Action("fetch")]
public void Fetch([Option("url")] Uri endpoint) { }

// Custom types via ITypeConverter<T>
public class EmailAddressConverter : ITypeConverter<EmailAddress>
{
    public EmailAddress Convert(string value)
    {
        if (!value.Contains("@"))
            throw new ArgumentException($"Invalid email: {value}");
        return new EmailAddress(value);
    }
}

[Action("send")]
public void Send(
    [Option("to")] [TypeConverter(typeof(EmailAddressConverter))] EmailAddress recipient)
{
    // recipient is automatically converted using EmailAddressConverter
}
```

**Implemented Features:**
- ✅ Built-in support for common .NET types:
  - `Uri`, `DateTime`, `DateTimeOffset`, `TimeSpan`, `Guid`, `FileInfo`, `DirectoryInfo`
- ✅ `ITypeConverter<T>` interface for custom type conversion
- ✅ `TypeConverterAttribute` for parameter-level registration
- ✅ Works with options, arguments, and collections
- ✅ Works with environment variables
- ✅ Clear error messages for conversion failures
- ✅ Comprehensive test coverage (35+ integration tests)

**Files Changed:**
- `TeCLI.Core/TypeConversion/ITypeConverter.cs` - Core converter interface
- `TeCLI.Core/TypeConversion/TypeConverterAttribute.cs` - Attribute for specifying converters
- `TeCLI.Tools/Generators/ParameterSourceInfo.cs` - Track custom converter info
- `TeCLI/Generators/ParameterInfoExtractor.cs` - Extract custom converter attributes
- `TeCLI/Generators/ParameterCodeGenerator.cs` - Generate custom converter usage code
- `TeCLI.Tests/TestTypes/EmailAddress.cs` - Example custom type
- `TeCLI.Tests/TestTypes/EmailAddressConverter.cs` - Example converter
- `TeCLI.Tests/TestTypes/PhoneNumber.cs` - Example custom type for collections
- `TeCLI.Tests/TestTypes/PhoneNumberConverter.cs` - Example converter for collections
- `TeCLI.Tests/TestCommands/CustomConverterCommand.cs` - Test command
- `TeCLI.Tests/CustomConverterTests.cs` - Comprehensive integration tests

---

### 📊 Validation Attributes
**Status:** ✅ Completed
**Priority:** Medium

TeCLI now supports declarative validation for options and arguments! This enables scenarios like:
```csharp
[Action("process")]
public void Process(
    [Option("port")] [Range(1, 65535)] int port,
    [Option("email")] [RegularExpression(@"^[^@]+@[^@]+\.[^@]+$")] string email,
    [Option("pattern")] [RegularExpression(@"^\w+$")] string pattern,
    [Argument] [FileExists] string inputFile,
    [Option("output")] [DirectoryExists] string? outputDir = null)
{
}
```

**Implemented Features:**
- ✅ `RangeAttribute` - Validates numeric values are within specified bounds
- ✅ `RegularExpressionAttribute` - Validates strings match a regex pattern
- ✅ `FileExistsAttribute` - Validates file paths point to existing files
- ✅ `DirectoryExistsAttribute` - Validates directory paths point to existing directories
- ✅ Custom error messages for validation failures
- ✅ Works with both options and arguments
- ✅ Proper handling of optional parameters (validation skipped when not provided)
- ✅ Comprehensive test coverage

**Files Changed:**
- `TeCLI.Core/Validation/RangeAttribute.cs` - Range validation attribute
- `TeCLI.Core/Validation/RegularExpressionAttribute.cs` - Regex validation attribute
- `TeCLI.Core/Validation/FileExistsAttribute.cs` - File existence validation attribute
- `TeCLI.Core/Validation/DirectoryExistsAttribute.cs` - Directory existence validation attribute
- `TeCLI.Tools/Generators/ParameterSourceInfo.cs` - Added validation tracking
- `TeCLI/Generators/ParameterInfoExtractor.cs` - Extract validation attributes
- `TeCLI/Generators/ParameterCodeGenerator.cs` - Generate validation code
- `TeCLI.Tests/TestCommands/ValidationCommand.cs` - Test command
- `TeCLI.Tests/ValidationTests.cs` - Comprehensive integration tests

---

### 📊 Environment Variable Binding
**Status:** ✅ Completed
**Priority:** Medium

TeCLI now supports environment variable binding for options! This enables scenarios like:
```csharp
[Action("connect")]
public void Connect(
    [Option("api-key", EnvVar = "API_KEY")] string apiKey,
    [Option("timeout", EnvVar = "TIMEOUT")] int timeout = 30,
    [Option("verbose", EnvVar = "VERBOSE")] bool verbose = false)
{
    // Can be set via --api-key OR API_KEY environment variable
}
```

**Implemented Features:**
- ✅ `EnvVar` property on `OptionAttribute` to specify environment variable name
- ✅ Automatic fallback to environment variables when option not provided via CLI
- ✅ Clear precedence: CLI option > environment variable > default value
- ✅ Support for all types: strings, integers, booleans, enums, common types
- ✅ Support for collections (comma-separated values in environment variables)
- ✅ Proper error handling with clear messages for invalid environment variable values
- ✅ Works with required options
- ✅ Works with short names
- ✅ Comprehensive test coverage (35+ integration tests)

**Files Changed:**
- `TeCLI.Core/OptionAttribute.cs` - Added `EnvVar` property
- `TeCLI.Tools/Generators/ParameterSourceInfo.cs` - Track environment variable name
- `TeCLI/Generators/ParameterInfoExtractor.cs` - Extract `EnvVar` from attribute
- `TeCLI/Generators/ParameterCodeGenerator.cs` - Generate environment variable fallback code
- `TeCLI.Tests/TestCommands/EnvVarCommand.cs` - Test command
- `TeCLI.Tests/EnvVarTests.cs` - Comprehensive integration tests

---

## Subcommand and Command Organization

### 🎯 Nested Subcommands
**Status:** ✅ Completed
**Priority:** High

TeCLI now supports hierarchical command structures with unlimited nesting depth! This enables scenarios like Git's command structure:
```csharp
[Command("git")]
public class GitCommand
{
    [Action("status")]
    public void Status() { }

    [Command("remote")]
    public class RemoteCommand
    {
        [Action("add")]
        public void Add(
            [Argument] string name,
            [Argument] string url) { }

        [Action("remove")]
        public void Remove([Argument] string name) { }
    }

    [Command("config")]
    public class ConfigCommand
    {
        [Action("get")]
        public void Get([Argument] string key) { }

        // 3-level nesting example
        [Command("user")]
        public class UserCommand
        {
            [Action("name")]
            public void Name([Argument] string name) { }
        }
    }
}
// Usage:
// myapp git status
// myapp git remote add origin https://...
// myapp git config user name "John Doe"
```

**Implemented Features:**
- ✅ Unlimited nesting depth (2-level, 3-level, N-level)
- ✅ Hierarchical command dispatch with proper routing
- ✅ Subcommand aliases work at all levels
- ✅ Action aliases within nested subcommands
- ✅ Help text generation showing full command paths
- ✅ Subcommands and actions can coexist in the same command
- ✅ Backward compatibility - existing flat commands work unchanged
- ✅ Comprehensive test coverage (25+ integration tests)
- ✅ Proper error handling with suggestions at all levels

**Architecture:**
- `CommandSourceInfo` class tracks hierarchical command structures
- Recursive extraction of nested classes with `[Command]` attribute
- Hierarchical dispatch methods generate multi-level routing
- Help text displays subcommands separately from actions

**Files Changed:**
- `TeCLI.Tools/Generators/CommandSourceInfo.cs` - New infrastructure for command hierarchy
- `TeCLI/Generators/CommandLineArgsGenerator.Commands.cs` - Updated to build and dispatch hierarchies
- `TeCLI/Generators/CommandLineArgsGenerator.Help.cs` - Enhanced help generation for nested structures
- `TeCLI.Tests/TestCommands/NestedCommand.cs` - Test command with 2-level and 3-level nesting
- `TeCLI.Tests/NestedCommandTests.cs` - Comprehensive integration tests

---

### 📊 Command Aliases
**Status:** ✅ Completed
**Priority:** Medium

TeCLI now supports multiple names for commands and actions!

```csharp
[Command("remove", Aliases = new[] { "rm", "delete" })]
public class RemoveCommand
{
    [Action("list", Aliases = new[] { "ls", "show" })]
    public void List() { }
}
// Usage: All of these work!
// myapp remove list
// myapp rm ls
// myapp delete show
```

**Implemented Features:**
- ✅ Command aliases - multiple names for commands
- ✅ Action aliases - multiple names for actions
- ✅ Unlimited aliases per command/action
- ✅ Case-insensitive alias matching
- ✅ Aliases work in command/action suggestions
- ✅ Help text displays aliases alongside names
- ✅ Full dispatcher support for aliases
- ✅ Comprehensive test coverage (17+ tests)

**Use Cases:**
- Short/long command names (`rm` vs `remove`)
- Backward compatibility when renaming commands
- Common abbreviations (`ls` for `list`, `dir` for `directory`)
- User-friendly alternatives (`delete` as alias for `remove`)

**Files Changed:**
- `TeCLI.Core/CommandAttribute.cs` - Added Aliases property
- `TeCLI.Core/ActionAttribute.cs` - Added Aliases property
- `TeCLI.Tools/Generators/ActionSourceInfo.cs` - Track aliases
- `TeCLI/Generators/CommandLineArgsGenerator.Commands.cs` - Command dispatch with aliases
- `TeCLI/Generators/CommandLineArgsGenerator.Actions.cs` - Action dispatch with aliases
- `TeCLI/Generators/CommandLineArgsGenerator.Help.cs` - Help text with aliases
- `TeCLI.Tests/TestCommands/AliasesCommand.cs` - Test command
- `TeCLI.Tests/AliasesTests.cs` - Integration tests

---

### 📊 Global Options
**Status:** ✅ Completed
**Priority:** Medium

TeCLI now supports global options that are available across all commands! Options defined in a class marked with `[GlobalOptions]` are automatically parsed before command dispatch and can be injected into any action method.

```csharp
[GlobalOptions]
public class AppGlobalOptions
{
    [Option("verbose", ShortName = 'v')]
    public bool Verbose { get; set; }

    [Option("config")]
    public string? ConfigFile { get; set; }

    [Option("log-level")]
    public string LogLevel { get; set; } = "info";

    [Option("timeout")]
    public int Timeout { get; set; } = 30;
}

[Command("build")]
public class BuildCommand
{
    [Action("run")]
    public void Run(AppGlobalOptions globals, [Option("output")] string output)
    {
        if (globals.Verbose)
            Console.WriteLine($"Building to {output}...");

        // Use other global options
        Console.WriteLine($"Log level: {globals.LogLevel}");
        Console.WriteLine($"Timeout: {globals.Timeout}s");
    }
}

[Command("deploy")]
public class DeployCommand
{
    [Action("prod")]
    public void Production(AppGlobalOptions globals, [Argument] string environment)
    {
        // Same global options available here
        if (globals.Verbose)
            Console.WriteLine($"Deploying to {environment}...");
    }
}

// Usage examples:
// myapp --verbose build run --output dist/
// myapp --config app.json deploy prod production
// myapp -v --log-level debug build run --output out/
```

**Implemented Features:**
- ✅ `[GlobalOptions]` attribute to mark global options class
- ✅ Automatic parsing before command dispatch
- ✅ Global options removed from args before action processing
- ✅ Automatic injection into action methods that request them
- ✅ Support for all option types (switches, strings, ints, enums, custom converters)
- ✅ Default values work as expected
- ✅ Short names and long names both supported
- ✅ Validation attributes work with global options
- ✅ Environment variable binding works with global options
- ✅ Actions can optionally receive global options (not required)
- ✅ Multiple actions can all receive the same global options instance
- ✅ Comprehensive test coverage (25+ integration tests)

**Use Cases:**
- Consistent verbose/debug flags across all commands
- Shared configuration file specification
- Common authentication tokens or API keys
- Logging level configuration
- Timeout and retry settings
- Output format preferences

**Architecture:**
- Global options parsed first, stored in `_globalOptions` field
- Parsed global options removed from args array
- Actions inspected for parameters matching global options type
- Global options instance automatically passed when requested

**Files Added:**
- `TeCLI.Core/GlobalOptionsAttribute.cs` - Attribute to mark global options class
- `TeCLI.Tools/Generators/GlobalOptionsSourceInfo.cs` - Data structure for tracking
- `TeCLI.Tests/TestCommands/GlobalOptionsCommand.cs` - Test command
- `TeCLI.Tests/GlobalOptionsTests.cs` - Comprehensive integration tests

**Files Modified:**
- `TeCLI/Generators/CommandLineArgsGenerator.cs` - Detect global options classes
- `TeCLI/Generators/CommandLineArgsGenerator.Commands.cs` - Parse global options
- `TeCLI/Generators/CommandLineArgsGenerator.Parameters.cs` - Inject into actions
- `TeCLI/Generators/CommandLineArgsGenerator.Actions.cs` - Thread through pipeline
- `TeCLI/Generators/ParameterInfoExtractor.cs` - Made methods internal for reuse

---

### 💡 Mutual Exclusivity
**Status:** ✅ Completed
**Priority:** Low

TeCLI now supports marking options as mutually exclusive! This enables scenarios where only one of a set of options can be specified:

```csharp
[Action("output")]
public void Output(
    [Option("json", MutuallyExclusiveSet = "format")] bool json,
    [Option("xml", MutuallyExclusiveSet = "format")] bool xml,
    [Option("yaml", MutuallyExclusiveSet = "format")] bool yaml)
{
    // Only one of json, xml, or yaml can be specified
}

// Multiple exclusive sets can be used in the same action
[Action("process")]
public void Process(
    [Option("json", MutuallyExclusiveSet = "format")] bool json,
    [Option("xml", MutuallyExclusiveSet = "format")] bool xml,
    [Option("compact", MutuallyExclusiveSet = "style")] bool compact,
    [Option("pretty", MutuallyExclusiveSet = "style")] bool pretty)
{
    // Can specify one from each set: --json --compact is valid
    // But --json --xml would fail (both in "format" set)
}

// Also works with value options
[Action("convert")]
public void Convert(
    [Option("format", MutuallyExclusiveSet = "outputType")] string? format,
    [Option("encoding", MutuallyExclusiveSet = "outputType")] string? encoding)
{
    // Only one of format or encoding can be specified
}
```

**Implemented Features:**
- ✅ `MutuallyExclusiveSet` property on `OptionAttribute` for grouping options
- ✅ Support for boolean switches (flags)
- ✅ Support for value options (strings, ints, etc.)
- ✅ Multiple exclusive sets in the same action
- ✅ Clear error messages showing which options conflict
- ✅ Works with short names
- ✅ Comprehensive test coverage

**Error Message Example:**
```
Options '--json' and '--xml' are mutually exclusive. Only one can be specified at a time.
```

**Files Changed:**
- `TeCLI/Generators/CommandLineArgsGenerator.Attributes.cs` - Added `MutuallyExclusiveSet` property
- `TeCLI.Tools/Generators/ParameterSourceInfo.cs` - Track exclusive set membership
- `TeCLI/Generators/ParameterInfoExtractor.cs` - Extract `MutuallyExclusiveSet` from attribute
- `TeCLI/Generators/CommandLineArgsGenerator.Parameters.cs` - Generate validation code
- `TeCLI.Tools/Constants.cs` - Error message constant
- `TeCLI.Tests/TestCommands/MutualExclusivityCommand.cs` - Test command
- `TeCLI.Tests/MutualExclusivityTests.cs` - Comprehensive integration tests

---

## User Experience Enhancements

### 🎯 Automatic Version Flag
**Status:** ✅ Completed
**Priority:** High

TeCLI now automatically handles the `--version` flag! This enables easy version display:
```csharp
[assembly: AssemblyVersion("1.2.3")]
[assembly: AssemblyInformationalVersion("1.2.3-beta")]

// Automatically generates --version handler
// myapp --version
// Output: myapp 1.2.3-beta
```

**Implemented Features:**
- ✅ Automatic `--version` flag detection
- ✅ Reads from `AssemblyInformationalVersionAttribute` (preferred)
- ✅ Falls back to `AssemblyVersion` if informational version not available
- ✅ Reserved switch like `--help`
- ✅ Included in global help text
- ✅ Works at application level (before command parsing)

**Files Changed:**
- `TeCLI/Generators/CommandLineArgsGenerator.Commands.cs` - Added version flag check
- `TeCLI/Generators/CommandLineArgsGenerator.Help.cs` - Generated DisplayVersion method
- `TeCLI/Generators/CommandLineArgsGenerator.Help.cs` - Updated help text to show --version

---

### 🎯 Improved Error Messages with Suggestions
**Status:** ✅ Completed
**Priority:** High

Provide helpful suggestions for typos and mistakes:
```
Error: Unknown command 'buld'
Did you mean 'build'?

Error: Unknown option '--enviornment'
Did you mean '--environment'?
```

**Implementation:**
- ✅ Levenshtein distance algorithm for string similarity
- ✅ Suggestions for unknown commands
- ✅ Suggestions for unknown actions
- ✅ Suggestions for unknown options (with detection - previously silently ignored!)
- ✅ Case-insensitive matching
- ✅ Comprehensive test coverage

**Files Changed:**
- `TeCLI/StringSimilarity.cs` - New utility class for calculating string similarity
- `TeCLI.Tools/Constants.cs` - Added error message templates
- `TeCLI/Generators/CommandLineArgsGenerator.Commands.cs` - Enhanced command and action error handling
- `TeCLI/Generators/CommandLineArgsGenerator.Parameters.cs` - Added unknown option detection and suggestions
- `TeCLI.Tests/StringSimilarityTests.cs` - Unit tests for similarity algorithm
- `TeCLI.Tests/ErrorSuggestionTests.cs` - Integration tests for error suggestions

---

### 📊 Interactive Mode
**Status:** ✅ Completed
**Priority:** Medium

TeCLI now supports interactive prompting for missing arguments and options! This enables user-friendly CLI applications that can prompt for values when they're not provided.

```csharp
[Action("deploy")]
public void Deploy(
    [Argument(Prompt = "Enter deployment environment")] string environment,
    [Option("region", Prompt = "Select deployment region")] string region = "us-west")
{
    // If environment not provided via CLI, user will be prompted
    // If region not provided via CLI or env var, user will be prompted (or default used)
}

[Action("login")]
public void Login(
    [Argument(Prompt = "Enter username")] string username,
    [Argument(Prompt = "Enter password", SecurePrompt = true)] string password)
{
    // Password input will be masked with asterisks
}
```

**Implemented Features:**
- ✅ `Prompt` property on `ArgumentAttribute` - Interactive prompt message for missing arguments
- ✅ `Prompt` property on `OptionAttribute` - Interactive prompt message for missing options
- ✅ `SecurePrompt` property - Mask input with asterisks for sensitive data (passwords, API keys)
- ✅ Validation on prompted input - All existing validation works with prompted values
- ✅ Type conversion support - Prompts work with all types (strings, ints, enums, custom converters)
- ✅ Precedence handling - CLI > environment variable > interactive prompt > default value
- ✅ Works with required and optional parameters
- ✅ Comprehensive test coverage

**Use Cases:**
- Password/credential input without exposing in command history
- User-friendly CLIs that guide users through required inputs
- Interactive configuration setup
- Simplified command syntax (fewer required CLI arguments)

**Files Changed:**
- `TeCLI.Core/ArgumentAttribute.cs` - Added `Prompt` and `SecurePrompt` properties
- `TeCLI.Core/OptionAttribute.cs` - Added `Prompt` and `SecurePrompt` properties
- `TeCLI.Tools/Generators/ParameterSourceInfo.cs` - Track prompt configuration
- `TeCLI/Generators/ParameterInfoExtractor.cs` - Extract prompt attributes
- `TeCLI/Generators/ParameterCodeGenerator.cs` - Generate interactive prompt code
- `TeCLI.Tests/TestCommands/InteractiveModeCommand.cs` - Test command
- `TeCLI.Tests/InteractiveModeTests.cs` - Integration tests

---

### 📊 Shell Completion Generation
**Status:** ✅ Completed
**Priority:** Medium

Generate tab completion scripts for various shells:
```bash
# Generate completion script
myapp --generate-completion bash > myapp-completion.sh
myapp --generate-completion powershell > myapp-completion.ps1
myapp --generate-completion zsh > _myapp
myapp --generate-completion fish > myapp.fish
```

**Implemented Features:**
- ✅ Bash completion script generation
- ✅ Zsh completion script generation
- ✅ PowerShell completion script generation
- ✅ Fish completion script generation
- ✅ Support for nested subcommands
- ✅ Support for command and action aliases
- ✅ Global options included in completions
- ✅ Action-specific options included
- ✅ Case-insensitive shell name matching
- ✅ Comprehensive test coverage (15+ integration tests)

**Usage:**
```bash
# Generate and install completions for each shell
# Bash
myapp --generate-completion bash > ~/.bash_completion.d/myapp

# Zsh
myapp --generate-completion zsh > ~/.zsh/completions/_myapp

# PowerShell
myapp --generate-completion powershell > $PROFILE

# Fish
myapp --generate-completion fish > ~/.config/fish/completions/myapp.fish
```

**Files Added:**
- `TeCLI/Generators/CommandLineArgsGenerator.Completion.cs` - Completion script generators
- `TeCLI.Tests/TestCommands/CompletionTestCommand.cs` - Test command for completion
- `TeCLI.Tests/CompletionGenerationTests.cs` - Comprehensive integration tests

**Files Modified:**
- `TeCLI/Generators/CommandLineArgsGenerator.Commands.cs` - Added --generate-completion flag check
- `TeCLI/Generators/CommandLineArgsGenerator.Help.cs` - Updated help text to show --generate-completion

---

### 📊 ANSI Color and Styling Support
**Status:** Planned
**Priority:** Medium

Enhance help text and output with colors:
```csharp
[Action("status")]
public void Status()
{
    Console.WriteSuccess("Operation completed successfully");
    Console.WriteWarning("Cache is stale");
    Console.WriteError("Connection failed");
}
```

**Features:**
- Colored help text (syntax highlighting)
- Helper methods for colored output
- Automatic color detection (NO_COLOR, terminal support)
- Integration with Spectre.Console or similar libraries

---

### 💡 Progress Indicators
**Status:** Planned
**Priority:** Low

Built-in progress reporting helpers:
```csharp
[Action("process")]
public async Task Process([Argument] string[] files)
{
    using var progress = Console.CreateProgressBar(files.Length);
    foreach (var file in files)
    {
        await ProcessFile(file);
        progress.Increment();
    }
}
```

---

## Configuration and Settings

### 📊 Configuration File Support
**Status:** Planned
**Priority:** Medium

Load options from configuration files:
```json
{
  "deploy": {
    "environment": "production",
    "region": "us-west",
    "verbose": true
  }
}
```

**Features:**
- Multiple format support (JSON, YAML, TOML, INI)
- Configuration file discovery (`.teclirc`, `tecli.json`, etc.)
- Merge strategy: file < environment < CLI arguments
- Per-command configuration sections

---

### 💡 Configuration Profiles
**Status:** Planned
**Priority:** Low

Named configuration profiles:
```json
{
  "profiles": {
    "dev": {
      "environment": "development",
      "verbose": true
    },
    "prod": {
      "environment": "production",
      "verbose": false
    }
  }
}
```
```bash
myapp deploy --profile prod
```

---

## Advanced Features

### 📊 Middleware/Hooks System
**Status:** ✅ Completed
**Priority:** Medium

TeCLI now supports a comprehensive middleware/hooks system with pre-execution, post-execution, and error handling hooks! This enables scenarios like:

```csharp
// Hook interfaces
public class AuthenticationHook : IBeforeExecuteHook
{
    public Task BeforeExecuteAsync(HookContext context)
    {
        // Validate authentication before action executes
        if (!IsAuthenticated())
        {
            context.IsCancelled = true;
            context.CancellationMessage = "Authentication required";
        }
        return Task.CompletedTask;
    }
}

public class LoggingHook : IAfterExecuteHook
{
    public Task AfterExecuteAsync(HookContext context, object? result)
    {
        // Log action execution
        Console.WriteLine($"Executed: {context.ActionName}");
        return Task.CompletedTask;
    }
}

public class ErrorHandlerHook : IOnErrorHook
{
    public Task<bool> OnErrorAsync(HookContext context, Exception exception)
    {
        // Handle or log errors
        Console.WriteLine($"Error: {exception.Message}");
        return Task.FromResult(true); // true = handled, false = propagate
    }
}

// Apply hooks at command level (inherited by all actions)
[Command("api")]
[BeforeExecute(typeof(AuthenticationHook))]
[AfterExecute(typeof(LoggingHook))]
[OnError(typeof(ErrorHandlerHook))]
public class ApiCommand
{
    // This action inherits command-level hooks
    [Action("call")]
    public void Call() { }

    // This action has both command-level and action-level hooks
    [Action("admin")]
    [BeforeExecute(typeof(AdminAuthHook), Order = 10)]
    public void Admin() { }
}
```

**Implemented Features:**
- ✅ Three hook types: `IBeforeExecuteHook`, `IAfterExecuteHook`, `IOnErrorHook`
- ✅ `HookContext` for sharing data and execution context between hooks
- ✅ Command-level hooks (inherited by all actions in the command)
- ✅ Action-level hooks (specific to individual actions)
- ✅ Hook ordering with `Order` property
- ✅ Cancellation support via `HookContext.IsCancelled`
- ✅ Error handling with option to suppress exceptions
- ✅ Multiple hooks per action with ordered execution
- ✅ Comprehensive test coverage

**Hook Interfaces:**
- `IBeforeExecuteHook` - Execute before action, can cancel execution
- `IAfterExecuteHook` - Execute after successful action completion
- `IOnErrorHook` - Execute when action throws exception, can handle or propagate

**Hook Attributes:**
- `[BeforeExecute(Type, Order = 0)]` - Apply before-execution hook
- `[AfterExecute(Type, Order = 0)]` - Apply after-execution hook
- `[OnError(Type, Order = 0)]` - Apply error-handling hook

**HookContext Properties:**
- `CommandName` - The command being executed
- `ActionName` - The action being executed
- `Arguments` - The command-line arguments
- `Data` - Dictionary for sharing data between hooks
- `IsCancelled` - Set to true to cancel action execution
- `CancellationMessage` - Message to display when cancelled

**Use Cases:**
- Authentication/authorization before action execution
- Logging and telemetry for all actions
- Resource initialization/cleanup
- Transaction management
- Error logging and handling
- Validation before execution
- Performance monitoring

**Files Changed:**
- `TeCLI.Core/Hooks/HookInterfaces.cs` - Hook interfaces and context
- `TeCLI.Core/Hooks/HookAttributes.cs` - Hook attributes
- `TeCLI.Tools/Generators/ActionSourceInfo.cs` - Hook tracking in actions
- `TeCLI.Tools/Generators/CommandSourceInfo.cs` - Hook tracking in commands
- `TeCLI/Generators/CommandLineArgsGenerator.Actions.cs` - Hook code generation
- `TeCLI/Generators/CommandLineArgsGenerator.Commands.cs` - Hook extraction and dispatch
- `TeCLI.Tests/TestHooks/TestHooks.cs` - Test hook implementations
- `TeCLI.Tests/TestCommands/HooksCommand.cs` - Test commands with hooks
- `TeCLI.Tests/HooksTests.cs` - Comprehensive integration tests

---

### 📊 Exit Code Management
**Status:** ✅ Completed
**Priority:** Medium

TeCLI now supports structured exit codes from actions! Actions can return `int`, `ExitCode` enum, `Task<int>`, or `Task<ExitCode>` to control the process exit code.

```csharp
// Built-in ExitCode enum
public enum ExitCode
{
    Success = 0,
    Error = 1,
    InvalidArguments = 2,
    FileNotFound = 3,
    PermissionDenied = 4,
    NetworkError = 5,
    // ... plus BSD sysexits.h compatible codes (64-78)
}

// Return ExitCode from actions
[Action("process")]
[MapExitCode(typeof(FileNotFoundException), ExitCode.FileNotFound)]
public ExitCode Process([Argument] string file)
{
    if (!File.Exists(file))
        return ExitCode.FileNotFound;

    // Process file
    return ExitCode.Success;
}

// Return int directly
[Action("copy")]
public int Copy([Argument] string source, [Argument] string dest)
{
    if (!File.Exists(source))
        return 3; // File not found

    // Copy file
    return 0; // Success
}

// Async actions with exit codes
[Action("download")]
public async Task<ExitCode> Download([Argument] string url)
{
    // ...
    return ExitCode.Success;
}

// Program.cs - capture exit code
var dispatcher = new CommandDispatcher();
var exitCode = await dispatcher.DispatchAsync(args);
return exitCode;
```

**Implemented Features:**
- ✅ `ExitCode` enum with standard exit codes (0-8 and BSD sysexits.h 64-78)
- ✅ Return `int` or `ExitCode` from sync actions
- ✅ Return `Task<int>` or `Task<ExitCode>` from async actions
- ✅ Custom enum types with int underlying type supported
- ✅ `[MapExitCode]` attribute for exception-to-exit-code mapping
- ✅ Exception mappings can be defined at command or action level
- ✅ `DispatchAsync` returns `Task<int>` with the exit code
- ✅ `LastExitCode` property on dispatcher
- ✅ Exit code passed to `AfterExecute` hooks
- ✅ Automatic enum-to-int conversion for exit codes

**Use Cases:**
- Script integration (exit codes for shell scripting)
- CI/CD pipelines that check exit codes
- Proper error signaling to parent processes
- Structured error handling with exception mapping

**Files Changed:**
- `TeCLI/AttributeNames.cs` - Added MapExitCodeAttribute constant
- `TeCLI/Generators/CommandLineArgsGenerator.Attributes.cs` - Added ExitCode enum and MapExitCodeAttribute
- `TeCLI/Generators/CommandLineArgsGenerator.Invoker.cs` - Added exit code support to invokers
- `TeCLI/Generators/CommandLineArgsGenerator.Commands.cs` - DispatchAsync returns Task<int>, added exit code extraction
- `TeCLI/Generators/CommandLineArgsGenerator.Actions.cs` - Exit code extraction and hook updates
- `TeCLI/Generators/CommandLineArgsGenerator.Parameters.cs` - Invoker selection based on return type
- `TeCLI.Tools/Generators/ActionSourceInfo.cs` - Added return type info and exit code mappings

---

### 💡 Pipeline and Stream Support
**Status:** ✅ Completed
**Priority:** Low

TeCLI now supports stream types for stdin/stdout handling! This enables Unix-style pipeline patterns:
```csharp
[Action("transform")]
public void Transform(
    [Option("input", ShortName = 'i')] TextReader input,
    [Option("output", ShortName = 'o')] TextWriter output)
{
    // Supports: cat input.txt | myapp transform | tee output.txt
    var content = input.ReadToEnd();
    output.Write(content.ToUpperInvariant());
}

[Action("process")]
public void Process([Option("data")] Stream data)
{
    // Binary stream support
}
```

**Implemented Features:**
- ✅ Support for `Stream`, `TextReader`, `TextWriter`, `StreamReader`, `StreamWriter` types
- ✅ Automatic stdin detection via `Console.IsInputRedirected`
- ✅ Automatic stdout detection via `Console.IsOutputRedirected`
- ✅ Special handling for `-` as stdin/stdout (Unix convention)
- ✅ File path support - automatically opens file streams
- ✅ Works with both options and arguments
- ✅ Environment variable fallback for stream paths
- ✅ Proper stream direction detection (input/output/bidirectional)
- ✅ Comprehensive test coverage

**Supported Stream Types:**
| Type | Direction | stdin/stdout | File |
|------|-----------|--------------|------|
| `Stream` | Bidirectional | `Console.OpenStandardInput/Output()` | `FileStream` |
| `TextReader` | Input | `Console.In` | `StreamReader` |
| `TextWriter` | Output | `Console.Out` | `StreamWriter` |
| `StreamReader` | Input | wraps stdin | from file path |
| `StreamWriter` | Output | wraps stdout | from file path |

**Files Changed:**
- `TeCLI.Tools/Extensions.cs` - Added stream type detection methods
- `TeCLI.Tools/Generators/ParameterSourceInfo.cs` - Added stream tracking properties
- `TeCLI/Generators/ParameterInfoExtractor.cs` - Extract stream type information
- `TeCLI/Generators/ParameterCodeGenerator.cs` - Generate stream creation code
- `TeCLI.Tests/TestCommands/StreamCommand.cs` - Test command for streams
- `TeCLI.Tests/StreamSupportTests.cs` - Comprehensive integration tests

---

### 💡 Dry Run Pattern
**Status:** Planned
**Priority:** Low

Common `--dry-run` pattern support:
```csharp
[Action("deploy")]
public void Deploy(
    [Option("dry-run")] bool dryRun,
    [Argument] string environment)
{
    if (dryRun)
        Console.WriteLine($"Would deploy to {environment}");
    else
        ActuallyDeploy(environment);
}
```

Consider making this a first-class feature with automatic simulation support.

---

## Developer Experience

### 📊 Better Testing Utilities
**Status:** ✅ Completed
**Priority:** Medium

TeCLI now provides a comprehensive testing extension package (`TeCLI.Extensions.Testing`) with utilities for testing CLI applications:

```csharp
// Create a test host for your dispatcher
var host = CommandTestHost.Create<CommandDispatcher>();

// Execute using fluent argument builder
var result = await host.ExecuteAsync(
    ArgumentBuilder.Command("deploy")
        .Action("production")
        .Option("region", "us-west-2")
        .Flag("force"));

// Use chainable assertions
result
    .ShouldSucceed()
    .ShouldContainOutput("Deployed successfully")
    .ShouldNotContainError("Error")
    .ShouldCompleteWithin(TimeSpan.FromSeconds(5));

// Test with mock console input for interactive commands
var result = await host.ExecuteWithInputAsync(
    new[] { "interactive", "login" },
    new[] { "username", "password" });
```

**Implemented Features:**
- ✅ `CommandTestHost<T>` - Test harness for executing commands in isolated environment
- ✅ `TestConsole` - Mock console for capturing stdout/stderr and providing stdin
- ✅ `ArgumentBuilder` - Fluent API for building command-line arguments
- ✅ `CommandResult` - Encapsulates execution results (output, error, exit code, exception)
- ✅ Chainable assertion methods (`ShouldSucceed`, `ShouldContainOutput`, `ShouldThrow<T>`, etc.)
- ✅ Support for testing interactive prompts with mock input
- ✅ Framework-agnostic (works with xUnit, NUnit, MSTest, etc.)
- ✅ Comprehensive test coverage (85+ tests)

**Files Added:**
- `TeCLI.Extensions.Testing/CommandTestHost.cs` - Test harness
- `TeCLI.Extensions.Testing/TestConsole.cs` - Mock console I/O
- `TeCLI.Extensions.Testing/ArgumentBuilder.cs` - Fluent argument builder
- `TeCLI.Extensions.Testing/CommandResult.cs` - Execution result wrapper
- `TeCLI.Extensions.Testing/CommandResultAssertions.cs` - Assertion helpers
- `TeCLI.Extensions.Testing.Tests/` - Comprehensive test suite

---

### 📊 Source Generator Debugging Improvements
**Status:** Planned
**Priority:** Medium

Better developer experience for source generators:
- Improved error messages with code context
- Roslyn analyzer suggestions for common mistakes
- Generated code documentation/comments
- Debug visualization of generated code

---

### 💡 Localization Support (i18n)
**Status:** Research Needed
**Priority:** Low

Internationalization for help text and error messages:
```csharp
[Command("greet", DescriptionResourceKey = "GreetCommand_Description")]
public class GreetCommand
{
    [Primary(DescriptionResourceKey = "GreetCommand_Hello_Description")]
    public void Hello([Argument] string name) { }
}
```

**Challenges:**
- Resource file integration
- Culture detection
- Pluralization support

---

### 💡 Plugin System
**Status:** Research Needed
**Priority:** Low

Allow dynamic loading of commands from external assemblies:
```csharp
CommandDispatcher.LoadPlugins("./plugins");
await CommandDispatcher.DispatchAsync(args);
```

**Use Cases:**
- Extensible CLI tools
- Third-party command integration
- Modular architecture

---

## Output and Formatting

### 📊 Structured Output Support
**Status:** Planned
**Priority:** Medium

Built-in JSON/XML output formatting:
```csharp
[Action("list")]
public object List([Option("format")] OutputFormat format)
{
    var items = GetItems();

    // Framework automatically serializes based on --format
    return items;
}
// myapp list --format json
```

**Features:**
- JSON serialization
- XML serialization
- Table formatting
- Custom formatters

---

### 💡 Paging Support
**Status:** Planned
**Priority:** Low

Automatic paging for long output:
```csharp
[Action("list")]
[EnablePaging]
public void List()
{
    // Long output automatically pipes to less/more
}
```

---

## Quality and Tooling

### 📊 Additional Analyzers
**Status:** Planned
**Priority:** Medium

New Roslyn analyzers:
- **CLI013**: Warn when argument has default value before required argument
- **CLI014**: Suggest using container parameters for 4+ options
- **CLI015**: Detect unused [Action] methods
- **CLI016**: Validate validation attribute combinations
- **CLI017**: Warn about potential name collisions with reserved switches

---

### 📊 Integration with Popular Libraries
**Status:** Planned
**Priority:** Medium

Official integration packages:
- `TeCLI.Extensions.Logging` - ILogger integration
- `TeCLI.Extensions.Configuration` - IConfiguration integration
- `TeCLI.Extensions.Hosting` - Generic Host integration
- `TeCLI.Extensions.Spectre` - Spectre.Console integration for rich output

---

## Future Extension Packages

The following extension packages are planned for future development to expand TeCLI's ecosystem.

### 🎯 TeCLI.Extensions.Logging
**Status:** Planned
**Priority:** High

Integration with `Microsoft.Extensions.Logging` and popular logging frameworks:

```csharp
[Command("process")]
public class ProcessCommand
{
    private readonly ILogger<ProcessCommand> _logger;

    public ProcessCommand(ILogger<ProcessCommand> logger)
    {
        _logger = logger;
    }

    [Action("run")]
    public void Run([Option("verbose", ShortName = 'v')] bool verbose)
    {
        _logger.LogInformation("Starting process...");
    }
}

// Program.cs
services.AddCommandDispatcher();
services.AddLogging(builder => builder.AddConsole());
```

**Planned Features:**
- Auto-inject `ILogger<T>` into commands
- Log command invocations, arguments, and execution times
- Configure log levels via CLI options (`--verbose`, `--quiet`, `--log-level`)
- Integration with Serilog, NLog, and other providers

---

### 🎯 TeCLI.Extensions.Configuration
**Status:** Planned
**Priority:** High

Integration with `Microsoft.Extensions.Configuration`:

```csharp
// appsettings.json
{
  "Deploy": {
    "DefaultEnvironment": "staging",
    "DefaultRegion": "us-west-2"
  }
}

[Command("deploy")]
public class DeployCommand
{
    [Action("run")]
    public void Run(
        [Option("environment")] string environment,  // Falls back to config
        [Option("region")] string region)
    {
    }
}

// Precedence: CLI > Environment Variable > Config File > Default Value
```

**Planned Features:**
- Load option defaults from `appsettings.json`, user secrets, or config files
- Support `--config <file>` option pattern
- Environment-specific configurations
- Per-command configuration sections

---

### 📊 TeCLI.Extensions.Output
**Status:** Planned
**Priority:** Medium

Structured output formatting with multiple format support:

```csharp
[Command("list")]
public class ListCommand
{
    [Action("users")]
    [OutputFormat]  // Enables --output json|xml|table|yaml
    public IEnumerable<User> ListUsers()
    {
        return _userService.GetAll();
    }
}

// Usage:
// myapp list users --output json
// myapp list users --output table
```

**Planned Features:**
- `[OutputFormat]` attribute to enable `--output json|xml|table|yaml`
- Custom `IOutputFormatter<T>` interface for custom formats
- Table rendering with column alignment, colors
- Integration with `Spectre.Console` for rich tables

---

### 📊 TeCLI.Extensions.Hosting
**Status:** Planned
**Priority:** Medium

Integration with `Microsoft.Extensions.Hosting` for long-running CLI services:

```csharp
await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddCommandDispatcher();
        services.AddHostedService<BackgroundWorker>();
    })
    .RunCommandLineAsync(args);
```

**Planned Features:**
- Long-running CLI services (daemons)
- Background task support with `IHostedService`
- Graceful shutdown handling
- Integration with hosted services
- Health checks support

---

### 📊 TeCLI.Extensions.Progress
**Status:** Planned
**Priority:** Medium

Rich terminal UI elements for progress and status:

```csharp
[Action("download")]
public async Task Download(
    [Argument] string url,
    IProgressContext progress)  // Auto-injected
{
    using var bar = progress.CreateProgressBar("Downloading...");

    await foreach (var chunk in DownloadChunksAsync(url))
    {
        bar.Increment(chunk.Length);
    }
}

[Action("process")]
public async Task Process([Argument] string[] files)
{
    using var spinner = progress.CreateSpinner("Processing...");

    foreach (var file in files)
    {
        spinner.Status = $"Processing {file}...";
        await ProcessFileAsync(file);
    }
}
```

**Planned Features:**
- `[Progress]` for progress bar support
- Spinner animations during async operations
- Status messages and tables
- Integration with `Spectre.Console`
- Auto-detect terminal capabilities

---

### 📊 TeCLI.Extensions.Resilience
**Status:** Planned
**Priority:** Medium

Integration with Polly for retry and resilience patterns:

```csharp
[Command("api")]
public class ApiCommand
{
    [Action("call")]
    [Retry(attempts: 3, delayMs: 1000)]
    [Timeout(seconds: 30)]
    [CircuitBreaker(failuresBeforeBreak: 5)]
    public async Task CallApi([Argument] string endpoint)
    {
        await _httpClient.GetAsync(endpoint);
    }
}
```

**Planned Features:**
- Retry policies via `[Retry(attempts: 3)]` attribute
- Timeout handling via `[Timeout(seconds: 30)]`
- Circuit breaker patterns for external calls
- Integration with Polly library
- Configurable backoff strategies

---

### 💡 TeCLI.Extensions.Auth
**Status:** Planned
**Priority:** Low

Authentication and authorization support for CLIs:

```csharp
[Command("api")]
[RequiresAuth]
public class ApiCommand
{
    [Action("call")]
    [RequiresScope("api.read")]
    public void Call([Argument] string endpoint) { }
}

// Built-in auth commands
// myapp auth login
// myapp auth logout
// myapp auth status
```

**Planned Features:**
- OAuth2/OIDC support (`--login`, `--logout`)
- Token caching and automatic refresh
- `[RequiresAuth]` and `[RequiresScope]` attributes
- API key management
- Device code flow for headless environments

---

### 💡 TeCLI.Extensions.Telemetry
**Status:** Planned
**Priority:** Low

Usage tracking and diagnostics:

```csharp
services.AddCommandDispatcher();
services.AddTelemetry(options =>
{
    options.EnableAnonymousUsageTracking = true;  // Opt-in
    options.AddAppInsights(connectionString);
    options.AddSentry(dsn);
});
```

**Planned Features:**
- Anonymous usage analytics (opt-in)
- Error reporting integration (Sentry, App Insights)
- Performance metrics collection
- `[TrackUsage]` attribute for specific commands
- GDPR-compliant data collection

---

### 💡 TeCLI.Extensions.Interactive
**Status:** Planned
**Priority:** Low

Enhanced REPL-like interactive functionality:

```csharp
[Command("shell")]
[Interactive]  // Enables REPL mode
public class ShellCommand
{
    [Action("query")]
    public void Query([Argument] string sql) { }
}

// Usage:
// myapp shell
// > query SELECT * FROM users
// > query SELECT * FROM orders
// > exit
```

**Planned Features:**
- `[Interactive]` attribute for REPL mode
- Command history with up/down arrows
- Auto-complete in shell
- Persistent session state between commands
- `Spectre.Console` or `Terminal.Gui` integration

---

### 💡 TeCLI.Extensions.Caching
**Status:** Planned
**Priority:** Low

Command result caching for expensive operations:

```csharp
[Command("api")]
public class ApiCommand
{
    [Action("fetch")]
    [Cacheable(duration: "1h", key: "{endpoint}")]
    public async Task<string> Fetch([Argument] string endpoint)
    {
        return await _httpClient.GetStringAsync(endpoint);
    }
}

// Usage:
// myapp api fetch https://api.example.com/data
// myapp api fetch https://api.example.com/data --no-cache
```

**Planned Features:**
- `[Cacheable(duration: "1h")]` attribute
- File-based or memory cache
- Cache invalidation via `--no-cache` flag
- Cache key templates with parameter substitution
- Cache statistics and management commands

---

### 🔬 TeCLI.Extensions.Plugins
**Status:** Research Needed
**Priority:** Low

Runtime-loadable command extensions:

```csharp
// Main application
await CommandDispatcher.DiscoverPlugins("./plugins");
await CommandDispatcher.DispatchAsync(args);

// Plugin assembly (separate project)
[Plugin("my-plugin", Version = "1.0.0")]
[Command("custom")]
public class CustomCommand
{
    [Action("run")]
    public void Run() { }
}
```

**Planned Features:**
- Discover commands from external assemblies
- Plugin manifest format for metadata
- Version compatibility checking
- Plugin install/update/remove commands
- Sandboxed execution for security

---

### 💡 Code Snippets and Templates
**Status:** Planned
**Priority:** Low

Project templates and code snippets:
```bash
dotnet new tecli-console -n MyCliApp
dotnet new tecli-command -n DeployCommand
```

**Includes:**
- Visual Studio snippets
- VS Code snippets
- Rider templates
- CLI project templates

---

## Performance Optimizations

### 📊 Performance Benchmarks Expansion
**Status:** Planned
**Priority:** Medium

Additional benchmarks to track:
- Full command dispatch performance
- Large command set (100+ commands) discovery
- Complex parameter parsing
- Comparison with System.CommandLine, CommandLineParser, etc.

---

### 💡 Lazy Command Discovery
**Status:** Research Needed
**Priority:** Low

Defer command reflection until needed for very large CLI apps with many commands.

---

## Documentation Enhancements

### 📊 Comprehensive Examples Repository
**Status:** Planned
**Priority:** Medium

Expand examples:
- Real-world CLI application (file processor, API client, etc.)
- Advanced DI scenarios
- Custom validators
- Middleware examples
- Testing patterns

---

### 📊 Migration Guides
**Status:** Planned
**Priority:** Medium

Help users migrate from other CLI libraries:
- From System.CommandLine
- From CommandLineParser
- From Spectre.Console.Cli
- From McMaster.Extensions.CommandLineUtils

---

## Breaking Changes (v2.0)

Items that would require major version bump:

### 🔬 Redesign Attribute Names
Consider more concise attribute names:
- `[Cmd]` instead of `[Command]`
- `[Opt]` instead of `[Option]`
- `[Arg]` instead of `[Argument]`

### 🔬 Move to Native AOT Support
Full support for .NET Native AOT compilation

### 🔬 Support for Source-Generated Dependency Injection
Replace reflection-based DI with source-generated factories

---

## Community Requests

This section will be populated based on GitHub issues and community feedback.

**How to Contribute:**
1. Open an issue describing the feature
2. Label it as `enhancement` or `feature-request`
3. Discuss implementation approach
4. Submit a PR with implementation and tests

---

## Priorities for Next Release

Based on impact and feasibility, the next release should focus on:

1. ✅ Array/Collection Support (High impact, moderate complexity) - **COMPLETED**
2. ✅ Enum Support (High impact, low complexity) - **COMPLETED**
3. ✅ Improved Error Messages with Suggestions (High impact, moderate complexity) - **COMPLETED**
4. ✅ Required Options (High impact, low complexity) - **COMPLETED**
5. ✅ Automatic Version Flag (High impact, low complexity) - **COMPLETED**

## Priorities for Future Releases

The following high-priority items should be considered next:

1. **Configuration File Support** (📊 Medium Priority) - Load options from configuration files
2. **ANSI Color and Styling Support** (📊 Medium Priority) - Enhanced help text and colored output
3. **Exit Code Management** (📊 Medium Priority) - Structured exit code support

---

*Last Updated: 2025-12-02*
