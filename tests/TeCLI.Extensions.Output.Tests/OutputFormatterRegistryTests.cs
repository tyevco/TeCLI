using System;
using System.Collections.Generic;
using System.Linq;
using TeCLI.Output;
using TeCLI.Output.Formatters;
using Xunit;

namespace TeCLI.Extensions.Output.Tests;

public class OutputFormatterRegistryTests
{
    [Fact]
    public void CreateDefault_RegistersAllBuiltInFormatters()
    {
        var registry = OutputFormatterRegistry.CreateDefault();

        Assert.True(registry.HasFormatter(OutputFormat.Json));
        Assert.True(registry.HasFormatter(OutputFormat.Xml));
        Assert.True(registry.HasFormatter(OutputFormat.Yaml));
        Assert.True(registry.HasFormatter(OutputFormat.Table));
    }

    [Fact]
    public void Default_ReturnsSameInstance()
    {
        var registry1 = OutputFormatterRegistry.Default;
        var registry2 = OutputFormatterRegistry.Default;

        Assert.Same(registry1, registry2);
    }

    [Fact]
    public void Register_AddsFormatter()
    {
        var registry = new OutputFormatterRegistry();
        var formatter = new JsonOutputFormatter();

        registry.Register(formatter);

        Assert.True(registry.HasFormatter(OutputFormat.Json));
        Assert.Same(formatter, registry.GetFormatter(OutputFormat.Json));
    }

    [Fact]
    public void Register_WithNullFormatter_ThrowsArgumentNullException()
    {
        var registry = new OutputFormatterRegistry();

        Assert.Throws<ArgumentNullException>(() => registry.Register(null!));
    }

    [Fact]
    public void Register_OverwritesExistingFormatter()
    {
        var registry = new OutputFormatterRegistry();
        var formatter1 = new JsonOutputFormatter();
        var formatter2 = new JsonOutputFormatter();

        registry.Register(formatter1);
        registry.Register(formatter2);

        Assert.Same(formatter2, registry.GetFormatter(OutputFormat.Json));
    }

    [Fact]
    public void GetFormatter_WithUnregisteredFormat_ThrowsKeyNotFoundException()
    {
        var registry = new OutputFormatterRegistry();

        Assert.Throws<KeyNotFoundException>(() => registry.GetFormatter(OutputFormat.Json));
    }

    [Fact]
    public void TryGetFormatter_WithRegisteredFormat_ReturnsTrue()
    {
        var registry = OutputFormatterRegistry.CreateDefault();

        var result = registry.TryGetFormatter(OutputFormat.Json, out var formatter);

        Assert.True(result);
        Assert.NotNull(formatter);
        Assert.Equal(OutputFormat.Json, formatter!.Format);
    }

    [Fact]
    public void TryGetFormatter_WithUnregisteredFormat_ReturnsFalse()
    {
        var registry = new OutputFormatterRegistry();

        var result = registry.TryGetFormatter(OutputFormat.Json, out var formatter);

        Assert.False(result);
        Assert.Null(formatter);
    }

    [Fact]
    public void GetFormatters_ReturnsAllRegisteredFormatters()
    {
        var registry = OutputFormatterRegistry.CreateDefault();

        var formatters = registry.GetFormatters().ToList();

        Assert.Equal(4, formatters.Count);
        Assert.Contains(formatters, f => f.Format == OutputFormat.Json);
        Assert.Contains(formatters, f => f.Format == OutputFormat.Xml);
        Assert.Contains(formatters, f => f.Format == OutputFormat.Yaml);
        Assert.Contains(formatters, f => f.Format == OutputFormat.Table);
    }

    [Fact]
    public void Unregister_RemovesFormatter()
    {
        var registry = OutputFormatterRegistry.CreateDefault();

        var result = registry.Unregister(OutputFormat.Json);

        Assert.True(result);
        Assert.False(registry.HasFormatter(OutputFormat.Json));
    }

    [Fact]
    public void Unregister_WithUnregisteredFormat_ReturnsFalse()
    {
        var registry = new OutputFormatterRegistry();

        var result = registry.Unregister(OutputFormat.Json);

        Assert.False(result);
    }

    [Theory]
    [InlineData("json", OutputFormat.Json)]
    [InlineData("JSON", OutputFormat.Json)]
    [InlineData("xml", OutputFormat.Xml)]
    [InlineData("XML", OutputFormat.Xml)]
    [InlineData("yaml", OutputFormat.Yaml)]
    [InlineData("yml", OutputFormat.Yaml)]
    [InlineData("table", OutputFormat.Table)]
    [InlineData("tbl", OutputFormat.Table)]
    public void ParseFormat_WithValidString_ReturnsCorrectFormat(string input, OutputFormat expected)
    {
        var result = OutputFormatterRegistry.ParseFormat(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ParseFormat_WithNullOrEmpty_ThrowsArgumentException(string? input)
    {
        Assert.Throws<ArgumentException>(() => OutputFormatterRegistry.ParseFormat(input!));
    }

    [Fact]
    public void ParseFormat_WithUnknownFormat_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => OutputFormatterRegistry.ParseFormat("csv"));

        Assert.Contains("Unknown output format", ex.Message);
        Assert.Contains("csv", ex.Message);
    }

    [Theory]
    [InlineData("json", OutputFormat.Json, true)]
    [InlineData("unknown", OutputFormat.Json, false)]
    [InlineData("", OutputFormat.Json, false)]
    [InlineData(null, OutputFormat.Json, false)]
    public void TryParseFormat_ReturnsExpectedResult(string? input, OutputFormat expectedFormat, bool expectedResult)
    {
        var result = OutputFormatterRegistry.TryParseFormat(input, out var format);

        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.Equal(expectedFormat, format);
        }
    }
}
