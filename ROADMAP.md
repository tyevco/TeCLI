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
**Status:** Planned
**Priority:** High

Currently, TeCLI only supports primitive types. Adding support for collections would enable scenarios like:
```csharp
[Action("process")]
public void Process(
    [Option("files", ShortName = 'f')] string[] files,
    [Option("tags")] List<string> tags)
{
    // myapp process --files file1.txt --files file2.txt --tags tag1 --tags tag2
}
```

**Implementation Considerations:**
- Support for arrays (`T[]`)
- Support for `List<T>`, `IEnumerable<T>`, `ICollection<T>`
- Repeatable options syntax (`--file a.txt --file b.txt`)
- Comma-separated values syntax (`--files a.txt,b.txt,c.txt`)
- New analyzers to validate collection types

---

### 🎯 Enum Support
**Status:** Planned
**Priority:** High

Enable parsing of enum types with automatic validation:
```csharp
public enum LogLevel { Debug, Info, Warning, Error }

[Action("run")]
public void Run([Option("log-level")] LogLevel level = LogLevel.Info)
{
    // myapp run --log-level Debug
}
```

**Features:**
- Case-insensitive enum parsing
- Automatic validation with helpful error messages
- Help text shows valid enum values
- Support for `[Flags]` enums

---

### 🎯 Required Options
**Status:** Planned
**Priority:** High

Allow marking options as required (currently only arguments can be required):
```csharp
[Action("deploy")]
public void Deploy(
    [Option("environment", Required = true)] string environment,
    [Option("region")] string region = "us-west")
{
    // myapp deploy --environment production --region us-east
}
```

**Implementation:**
- Add `Required` property to `OptionAttribute`
- Validation during parsing
- Clear error messages for missing required options

---

### 📊 Custom Type Converters
**Status:** Planned
**Priority:** Medium

Allow users to register custom parsers for complex types:
```csharp
public class UriConverter : ITypeConverter<Uri>
{
    public Uri Parse(string value) => new Uri(value);
}

[Action("fetch")]
public void Fetch([Option("url")] Uri endpoint)
{
    // Custom parsing handles Uri type
}
```

**Features:**
- `ITypeConverter<T>` interface
- Registration mechanism (attribute or global registry)
- Support for common types (Uri, DateTime, TimeSpan, FileInfo, DirectoryInfo)

---

### 📊 Validation Attributes
**Status:** Planned
**Priority:** Medium

Add declarative validation for options and arguments:
```csharp
[Action("process")]
public void Process(
    [Option("port")] [Range(1, 65535)] int port,
    [Option("email")] [EmailAddress] string email,
    [Option("pattern")] [RegularExpression(@"^\w+$")] string pattern,
    [Argument] [FileExists] string inputFile)
{
}
```

**Validation Types:**
- Range validation (numeric bounds)
- Regular expression patterns
- File/directory existence checks
- Email, URL validation
- Custom validation attributes

---

### 📊 Environment Variable Binding
**Status:** Planned
**Priority:** Medium

Allow options to be populated from environment variables:
```csharp
[Action("connect")]
public void Connect(
    [Option("api-key", EnvVar = "API_KEY")] string apiKey,
    [Option("timeout", EnvVar = "TIMEOUT")] int timeout = 30)
{
    // Can be set via --api-key OR API_KEY environment variable
}
```

**Features:**
- Environment variable fallback when option not provided
- Prefix support for namespacing (e.g., `MYAPP_`)
- Clear precedence: CLI option > environment variable > default value

---

## Subcommand and Command Organization

### 🎯 Nested Subcommands
**Status:** Research Needed
**Priority:** High

Support hierarchical command structures like Git:
```csharp
[Command("git")]
public class GitCommand
{
    [Command("remote")]
    public class RemoteCommand
    {
        [Action("add")]
        public void Add(string name, string url) { }

        [Action("remove")]
        public void Remove(string name) { }
    }
}
// Usage: myapp git remote add origin https://...
```

**Challenges:**
- Source generator complexity
- Help text generation for nested structures
- Backward compatibility

---

### 📊 Command Aliases
**Status:** Planned
**Priority:** Medium

Allow commands and actions to have multiple names:
```csharp
[Command("remove", Aliases = new[] { "rm", "delete" })]
public class RemoveCommand { }

[Action("list", Aliases = new[] { "ls" })]
public void List() { }
```

**Use Cases:**
- Short/long command names (`rm` vs `remove`)
- Backward compatibility when renaming commands
- Common abbreviations

---

### 📊 Global Options
**Status:** Planned
**Priority:** Medium

Options available across all commands:
```csharp
public class GlobalOptions
{
    [Option("verbose", ShortName = 'v')]
    public bool Verbose { get; set; }

    [Option("config")]
    public string ConfigFile { get; set; }
}

[Command("build")]
public class BuildCommand
{
    [Primary]
    public void Build(GlobalOptions globals, [Option("output")] string output)
    {
        if (globals.Verbose) { /* ... */ }
    }
}
```

---

### 💡 Mutual Exclusivity
**Status:** Planned
**Priority:** Low

Mark options as mutually exclusive:
```csharp
[Action("output")]
public void Output(
    [Option("json", MutuallyExclusiveSet = "format")] bool json,
    [Option("xml", MutuallyExclusiveSet = "format")] bool xml,
    [Option("yaml", MutuallyExclusiveSet = "format")] bool yaml)
{
    // Only one of json, xml, or yaml can be specified
}
```

---

## User Experience Enhancements

### 🎯 Automatic Version Flag
**Status:** Planned
**Priority:** High

Automatically handle `--version` flag:
```csharp
[assembly: AssemblyVersion("1.2.3")]
[assembly: AssemblyInformationalVersion("1.2.3-beta")]

// Automatically generates --version handler
// myapp --version
// Output: myapp 1.2.3-beta
```

**Features:**
- Reads from assembly attributes
- Customizable version format
- Reserved switch like `--help`

---

### 🎯 Improved Error Messages with Suggestions
**Status:** Planned
**Priority:** High

Provide helpful suggestions for typos and mistakes:
```
Error: Unknown command 'buld'
Did you mean 'build'?

Error: Unknown option '--enviornment'
Did you mean '--environment'?
```

**Implementation:**
- Levenshtein distance for suggestions
- Show similar commands/options
- Contextual help hints

---

### 📊 Interactive Mode
**Status:** Planned
**Priority:** Medium

Prompt users for missing required arguments:
```csharp
[Action("deploy")]
public void Deploy(
    [Argument(Prompt = "Enter deployment environment")] string environment,
    [Option("region", Prompt = "Select region")] string region)
{
}
```

**Features:**
- Optional prompts for missing values
- Validation on prompted input
- Support for secure input (passwords)
- Integration with libraries like Spectre.Console

---

### 📊 Shell Completion Generation
**Status:** Planned
**Priority:** Medium

Generate tab completion scripts for various shells:
```bash
# Generate completion script
myapp --generate-completion bash > myapp-completion.sh
myapp --generate-completion powershell > myapp-completion.ps1
myapp --generate-completion zsh > _myapp
```

**Supported Shells:**
- Bash
- Zsh
- PowerShell
- Fish

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
**Status:** Planned
**Priority:** Medium

Pre and post-execution hooks:
```csharp
[Command("api")]
[BeforeExecute(typeof(AuthenticationMiddleware))]
[AfterExecute(typeof(LoggingMiddleware))]
public class ApiCommand
{
    [Action("call")]
    public void Call() { }
}
```

**Use Cases:**
- Authentication/authorization
- Logging and telemetry
- Resource initialization/cleanup
- Transaction management

---

### 📊 Exit Code Management
**Status:** Planned
**Priority:** Medium

Structured exit code support:
```csharp
public enum ExitCode
{
    Success = 0,
    InvalidArguments = 1,
    FileNotFound = 2,
    NetworkError = 3
}

[Action("process")]
public ExitCode Process([Argument] string file)
{
    if (!File.Exists(file))
        return ExitCode.FileNotFound;

    // Process file
    return ExitCode.Success;
}
```

**Features:**
- Return exit codes from actions
- Automatic exit code mapping
- Convention-based codes (exceptions → specific codes)

---

### 💡 Pipeline and Stream Support
**Status:** Planned
**Priority:** Low

Better stdin/stdout handling:
```csharp
[Action("transform")]
public void Transform(
    [Option("input")] Stream input = Console.In,
    [Option("output")] Stream output = Console.Out)
{
    // Supports: cat input.txt | myapp transform | tee output.txt
}
```

**Features:**
- Detect piped input
- Special handling for `-` as stdin/stdout
- Binary stream support

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
**Status:** Planned
**Priority:** Medium

Helpers for testing CLI applications:
```csharp
[Fact]
public async Task TestDeployCommand()
{
    var result = await CommandTester.ExecuteAsync<DeployCommand>(
        "deploy", "--environment", "staging");

    Assert.Equal(0, result.ExitCode);
    Assert.Contains("Deployed successfully", result.Output);
}
```

**Features:**
- In-memory command execution
- Output capturing (stdout/stderr)
- Exit code verification
- Integration with xUnit, NUnit, MSTest

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

1. ✅ Array/Collection Support (High impact, moderate complexity)
2. ✅ Enum Support (High impact, low complexity)
3. ✅ Required Options (High impact, low complexity)
4. ✅ Automatic Version Flag (Medium impact, low complexity)
5. ✅ Better Error Messages (High impact, moderate complexity)

---

*Last Updated: 2025-11-20*
