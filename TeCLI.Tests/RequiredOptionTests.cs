using TeCLI.Tests.TestCommands;
using Xunit;
using System;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for required option support
/// </summary>
public class RequiredOptionTests
{
    [Fact]
    public void RequiredOption_WhenProvided_ShouldParse()
    {
        // Arrange
        RequiredOptionsCommand.Reset();
        var args = new[] { "required", "deploy", "--environment", "production" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(RequiredOptionsCommand.WasCalled);
        Assert.Equal("production", RequiredOptionsCommand.CapturedEnvironment);
        Assert.Equal("us-west", RequiredOptionsCommand.CapturedRegion); // default value
    }

    [Fact]
    public void RequiredOption_WhenNotProvided_ShouldThrowException()
    {
        // Arrange
        RequiredOptionsCommand.Reset();
        var args = new[] { "required", "deploy" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = Assert.ThrowsAsync<ArgumentException>(() => dispatcher.DispatchAsync(args)).Result;

        Assert.Contains("environment", exception.Message);
        Assert.Contains("Required option", exception.Message);
    }

    [Fact]
    public void RequiredOption_WithShortName_ShouldParse()
    {
        // Arrange
        RequiredOptionsCommand.Reset();
        var args = new[] { "required", "deploy", "-e", "staging" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(RequiredOptionsCommand.WasCalled);
        Assert.Equal("staging", RequiredOptionsCommand.CapturedEnvironment);
    }

    [Fact]
    public void RequiredOption_WithOptionalOptions_ShouldParse()
    {
        // Arrange
        RequiredOptionsCommand.Reset();
        var args = new[] { "required", "deploy", "--environment", "production", "--region", "us-east", "-v" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(RequiredOptionsCommand.WasCalled);
        Assert.Equal("production", RequiredOptionsCommand.CapturedEnvironment);
        Assert.Equal("us-east", RequiredOptionsCommand.CapturedRegion);
        Assert.True(RequiredOptionsCommand.CapturedVerbose);
    }

    [Fact]
    public void RequiredOption_WithOnlyOptionalOptions_ShouldThrowException()
    {
        // Arrange
        RequiredOptionsCommand.Reset();
        var args = new[] { "required", "deploy", "--region", "us-east", "-v" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = Assert.ThrowsAsync<ArgumentException>(() => dispatcher.DispatchAsync(args)).Result;

        Assert.Contains("environment", exception.Message);
        Assert.Contains("Required option", exception.Message);
    }

    [Fact]
    public void RequiredCollectionOption_WhenProvided_ShouldParse()
    {
        // Arrange
        RequiredOptionsCommand.Reset();
        var args = new[] { "required", "process", "--tags", "tag1", "--tags", "tag2" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(RequiredOptionsCommand.WasCalled);
        Assert.NotNull(RequiredOptionsCommand.CapturedTags);
        Assert.Equal(2, RequiredOptionsCommand.CapturedTags.Length);
        Assert.Equal("tag1", RequiredOptionsCommand.CapturedTags[0]);
        Assert.Equal("tag2", RequiredOptionsCommand.CapturedTags[1]);
    }

    [Fact]
    public void RequiredCollectionOption_WithCommaSeparated_ShouldParse()
    {
        // Arrange
        RequiredOptionsCommand.Reset();
        var args = new[] { "required", "process", "--tags", "tag1,tag2,tag3" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(RequiredOptionsCommand.WasCalled);
        Assert.NotNull(RequiredOptionsCommand.CapturedTags);
        Assert.Equal(3, RequiredOptionsCommand.CapturedTags.Length);
        Assert.Equal("tag1", RequiredOptionsCommand.CapturedTags[0]);
        Assert.Equal("tag2", RequiredOptionsCommand.CapturedTags[1]);
        Assert.Equal("tag3", RequiredOptionsCommand.CapturedTags[2]);
    }

    [Fact]
    public void RequiredCollectionOption_WithShortName_ShouldParse()
    {
        // Arrange
        RequiredOptionsCommand.Reset();
        var args = new[] { "required", "process", "-t", "tag1,tag2" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(RequiredOptionsCommand.WasCalled);
        Assert.NotNull(RequiredOptionsCommand.CapturedTags);
        Assert.Equal(2, RequiredOptionsCommand.CapturedTags.Length);
    }

    [Fact]
    public void RequiredCollectionOption_WhenNotProvided_ShouldThrowException()
    {
        // Arrange
        RequiredOptionsCommand.Reset();
        var args = new[] { "required", "process" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = Assert.ThrowsAsync<ArgumentException>(() => dispatcher.DispatchAsync(args)).Result;

        Assert.Contains("tags", exception.Message);
        Assert.Contains("Required option", exception.Message);
    }

    [Fact]
    public void RequiredOption_WithLongName_ShouldParse()
    {
        // Arrange
        RequiredOptionsCommand.Reset();
        var args = new[] { "required", "connect", "--api-key", "my-secret-key" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(RequiredOptionsCommand.WasCalled);
        Assert.Equal("my-secret-key", RequiredOptionsCommand.CapturedApiKey);
    }

    [Fact]
    public void RequiredOption_WithHyphenatedName_WhenNotProvided_ShouldThrowException()
    {
        // Arrange
        RequiredOptionsCommand.Reset();
        var args = new[] { "required", "connect" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = Assert.ThrowsAsync<ArgumentException>(() => dispatcher.DispatchAsync(args)).Result;

        Assert.Contains("api-key", exception.Message);
        Assert.Contains("Required option", exception.Message);
    }

    [Fact]
    public void RequiredCollectionOption_WithMixedRepeatedAndCommaSeparated_ShouldCombine()
    {
        // Arrange
        RequiredOptionsCommand.Reset();
        var args = new[] { "required", "process", "--tags", "tag1,tag2", "--tags", "tag3" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(RequiredOptionsCommand.WasCalled);
        Assert.NotNull(RequiredOptionsCommand.CapturedTags);
        Assert.Equal(3, RequiredOptionsCommand.CapturedTags.Length);
        Assert.Equal("tag1", RequiredOptionsCommand.CapturedTags[0]);
        Assert.Equal("tag2", RequiredOptionsCommand.CapturedTags[1]);
        Assert.Equal("tag3", RequiredOptionsCommand.CapturedTags[2]);
    }
}
