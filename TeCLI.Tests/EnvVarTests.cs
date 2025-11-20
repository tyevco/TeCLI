using TeCLI.Tests.TestCommands;
using Xunit;
using System;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for environment variable binding support
/// </summary>
public class EnvVarTests
{
    public EnvVarTests()
    {
        // Clean up any test environment variables before each test
        Environment.SetEnvironmentVariable("API_KEY", null);
        Environment.SetEnvironmentVariable("PORT", null);
        Environment.SetEnvironmentVariable("TIMEOUT", null);
        Environment.SetEnvironmentVariable("DEPLOY_ENV", null);
        Environment.SetEnvironmentVariable("VERBOSE", null);
        Environment.SetEnvironmentVariable("REGION", null);
        Environment.SetEnvironmentVariable("TAGS", null);
    }

    #region Basic Environment Variable Tests

    [Fact]
    public void EnvVar_WhenNotProvidedViaCLI_ShouldUseEnvVar()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("API_KEY", "secret-key-from-env");
        var args = new[] { "envvar", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.Equal("secret-key-from-env", EnvVarCommand.CapturedApiKey);
        Assert.Equal(8080, EnvVarCommand.CapturedPort); // default value
        Assert.Equal(30, EnvVarCommand.CapturedTimeout); // default value
    }

    [Fact]
    public void EnvVar_WhenProvidedViaCLI_ShouldOverrideEnvVar()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("API_KEY", "secret-key-from-env");
        Environment.SetEnvironmentVariable("PORT", "3000");
        var args = new[] { "envvar", "connect", "--api-key", "secret-key-from-cli", "--port", "9000" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.Equal("secret-key-from-cli", EnvVarCommand.CapturedApiKey);
        Assert.Equal(9000, EnvVarCommand.CapturedPort);
    }

    [Fact]
    public void EnvVar_WhenNotProvidedAndNoEnvVar_ShouldUseDefaultValue()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("API_KEY", "test-key");
        var args = new[] { "envvar", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.Equal(8080, EnvVarCommand.CapturedPort); // default value
        Assert.Equal(30, EnvVarCommand.CapturedTimeout); // default value
    }

    #endregion

    #region Precedence Tests

    [Fact]
    public void Precedence_CLIOptionTakesPrecedence()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("API_KEY", "env-key");
        Environment.SetEnvironmentVariable("PORT", "3000");
        Environment.SetEnvironmentVariable("TIMEOUT", "60");
        var args = new[] { "envvar", "connect", "--api-key", "cli-key", "--port", "9000", "--timeout", "120" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.Equal("cli-key", EnvVarCommand.CapturedApiKey); // CLI wins
        Assert.Equal(9000, EnvVarCommand.CapturedPort); // CLI wins
        Assert.Equal(120, EnvVarCommand.CapturedTimeout); // CLI wins
    }

    [Fact]
    public void Precedence_EnvVarTakesPrecedenceOverDefault()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("API_KEY", "env-key");
        Environment.SetEnvironmentVariable("PORT", "3000");
        Environment.SetEnvironmentVariable("TIMEOUT", "60");
        var args = new[] { "envvar", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.Equal("env-key", EnvVarCommand.CapturedApiKey);
        Assert.Equal(3000, EnvVarCommand.CapturedPort); // EnvVar wins over default
        Assert.Equal(60, EnvVarCommand.CapturedTimeout); // EnvVar wins over default
    }

    [Fact]
    public void Precedence_MixedSources()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("API_KEY", "env-key");
        Environment.SetEnvironmentVariable("PORT", "3000");
        // TIMEOUT not set, should use default
        var args = new[] { "envvar", "connect", "--port", "9000" }; // Override PORT via CLI

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.Equal("env-key", EnvVarCommand.CapturedApiKey); // From EnvVar
        Assert.Equal(9000, EnvVarCommand.CapturedPort); // From CLI (overrides EnvVar)
        Assert.Equal(30, EnvVarCommand.CapturedTimeout); // From default
    }

    #endregion

    #region Required Options with EnvVar

    [Fact]
    public void RequiredOption_WithEnvVar_ShouldSatisfyRequirement()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("API_KEY", "required-key-from-env");
        var args = new[] { "envvar", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.Equal("required-key-from-env", EnvVarCommand.CapturedApiKey);
    }

    [Fact]
    public void RequiredOption_WithoutEnvVarOrCLI_ShouldThrowException()
    {
        // Arrange
        EnvVarCommand.Reset();
        // API_KEY is required but not provided
        var args = new[] { "envvar", "connect" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = Assert.ThrowsAsync<ArgumentException>(() => dispatcher.DispatchAsync(args)).Result;

        Assert.Contains("api-key", exception.Message);
        Assert.Contains("Required option", exception.Message);
    }

    #endregion

    #region Boolean Switch with EnvVar

    [Fact]
    public void BooleanSwitch_WhenEnvVarTrue_ShouldBeTrue()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("VERBOSE", "true");
        var args = new[] { "envvar", "deploy" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.True(EnvVarCommand.CapturedVerbose);
    }

    [Fact]
    public void BooleanSwitch_WhenEnvVarFalse_ShouldBeFalse()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("VERBOSE", "false");
        var args = new[] { "envvar", "deploy" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.False(EnvVarCommand.CapturedVerbose);
    }

    [Fact]
    public void BooleanSwitch_CLITakesPrecedenceOverEnvVar()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("VERBOSE", "false");
        var args = new[] { "envvar", "deploy", "--verbose" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.True(EnvVarCommand.CapturedVerbose); // CLI switch wins
    }

    [Fact]
    public void BooleanSwitch_WhenEnvVarInvalid_ShouldUseFalse()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("VERBOSE", "invalid-value");
        var args = new[] { "envvar", "deploy" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.False(EnvVarCommand.CapturedVerbose); // Should default to false for invalid values
    }

    #endregion

    #region Collection with EnvVar

    [Fact]
    public void Collection_WhenEnvVarProvided_ShouldParseCommaSeparated()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("TAGS", "tag1,tag2,tag3");
        var args = new[] { "envvar", "process" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.NotNull(EnvVarCommand.CapturedTags);
        Assert.Equal(3, EnvVarCommand.CapturedTags.Length);
        Assert.Equal("tag1", EnvVarCommand.CapturedTags[0]);
        Assert.Equal("tag2", EnvVarCommand.CapturedTags[1]);
        Assert.Equal("tag3", EnvVarCommand.CapturedTags[2]);
    }

    [Fact]
    public void Collection_CLITakesPrecedenceOverEnvVar()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("TAGS", "env1,env2");
        var args = new[] { "envvar", "process", "--tags", "cli1,cli2,cli3" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.NotNull(EnvVarCommand.CapturedTags);
        Assert.Equal(3, EnvVarCommand.CapturedTags.Length);
        Assert.Equal("cli1", EnvVarCommand.CapturedTags[0]);
        Assert.Equal("cli2", EnvVarCommand.CapturedTags[1]);
        Assert.Equal("cli3", EnvVarCommand.CapturedTags[2]);
    }

    [Fact]
    public void Collection_WhenEnvVarHasSpaces_ShouldTrim()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("TAGS", "tag1, tag2 , tag3");
        var args = new[] { "envvar", "process" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.NotNull(EnvVarCommand.CapturedTags);
        Assert.Equal(3, EnvVarCommand.CapturedTags.Length);
        Assert.Equal("tag1", EnvVarCommand.CapturedTags[0]);
        Assert.Equal("tag2", EnvVarCommand.CapturedTags[1]);
        Assert.Equal("tag3", EnvVarCommand.CapturedTags[2]);
    }

    #endregion

    #region Type Conversion Tests

    [Fact]
    public void EnvVar_WithInteger_ShouldConvert()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("API_KEY", "test");
        Environment.SetEnvironmentVariable("PORT", "5432");
        var args = new[] { "envvar", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.Equal(5432, EnvVarCommand.CapturedPort);
    }

    [Fact]
    public void EnvVar_WithInvalidInteger_ShouldThrowException()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("API_KEY", "test");
        Environment.SetEnvironmentVariable("PORT", "invalid");
        var args = new[] { "envvar", "connect" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = Assert.ThrowsAsync<ArgumentException>(() => dispatcher.DispatchAsync(args)).Result;

        Assert.Contains("PORT", exception.Message);
        Assert.Contains("invalid", exception.Message);
    }

    #endregion

    #region Short Name Tests

    [Fact]
    public void EnvVar_WithShortName_ShouldWork()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("DEPLOY_ENV", "production");
        var args = new[] { "envvar", "deploy" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.Equal("production", EnvVarCommand.CapturedEnvironment);
    }

    [Fact]
    public void EnvVar_ShortNameTakesPrecedenceOverEnvVar()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("DEPLOY_ENV", "env-value");
        var args = new[] { "envvar", "deploy", "-e", "cli-value" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.Equal("cli-value", EnvVarCommand.CapturedEnvironment);
    }

    #endregion

    #region Null/Empty Environment Variable Tests

    [Fact]
    public void EnvVar_WhenEmpty_ShouldUseDefault()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("API_KEY", "test");
        Environment.SetEnvironmentVariable("PORT", "");
        var args = new[] { "envvar", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.Equal(8080, EnvVarCommand.CapturedPort); // default value
    }

    [Fact]
    public void EnvVar_WhenNull_ShouldUseDefault()
    {
        // Arrange
        EnvVarCommand.Reset();
        Environment.SetEnvironmentVariable("API_KEY", "test");
        Environment.SetEnvironmentVariable("PORT", null);
        var args = new[] { "envvar", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnvVarCommand.WasCalled);
        Assert.Equal(8080, EnvVarCommand.CapturedPort); // default value
    }

    #endregion
}
