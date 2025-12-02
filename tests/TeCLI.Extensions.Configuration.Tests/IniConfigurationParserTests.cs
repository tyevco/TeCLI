using TeCLI.Configuration.Parsers;
using Xunit;

namespace TeCLI.Extensions.Configuration.Tests;

public class IniConfigurationParserTests
{
    private readonly IniConfigurationParser _parser = new();

    [Fact]
    public void CanParse_WithIniExtensions_ReturnsTrue()
    {
        Assert.True(_parser.CanParse(".ini"));
        Assert.True(_parser.CanParse(".cfg"));
        Assert.True(_parser.CanParse(".conf"));
        Assert.True(_parser.CanParse(".INI"));
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
        var ini = @"
name = test
count = 42
enabled = true
";

        var result = _parser.Parse(ini);

        Assert.Equal("test", result["name"]);
        Assert.Equal(42, result["count"]);
        Assert.Equal(true, result["enabled"]);
    }

    [Fact]
    public void Parse_ColonSeparator_ReturnsCorrectValues()
    {
        var ini = @"
name: test
count: 42
";

        var result = _parser.Parse(ini);

        Assert.Equal("test", result["name"]);
        Assert.Equal(42, result["count"]);
    }

    [Fact]
    public void Parse_Section_ReturnsNestedDictionary()
    {
        var ini = @"
[deploy]
environment = production
region = us-west
";

        var result = _parser.Parse(ini);

        Assert.True(result["deploy"] is IDictionary<string, object?>);
        var deploy = (IDictionary<string, object?>)result["deploy"]!;
        Assert.Equal("production", deploy["environment"]);
        Assert.Equal("us-west", deploy["region"]);
    }

    [Fact]
    public void Parse_DottedSection_ReturnsNestedDictionary()
    {
        var ini = @"
[deploy.settings]
timeout = 30
retries = 3
";

        var result = _parser.Parse(ini);

        Assert.True(result["deploy"] is IDictionary<string, object?>);
        var deploy = (IDictionary<string, object?>)result["deploy"]!;
        Assert.True(deploy["settings"] is IDictionary<string, object?>);
        var settings = (IDictionary<string, object?>)deploy["settings"]!;
        Assert.Equal(30, settings["timeout"]);
        Assert.Equal(3, settings["retries"]);
    }

    [Fact]
    public void Parse_SemicolonComment_IsIgnored()
    {
        var ini = @"
; This is a comment
name = test ; inline comment
";

        var result = _parser.Parse(ini);

        Assert.Equal("test", result["name"]);
    }

    [Fact]
    public void Parse_HashComment_IsIgnored()
    {
        var ini = @"
# This is a comment
name = test # inline comment
";

        var result = _parser.Parse(ini);

        Assert.Equal("test", result["name"]);
    }

    [Fact]
    public void Parse_QuotedValue_UnquotesValue()
    {
        var ini = @"
single = 'hello world'
double = ""hello world""
";

        var result = _parser.Parse(ini);

        Assert.Equal("hello world", result["single"]);
        Assert.Equal("hello world", result["double"]);
    }

    [Fact]
    public void Parse_BooleanVariants_ParsesCorrectly()
    {
        var ini = @"
yes_value = yes
no_value = no
on_value = on
off_value = off
one_value = 1
zero_value = 0
";

        var result = _parser.Parse(ini);

        Assert.Equal(true, result["yes_value"]);
        Assert.Equal(false, result["no_value"]);
        Assert.Equal(true, result["on_value"]);
        Assert.Equal(false, result["off_value"]);
        Assert.Equal(true, result["one_value"]);
        Assert.Equal(false, result["zero_value"]);
    }

    [Fact]
    public void Parse_CommaSeparatedList_ReturnsListOfValues()
    {
        var ini = @"items = a, b, c";

        var result = _parser.Parse(ini);

        Assert.True(result["items"] is List<object?>);
        var items = (List<object?>)result["items"]!;
        Assert.Equal(3, items.Count);
        Assert.Equal("a", items[0]);
        Assert.Equal("b", items[1]);
        Assert.Equal("c", items[2]);
    }
}
