# TeCLI.Example.Console

Example demonstrating TeCLI.Extensions.Console features.

## Features Demonstrated

- Colored console output with `StyledConsole`
- Status messages (success, warning, error, info, debug)
- Inline string styling extensions (`.Red()`, `.Green()`, `.Bold()`)
- Progress bars with customizable appearance
- Spinners for indeterminate operations
- Terminal capability detection
- Custom progress bar styles

## Commands

### Status Command

Display colored status messages:

```bash
# Show all status message types
dotnet run -- status show

# Display inline string styling
dotnet run -- status inline

# Show all available colors
dotnet run -- status colors

# Check terminal capabilities
dotnet run -- status capabilities
```

### Progress Command

Demonstrate progress indicators:

```bash
# Display a progress bar (default 3 seconds)
dotnet run -- progress bar
dotnet run -- progress bar -d 5    # 5 second duration

# Display a spinner
dotnet run -- progress spinner
dotnet run -- progress spinner --fail    # Simulate failure

# Multiple progress operations
dotnet run -- progress multi

# Show different spinner outcomes (success, warning, info, fail)
dotnet run -- progress outcomes

# Custom progress bar styles
dotnet run -- progress custom
```

## Code Structure

- `Program.cs` - Entry point
- `StatusCommand.cs` - Colored output and styling demonstrations
- `ProgressCommand.cs` - Progress bars and spinners

## More Information

See the [TeCLI.Extensions.Console](../../extensions/TeCLI.Extensions.Console/README.md) package documentation.
