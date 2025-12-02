# TeCLI.Extensions.Output

Structured output formatting extension for TeCLI that provides multiple output formats including JSON, XML, YAML, and table output with Spectre.Console integration.

## Installation

```bash
dotnet add package TeCLI.Extensions.Output
```

## Features

- **Multiple Output Formats**: JSON, XML, YAML, and rich table output
- **`[OutputFormat]` Attribute**: Enable `--output` option on action methods
- **Custom `IOutputFormatter<T>`**: Create custom formatters for specific types
- **Table Rendering**: Rich tables with Spectre.Console (colors, borders, alignment)
- **Fluent API**: Easy-to-use `OutputContext` for formatting data
- **Format Registry**: Centralized formatter management

## Quick Start

### Using the OutputFormat Attribute

Apply `[OutputFormat]` to action methods to automatically enable output format selection:

```csharp
using TeCLI.Attributes;
using TeCLI.Output;

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
```

Usage:
```bash
myapp list users --output json
myapp list users --output xml
myapp list users --output yaml
myapp list users --output table
myapp list users -o json  # Short form
```

### Using OutputContext Directly

For more control, use `OutputContext` directly:

```csharp
using TeCLI.Output;

// Fluent API
OutputContext.Create()
    .WithFormat(OutputFormat.Json)
    .WriteTo(Console.Out)
    .Write(users);

// Or construct directly
var context = new OutputContext(OutputFormat.Table, Console.Out);
context.Write(users);

// Format to string
var json = context.FormatToString(users);
```

## Output Format Examples

### JSON Output

```csharp
context.WithFormat(OutputFormat.Json).Write(user);
```

Output:
```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "isActive": true,
  "role": "Admin"
}
```

### XML Output

```csharp
context.WithFormat(OutputFormat.Xml).Write(user);
```

Output:
```xml
<?xml version="1.0" encoding="utf-8"?>
<User>
  <Id>1</Id>
  <Name>John Doe</Name>
  <Email>john@example.com</Email>
  <IsActive>true</IsActive>
  <Role>Admin</Role>
</User>
```

### YAML Output

```csharp
context.WithFormat(OutputFormat.Yaml).Write(user);
```

Output:
```yaml
id: 1
name: John Doe
email: john@example.com
isActive: true
role: Admin
```

### Table Output

```csharp
context.WithFormat(OutputFormat.Table).Write(users);
```

Output:
```
╭────┬──────────┬─────────────────────┬──────────╮
│ Id │ Name     │ Email               │ IsActive │
├────┼──────────┼─────────────────────┼──────────┤
│ 1  │ John Doe │ john@example.com    │ Yes      │
│ 2  │ Jane Doe │ jane@example.com    │ Yes      │
╰────┴──────────┴─────────────────────┴──────────╯
```

## Custom Formatters

Implement `IOutputFormatter<T>` for custom formatting:

```csharp
public class UserCsvFormatter : IOutputFormatter<User>
{
    public OutputFormat Format => OutputFormat.Csv; // Custom format
    public string FileExtension => ".csv";
    public string MimeType => "text/csv";

    public void Format(User value, TextWriter output)
    {
        output.WriteLine($"{value.Id},{value.Name},{value.Email}");
    }

    public void FormatCollection(IEnumerable<User> values, TextWriter output)
    {
        output.WriteLine("Id,Name,Email");
        foreach (var user in values)
        {
            Format(user, output);
        }
    }

    // IOutputFormatter interface implementation
    void IOutputFormatter.Format(object? value, TextWriter output)
    {
        if (value is User user) Format(user, output);
    }

    void IOutputFormatter.FormatCollection(IEnumerable<object> values, TextWriter output)
    {
        FormatCollection(values.Cast<User>(), output);
    }
}

// Register custom formatter
var registry = new OutputFormatterRegistry();
registry.Register(new UserCsvFormatter());
```

## Formatter Configuration

### JSON Formatter Options

```csharp
var formatter = new JsonOutputFormatter
{
    Indent = true  // Pretty print JSON
};
```

### XML Formatter Options

```csharp
var formatter = new XmlOutputFormatter
{
    Indent = true,
    RootElementName = "Users",
    ItemElementName = "User"
};
```

### YAML Formatter Options

```csharp
var formatter = new YamlOutputFormatter
{
    IndentSize = 2  // Spaces per indent level
};
```

### Table Formatter Options

```csharp
using Spectre.Console;

var formatter = new TableOutputFormatter
{
    Border = TableBorder.Rounded,  // Rounded, Square, Double, etc.
    ShowHeaders = true,
    HeaderStyle = new Style(Color.Blue, decoration: Decoration.Bold),
    Expand = false,
    Title = "User List",
    ColumnConfigs = new Dictionary<string, TableColumnConfig>
    {
        ["Id"] = new TableColumnConfig { Alignment = ColumnAlignment.Right },
        ["Price"] = new TableColumnConfig { Alignment = ColumnAlignment.Right }
    }
};
```

## OutputFormatAttribute Options

```csharp
[OutputFormat(
    DefaultFormat = OutputFormat.Table,  // Default when --output not specified
    Indent = true,                       // Enable indented output
    OptionName = "output",               // Long option name
    ShortName = 'o',                     // Short option name
    Description = "Output format",       // Help text
    AvailableFormats = new[] {           // Restrict available formats
        OutputFormat.Json,
        OutputFormat.Table
    }
)]
public IEnumerable<User> ListUsers() { }
```

## API Reference

### Key Classes

| Class | Description |
|-------|-------------|
| `OutputFormat` | Enum of supported formats (Json, Xml, Yaml, Table) |
| `OutputFormatAttribute` | Attribute to enable --output option |
| `IOutputFormatter` | Interface for output formatters |
| `IOutputFormatter<T>` | Generic interface for typed formatters |
| `OutputFormatterRegistry` | Registry for managing formatters |
| `OutputContext` | Context for formatting and outputting data |

### Built-in Formatters

| Formatter | Description |
|-----------|-------------|
| `JsonOutputFormatter` | JSON output using System.Text.Json |
| `XmlOutputFormatter` | XML output with configurable element names |
| `YamlOutputFormatter` | YAML output (lightweight, no external deps) |
| `TableOutputFormatter` | Rich table output using Spectre.Console |

## Integration with Spectre.Console

The table formatter uses Spectre.Console for rich terminal output, providing:

- Multiple border styles (Rounded, Square, Double, Heavy, etc.)
- Column alignment (Left, Right, Center)
- Header styling with colors and decorations
- Automatic width calculation
- Unicode support

## See Also

- [TeCLI Documentation](../../README.md)
- [Example Project](../../examples/TeCLI.Example.Output/)
- [Spectre.Console Documentation](https://spectreconsole.net/)
