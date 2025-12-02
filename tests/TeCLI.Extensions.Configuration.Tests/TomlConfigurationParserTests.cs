using TeCLI.Configuration.Parsers;
using Xunit;

namespace TeCLI.Extensions.Configuration.Tests;

public class TomlConfigurationParserTests
{
    private readonly TomlConfigurationParser _parser = new();

    [Fact]
    public void CanParse_WithTomlExtension_ReturnsTrue()
    {
        Assert.True(_parser.CanParse(".toml"));
        Assert.True(_parser.CanParse(".TOML"));
    }

    [Fact]
    public void CanParse_WithOtherExtension_ReturnsFalse()
    {
        Assert.False(_parser.CanParse(".json"));
        Assert.False(_parser.CanParse(".yaml"));
    }

    [Fact]
    public void Parse_EmptyContent_ReturnsEmptyDictionary()
    {
        var result = _parser.Parse("");
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_SimpleKeyValue_ReturnsCorrectValues()
    {
        var toml = @"
name = ""test""
count = 42
enabled = true
";

        var result = _parser.Parse(toml);

        Assert.Equal("test", result["name"]);
        Assert.Equal(42, result["count"]);
        Assert.Equal(true, result["enabled"]);
    }

    [Fact]
    public void Parse_Section_ReturnsNestedDictionary()
    {
        var toml = @"
[deploy]
environment = ""production""
region = ""us-west""
verbose = true
";

        var result = _parser.Parse(toml);

        Assert.True(result["deploy"] is IDictionary<string, object?>);
        var deploy = (IDictionary<string, object?>)result["deploy"]!;
        Assert.Equal("production", deploy["environment"]);
        Assert.Equal("us-west", deploy["region"]);
        Assert.Equal(true, deploy["verbose"]);
    }

    [Fact]
    public void Parse_DottedSection_ReturnsNestedDictionary()
    {
        var toml = @"
[deploy.settings]
timeout = 30
retries = 3
";

        var result = _parser.Parse(toml);

        Assert.True(result["deploy"] is IDictionary<string, object?>);
        var deploy = (IDictionary<string, object?>)result["deploy"]!;
        Assert.True(deploy["settings"] is IDictionary<string, object?>);
        var settings = (IDictionary<string, object?>)deploy["settings"]!;
        Assert.Equal(30, settings["timeout"]);
        Assert.Equal(3, settings["retries"]);
    }

    [Fact]
    public void Parse_Array_ReturnsListOfValues()
    {
        var toml = @"items = [""a"", ""b"", ""c""]";

        var result = _parser.Parse(toml);

        Assert.True(result["items"] is List<object?>);
        var items = (List<object?>)result["items"]!;
        Assert.Equal(3, items.Count);
        Assert.Equal("a", items[0]);
    }

    [Fact]
    public void Parse_InlineTable_ReturnsDictionary()
    {
        var toml = @"point = { x = 1, y = 2 }";

        var result = _parser.Parse(toml);

        Assert.True(result["point"] is IDictionary<string, object?>);
        var point = (IDictionary<string, object?>)result["point"]!;
        Assert.Equal(1, point["x"]);
        Assert.Equal(2, point["y"]);
    }

    [Fact]
    public void Parse_Comments_AreIgnored()
    {
        var toml = @"
# This is a comment
name = ""test"" # inline comment
count = 42
";

        var result = _parser.Parse(toml);

        Assert.Equal("test", result["name"]);
        Assert.Equal(42, result["count"]);
    }

    [Fact]
    public void Parse_LiteralString_PreservesContent()
    {
        var toml = @"path = 'C:\Users\test'";

        var result = _parser.Parse(toml);

        Assert.Equal(@"C:\Users\test", result["path"]);
    }

    [Fact]
    public void Parse_HexNumber_ParsesCorrectly()
    {
        var toml = @"color = 0xFF00FF";

        var result = _parser.Parse(toml);

        Assert.Equal(0xFF00FF, result["color"]);
    }

    [Fact]
    public void Parse_FloatingPoint_ParsesCorrectly()
    {
        var toml = @"
pi = 3.14159
negative = -0.5
";

        var result = _parser.Parse(toml);

        Assert.Equal(3.14159, result["pi"]);
        Assert.Equal(-0.5, result["negative"]);
    }

    [Fact]
    public void Parse_UnderscoreInNumber_ParsesCorrectly()
    {
        var toml = @"large = 1_000_000";

        var result = _parser.Parse(toml);

        Assert.Equal(1000000, result["large"]);
    }
}
