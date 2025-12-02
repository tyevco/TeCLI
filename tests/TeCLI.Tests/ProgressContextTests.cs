using TeCLI.Tests.TestCommands;
using TeCLI.Console;
using Xunit;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for progress context auto-injection support
/// Tests: IProgressContext, IProgressBar, ISpinner auto-injection into action methods
/// </summary>
public class ProgressContextTests
{
    [Fact]
    public async Task ProgressContext_WhenUsedAsParameter_ShouldBeAutoInjected()
    {
        // Arrange
        ProgressContextCommand.Reset();
        var args = new[] { "progress", "basic" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ProgressContextCommand.WasCalled);
        Assert.NotNull(ProgressContextCommand.CapturedProgressContext);
        Assert.IsAssignableFrom<IProgressContext>(ProgressContextCommand.CapturedProgressContext);
    }

    [Fact]
    public async Task ProgressContext_CreateProgressBar_ShouldReturnProgressBar()
    {
        // Arrange
        ProgressContextCommand.Reset();
        var args = new[] { "progress", "bar" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ProgressContextCommand.WasCalled);
        Assert.NotNull(ProgressContextCommand.CapturedProgressContext);
        Assert.NotNull(ProgressContextCommand.CapturedProgressBar);
    }

    [Fact]
    public async Task ProgressContext_CreateSpinner_ShouldReturnSpinner()
    {
        // Arrange
        ProgressContextCommand.Reset();
        var args = new[] { "progress", "spinner" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ProgressContextCommand.WasCalled);
        Assert.NotNull(ProgressContextCommand.CapturedProgressContext);
        Assert.NotNull(ProgressContextCommand.CapturedSpinner);
    }

    [Fact]
    public async Task ProgressContext_WithOtherArguments_ShouldInjectAlongsideOtherParameters()
    {
        // Arrange
        ProgressContextCommand.Reset();
        var args = new[] { "progress", "with-args", "myfile.txt", "-v" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ProgressContextCommand.WasCalled);
        Assert.NotNull(ProgressContextCommand.CapturedProgressContext);
        Assert.NotNull(ProgressContextCommand.CapturedProgressBar);
        Assert.Equal("Processing myfile.txt, verbose=True", ProgressContextCommand.CapturedMessage);
    }

    [Fact]
    public async Task ProgressContext_WithOtherArguments_WithoutOptionalFlag_ShouldWork()
    {
        // Arrange
        ProgressContextCommand.Reset();
        var args = new[] { "progress", "with-args", "data.csv" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ProgressContextCommand.WasCalled);
        Assert.NotNull(ProgressContextCommand.CapturedProgressContext);
        Assert.Equal("Processing data.csv, verbose=False", ProgressContextCommand.CapturedMessage);
    }

    [Fact]
    public async Task ProgressContext_CreateProgress_ShouldReturnProgressIndicator()
    {
        // Arrange
        ProgressContextCommand.Reset();
        var args = new[] { "progress", "indicator" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ProgressContextCommand.WasCalled);
        Assert.NotNull(ProgressContextCommand.CapturedProgressContext);
    }

    [Fact]
    public async Task ProgressContext_Console_ShouldBeAccessible()
    {
        // Arrange
        ProgressContextCommand.Reset();
        var args = new[] { "progress", "basic" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ProgressContextCommand.WasCalled);
        Assert.NotNull(ProgressContextCommand.CapturedProgressContext);
        Assert.NotNull(ProgressContextCommand.CapturedProgressContext!.Console);
        Assert.IsAssignableFrom<IConsoleOutput>(ProgressContextCommand.CapturedProgressContext.Console);
    }
}
