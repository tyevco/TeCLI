using TeCLI.Tests.TestCommands;
using Xunit;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests that verify the generated CommandDispatcher works correctly
/// </summary>
public class IntegrationTests
{
    [Fact]
    public async Task SimpleCommand_Primary_ShouldExecute()
    {
        // Arrange
        SimpleCommand.Reset();
        var args = new[] { "simple" };

        // Act
        // Note: This would require the CommandDispatcher to be generated
        // For now, this demonstrates the test pattern
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(SimpleCommand.WasCalled);
        Assert.Equal("Run called", SimpleCommand.LastMessage);
    }

    [Fact]
    public async Task SimpleCommand_GreetAction_WithArgument_ShouldExecute()
    {
        // Arrange
        SimpleCommand.Reset();
        var args = new[] { "simple", "greet", "World" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(SimpleCommand.WasCalled);
        Assert.Equal("Hello, World!", SimpleCommand.LastMessage);
    }

    [Fact]
    public async Task OptionsCommand_WithShortOptions_ShouldParseCorrectly()
    {
        // Arrange
        OptionsCommand.Reset();
        var args = new[] { "options", "deploy", "-e", "prod", "-f", "-t", "60" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(OptionsCommand.WasCalled);
        Assert.Equal("prod", OptionsCommand.CapturedEnvironment);
        Assert.True(OptionsCommand.CapturedForce);
        Assert.Equal(60, OptionsCommand.CapturedTimeout);
    }

    [Fact]
    public async Task OptionsCommand_WithLongOptions_ShouldParseCorrectly()
    {
        // Arrange
        OptionsCommand.Reset();
        var args = new[] { "options", "deploy", "--environment", "staging", "--force", "--timeout", "120" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(OptionsCommand.WasCalled);
        Assert.Equal("staging", OptionsCommand.CapturedEnvironment);
        Assert.True(OptionsCommand.CapturedForce);
        Assert.Equal(120, OptionsCommand.CapturedTimeout);
    }

    [Fact]
    public async Task OptionsCommand_WithDefaultValues_ShouldUseDefaults()
    {
        // Arrange
        OptionsCommand.Reset();
        var args = new[] { "options", "deploy" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(OptionsCommand.WasCalled);
        Assert.Equal("dev", OptionsCommand.CapturedEnvironment);
        Assert.False(OptionsCommand.CapturedForce);
        Assert.Equal(30, OptionsCommand.CapturedTimeout);
    }
}
