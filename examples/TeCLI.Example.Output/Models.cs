namespace TeCLI.Example.Output;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserRole Role { get; set; }
}

/// <summary>
/// User roles.
/// </summary>
public enum UserRole
{
    Guest,
    User,
    Admin,
    SuperAdmin
}

/// <summary>
/// Represents a product in the catalog.
/// </summary>
public class Product
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool InStock => Stock > 0;
}

/// <summary>
/// Represents a server status.
/// </summary>
public class ServerStatus
{
    public string Name { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public DateTime LastChecked { get; set; }
}
