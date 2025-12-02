using System;
using System.Collections.Generic;

namespace TeCLI.Extensions.Output.Tests;

/// <summary>
/// Test user model for output formatting tests.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserRole Role { get; set; }
}

/// <summary>
/// Test user role enum.
/// </summary>
public enum UserRole
{
    Guest,
    User,
    Admin,
    SuperAdmin
}

/// <summary>
/// Test order model with nested objects.
/// </summary>
public class Order
{
    public int OrderId { get; set; }
    public User Customer { get; set; } = new();
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
}

/// <summary>
/// Test order item model.
/// </summary>
public class OrderItem
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

/// <summary>
/// Test order status enum.
/// </summary>
public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>
/// Simple test record.
/// </summary>
public record SimpleRecord(string Name, int Value);
