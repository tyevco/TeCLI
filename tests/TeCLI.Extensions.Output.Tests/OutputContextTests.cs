using System;
using System.Collections.Generic;
using System.IO;
using TeCLI.Output;
using Xunit;

namespace TeCLI.Extensions.Output.Tests;

public class OutputContextTests
{
    [Fact]
    public void Constructor_DefaultValues()
    {
        var context = new OutputContext();

        Assert.Equal(OutputFormat.Table, context.Format);
        Assert.NotNull(context.Output);
        Assert.NotNull(context.Registry);
    }

    [Fact]
    public void Constructor_WithFormatAndOutput()
    {
        using var writer = new StringWriter();
        var context = new OutputContext(OutputFormat.Json, writer);

        Assert.Equal(OutputFormat.Json, context.Format);
        Assert.Same(writer, context.Output);
    }

    [Fact]
    public void WithFormat_SetsFormat()
    {
        var context = new OutputContext();

        context.WithFormat(OutputFormat.Xml);

        Assert.Equal(OutputFormat.Xml, context.Format);
    }

    [Fact]
    public void WithFormat_String_SetsFormat()
    {
        var context = new OutputContext();

        context.WithFormat("yaml");

        Assert.Equal(OutputFormat.Yaml, context.Format);
    }

    [Fact]
    public void WriteTo_SetsOutput()
    {
        using var writer = new StringWriter();
        var context = new OutputContext();

        context.WriteTo(writer);

        Assert.Same(writer, context.Output);
    }

    [Fact]
    public void WriteTo_WithNull_ThrowsArgumentNullException()
    {
        var context = new OutputContext();

        Assert.Throws<ArgumentNullException>(() => context.WriteTo(null!));
    }

    [Fact]
    public void Write_SingleObject_FormatsCorrectly()
    {
        using var writer = new StringWriter();
        var context = new OutputContext(OutputFormat.Json, writer);
        var user = new User { Id = 1, Name = "Test" };

        context.Write(user);

        var output = writer.ToString();
        Assert.Contains("\"id\": 1", output);
        Assert.Contains("\"name\": \"Test\"", output);
    }

    [Fact]
    public void Write_Collection_FormatsCorrectly()
    {
        using var writer = new StringWriter();
        var context = new OutputContext(OutputFormat.Json, writer);
        var users = new List<User>
        {
            new User { Id = 1, Name = "Alice" },
            new User { Id = 2, Name = "Bob" }
        };

        context.Write(users);

        var output = writer.ToString();
        Assert.Contains("\"name\": \"Alice\"", output);
        Assert.Contains("\"name\": \"Bob\"", output);
    }

    [Fact]
    public void Write_Null_FormatsCorrectly()
    {
        using var writer = new StringWriter();
        var context = new OutputContext(OutputFormat.Json, writer);

        context.Write(null);

        Assert.Contains("null", writer.ToString());
    }

    [Fact]
    public void WriteCollection_FormatsCorrectly()
    {
        using var writer = new StringWriter();
        var context = new OutputContext(OutputFormat.Json, writer);
        var users = new List<User>
        {
            new User { Id = 1, Name = "Alice" }
        };

        context.WriteCollection(users);

        var output = writer.ToString();
        Assert.Contains("\"name\": \"Alice\"", output);
    }

    [Fact]
    public void FormatToString_ReturnsFormattedString()
    {
        var context = new OutputContext(OutputFormat.Json, Console.Out);
        var user = new User { Id = 1, Name = "Test" };

        var result = context.FormatToString(user);

        Assert.Contains("\"id\": 1", result);
        Assert.Contains("\"name\": \"Test\"", result);
    }

    [Fact]
    public void FormatCollectionToString_ReturnsFormattedString()
    {
        var context = new OutputContext(OutputFormat.Json, Console.Out);
        var users = new List<User>
        {
            new User { Id = 1, Name = "Test" }
        };

        var result = context.FormatCollectionToString(users);

        Assert.Contains("[", result);
        Assert.Contains("\"name\": \"Test\"", result);
    }

    [Fact]
    public void Create_ReturnsBuilder()
    {
        var builder = OutputContext.Create();

        Assert.NotNull(builder);
    }

    [Fact]
    public void Builder_FluentApi_Works()
    {
        using var writer = new StringWriter();
        var user = new User { Id = 1, Name = "Test" };

        OutputContext.Create()
            .WithFormat(OutputFormat.Json)
            .WriteTo(writer)
            .Write(user);

        var output = writer.ToString();
        Assert.Contains("\"id\": 1", output);
    }

    [Fact]
    public void Builder_WithFormatString_Works()
    {
        using var writer = new StringWriter();
        var user = new User { Id = 1, Name = "Test" };

        OutputContext.Create()
            .WithFormat("json")
            .WriteTo(writer)
            .Write(user);

        var output = writer.ToString();
        Assert.Contains("\"id\": 1", output);
    }

    [Fact]
    public void Builder_Build_CreatesContext()
    {
        using var writer = new StringWriter();

        var context = OutputContext.Create()
            .WithFormat(OutputFormat.Yaml)
            .WriteTo(writer)
            .Build();

        Assert.Equal(OutputFormat.Yaml, context.Format);
        Assert.Same(writer, context.Output);
    }

    [Fact]
    public void Builder_WriteCollection_Works()
    {
        using var writer = new StringWriter();
        var users = new List<User>
        {
            new User { Id = 1, Name = "Test" }
        };

        OutputContext.Create()
            .WithFormat(OutputFormat.Json)
            .WriteTo(writer)
            .WriteCollection(users);

        var output = writer.ToString();
        Assert.Contains("[", output);
    }

    [Fact]
    public void Write_String_TreatsAsObject()
    {
        using var writer = new StringWriter();
        var context = new OutputContext(OutputFormat.Json, writer);

        context.Write("Hello World");

        var output = writer.ToString();
        Assert.Contains("Hello World", output);
    }
}
