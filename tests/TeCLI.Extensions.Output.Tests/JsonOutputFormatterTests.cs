using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TeCLI.Output;
using TeCLI.Output.Formatters;
using Xunit;

namespace TeCLI.Extensions.Output.Tests;

public class JsonOutputFormatterTests
{
    private readonly JsonOutputFormatter _formatter = new();

    [Fact]
    public void Format_NullValue_OutputsNull()
    {
        using var writer = new StringWriter();
        _formatter.Format(null, writer);

        Assert.Equal("null" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void Format_SimpleObject_OutputsValidJson()
    {
        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            IsActive = true,
            CreatedAt = new DateTime(2023, 1, 15, 10, 30, 0),
            Role = UserRole.Admin
        };

        using var writer = new StringWriter();
        _formatter.Format(user, writer);

        var json = writer.ToString();
        Assert.Contains("\"id\": 1", json);
        Assert.Contains("\"name\": \"John Doe\"", json);
        Assert.Contains("\"email\": \"john@example.com\"", json);
        Assert.Contains("\"isActive\": true", json);
        Assert.Contains("\"role\": \"Admin\"", json);
    }

    [Fact]
    public void Format_WithIndent_OutputsPrettyJson()
    {
        _formatter.Indent = true;
        var user = new User { Id = 1, Name = "Test" };

        using var writer = new StringWriter();
        _formatter.Format(user, writer);

        var json = writer.ToString();
        Assert.Contains("\n", json);  // Pretty print includes newlines
    }

    [Fact]
    public void FormatCollection_EmptyCollection_OutputsEmptyArray()
    {
        var users = new List<User>();

        using var writer = new StringWriter();
        _formatter.FormatCollection(users.Cast<object>(), writer);

        Assert.Equal("[]" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void FormatCollection_MultipleItems_OutputsValidJsonArray()
    {
        var users = new List<User>
        {
            new User { Id = 1, Name = "Alice", Email = "alice@example.com" },
            new User { Id = 2, Name = "Bob", Email = "bob@example.com" }
        };

        using var writer = new StringWriter();
        _formatter.FormatCollection(users.Cast<object>(), writer);

        var json = writer.ToString();
        Assert.Contains("\"id\": 1", json);
        Assert.Contains("\"id\": 2", json);
        Assert.Contains("\"name\": \"Alice\"", json);
        Assert.Contains("\"name\": \"Bob\"", json);
    }

    [Fact]
    public void Formatter_HasCorrectProperties()
    {
        Assert.Equal(OutputFormat.Json, _formatter.Format);
        Assert.Equal(".json", _formatter.FileExtension);
        Assert.Equal("application/json", _formatter.MimeType);
    }

    [Fact]
    public void Format_EnumValue_OutputsEnumString()
    {
        var user = new User { Role = UserRole.SuperAdmin };

        using var writer = new StringWriter();
        _formatter.Format(user, writer);

        var json = writer.ToString();
        Assert.Contains("\"SuperAdmin\"", json);
    }

    [Fact]
    public void Format_NestedObject_OutputsValidJson()
    {
        var order = new Order
        {
            OrderId = 100,
            Customer = new User { Id = 1, Name = "Customer" },
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "Widget", Quantity = 2, Price = 9.99m }
            },
            Total = 19.98m,
            Status = OrderStatus.Confirmed
        };

        using var writer = new StringWriter();
        _formatter.Format(order, writer);

        var json = writer.ToString();
        Assert.Contains("\"orderId\": 100", json);
        Assert.Contains("\"customer\":", json);
        Assert.Contains("\"items\":", json);
        Assert.Contains("\"productName\": \"Widget\"", json);
    }

    [Fact]
    public void GenericFormatter_FormatsTypedValue()
    {
        var formatter = new JsonOutputFormatter<User>();
        var user = new User { Id = 1, Name = "Test" };

        using var writer = new StringWriter();
        formatter.Format(user, writer);

        var json = writer.ToString();
        Assert.Contains("\"id\": 1", json);
        Assert.Contains("\"name\": \"Test\"", json);
    }

    [Fact]
    public void GenericFormatter_FormatsTypedCollection()
    {
        var formatter = new JsonOutputFormatter<User>();
        var users = new List<User>
        {
            new User { Id = 1, Name = "Alice" },
            new User { Id = 2, Name = "Bob" }
        };

        using var writer = new StringWriter();
        formatter.FormatCollection(users, writer);

        var json = writer.ToString();
        Assert.Contains("\"name\": \"Alice\"", json);
        Assert.Contains("\"name\": \"Bob\"", json);
    }
}
