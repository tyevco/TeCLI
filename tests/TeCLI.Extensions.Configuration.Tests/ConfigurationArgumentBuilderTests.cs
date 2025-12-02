using TeCLI.Configuration;
using Xunit;

namespace TeCLI.Extensions.Configuration.Tests;

public class ConfigurationArgumentBuilderTests
{
    private readonly ConfigurationArgumentBuilder _builder = new();

    [Fact]
    public void BuildConfigArguments_EmptyConfig_ReturnsEmptyArray()
    {
        var config = new Dictionary<string, object?>();

        var result = _builder.BuildConfigArguments(config);

        Assert.Empty(result);
    }

    [Fact]
    public void BuildConfigArguments_StringValue_ReturnsOptionWithValue()
    {
        var config = new Dictionary<string, object?>
        {
            ["environment"] = "production"
        };

        var result = _builder.BuildConfigArguments(config);

        Assert.Contains("--environment", result);
        Assert.Contains("production", result);
    }

    [Fact]
    public void BuildConfigArguments_BoolTrue_ReturnsOptionWithoutValue()
    {
        var config = new Dictionary<string, object?>
        {
            ["verbose"] = true
        };

        var result = _builder.BuildConfigArguments(config);

        Assert.Contains("--verbose", result);
        Assert.Single(result); // No value, just flag
    }

    [Fact]
    public void BuildConfigArguments_BoolFalse_OmitsOption()
    {
        var config = new Dictionary<string, object?>
        {
            ["verbose"] = false
        };

        var result = _builder.BuildConfigArguments(config);

        Assert.Empty(result);
    }

    [Fact]
    public void BuildConfigArguments_IntValue_ReturnsOptionWithValue()
    {
        var config = new Dictionary<string, object?>
        {
            ["timeout"] = 30
        };

        var result = _builder.BuildConfigArguments(config);

        Assert.Contains("--timeout", result);
        Assert.Contains("30", result);
    }

    [Fact]
    public void BuildConfigArguments_ExcludesSpecifiedOptions()
    {
        var config = new Dictionary<string, object?>
        {
            ["environment"] = "production",
            ["region"] = "us-west"
        };

        var exclude = new HashSet<string> { "environment" };
        var result = _builder.BuildConfigArguments(config, exclude);

        Assert.DoesNotContain("--environment", result);
        Assert.Contains("--region", result);
    }

    [Fact]
    public void BuildConfigArguments_SkipsProfilesSection()
    {
        var config = new Dictionary<string, object?>
        {
            ["name"] = "test",
            ["profiles"] = new Dictionary<string, object?>
            {
                ["dev"] = new Dictionary<string, object?>()
            }
        };

        var result = _builder.BuildConfigArguments(config);

        Assert.Contains("--name", result);
        Assert.DoesNotContain("--profiles", result);
    }

    [Fact]
    public void BuildConfigArguments_CamelCaseToKebabCase()
    {
        var config = new Dictionary<string, object?>
        {
            ["outputPath"] = "/tmp/output"
        };

        var result = _builder.BuildConfigArguments(config);

        Assert.Contains("--output-path", result);
    }

    [Fact]
    public void MergeWithArguments_ConfigArgsFirst_ThenOriginal()
    {
        var config = new Dictionary<string, object?>
        {
            ["verbose"] = true,
            ["timeout"] = 30
        };

        var args = new[] { "deploy", "--environment", "prod" };

        var result = _builder.MergeWithArguments(args, config);

        // Original args should be at the end (higher precedence)
        var deployIndex = Array.IndexOf(result, "deploy");
        var verboseIndex = Array.IndexOf(result, "--verbose");

        Assert.True(verboseIndex < deployIndex, "Config args should come before original args");
    }

    [Fact]
    public void MergeWithArguments_SkipsAlreadyProvidedOptions()
    {
        var config = new Dictionary<string, object?>
        {
            ["environment"] = "development",
            ["region"] = "us-east"
        };

        var args = new[] { "--environment", "production" };

        var result = _builder.MergeWithArguments(args, config);

        // Should only have environment once (from args) plus region (from config)
        var envCount = result.Count(a => a == "--environment");
        Assert.Equal(1, envCount);
        Assert.Contains("--region", result);
    }
}
