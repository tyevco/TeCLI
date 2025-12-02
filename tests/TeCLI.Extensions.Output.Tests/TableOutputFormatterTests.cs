using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spectre.Console;
using TeCLI.Output;
using TeCLI.Output.Formatters;
using Xunit;

namespace TeCLI.Extensions.Output.Tests;

public class TableOutputFormatterTests
{
    private readonly TableOutputFormatter _formatter = new();

    [Fact]
    public void Format_NullValue_OutputsNullIndicator()
    {
        using var writer = new StringWriter();
        _formatter.Format(null, writer);

        Assert.Contains("(null)", writer.ToString());
    }

    [Fact]
    public void Format_SimpleObject_OutputsPropertyValuePairs()
    {
        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            IsActive = true
        };

        using var writer = new StringWriter();
        _formatter.Format(user, writer);

        var output = writer.ToString();
        Assert.Contains("Property", output);
        Assert.Contains("Value", output);
        Assert.Contains("Id", output);
        Assert.Contains("1", output);
        Assert.Contains("Name", output);
        Assert.Contains("John Doe", output);
    }

    [Fact]
    public void FormatCollection_EmptyCollection_OutputsNoItemsMessage()
    {
        var users = new List<User>();

        using var writer = new StringWriter();
        _formatter.FormatCollection(users.Cast<object>(), writer);

        Assert.Contains("(no items)", writer.ToString());
    }

    [Fact]
    public void FormatCollection_MultipleItems_OutputsTable()
    {
        var users = new List<User>
        {
            new User { Id = 1, Name = "Alice", Email = "alice@example.com" },
            new User { Id = 2, Name = "Bob", Email = "bob@example.com" }
        };

        using var writer = new StringWriter();
        _formatter.FormatCollection(users.Cast<object>(), writer);

        var output = writer.ToString();
        Assert.Contains("Id", output);
        Assert.Contains("Name", output);
        Assert.Contains("Email", output);
        Assert.Contains("Alice", output);
        Assert.Contains("Bob", output);
    }

    [Fact]
    public void Formatter_HasCorrectProperties()
    {
        Assert.Equal(OutputFormat.Table, _formatter.Format);
        Assert.Equal(".txt", _formatter.FileExtension);
        Assert.Equal("text/plain", _formatter.MimeType);
    }

    [Fact]
    public void Format_WithTitle_IncludesTitle()
    {
        _formatter.Title = "User Details";
        var user = new User { Id = 1, Name = "Test" };

        using var writer = new StringWriter();
        _formatter.Format(user, writer);

        var output = writer.ToString();
        Assert.Contains("User Details", output);
    }

    [Fact]
    public void Format_BooleanValue_DisplaysYesNo()
    {
        var user = new User { Id = 1, IsActive = true };

        using var writer = new StringWriter();
        _formatter.Format(user, writer);

        var output = writer.ToString();
        Assert.Contains("Yes", output);
    }

    [Fact]
    public void FormatCollection_WithAllNulls_OutputsNullIndicators()
    {
        var items = new List<User?> { null, null };

        using var writer = new StringWriter();
        _formatter.FormatCollection(items.Cast<object>()!, writer);

        var output = writer.ToString();
        Assert.Contains("(null)", output);
    }

    [Fact]
    public void Format_DateTime_FormatsNicely()
    {
        var user = new User
        {
            Id = 1,
            CreatedAt = new DateTime(2023, 6, 15, 10, 30, 0)
        };

        using var writer = new StringWriter();
        _formatter.Format(user, writer);

        var output = writer.ToString();
        Assert.Contains("2023-06-15", output);
    }

    [Fact]
    public void GenericFormatter_FormatsTypedValue()
    {
        var formatter = new TableOutputFormatter<User>();
        var user = new User { Id = 1, Name = "Test" };

        using var writer = new StringWriter();
        formatter.Format(user, writer);

        var output = writer.ToString();
        Assert.Contains("Id", output);
        Assert.Contains("Name", output);
    }

    [Fact]
    public void FormatCollection_SimpleTypeCollection_OutputsSimpleTable()
    {
        var numbers = new List<int> { 1, 2, 3 };

        using var writer = new StringWriter();
        _formatter.FormatCollection(numbers.Cast<object>(), writer);

        var output = writer.ToString();
        Assert.Contains("Value", output);
        Assert.Contains("1", output);
        Assert.Contains("2", output);
        Assert.Contains("3", output);
    }
}
