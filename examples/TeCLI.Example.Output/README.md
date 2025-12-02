# TeCLI.Example.Output

This example demonstrates the structured output formatting capabilities of TeCLI using the `TeCLI.Extensions.Output` package.

## Features Demonstrated

- **Multiple output formats**: JSON, XML, YAML, and Table
- **`[OutputFormat]` attribute**: Enable `--output` option on actions
- **`OutputContext`**: Fluent API for formatting data
- **Custom formatters**: Configure table borders, titles, and more
- **Format parsing**: Parse format strings dynamically

## Running the Example

```bash
# List users in different formats
dotnet run -- list users --output json
dotnet run -- list users --output xml
dotnet run -- list users --output yaml
dotnet run -- list users --output table

# Filter users
dotnet run -- list users --department Engineering --output json
dotnet run -- list users --active --output table

# List products
dotnet run -- list products --output json
dotnet run -- list products --category Electronics --in-stock

# List servers
dotnet run -- list servers --output yaml
dotnet run -- list servers --status Running

# Get a single user
dotnet run -- list user 1 --output json

# Format demonstration
dotnet run -- format demo --format json
dotnet run -- format demo --format yaml

# Compare all formats
dotnet run -- format compare

# Custom formatter configuration
dotnet run -- format custom

# Format to string
dotnet run -- format tostring --format json

# Parse format strings
dotnet run -- format parse
```

## Example Output

### JSON Format
```json
[
  {
    "id": 1,
    "name": "Alice Johnson",
    "email": "alice@example.com",
    "department": "Engineering",
    "isActive": true,
    "role": "Admin"
  }
]
```

### YAML Format
```yaml
- id: 1
  name: Alice Johnson
  email: alice@example.com
  department: Engineering
  isActive: true
  role: Admin
```

### Table Format
```
╭────┬───────────────┬─────────────────────┬─────────────┬──────────╮
│ Id │ Name          │ Email               │ Department  │ IsActive │
├────┼───────────────┼─────────────────────┼─────────────┼──────────┤
│ 1  │ Alice Johnson │ alice@example.com   │ Engineering │ Yes      │
│ 2  │ Bob Smith     │ bob@example.com     │ Marketing   │ Yes      │
╰────┴───────────────┴─────────────────────┴─────────────┴──────────╯
```

## Key Components

### OutputFormatAttribute

Apply to methods to enable the `--output` option:

```csharp
[Action("users")]
[OutputFormat]  // Enables --output json|xml|table|yaml
public IEnumerable<User> ListUsers()
{
    return _userService.GetAll();
}
```

### OutputContext

Use the fluent API for programmatic output:

```csharp
OutputContext.Create()
    .WithFormat("json")
    .WriteTo(Console.Out)
    .Write(users);
```

### Custom Formatters

Create custom formatter configurations:

```csharp
var tableFormatter = new TableOutputFormatter
{
    Title = "My Custom Table",
    Border = TableBorder.Double,
    Expand = true
};

var registry = new OutputFormatterRegistry();
registry.Register(tableFormatter);

var context = new OutputContext(OutputFormat.Table, Console.Out, registry);
context.Write(data);
```

## See Also

- [TeCLI.Extensions.Output Documentation](../../extensions/TeCLI.Extensions.Output/README.md)
- [Main TeCLI Documentation](../../README.md)
