using TeCLI.Output;
using Xunit;

namespace TeCLI.Extensions.Output.Tests;

public class OutputFormatAttributeTests
{
    [Fact]
    public void Constructor_DefaultValues()
    {
        var attr = new OutputFormatAttribute();

        Assert.Equal(OutputFormat.Table, attr.DefaultFormat);
        Assert.True(attr.Indent);
        Assert.Equal("output", attr.OptionName);
        Assert.Equal('o', attr.ShortName);
        Assert.NotNull(attr.Description);
        Assert.Null(attr.AvailableFormats);
    }

    [Fact]
    public void Constructor_WithDefaultFormat()
    {
        var attr = new OutputFormatAttribute(OutputFormat.Json);

        Assert.Equal(OutputFormat.Json, attr.DefaultFormat);
    }

    [Fact]
    public void DefaultFormat_CanBeSet()
    {
        var attr = new OutputFormatAttribute();

        attr.DefaultFormat = OutputFormat.Yaml;

        Assert.Equal(OutputFormat.Yaml, attr.DefaultFormat);
    }

    [Fact]
    public void Indent_CanBeSet()
    {
        var attr = new OutputFormatAttribute();

        attr.Indent = false;

        Assert.False(attr.Indent);
    }

    [Fact]
    public void OptionName_CanBeSet()
    {
        var attr = new OutputFormatAttribute();

        attr.OptionName = "format";

        Assert.Equal("format", attr.OptionName);
    }

    [Fact]
    public void ShortName_CanBeSet()
    {
        var attr = new OutputFormatAttribute();

        attr.ShortName = 'f';

        Assert.Equal('f', attr.ShortName);
    }

    [Fact]
    public void Description_CanBeSet()
    {
        var attr = new OutputFormatAttribute();

        attr.Description = "Custom description";

        Assert.Equal("Custom description", attr.Description);
    }

    [Fact]
    public void AvailableFormats_CanBeSet()
    {
        var attr = new OutputFormatAttribute();
        var formats = new[] { OutputFormat.Json, OutputFormat.Xml };

        attr.AvailableFormats = formats;

        Assert.Equal(formats, attr.AvailableFormats);
    }

    [Fact]
    public void Attribute_CanBeAppliedToMethod()
    {
        var method = typeof(TestClass).GetMethod(nameof(TestClass.TestMethod));
        var attrs = method!.GetCustomAttributes(typeof(OutputFormatAttribute), false);

        Assert.Single(attrs);
        var attr = (OutputFormatAttribute)attrs[0];
        Assert.Equal(OutputFormat.Json, attr.DefaultFormat);
    }

    private class TestClass
    {
        [OutputFormat(OutputFormat.Json)]
        public void TestMethod() { }
    }
}
