using TeCLI.Configuration.Parsers;
using Xunit;

namespace TeCLI.Extensions.Configuration.Tests;

public class JsonConfigurationParserTests
{
    private readonly JsonConfigurationParser _parser = new();

    [Fact]
    public void CanParse_WithJsonExtension_ReturnsTrue()
    {
        Assert.True(_parser.CanParse(".json"));
        Assert.True(_parser.CanParse(".JSON"));
    }

    [Fact]
    public void CanParse_WithOtherExtension_ReturnsFalse()
    {
        Assert.False(_parser.CanParse(".yaml"));
        Assert.False(_parser.CanParse(".toml"));
    }

    [Fact]
    public void Parse_EmptyContent_ReturnsEmptyDictionary()
    {
        var result = _parser.Parse("");
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_SimpleObject_ReturnsCorrectValues()
    {
        var json = @"{
            ""name"": ""test"",
            ""count"": 42,
            ""enabled"": true
        }";

        var result = _parser.Parse(json);

        Assert.Equal("test", result["name"]);
        Assert.Equal(42, result["count"]);
        Assert.Equal(true, result["enabled"]);
    }

    [Fact]
    public void Parse_NestedObject_ReturnsNestedDictionary()
    {
        var json = @"{
            ""deploy"": {
                ""environment"": ""production"",
                ""region"": ""us-west""
            }
        }";

        var result = _parser.Parse(json);

        Assert.True(result["deploy"] is IDictionary<string, object?>);
        var deploy = (IDictionary<string, object?>)result["deploy"]!;
        Assert.Equal("production", deploy["environment"]);
        Assert.Equal("us-west", deploy["region"]);
    }

    [Fact]
    public void Parse_Array_ReturnsListOfObjects()
    {
        var json = @"{
            ""items"": [""a"", ""b"", ""c""]
        }";

        var result = _parser.Parse(json);

        Assert.True(result["items"] is List<object?>);
        var items = (List<object?>)result["items"]!;
        Assert.Equal(3, items.Count);
        Assert.Equal("a", items[0]);
    }

    [Fact]
    public void Parse_CommentsAllowed_IgnoresComments()
    {
        var json = @"{
            // This is a comment
            ""name"": ""test""
        }";

        var result = _parser.Parse(json);

        Assert.Equal("test", result["name"]);
    }

    [Fact]
    public void Parse_TrailingCommasAllowed_ParsesSuccessfully()
    {
        var json = @"{
            ""name"": ""test"",
            ""count"": 42,
        }";

        var result = _parser.Parse(json);

        Assert.Equal("test", result["name"]);
        Assert.Equal(42, result["count"]);
    }

    [Fact]
    public void Parse_NullValue_ReturnsNull()
    {
        var json = @"{ ""value"": null }";

        var result = _parser.Parse(json);

        Assert.Null(result["value"]);
    }

    [Fact]
    public void Parse_FloatingPointNumber_ReturnsDouble()
    {
        var json = @"{ ""value"": 3.14 }";

        var result = _parser.Parse(json);

        Assert.Equal(3.14, result["value"]);
    }
}
