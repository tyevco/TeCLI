using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeCLI.Output;
using TeCLI.Output.Formatters;
using Xunit;

namespace TeCLI.Extensions.Output.Tests;

public class YamlOutputFormatterTests
{
    private readonly YamlOutputFormatter _formatter = new();

    [Fact]
    public void Format_NullValue_OutputsNull()
    {
        using var writer = new StringWriter();
        _formatter.Format(null, writer);

        Assert.Equal("null" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void Format_SimpleObject_OutputsValidYaml()
    {
        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            IsActive = true,
            Role = UserRole.Admin
        };

        using var writer = new StringWriter();
        _formatter.Format(user, writer);

        var yaml = writer.ToString();
        Assert.Contains("id: 1", yaml);
        Assert.Contains("name: John Doe", yaml);
        Assert.Contains("email: john@example.com", yaml);
        Assert.Contains("isActive: true", yaml);
        Assert.Contains("role: Admin", yaml);
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
    public void FormatCollection_MultipleItems_OutputsValidYamlArray()
    {
        var users = new List<User>
        {
            new User { Id = 1, Name = "Alice", Email = "alice@example.com" },
            new User { Id = 2, Name = "Bob", Email = "bob@example.com" }
        };

        using var writer = new StringWriter();
        _formatter.FormatCollection(users.Cast<object>(), writer);

        var yaml = writer.ToString();
        Assert.Contains("- id: 1", yaml);
        Assert.Contains("name: Alice", yaml);
        Assert.Contains("- id: 2", yaml);
        Assert.Contains("name: Bob", yaml);
    }

    [Fact]
    public void Formatter_HasCorrectProperties()
    {
        Assert.Equal(OutputFormat.Yaml, _formatter.Format);
        Assert.Equal(".yaml", _formatter.FileExtension);
        Assert.Equal("application/x-yaml", _formatter.MimeType);
    }

    [Fact]
    public void Format_StringWithSpecialChars_QuotesString()
    {
        var record = new SimpleRecord("Hello: World", 42);

        using var writer = new StringWriter();
        _formatter.Format(record, writer);

        var yaml = writer.ToString();
        Assert.Contains("name: \"Hello: World\"", yaml);
    }

    [Fact]
    public void Format_BooleanValue_OutputsLowercase()
    {
        var user = new User { IsActive = true };

        using var writer = new StringWriter();
        _formatter.Format(user, writer);

        var yaml = writer.ToString();
        Assert.Contains("isActive: true", yaml);
    }

    [Fact]
    public void Format_NestedObject_OutputsNestedYaml()
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

        var yaml = writer.ToString();
        Assert.Contains("orderId: 100", yaml);
        Assert.Contains("customer:", yaml);
        Assert.Contains("name: Customer", yaml);
        Assert.Contains("items:", yaml);
        Assert.Contains("productName: Widget", yaml);
    }

    [Fact]
    public void Format_CamelCasesPropertyNames()
    {
        var user = new User { Id = 1, IsActive = true };

        using var writer = new StringWriter();
        _formatter.Format(user, writer);

        var yaml = writer.ToString();
        Assert.Contains("id:", yaml);
        Assert.Contains("isActive:", yaml);
        Assert.DoesNotContain("Id:", yaml);
        Assert.DoesNotContain("IsActive:", yaml);
    }

    [Fact]
    public void GenericFormatter_FormatsTypedValue()
    {
        var formatter = new YamlOutputFormatter<User>();
        var user = new User { Id = 1, Name = "Test" };

        using var writer = new StringWriter();
        formatter.Format(user, writer);

        var yaml = writer.ToString();
        Assert.Contains("id: 1", yaml);
        Assert.Contains("name: Test", yaml);
    }
}
