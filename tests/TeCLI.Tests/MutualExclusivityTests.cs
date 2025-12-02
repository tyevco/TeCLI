using TeCLI.Tests.TestCommands;
using Xunit;
using System;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for mutual exclusivity option support
/// </summary>
public class MutualExclusivityTests
{
    #region Single Option from Exclusive Set (should pass)

    [Fact]
    public async Task MutualExclusivity_WhenSingleOptionProvided_ShouldSucceed()
    {
        // Arrange
        MutualExclusivityCommand.Reset();
        var args = new[] { "mutualexclusive", "output", "--json" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(MutualExclusivityCommand.WasCalled);
        Assert.True(MutualExclusivityCommand.CapturedJson);
        Assert.False(MutualExclusivityCommand.CapturedXml);
        Assert.False(MutualExclusivityCommand.CapturedYaml);
    }

    [Fact]
    public async Task MutualExclusivity_WhenSingleOptionWithShortNameProvided_ShouldSucceed()
    {
        // Arrange
        MutualExclusivityCommand.Reset();
        var args = new[] { "mutualexclusive", "output", "-x" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(MutualExclusivityCommand.WasCalled);
        Assert.False(MutualExclusivityCommand.CapturedJson);
        Assert.True(MutualExclusivityCommand.CapturedXml);
        Assert.False(MutualExclusivityCommand.CapturedYaml);
    }

    [Fact]
    public async Task MutualExclusivity_WhenNoOptionsProvided_ShouldSucceed()
    {
        // Arrange
        MutualExclusivityCommand.Reset();
        var args = new[] { "mutualexclusive", "output" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(MutualExclusivityCommand.WasCalled);
        Assert.False(MutualExclusivityCommand.CapturedJson);
        Assert.False(MutualExclusivityCommand.CapturedXml);
        Assert.False(MutualExclusivityCommand.CapturedYaml);
    }

    #endregion

    #region Multiple Options from Exclusive Set (should fail)

    [Fact]
    public async Task MutualExclusivity_WhenTwoOptionsProvided_ShouldThrowException()
    {
        // Arrange
        MutualExclusivityCommand.Reset();
        var args = new[] { "mutualexclusive", "output", "--json", "--xml" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("mutually exclusive", exception.Message);
        Assert.Contains("'--json'", exception.Message);
        Assert.Contains("'--xml'", exception.Message);
    }

    [Fact]
    public async Task MutualExclusivity_WhenAllThreeOptionsProvided_ShouldThrowException()
    {
        // Arrange
        MutualExclusivityCommand.Reset();
        var args = new[] { "mutualexclusive", "output", "--json", "--xml", "--yaml" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("mutually exclusive", exception.Message);
    }

    [Fact]
    public async Task MutualExclusivity_WhenMixingLongAndShortOptions_ShouldThrowException()
    {
        // Arrange
        MutualExclusivityCommand.Reset();
        var args = new[] { "mutualexclusive", "output", "--json", "-y" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("mutually exclusive", exception.Message);
    }

    #endregion

    #region Value Options (non-switches) Mutual Exclusivity

    [Fact]
    public async Task MutualExclusivity_ValueOptions_WhenSingleOptionProvided_ShouldSucceed()
    {
        // Arrange
        MutualExclusivityCommand.Reset();
        var args = new[] { "mutualexclusive", "convert", "--format", "utf8" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(MutualExclusivityCommand.WasCalled);
        Assert.Equal("utf8", MutualExclusivityCommand.CapturedFormat);
        Assert.Null(MutualExclusivityCommand.CapturedEncoding);
    }

    [Fact]
    public async Task MutualExclusivity_ValueOptions_WhenBothProvided_ShouldThrowException()
    {
        // Arrange
        MutualExclusivityCommand.Reset();
        var args = new[] { "mutualexclusive", "convert", "--format", "utf8", "--encoding", "ascii" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("mutually exclusive", exception.Message);
        Assert.Contains("'--format'", exception.Message);
        Assert.Contains("'--encoding'", exception.Message);
    }

    #endregion

    #region Multiple Exclusive Sets

    [Fact]
    public async Task MutualExclusivity_MultipleSets_WhenOneFromEachSet_ShouldSucceed()
    {
        // Arrange
        MutualExclusivityCommand.Reset();
        var args = new[] { "mutualexclusive", "process", "--json", "--compact" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(MutualExclusivityCommand.WasCalled);
        Assert.True(MutualExclusivityCommand.CapturedJson);
        Assert.False(MutualExclusivityCommand.CapturedXml);
        Assert.True(MutualExclusivityCommand.CapturedCompact);
        Assert.False(MutualExclusivityCommand.CapturedPretty);
    }

    [Fact]
    public async Task MutualExclusivity_MultipleSets_WhenTwoFromFormatSet_ShouldThrowException()
    {
        // Arrange
        MutualExclusivityCommand.Reset();
        var args = new[] { "mutualexclusive", "process", "--json", "--xml", "--compact" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("mutually exclusive", exception.Message);
        Assert.Contains("'--json'", exception.Message);
        Assert.Contains("'--xml'", exception.Message);
    }

    [Fact]
    public async Task MutualExclusivity_MultipleSets_WhenTwoFromStyleSet_ShouldThrowException()
    {
        // Arrange
        MutualExclusivityCommand.Reset();
        var args = new[] { "mutualexclusive", "process", "--json", "--compact", "--pretty" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("mutually exclusive", exception.Message);
        Assert.Contains("'--compact'", exception.Message);
        Assert.Contains("'--pretty'", exception.Message);
    }

    #endregion

    #region Mixed Exclusive and Non-Exclusive Options

    [Fact]
    public async Task MutualExclusivity_MixedOptions_WhenNonExclusiveWithExclusive_ShouldSucceed()
    {
        // Arrange
        MutualExclusivityCommand.Reset();
        var args = new[] { "mutualexclusive", "export", "--json", "--verbose" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(MutualExclusivityCommand.WasCalled);
        Assert.True(MutualExclusivityCommand.CapturedJson);
        Assert.False(MutualExclusivityCommand.CapturedXml);
    }

    [Fact]
    public async Task MutualExclusivity_MixedOptions_WhenExclusiveViolatedWithNonExclusive_ShouldThrowException()
    {
        // Arrange
        MutualExclusivityCommand.Reset();
        var args = new[] { "mutualexclusive", "export", "--json", "--xml", "--verbose" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("mutually exclusive", exception.Message);
    }

    #endregion
}
