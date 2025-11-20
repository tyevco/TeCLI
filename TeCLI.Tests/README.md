# TeCLI Integration Tests

This project contains integration tests that verify the generated CommandDispatcher works correctly with real command classes.

## Running Tests

```bash
cd TeCLI.Tests
dotnet test
```

## Test Structure

### TestCommands/
Contains sample command classes used in integration tests:
- **SimpleCommand** - Basic command with primary action and arguments
- **OptionsCommand** - Command demonstrating option parsing (long and short forms)

### IntegrationTests.cs
End-to-end tests that:
1. Create command line arguments
2. Invoke the generated CommandDispatcher
3. Verify the correct command/action was executed
4. Validate parameter parsing worked correctly

## What's Being Tested

- ✅ Primary action invocation (default command behavior)
- ✅ Named action invocation with arguments
- ✅ Short option parsing (-e, -f, -t)
- ✅ Long option parsing (--environment, --force, --timeout)
- ✅ Default value handling for optional parameters
- ✅ Boolean switch options
- ✅ Typed option parsing (string, bool, int)

## Future Test Cases

Consider adding tests for:
- Multiple required arguments
- Mixed options and arguments
- Error cases (missing required arguments, invalid values)
- Async action methods
- Container parameters
- Help text generation
- Command discovery
