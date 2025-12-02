# TeCLI Simple Example

This example demonstrates the basic features of TeCLI for building command-line applications.

## Features Demonstrated

- **Commands** with descriptions
- **Actions** (subcommands) with descriptions
- **Primary actions** (default when no action specified)
- **Arguments** (positional parameters)
- **Options** with short names (`-p` for `--precision`)
- **Default values** for optional parameters
- **Boolean flags/switches**
- **Async actions**

## Usage

### Calculator Command

```bash
# Add two numbers (primary action)
dotnet run -- calc add 10 5
dotnet run -- calc 10 5              # Same as above (add is primary)

# Subtract
dotnet run -- calc subtract 10 3

# Multiply with custom precision
dotnet run -- calc multiply 3.14159 2 --precision 4
dotnet run -- calc multiply 3.14159 2 -p 4    # Short form

# Divide
dotnet run -- calc divide 22 7 -p 6
```

### Greet Command

```bash
# Say hello (primary action)
dotnet run -- greet hello World
dotnet run -- greet World            # Same as above

# Excited greeting
dotnet run -- greet hello World --excited
dotnet run -- greet hello World -e

# Repeat greeting
dotnet run -- greet hello World -t 3

# Say goodbye
dotnet run -- greet goodbye Friend
dotnet run -- greet goodbye Friend --formal

# Send a message with delay
dotnet run -- greet message Alice "How are you?" --delay 1000
```

## Code Structure

- `Program.cs` - Entry point, creates and runs the dispatcher
- `CalculatorCommand.cs` - Calculator with add/subtract/multiply/divide actions
- `GreetCommand.cs` - Greeting utilities with hello/goodbye/message actions
