using TeCLI.Tests.TestCommands;
using Xunit;
using System;
using System.Linq;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for enum parameter support
/// </summary>
public class EnumSupportTests
{
    [Fact]
    public void EnumOption_WithValidValue_ShouldParse()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "run", "--log-level", "Debug" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.Equal(LogLevel.Debug, EnumCommand.CapturedLogLevel);
    }

    [Fact]
    public void EnumOption_WithCaseInsensitiveValue_ShouldParse()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "run", "--log-level", "debug" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.Equal(LogLevel.Debug, EnumCommand.CapturedLogLevel);
    }

    [Fact]
    public void EnumOption_WithMixedCaseValue_ShouldParse()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "run", "--log-level", "WaRnInG" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.Equal(LogLevel.Warning, EnumCommand.CapturedLogLevel);
    }

    [Fact]
    public void EnumOption_WithShortName_ShouldParse()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "run", "-l", "Error" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.Equal(LogLevel.Error, EnumCommand.CapturedLogLevel);
    }

    [Fact]
    public void EnumOption_WithDefaultValue_ShouldUseDefault()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "run" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.Equal(LogLevel.Info, EnumCommand.CapturedLogLevel);
    }

    [Fact]
    public void EnumOption_WithInvalidValue_ShouldThrowWithSuggestions()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "run", "--log-level", "InvalidLevel" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = Assert.ThrowsAsync<ArgumentException>(() => dispatcher.DispatchAsync(args)).Result;

        // Error message should mention the invalid value and show valid values
        Assert.Contains("InvalidLevel", exception.Message);
        Assert.Contains("Debug", exception.Message);
        Assert.Contains("Info", exception.Message);
        Assert.Contains("Warning", exception.Message);
        Assert.Contains("Error", exception.Message);
    }

    [Fact]
    public void EnumArgument_WithValidValue_ShouldParse()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "process", "High" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.Equal(Priority.High, EnumCommand.CapturedPriority);
    }

    [Fact]
    public void EnumArgument_WithCaseInsensitiveValue_ShouldParse()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "process", "critical" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.Equal(Priority.Critical, EnumCommand.CapturedPriority);
    }

    [Fact]
    public void EnumArgument_WithInvalidValue_ShouldThrowWithSuggestions()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "process", "Invalid" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = Assert.ThrowsAsync<ArgumentException>(() => dispatcher.DispatchAsync(args)).Result;

        // Error message should mention the invalid value and show valid values
        Assert.Contains("Invalid", exception.Message);
        Assert.Contains("Low", exception.Message);
        Assert.Contains("Medium", exception.Message);
        Assert.Contains("High", exception.Message);
        Assert.Contains("Critical", exception.Message);
    }

    [Fact]
    public void FlagsEnum_WithSingleValue_ShouldParse()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "run", "--permissions", "Read" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.Equal(FilePermissions.Read, EnumCommand.CapturedPermissions);
    }

    [Fact]
    public void FlagsEnum_WithCombinedValue_ShouldParse()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "run", "--permissions", "Read,Write" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.Equal(FilePermissions.Read | FilePermissions.Write, EnumCommand.CapturedPermissions);
    }

    [Fact]
    public void FlagsEnum_WithCombinedValueAndSpaces_ShouldParse()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "run", "--permissions", "Read, Write, Execute" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.Equal(FilePermissions.All, EnumCommand.CapturedPermissions);
    }

    [Fact]
    public void EnumCollection_WithRepeatedValues_ShouldCollectAll()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "batch", "--levels", "Debug", "--levels", "Info", "--levels", "Error" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.NotNull(EnumCommand.CapturedLevels);
        Assert.Equal(3, EnumCommand.CapturedLevels.Length);
        Assert.Equal(LogLevel.Debug, EnumCommand.CapturedLevels[0]);
        Assert.Equal(LogLevel.Info, EnumCommand.CapturedLevels[1]);
        Assert.Equal(LogLevel.Error, EnumCommand.CapturedLevels[2]);
    }

    [Fact]
    public void EnumCollection_WithCommaSeparated_ShouldSplit()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "batch", "--levels", "Debug,Warning,Error" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.NotNull(EnumCommand.CapturedLevels);
        Assert.Equal(3, EnumCommand.CapturedLevels.Length);
        Assert.Equal(LogLevel.Debug, EnumCommand.CapturedLevels[0]);
        Assert.Equal(LogLevel.Warning, EnumCommand.CapturedLevels[1]);
        Assert.Equal(LogLevel.Error, EnumCommand.CapturedLevels[2]);
    }

    [Fact]
    public void EnumCollection_WithMixedRepeatedAndCommaSeparated_ShouldCombine()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "batch", "--levels", "Debug,Info", "--levels", "Warning" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.NotNull(EnumCommand.CapturedLevels);
        Assert.Equal(3, EnumCommand.CapturedLevels.Length);
        Assert.Equal(LogLevel.Debug, EnumCommand.CapturedLevels[0]);
        Assert.Equal(LogLevel.Info, EnumCommand.CapturedLevels[1]);
        Assert.Equal(LogLevel.Warning, EnumCommand.CapturedLevels[2]);
    }

    [Fact]
    public void EnumCollection_WithListType_ShouldWork()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "batch", "--levels", "Debug", "--priorities", "Low", "--priorities", "High" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.NotNull(EnumCommand.CapturedPriorities);
        Assert.Equal(2, EnumCommand.CapturedPriorities.Count);
        Assert.Equal(Priority.Low, EnumCommand.CapturedPriorities[0]);
        Assert.Equal(Priority.High, EnumCommand.CapturedPriorities[1]);
    }

    [Fact]
    public void EnumCollection_WithCaseInsensitiveValues_ShouldParse()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "batch", "--levels", "debug,WARNING,error" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.NotNull(EnumCommand.CapturedLevels);
        Assert.Equal(3, EnumCommand.CapturedLevels.Length);
        Assert.Equal(LogLevel.Debug, EnumCommand.CapturedLevels[0]);
        Assert.Equal(LogLevel.Warning, EnumCommand.CapturedLevels[1]);
        Assert.Equal(LogLevel.Error, EnumCommand.CapturedLevels[2]);
    }

    [Fact]
    public void EnumCollectionArgument_WithMultipleValues_ShouldCollectAll()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "multi", "Low", "Medium", "High", "Critical" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.NotNull(EnumCommand.CapturedPriorities);
        Assert.Equal(4, EnumCommand.CapturedPriorities.Count);
        Assert.Equal(Priority.Low, EnumCommand.CapturedPriorities[0]);
        Assert.Equal(Priority.Medium, EnumCommand.CapturedPriorities[1]);
        Assert.Equal(Priority.High, EnumCommand.CapturedPriorities[2]);
        Assert.Equal(Priority.Critical, EnumCommand.CapturedPriorities[3]);
    }

    [Fact]
    public void EnumCollectionArgument_WithCommaSeparated_ShouldSplit()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "multi", "Low,Medium,High" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(EnumCommand.WasCalled);
        Assert.NotNull(EnumCommand.CapturedPriorities);
        Assert.Equal(3, EnumCommand.CapturedPriorities.Count);
        Assert.Equal(Priority.Low, EnumCommand.CapturedPriorities[0]);
        Assert.Equal(Priority.Medium, EnumCommand.CapturedPriorities[1]);
        Assert.Equal(Priority.High, EnumCommand.CapturedPriorities[2]);
    }

    [Fact]
    public void EnumCollection_WithInvalidValue_ShouldThrowWithSuggestions()
    {
        // Arrange
        EnumCommand.Reset();
        var args = new[] { "enum", "batch", "--levels", "Debug,InvalidLevel,Error" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = Assert.ThrowsAsync<ArgumentException>(() => dispatcher.DispatchAsync(args)).Result;

        // Error message should show valid values
        Assert.Contains("Debug", exception.Message);
        Assert.Contains("Info", exception.Message);
        Assert.Contains("Warning", exception.Message);
        Assert.Contains("Error", exception.Message);
    }
}
