# TeCLI Integration Tests

This project contains integration tests that verify the generated CommandDispatcher works correctly with real command classes.

## Running Tests

```bash
cd tests/TeCLI.Tests
dotnet test
```

## Test Structure

### TestCommands/

Contains sample command classes used in integration tests:

- **SimpleCommand** - Basic command with primary action and arguments
- **OptionsCommand** - Command demonstrating option parsing (long and short forms)
- **CollectionCommand** - Tests array and collection option/argument parsing
- **EnumCommand** - Tests enum parsing including [Flags] enums
- **ValidationCommand** - Tests validation attributes (Range, RegularExpression, FileExists, DirectoryExists)
- **RequiredOptionsCommand** - Tests required option validation
- **EnvVarCommand** - Tests environment variable binding
- **NestedCommand** - Tests nested subcommand hierarchies
- **AliasesCommand** - Tests command and action aliases
- **GlobalOptionsCommand** - Tests global options shared across commands
- **MutualExclusivityCommand** - Tests mutually exclusive options
- **HooksCommand** - Tests pre/post execution hooks
- **InteractiveModeCommand** - Tests interactive prompts
- **CustomConverterCommand** - Tests custom type converters
- **CommonTypesCommand** - Tests built-in type converters (Uri, DateTime, etc.)
- **ExitCodeCommand** - Tests exit code handling
- **CompletionTestCommand** - Tests shell completion generation
- **StreamCommand** - Tests stream/pipeline support (Stream, TextReader, TextWriter)

### Test Files

Integration tests are organized by feature:

- `IntegrationTests.cs` - Core command dispatch and parsing
- `StreamSupportTests.cs` - Stream and pipeline support tests
- `CollectionSupportTests.cs` - Array and collection parsing
- `EnumSupportTests.cs` - Enum type parsing
- `ValidationTests.cs` - Validation attribute tests
- `RequiredOptionTests.cs` - Required option validation
- `EnvVarTests.cs` - Environment variable binding
- `NestedCommandTests.cs` - Subcommand hierarchy tests
- `AliasesTests.cs` - Command/action alias tests
- `GlobalOptionsTests.cs` - Global options tests
- `MutualExclusivityTests.cs` - Mutually exclusive options
- `HooksTests.cs` - Middleware/hooks system tests
- `InteractiveModeTests.cs` - Interactive prompt tests
- `CustomConverterTests.cs` - Custom type converter tests
- `ErrorSuggestionTests.cs` - Error suggestion tests
- `ExitCodeTests.cs` - Exit code handling tests
- `CompletionGenerationTests.cs` - Shell completion tests

## What's Being Tested

- Primary action invocation (default command behavior)
- Named action invocation with arguments
- Short option parsing (-e, -f, -t)
- Long option parsing (--environment, --force, --timeout)
- Default value handling for optional parameters
- Boolean switch options
- Typed option parsing (string, bool, int, enums)
- Collection options and arguments (arrays, lists)
- Enum parsing with case-insensitivity
- [Flags] enum parsing with comma-separated values
- Validation attributes (Range, RegularExpression, FileExists, DirectoryExists)
- Required options validation
- Environment variable fallback
- Nested subcommand dispatch
- Command and action aliases
- Global options injection
- Mutually exclusive options
- Pre/post execution hooks
- Error hooks
- Interactive prompts
- Custom type converters
- Common type converters (Uri, DateTime, TimeSpan, Guid, etc.)
- Exit code handling and exception mapping
- Shell completion script generation
- Stream and pipeline support (stdin/stdout, TextReader, TextWriter)
- Error messages with suggestions
- Help text generation
