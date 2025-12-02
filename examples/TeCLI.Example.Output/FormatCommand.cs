using TeCLI.Attributes;
using TeCLI.Output;
using TeCLI.Output.Formatters;

namespace TeCLI.Example.Output;

/// <summary>
/// Commands demonstrating direct use of output formatters and contexts.
/// </summary>
[Command("format", Description = "Demonstrates output formatting APIs")]
public class FormatCommand
{
    /// <summary>
    /// Demonstrates the fluent OutputContext API.
    /// </summary>
    [Action("demo", Description = "Demonstrate various output formats")]
    public void Demo(
        [Option("format", ShortName = 'f', Description = "Output format")] string format = "json")
    {
        var data = new
        {
            Name = "Demo Object",
            Value = 42,
            Tags = new[] { "example", "demo", "test" },
            Metadata = new
            {
                CreatedAt = DateTime.Now,
                Version = "1.0.0"
            }
        };

        Console.WriteLine($"Formatting data as {format.ToUpper()}:");
        Console.WriteLine(new string('-', 40));

        OutputContext.Create()
            .WithFormat(format)
            .WriteTo(Console.Out)
            .Write(data);
    }

    /// <summary>
    /// Shows all available formats side by side.
    /// </summary>
    [Action("compare", Description = "Compare all output formats")]
    public void Compare()
    {
        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            Department = "Engineering",
            IsActive = true,
            CreatedAt = DateTime.Now,
            Role = UserRole.Admin
        };

        var formats = new[] { "json", "xml", "yaml", "table" };

        foreach (var format in formats)
        {
            Console.WriteLine();
            Console.WriteLine($"=== {format.ToUpper()} ===");
            Console.WriteLine();

            var context = new OutputContext();
            context.WithFormat(format);
            context.Write(user);

            Console.WriteLine();
        }
    }

    /// <summary>
    /// Demonstrates custom formatter configuration.
    /// </summary>
    [Action("custom", Description = "Demonstrate custom formatter configuration")]
    public void Custom()
    {
        var users = new[]
        {
            new User { Id = 1, Name = "Alice", Department = "Engineering", IsActive = true },
            new User { Id = 2, Name = "Bob", Department = "Marketing", IsActive = false }
        };

        // Create a custom table formatter with configuration
        var tableFormatter = new TableOutputFormatter
        {
            Title = "Custom Table Example",
            Border = Spectre.Console.TableBorder.Double,
            Expand = true
        };

        // Create a custom registry
        var registry = new OutputFormatterRegistry();
        registry.Register(tableFormatter);
        registry.Register(new JsonOutputFormatter { Indent = true });
        registry.Register(new YamlOutputFormatter { IndentSize = 4 });
        registry.Register(new XmlOutputFormatter { RootElementName = "Users", ItemElementName = "User" });

        Console.WriteLine("Custom Table (with double border and title):");
        Console.WriteLine();

        var context = new OutputContext(OutputFormat.Table, Console.Out, registry);
        context.Write(users);
    }

    /// <summary>
    /// Demonstrates formatting to a string instead of direct output.
    /// </summary>
    [Action("tostring", Description = "Format data to a string")]
    public void ToString(
        [Option("format", ShortName = 'f', Description = "Output format")] string format = "json")
    {
        var product = new Product
        {
            Sku = "DEMO-001",
            Name = "Demo Product",
            Category = "Examples",
            Price = 99.99m,
            Stock = 42
        };

        var context = new OutputContext();
        context.WithFormat(format);

        var formattedString = context.FormatToString(product);

        Console.WriteLine($"Formatted string (length: {formattedString.Length} characters):");
        Console.WriteLine();
        Console.WriteLine(formattedString);
    }

    /// <summary>
    /// Demonstrates parsing format strings.
    /// </summary>
    [Action("parse", Description = "Demonstrate format string parsing")]
    public void Parse()
    {
        var testInputs = new[] { "json", "JSON", "xml", "yaml", "yml", "table", "tbl", "csv" };

        Console.WriteLine("Format String Parsing:");
        Console.WriteLine();

        foreach (var input in testInputs)
        {
            if (OutputFormatterRegistry.TryParseFormat(input, out var format))
            {
                Console.WriteLine($"  '{input}' -> {format} (valid)");
            }
            else
            {
                Console.WriteLine($"  '{input}' -> invalid");
            }
        }
    }
}
