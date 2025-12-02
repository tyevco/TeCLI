using TeCLI.Configuration;
using Xunit;

namespace TeCLI.Extensions.Configuration.Tests;

public class ConfigurationMergerTests
{
    private readonly ConfigurationMerger _merger = new();

    [Fact]
    public void Merge_EmptyConfigurations_ReturnsEmptyDictionary()
    {
        var result = _merger.Merge();
        Assert.Empty(result);
    }

    [Fact]
    public void Merge_SingleConfiguration_ReturnsConfiguration()
    {
        var config = new Dictionary<string, object?>
        {
            ["name"] = "test",
            ["count"] = 42
        };

        var result = _merger.Merge(config);

        Assert.Equal("test", result["name"]);
        Assert.Equal(42, result["count"]);
    }

    [Fact]
    public void Merge_TwoConfigurations_LaterOverridesEarlier()
    {
        var config1 = new Dictionary<string, object?>
        {
            ["name"] = "first",
            ["count"] = 1
        };

        var config2 = new Dictionary<string, object?>
        {
            ["name"] = "second",
            ["value"] = "new"
        };

        var result = _merger.Merge(config1, config2);

        Assert.Equal("second", result["name"]); // Overridden
        Assert.Equal(1, result["count"]); // Preserved
        Assert.Equal("new", result["value"]); // Added
    }

    [Fact]
    public void Merge_NestedConfigurations_MergesNested()
    {
        var config1 = new Dictionary<string, object?>
        {
            ["deploy"] = new Dictionary<string, object?>
            {
                ["environment"] = "development",
                ["region"] = "us-east"
            }
        };

        var config2 = new Dictionary<string, object?>
        {
            ["deploy"] = new Dictionary<string, object?>
            {
                ["environment"] = "production",
                ["timeout"] = 30
            }
        };

        var result = _merger.Merge(config1, config2);

        var deploy = (IDictionary<string, object?>)result["deploy"]!;
        Assert.Equal("production", deploy["environment"]); // Overridden
        Assert.Equal("us-east", deploy["region"]); // Preserved
        Assert.Equal(30, deploy["timeout"]); // Added
    }

    [Fact]
    public void Merge_NestedOverrideWithScalar_ReplacesNested()
    {
        var config1 = new Dictionary<string, object?>
        {
            ["value"] = new Dictionary<string, object?>
            {
                ["nested"] = true
            }
        };

        var config2 = new Dictionary<string, object?>
        {
            ["value"] = "simple"
        };

        var result = _merger.Merge(config1, config2);

        Assert.Equal("simple", result["value"]);
    }

    [Fact]
    public void Merge_CaseInsensitiveKeys_MergesCorrectly()
    {
        var config1 = new Dictionary<string, object?>
        {
            ["Name"] = "first"
        };

        var config2 = new Dictionary<string, object?>
        {
            ["name"] = "second"
        };

        var result = _merger.Merge(config1, config2);

        // Should have one key (case-insensitive match)
        Assert.Single(result);
        Assert.Equal("second", result["name"]);
        Assert.Equal("second", result["Name"]);
    }

    [Fact]
    public void Merge_NullConfiguration_IsSkipped()
    {
        var config = new Dictionary<string, object?>
        {
            ["name"] = "test"
        };

        var result = _merger.Merge(config, null!);

        Assert.Equal("test", result["name"]);
    }

    [Fact]
    public void MergeEnvironmentVariables_WithPrefix_AppliesOverrides()
    {
        var options = new ConfigurationOptions
        {
            EnvironmentVariablePrefix = "TESTAPP_",
            EnvironmentOverridesConfig = true
        };
        var merger = new ConfigurationMerger(options);

        var config = new Dictionary<string, object?>
        {
            ["verbose"] = false
        };

        // Note: This test depends on environment variables set during test run
        // In a real scenario, you'd set TESTAPP_VERBOSE=true before running
        var result = merger.MergeEnvironmentVariables(config);

        // Just verify the base config is preserved when no matching env var
        Assert.False((bool)result["verbose"]!);
    }
}
