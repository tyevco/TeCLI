using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeCLI.Output;
using TeCLI.Output.Formatters;
using Xunit;

namespace TeCLI.Extensions.Output.Tests;

public class XmlOutputFormatterTests
{
    private readonly XmlOutputFormatter _formatter = new();

    [Fact]
    public void Format_NullValue_OutputsNullElement()
    {
        using var writer = new StringWriter();
        _formatter.Format(null, writer);

        var xml = writer.ToString();
        Assert.Contains("<?xml", xml);
        Assert.Contains("<null />", xml);
    }

    [Fact]
    public void Format_SimpleObject_OutputsValidXml()
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

        var xml = writer.ToString();
        Assert.Contains("<?xml version=\"1.0\"", xml);
        Assert.Contains("<User>", xml);
        Assert.Contains("<Id>1</Id>", xml);
        Assert.Contains("<Name>John Doe</Name>", xml);
        Assert.Contains("<Email>john@example.com</Email>", xml);
        Assert.Contains("<IsActive>true</IsActive>", xml);
        Assert.Contains("<Role>Admin</Role>", xml);
        Assert.Contains("</User>", xml);
    }

    [Fact]
    public void FormatCollection_EmptyCollection_OutputsEmptyRoot()
    {
        var users = new List<User>();

        using var writer = new StringWriter();
        _formatter.FormatCollection(users.Cast<object>(), writer);

        var xml = writer.ToString();
        Assert.Contains("<Items>", xml);
        Assert.Contains("</Items>", xml);
    }

    [Fact]
    public void FormatCollection_MultipleItems_OutputsValidXmlArray()
    {
        var users = new List<User>
        {
            new User { Id = 1, Name = "Alice" },
            new User { Id = 2, Name = "Bob" }
        };

        using var writer = new StringWriter();
        _formatter.FormatCollection(users.Cast<object>(), writer);

        var xml = writer.ToString();
        Assert.Contains("<Items>", xml);
        Assert.Contains("<User>", xml);
        Assert.Contains("<Id>1</Id>", xml);
        Assert.Contains("<Name>Alice</Name>", xml);
        Assert.Contains("<Id>2</Id>", xml);
        Assert.Contains("<Name>Bob</Name>", xml);
        Assert.Contains("</Items>", xml);
    }

    [Fact]
    public void Formatter_HasCorrectProperties()
    {
        Assert.Equal(OutputFormat.Xml, _formatter.Format);
        Assert.Equal(".xml", _formatter.FileExtension);
        Assert.Equal("application/xml", _formatter.MimeType);
    }

    [Fact]
    public void Format_WithCustomRootElement_UsesCustomName()
    {
        _formatter.RootElementName = "Users";
        var users = new List<User>
        {
            new User { Id = 1, Name = "Test" }
        };

        using var writer = new StringWriter();
        _formatter.FormatCollection(users.Cast<object>(), writer);

        var xml = writer.ToString();
        Assert.Contains("<Users>", xml);
        Assert.Contains("</Users>", xml);
    }

    [Fact]
    public void Format_NestedObject_OutputsNestedXml()
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

        var xml = writer.ToString();
        Assert.Contains("<Order>", xml);
        Assert.Contains("<OrderId>100</OrderId>", xml);
        Assert.Contains("<Customer>", xml);
        Assert.Contains("<Name>Customer</Name>", xml);
        Assert.Contains("</Customer>", xml);
        Assert.Contains("<Items>", xml);
        Assert.Contains("<OrderItem>", xml);
        Assert.Contains("<ProductName>Widget</ProductName>", xml);
        Assert.Contains("</Order>", xml);
    }

    [Fact]
    public void Format_DateTime_UsesIso8601Format()
    {
        var user = new User
        {
            Id = 1,
            CreatedAt = new DateTime(2023, 6, 15, 10, 30, 0)
        };

        using var writer = new StringWriter();
        _formatter.Format(user, writer);

        var xml = writer.ToString();
        Assert.Contains("<CreatedAt>2023-06-15T10:30:00", xml);
    }

    [Fact]
    public void GenericFormatter_FormatsTypedValue()
    {
        var formatter = new XmlOutputFormatter<User>();
        var user = new User { Id = 1, Name = "Test" };

        using var writer = new StringWriter();
        formatter.Format(user, writer);

        var xml = writer.ToString();
        Assert.Contains("<User>", xml);
        Assert.Contains("<Id>1</Id>", xml);
    }
}
