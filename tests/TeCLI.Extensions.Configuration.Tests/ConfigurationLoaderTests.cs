using TeCLI.Configuration;
using Xunit;

namespace TeCLI.Extensions.Configuration.Tests;

public class ConfigurationLoaderTests
{
    [Fact]
    public void GetValue_StringValue_ReturnsString()
    {
        var loader = new ConfigurationLoader();
        var config = new Dictionary<string, object?>
        {
            ["name"] = "test"
        };

        var result = loader.GetValue<string>(config, "name");

        Assert.Equal("test", result);
    }

    [Fact]
    public void GetValue_IntValue_ReturnsInt()
    {
        var loader = new ConfigurationLoader();
        var config = new Dictionary<string, object?>
        {
            ["count"] = 42
        };

        var result = loader.GetValue<int>(config, "count");

        Assert.Equal(42, result);
    }

    [Fact]
    public void GetValue_BoolValue_ReturnsBool()
    {
        var loader = new ConfigurationLoader();
        var config = new Dictionary<string, object?>
        {
            ["enabled"] = true
        };

        var result = loader.GetValue<bool>(config, "enabled");

        Assert.True(result);
    }

    [Fact]
    public void GetValue_NestedValue_ReturnsNestedValue()
    {
        var loader = new ConfigurationLoader();
        var config = new Dictionary<string, object?>
        {
            ["deploy"] = new Dictionary<string, object?>
            {
                ["environment"] = "production"
            }
        };

        var result = loader.GetValue<string>(config, "deploy.environment");

        Assert.Equal("production", result);
    }

    [Fact]
    public void GetValue_MissingKey_ReturnsDefault()
    {
        var loader = new ConfigurationLoader();
        var config = new Dictionary<string, object?>();

        var stringResult = loader.GetValue(config, "missing", "default");
        var intResult = loader.GetValue(config, "missing", 42);

        Assert.Equal("default", stringResult);
        Assert.Equal(42, intResult);
    }

    [Fact]
    public void HasValue_ExistingKey_ReturnsTrue()
    {
        var loader = new ConfigurationLoader();
        var config = new Dictionary<string, object?>
        {
            ["name"] = "test"
        };

        Assert.True(loader.HasValue(config, "name"));
    }

    [Fact]
    public void HasValue_MissingKey_ReturnsFalse()
    {
        var loader = new ConfigurationLoader();
        var config = new Dictionary<string, object?>();

        Assert.False(loader.HasValue(config, "missing"));
    }

    [Fact]
    public void HasValue_NestedKey_ReturnsCorrectValue()
    {
        var loader = new ConfigurationLoader();
        var config = new Dictionary<string, object?>
        {
            ["deploy"] = new Dictionary<string, object?>
            {
                ["environment"] = "production"
            }
        };

        Assert.True(loader.HasValue(config, "deploy.environment"));
        Assert.False(loader.HasValue(config, "deploy.missing"));
    }

    [Fact]
    public void GetCommandConfiguration_WithCommandSection_MergesWithRoot()
    {
        var loader = new ConfigurationLoader();
        var config = new Dictionary<string, object?>
        {
            ["verbose"] = false,
            ["timeout"] = 30,
            ["deploy"] = new Dictionary<string, object?>
            {
                ["environment"] = "production",
                ["verbose"] = true
            }
        };

        var result = loader.GetCommandConfiguration(config, "deploy");

        Assert.Equal(true, result["verbose"]); // Command overrides root
        Assert.Equal(30, result["timeout"]); // Root preserved
        Assert.Equal("production", result["environment"]); // From command
    }

    [Fact]
    public void GetCommandConfiguration_WithCommandsSection_UsesCommandsSection()
    {
        var loader = new ConfigurationLoader();
        var config = new Dictionary<string, object?>
        {
            ["verbose"] = false,
            ["commands"] = new Dictionary<string, object?>
            {
                ["deploy"] = new Dictionary<string, object?>
                {
                    ["environment"] = "production",
                    ["verbose"] = true
                }
            }
        };

        var result = loader.GetCommandConfiguration(config, "deploy");

        Assert.Equal(true, result["verbose"]);
        Assert.Equal("production", result["environment"]);
    }

    [Fact]
    public void GetCommandConfiguration_NoMatchingCommand_ReturnsRootConfig()
    {
        var loader = new ConfigurationLoader();
        var config = new Dictionary<string, object?>
        {
            ["verbose"] = false,
            ["timeout"] = 30
        };

        var result = loader.GetCommandConfiguration(config, "nonexistent");

        Assert.Equal(false, result["verbose"]);
        Assert.Equal(30, result["timeout"]);
    }
}
