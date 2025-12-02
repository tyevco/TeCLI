using TeCLI.Configuration;
using Xunit;

namespace TeCLI.Extensions.Configuration.Tests;

public class ProfileResolverTests
{
    private readonly ProfileResolver _resolver = new();

    [Fact]
    public void ExtractProfiles_NoProfilesSection_ReturnsEmptyDictionary()
    {
        var config = new Dictionary<string, object?>
        {
            ["name"] = "test"
        };

        var profiles = _resolver.ExtractProfiles(config);

        Assert.Empty(profiles);
    }

    [Fact]
    public void ExtractProfiles_WithProfiles_ReturnsProfiles()
    {
        var config = new Dictionary<string, object?>
        {
            ["profiles"] = new Dictionary<string, object?>
            {
                ["dev"] = new Dictionary<string, object?>
                {
                    ["environment"] = "development",
                    ["verbose"] = true
                },
                ["prod"] = new Dictionary<string, object?>
                {
                    ["environment"] = "production",
                    ["verbose"] = false
                }
            }
        };

        var profiles = _resolver.ExtractProfiles(config);

        Assert.Equal(2, profiles.Count);
        Assert.True(profiles.ContainsKey("dev"));
        Assert.True(profiles.ContainsKey("prod"));
        Assert.Equal("development", profiles["dev"].Values["environment"]);
        Assert.Equal("production", profiles["prod"].Values["environment"]);
    }

    [Fact]
    public void ExtractProfiles_WithInheritance_CapturesInheritsProperty()
    {
        var config = new Dictionary<string, object?>
        {
            ["profiles"] = new Dictionary<string, object?>
            {
                ["base"] = new Dictionary<string, object?>
                {
                    ["timeout"] = 30
                },
                ["extended"] = new Dictionary<string, object?>
                {
                    ["inherits"] = "base",
                    ["verbose"] = true
                }
            }
        };

        var profiles = _resolver.ExtractProfiles(config);

        Assert.Equal("base", profiles["extended"].Inherits);
        Assert.False(profiles["extended"].Values.ContainsKey("inherits")); // Should not be in values
    }

    [Fact]
    public void ResolveProfile_SimpleProfile_ReturnsProfileValues()
    {
        var profiles = new Dictionary<string, ConfigurationProfile>
        {
            ["dev"] = new ConfigurationProfile
            {
                Name = "dev",
                Values = new Dictionary<string, object?>
                {
                    ["environment"] = "development",
                    ["verbose"] = true
                }
            }
        };

        var result = _resolver.ResolveProfile(profiles, "dev");

        Assert.Equal("development", result["environment"]);
        Assert.Equal(true, result["verbose"]);
    }

    [Fact]
    public void ResolveProfile_WithInheritance_MergesParentProfile()
    {
        var profiles = new Dictionary<string, ConfigurationProfile>
        {
            ["base"] = new ConfigurationProfile
            {
                Name = "base",
                Values = new Dictionary<string, object?>
                {
                    ["timeout"] = 30,
                    ["retries"] = 3
                }
            },
            ["extended"] = new ConfigurationProfile
            {
                Name = "extended",
                Inherits = "base",
                Values = new Dictionary<string, object?>
                {
                    ["timeout"] = 60, // Override parent
                    ["verbose"] = true // Add new
                }
            }
        };

        var result = _resolver.ResolveProfile(profiles, "extended");

        Assert.Equal(60, result["timeout"]); // Overridden by child
        Assert.Equal(3, result["retries"]); // Inherited from parent
        Assert.Equal(true, result["verbose"]); // Added by child
    }

    [Fact]
    public void ResolveProfile_MultiLevelInheritance_MergesAllLevels()
    {
        var profiles = new Dictionary<string, ConfigurationProfile>
        {
            ["level1"] = new ConfigurationProfile
            {
                Name = "level1",
                Values = new Dictionary<string, object?> { ["a"] = 1 }
            },
            ["level2"] = new ConfigurationProfile
            {
                Name = "level2",
                Inherits = "level1",
                Values = new Dictionary<string, object?> { ["b"] = 2 }
            },
            ["level3"] = new ConfigurationProfile
            {
                Name = "level3",
                Inherits = "level2",
                Values = new Dictionary<string, object?> { ["c"] = 3 }
            }
        };

        var result = _resolver.ResolveProfile(profiles, "level3");

        Assert.Equal(1, result["a"]);
        Assert.Equal(2, result["b"]);
        Assert.Equal(3, result["c"]);
    }

    [Fact]
    public void ResolveProfile_CircularInheritance_DoesNotInfiniteLoop()
    {
        var profiles = new Dictionary<string, ConfigurationProfile>
        {
            ["a"] = new ConfigurationProfile
            {
                Name = "a",
                Inherits = "b",
                Values = new Dictionary<string, object?> { ["from_a"] = true }
            },
            ["b"] = new ConfigurationProfile
            {
                Name = "b",
                Inherits = "a",
                Values = new Dictionary<string, object?> { ["from_b"] = true }
            }
        };

        // Should not throw or hang
        var result = _resolver.ResolveProfile(profiles, "a");

        // Result should contain at least the values from "a"
        Assert.True(result.ContainsKey("from_a"));
    }

    [Fact]
    public void ApplyProfile_AppliesProfileToBaseConfig()
    {
        var baseConfig = new Dictionary<string, object?>
        {
            ["timeout"] = 30,
            ["region"] = "us-east"
        };

        var fullConfig = new Dictionary<string, object?>
        {
            ["timeout"] = 30,
            ["region"] = "us-east",
            ["profiles"] = new Dictionary<string, object?>
            {
                ["prod"] = new Dictionary<string, object?>
                {
                    ["region"] = "us-west",
                    ["verbose"] = false
                }
            }
        };

        var result = _resolver.ApplyProfile(baseConfig, fullConfig, "prod");

        Assert.Equal(30, result["timeout"]); // Base preserved
        Assert.Equal("us-west", result["region"]); // Profile overrides
        Assert.Equal(false, result["verbose"]); // Profile adds
    }

    [Fact]
    public void ApplyProfile_NonExistentProfile_ReturnsBaseConfig()
    {
        var baseConfig = new Dictionary<string, object?>
        {
            ["name"] = "test"
        };

        var fullConfig = new Dictionary<string, object?>
        {
            ["name"] = "test",
            ["profiles"] = new Dictionary<string, object?>()
        };

        var result = _resolver.ApplyProfile(baseConfig, fullConfig, "nonexistent");

        Assert.Equal("test", result["name"]);
    }
}
