using System.IO;
using TeCLI.Tests.TestCommands;
using Xunit;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for configuration file support
/// </summary>
public class ConfigFileTests : IDisposable
{
    private readonly string _originalDirectory;
    private readonly string _testDirectory;

    public ConfigFileTests()
    {
        _originalDirectory = Directory.GetCurrentDirectory();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"TeCLI_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        Directory.SetCurrentDirectory(_testDirectory);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDirectory);
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    #region JSON Configuration File Tests

    [Fact]
    public void ConfigFile_JSON_ShouldLoadCommandOptions()
    {
        // Arrange
        ConfigFileCommand.Reset();
        var configPath = Path.Combine(_testDirectory, ".teclirc.json");
        File.WriteAllText(configPath, @"{
  ""commands"": {
    ""config"": {
      ""environment"": ""production"",
      ""region"": ""us-west""
    }
  }
}");

        var args = new[] { "config", "deploy" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(ConfigFileCommand.WasCalled);
        Assert.Equal("deploy", ConfigFileCommand.LastAction);
        Assert.Equal("production", ConfigFileCommand.CapturedEnvironment);
        Assert.Equal("us-west", ConfigFileCommand.CapturedRegion);
    }

    [Fact]
    public void ConfigFile_JSON_ShouldLoadBooleanOptions()
    {
        // Arrange
        ConfigFileCommand.Reset();
        var configPath = Path.Combine(_testDirectory, ".teclirc.json");
        File.WriteAllText(configPath, @"{
  ""commands"": {
    ""config"": {
      ""verbose"": ""true""
    }
  }
}");

        var args = new[] { "config", "deploy" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(ConfigFileCommand.WasCalled);
        Assert.True(ConfigFileCommand.CapturedVerbose);
    }

    [Fact]
    public void ConfigFile_JSON_ShouldLoadIntegerOptions()
    {
        // Arrange
        ConfigFileCommand.Reset();
        var configPath = Path.Combine(_testDirectory, ".teclirc.json");
        File.WriteAllText(configPath, @"{
  ""commands"": {
    ""config"": {
      ""port"": ""9000""
    }
  }
}");

        var args = new[] { "config", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(ConfigFileCommand.WasCalled);
        Assert.Equal(9000, ConfigFileCommand.CapturedPort);
    }

    #endregion

    #region Config File Discovery Tests

    [Fact]
    public void ConfigFile_ShouldPrefer_TecliRcJson()
    {
        // Arrange
        ConfigFileCommand.Reset();

        // Create multiple config files
        File.WriteAllText(Path.Combine(_testDirectory, ".teclirc.json"), @"{
  ""commands"": {
    ""config"": {
      ""environment"": ""from-teclirc-json""
    }
  }
}");
        File.WriteAllText(Path.Combine(_testDirectory, "tecli.json"), @"{
  ""commands"": {
    ""config"": {
      ""environment"": ""from-tecli-json""
    }
  }
}");

        var args = new[] { "config", "deploy" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(ConfigFileCommand.WasCalled);
        Assert.Equal("from-teclirc-json", ConfigFileCommand.CapturedEnvironment);
    }

    [Fact]
    public void ConfigFile_ShouldFallbackTo_TecliJson()
    {
        // Arrange
        ConfigFileCommand.Reset();

        // Only create tecli.json
        File.WriteAllText(Path.Combine(_testDirectory, "tecli.json"), @"{
  ""commands"": {
    ""config"": {
      ""environment"": ""from-tecli-json""
    }
  }
}");

        var args = new[] { "config", "deploy" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(ConfigFileCommand.WasCalled);
        Assert.Equal("from-tecli-json", ConfigFileCommand.CapturedEnvironment);
    }

    [Fact]
    public void ConfigFile_NoConfigFile_ShouldUseDefaults()
    {
        // Arrange
        ConfigFileCommand.Reset();
        var args = new[] { "config", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(ConfigFileCommand.WasCalled);
        Assert.Equal(8080, ConfigFileCommand.CapturedPort); // default value
    }

    #endregion

    #region Merge Strategy / Precedence Tests

    [Fact]
    public void ConfigFile_CLIArguments_ShouldOverrideConfigFile()
    {
        // Arrange
        ConfigFileCommand.Reset();
        var configPath = Path.Combine(_testDirectory, ".teclirc.json");
        File.WriteAllText(configPath, @"{
  ""commands"": {
    ""config"": {
      ""environment"": ""from-config"",
      ""region"": ""from-config""
    }
  }
}");

        var args = new[] { "config", "deploy", "--environment", "from-cli" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(ConfigFileCommand.WasCalled);
        Assert.Equal("from-cli", ConfigFileCommand.CapturedEnvironment); // CLI overrides config
        Assert.Equal("from-config", ConfigFileCommand.CapturedRegion); // config used when not in CLI
    }

    [Fact]
    public void ConfigFile_ConfigOverridesDefaults()
    {
        // Arrange
        ConfigFileCommand.Reset();
        var configPath = Path.Combine(_testDirectory, ".teclirc.json");
        File.WriteAllText(configPath, @"{
  ""commands"": {
    ""config"": {
      ""port"": ""3000""
    }
  }
}");

        var args = new[] { "config", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(ConfigFileCommand.WasCalled);
        Assert.Equal(3000, ConfigFileCommand.CapturedPort); // config overrides default (8080)
    }

    [Fact]
    public void ConfigFile_BooleanSwitch_CLIOverridesConfig()
    {
        // Arrange
        ConfigFileCommand.Reset();
        var configPath = Path.Combine(_testDirectory, ".teclirc.json");
        File.WriteAllText(configPath, @"{
  ""commands"": {
    ""config"": {
      ""verbose"": ""false""
    }
  }
}");

        var args = new[] { "config", "deploy", "--verbose" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(ConfigFileCommand.WasCalled);
        Assert.True(ConfigFileCommand.CapturedVerbose); // CLI --verbose overrides config false
    }

    #endregion

    #region Global Options Config Tests

    [Fact]
    public void ConfigFile_GlobalOptions_ShouldLoad()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var configPath = Path.Combine(_testDirectory, ".teclirc.json");
        File.WriteAllText(configPath, @"{
  ""globalOptions"": {
    ""verbose"": ""true"",
    ""log-level"": ""debug"",
    ""timeout"": ""60""
  }
}");

        var args = new[] { "globaltest", "simple" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.True(GlobalOptionsCommand.CapturedVerbose);
        Assert.Equal("debug", GlobalOptionsCommand.CapturedLogLevel);
        Assert.Equal(60, GlobalOptionsCommand.CapturedTimeout);
    }

    [Fact]
    public void ConfigFile_GlobalOptions_CLIOverridesConfig()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var configPath = Path.Combine(_testDirectory, ".teclirc.json");
        File.WriteAllText(configPath, @"{
  ""globalOptions"": {
    ""verbose"": ""false"",
    ""log-level"": ""info""
  }
}");

        var args = new[] { "--verbose", "--log-level", "error", "globaltest", "simple" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.True(GlobalOptionsCommand.CapturedVerbose); // CLI overrides config
        Assert.Equal("error", GlobalOptionsCommand.CapturedLogLevel); // CLI overrides config
    }

    [Fact]
    public void ConfigFile_GlobalOptions_PartialCLIOverride()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var configPath = Path.Combine(_testDirectory, ".teclirc.json");
        File.WriteAllText(configPath, @"{
  ""globalOptions"": {
    ""verbose"": ""true"",
    ""log-level"": ""debug"",
    ""timeout"": ""120""
  }
}");

        var args = new[] { "--log-level", "warn", "globaltest", "simple" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.True(GlobalOptionsCommand.CapturedVerbose); // from config
        Assert.Equal("warn", GlobalOptionsCommand.CapturedLogLevel); // from CLI
        Assert.Equal(120, GlobalOptionsCommand.CapturedTimeout); // from config
    }

    #endregion

    #region Mixed GlobalOptions and Command Options

    [Fact]
    public void ConfigFile_MixedGlobalAndCommandOptions_ShouldWork()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var configPath = Path.Combine(_testDirectory, ".teclirc.json");
        File.WriteAllText(configPath, @"{
  ""globalOptions"": {
    ""verbose"": ""true"",
    ""timeout"": ""90""
  },
  ""commands"": {
    ""globaltest"": {
      ""count"": ""5""
    }
  }
}");

        var args = new[] { "globaltest", "deploy", "production" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.Equal("deploy", GlobalOptionsCommand.LastAction);
        Assert.True(GlobalOptionsCommand.CapturedVerbose); // global from config
        Assert.Equal(90, GlobalOptionsCommand.CapturedTimeout); // global from config
        Assert.Equal(5, GlobalOptionsCommand.CapturedCount); // command option from config
    }

    #endregion

    #region Error Handling

    [Fact]
    public void ConfigFile_InvalidJSON_ShouldBeSilentlyIgnored()
    {
        // Arrange
        ConfigFileCommand.Reset();
        var configPath = Path.Combine(_testDirectory, ".teclirc.json");
        File.WriteAllText(configPath, @"{ invalid json here }");

        var args = new[] { "config", "deploy", "--environment", "test" };

        // Act & Assert - should not throw
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        Assert.True(ConfigFileCommand.WasCalled);
        Assert.Equal("test", ConfigFileCommand.CapturedEnvironment);
    }

    [Fact]
    public void ConfigFile_InvalidOptionValue_ShouldBeSilentlyIgnored()
    {
        // Arrange
        ConfigFileCommand.Reset();
        var configPath = Path.Combine(_testDirectory, ".teclirc.json");
        File.WriteAllText(configPath, @"{
  ""commands"": {
    ""config"": {
      ""port"": ""not-a-number""
    }
  }
}");

        var args = new[] { "config", "connect" };

        // Act & Assert - should not throw
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        Assert.True(ConfigFileCommand.WasCalled);
        Assert.Equal(8080, ConfigFileCommand.CapturedPort); // falls back to default
    }

    #endregion
}
