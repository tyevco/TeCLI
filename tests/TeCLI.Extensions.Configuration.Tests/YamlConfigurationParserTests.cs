using TeCLI.Configuration.Parsers;
using Xunit;

namespace TeCLI.Extensions.Configuration.Tests;

public class YamlConfigurationParserTests
{
    private readonly YamlConfigurationParser _parser = new();

    [Fact]
    public void CanParse_WithYamlExtension_ReturnsTrue()
    {
        Assert.True(_parser.CanParse(".yaml"));
        Assert.True(_parser.CanParse(".yml"));
        Assert.True(_parser.CanParse(".YAML"));
    }

    [Fact]
    public void CanParse_WithOtherExtension_ReturnsFalse()
    {
        Assert.False(_parser.CanParse(".json"));
        Assert.False(_parser.CanParse(".toml"));
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
        var yaml = @"
name: test
count: 42
enabled: true
";

        var result = _parser.Parse(yaml);

        Assert.Equal("test", result["name"]);
        Assert.Equal(42, result["count"]);
        Assert.Equal(true, result["enabled"]);
    }

    [Fact]
    public void Parse_QuotedStrings_ReturnsUnquotedValue()
    {
        var yaml = @"
single: 'hello'
double: ""world""
";

        var result = _parser.Parse(yaml);

        Assert.Equal("hello", result["single"]);
        Assert.Equal("world", result["double"]);
    }

    [Fact]
    public void Parse_NestedObjects_ReturnsNestedDictionary()
    {
        var yaml = @"
deploy:
  environment: production
  region: us-west
  verbose: true
";

        var result = _parser.Parse(yaml);

        Assert.True(result["deploy"] is IDictionary<string, object?>);
        var deploy = (IDictionary<string, object?>)result["deploy"]!;
        Assert.Equal("production", deploy["environment"]);
        Assert.Equal("us-west", deploy["region"]);
        Assert.Equal(true, deploy["verbose"]);
    }

    [Fact]
    public void Parse_List_ReturnsListOfValues()
    {
        var yaml = @"
items:
  - first
  - second
  - third
";

        var result = _parser.Parse(yaml);

        Assert.True(result["items"] is List<object?>);
        var items = (List<object?>)result["items"]!;
        Assert.Equal(3, items.Count);
        Assert.Equal("first", items[0]);
        Assert.Equal("second", items[1]);
        Assert.Equal("third", items[2]);
    }

    [Fact]
    public void Parse_InlineList_ReturnsListOfValues()
    {
        var yaml = @"items: [a, b, c]";

        var result = _parser.Parse(yaml);

        Assert.True(result["items"] is List<object?>);
        var items = (List<object?>)result["items"]!;
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void Parse_Comments_AreIgnored()
    {
        var yaml = @"
# This is a comment
name: test # inline comment
count: 42
";

        var result = _parser.Parse(yaml);

        Assert.Equal("test", result["name"]);
        Assert.Equal(42, result["count"]);
    }

    [Fact]
    public void Parse_BooleanVariants_ParsesCorrectly()
    {
        var yaml = @"
yes_value: yes
no_value: no
on_value: on
off_value: off
true_value: true
false_value: false
";

        var result = _parser.Parse(yaml);

        Assert.Equal(true, result["yes_value"]);
        Assert.Equal(false, result["no_value"]);
        Assert.Equal(true, result["on_value"]);
        Assert.Equal(false, result["off_value"]);
        Assert.Equal(true, result["true_value"]);
        Assert.Equal(false, result["false_value"]);
    }

    [Fact]
    public void Parse_NullValue_ReturnsNull()
    {
        var yaml = @"
null_value: null
tilde_null: ~
";

        var result = _parser.Parse(yaml);

        Assert.Null(result["null_value"]);
        Assert.Null(result["tilde_null"]);
    }

    [Fact]
    public void Parse_Numbers_ParsesCorrectly()
    {
        var yaml = @"
integer: 42
negative: -10
float: 3.14
";

        var result = _parser.Parse(yaml);

        Assert.Equal(42, result["integer"]);
        Assert.Equal(-10, result["negative"]);
        Assert.Equal(3.14, result["float"]);
    }
}
