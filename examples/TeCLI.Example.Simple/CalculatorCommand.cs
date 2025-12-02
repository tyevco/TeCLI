using TeCLI.Attributes;

namespace TeCLI.Example.Simple;

/// <summary>
/// A simple calculator command demonstrating basic TeCLI features:
/// - Command with description
/// - Multiple actions
/// - Arguments (positional parameters)
/// - Options with short names and defaults
/// - Primary action
/// </summary>
[Command("calc", Description = "A simple calculator for basic arithmetic operations")]
public class CalculatorCommand
{
    /// <summary>
    /// Add two numbers together. This is marked as the primary action,
    /// so it runs when no action is specified: calc 5 3
    /// </summary>
    [Primary]
    [Action("add", Description = "Add two numbers together")]
    public void Add(
        [Argument(Description = "First number")] double a,
        [Argument(Description = "Second number")] double b,
        [Option("precision", ShortName = 'p', Description = "Decimal places to display")] int precision = 2)
    {
        var result = a + b;
        Console.WriteLine($"{a} + {b} = {result.ToString($"F{precision}")}");
    }

    /// <summary>
    /// Subtract the second number from the first
    /// </summary>
    [Action("subtract", Description = "Subtract second number from first")]
    public void Subtract(
        [Argument(Description = "First number")] double a,
        [Argument(Description = "Second number")] double b,
        [Option("precision", ShortName = 'p', Description = "Decimal places to display")] int precision = 2)
    {
        var result = a - b;
        Console.WriteLine($"{a} - {b} = {result.ToString($"F{precision}")}");
    }

    /// <summary>
    /// Multiply two numbers
    /// </summary>
    [Action("multiply", Description = "Multiply two numbers")]
    public void Multiply(
        [Argument(Description = "First number")] double a,
        [Argument(Description = "Second number")] double b,
        [Option("precision", ShortName = 'p', Description = "Decimal places to display")] int precision = 2)
    {
        var result = a * b;
        Console.WriteLine($"{a} * {b} = {result.ToString($"F{precision}")}");
    }

    /// <summary>
    /// Divide the first number by the second
    /// </summary>
    [Action("divide", Description = "Divide first number by second")]
    public void Divide(
        [Argument(Description = "Dividend")] double a,
        [Argument(Description = "Divisor")] double b,
        [Option("precision", ShortName = 'p', Description = "Decimal places to display")] int precision = 4)
    {
        if (b == 0)
        {
            Console.WriteLine("Error: Cannot divide by zero!");
            return;
        }
        var result = a / b;
        Console.WriteLine($"{a} / {b} = {result.ToString($"F{precision}")}");
    }
}
