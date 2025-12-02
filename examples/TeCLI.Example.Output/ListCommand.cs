using TeCLI.Attributes;
using TeCLI.Output;

namespace TeCLI.Example.Output;

/// <summary>
/// Commands for listing data with structured output formatting.
/// </summary>
[Command("list", Description = "List various data with formatted output")]
public class ListCommand
{
    /// <summary>
    /// Sample user data.
    /// </summary>
    private static readonly List<User> Users = new()
    {
        new User { Id = 1, Name = "Alice Johnson", Email = "alice@example.com", Department = "Engineering", IsActive = true, CreatedAt = DateTime.Now.AddDays(-365), Role = UserRole.Admin },
        new User { Id = 2, Name = "Bob Smith", Email = "bob@example.com", Department = "Marketing", IsActive = true, CreatedAt = DateTime.Now.AddDays(-180), Role = UserRole.User },
        new User { Id = 3, Name = "Carol White", Email = "carol@example.com", Department = "Engineering", IsActive = false, CreatedAt = DateTime.Now.AddDays(-90), Role = UserRole.User },
        new User { Id = 4, Name = "David Brown", Email = "david@example.com", Department = "Sales", IsActive = true, CreatedAt = DateTime.Now.AddDays(-30), Role = UserRole.Guest },
        new User { Id = 5, Name = "Eva Martinez", Email = "eva@example.com", Department = "Engineering", IsActive = true, CreatedAt = DateTime.Now.AddDays(-7), Role = UserRole.SuperAdmin }
    };

    /// <summary>
    /// Sample product data.
    /// </summary>
    private static readonly List<Product> Products = new()
    {
        new Product { Sku = "WDG-001", Name = "Widget Pro", Category = "Electronics", Price = 29.99m, Stock = 150 },
        new Product { Sku = "WDG-002", Name = "Widget Basic", Category = "Electronics", Price = 19.99m, Stock = 300 },
        new Product { Sku = "GAD-001", Name = "Gadget X", Category = "Electronics", Price = 99.99m, Stock = 0 },
        new Product { Sku = "TOL-001", Name = "Tool Set", Category = "Hardware", Price = 49.99m, Stock = 75 },
        new Product { Sku = "TOL-002", Name = "Power Drill", Category = "Hardware", Price = 149.99m, Stock = 25 }
    };

    /// <summary>
    /// Sample server status data.
    /// </summary>
    private static readonly List<ServerStatus> Servers = new()
    {
        new ServerStatus { Name = "web-01", Ip = "192.168.1.10", Status = "Running", CpuUsage = 45.2, MemoryUsage = 62.5, LastChecked = DateTime.Now },
        new ServerStatus { Name = "web-02", Ip = "192.168.1.11", Status = "Running", CpuUsage = 38.7, MemoryUsage = 55.3, LastChecked = DateTime.Now },
        new ServerStatus { Name = "db-01", Ip = "192.168.1.20", Status = "Running", CpuUsage = 72.1, MemoryUsage = 85.9, LastChecked = DateTime.Now },
        new ServerStatus { Name = "cache-01", Ip = "192.168.1.30", Status = "Warning", CpuUsage = 91.3, MemoryUsage = 78.2, LastChecked = DateTime.Now },
        new ServerStatus { Name = "backup-01", Ip = "192.168.1.40", Status = "Stopped", CpuUsage = 0, MemoryUsage = 0, LastChecked = DateTime.Now.AddHours(-2) }
    };

    /// <summary>
    /// Lists all users with optional department filter.
    /// Demonstrates the [OutputFormat] attribute for structured output.
    /// </summary>
    /// <example>
    /// myapp list users --output json
    /// myapp list users --output table
    /// myapp list users -o yaml --department Engineering
    /// </example>
    [Action("users", Description = "List all users")]
    [OutputFormat]
    public IEnumerable<User> ListUsers(
        [Option("department", ShortName = 'd', Description = "Filter by department")] string? department = null,
        [Option("active", ShortName = 'a', Description = "Show only active users")] bool activeOnly = false,
        [Option("output", ShortName = 'o', Description = "Output format (json, xml, yaml, table)")] string format = "table")
    {
        var result = Users.AsEnumerable();

        if (!string.IsNullOrEmpty(department))
        {
            result = result.Where(u => u.Department.Equals(department, StringComparison.OrdinalIgnoreCase));
        }

        if (activeOnly)
        {
            result = result.Where(u => u.IsActive);
        }

        // Format output using the output context
        var context = new OutputContext();
        context.WithFormat(format);
        context.Write(result.ToList());

        return result;
    }

    /// <summary>
    /// Lists all products with optional category filter.
    /// </summary>
    [Action("products", Description = "List all products")]
    [OutputFormat(OutputFormat.Table)]
    public IEnumerable<Product> ListProducts(
        [Option("category", ShortName = 'c', Description = "Filter by category")] string? category = null,
        [Option("in-stock", Description = "Show only in-stock products")] bool inStockOnly = false,
        [Option("output", ShortName = 'o', Description = "Output format (json, xml, yaml, table)")] string format = "table")
    {
        var result = Products.AsEnumerable();

        if (!string.IsNullOrEmpty(category))
        {
            result = result.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if (inStockOnly)
        {
            result = result.Where(p => p.InStock);
        }

        var context = new OutputContext();
        context.WithFormat(format);
        context.Write(result.ToList());

        return result;
    }

    /// <summary>
    /// Lists server statuses.
    /// </summary>
    [Action("servers", Description = "List server statuses")]
    [OutputFormat(OutputFormat.Table)]
    public IEnumerable<ServerStatus> ListServers(
        [Option("status", ShortName = 's', Description = "Filter by status")] string? status = null,
        [Option("output", ShortName = 'o', Description = "Output format (json, xml, yaml, table)")] string format = "table")
    {
        var result = Servers.AsEnumerable();

        if (!string.IsNullOrEmpty(status))
        {
            result = result.Where(s => s.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        var context = new OutputContext();
        context.WithFormat(format);
        context.Write(result.ToList());

        return result;
    }

    /// <summary>
    /// Gets a single user by ID.
    /// </summary>
    [Action("user", Description = "Get a single user by ID")]
    [OutputFormat]
    public User? GetUser(
        [Argument(Description = "User ID")] int id,
        [Option("output", ShortName = 'o', Description = "Output format (json, xml, yaml, table)")] string format = "table")
    {
        var user = Users.FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            Console.Error.WriteLine($"User with ID {id} not found.");
            return null;
        }

        var context = new OutputContext();
        context.WithFormat(format);
        context.Write(user);

        return user;
    }
}
