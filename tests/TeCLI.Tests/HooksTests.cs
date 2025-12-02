using TeCLI.Tests.TestCommands;
using TeCLI.Tests.TestHooks;
using Xunit;

namespace TeCLI.Tests;

public class HooksTests
{
    private readonly CommandDispatcher _dispatcher = new();

    [Fact]
    public async Task SimpleAction_ExecutesWithInheritedCommandHooks()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        var args = new[] { "hooktest", "simple" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(HooksCommand.WasCalled);
        Assert.Equal("simple", HooksCommand.LastAction);
        Assert.True(LoggingBeforeHook.WasCalled);
        Assert.True(LoggingAfterHook.WasCalled);
        Assert.NotNull(LoggingBeforeHook.LastContext);
        Assert.Equal("hooktest", LoggingBeforeHook.LastContext.CommandName);
        Assert.Equal("simple", LoggingBeforeHook.LastContext.ActionName);
    }

    [Fact]
    public async Task ActionWithHooks_ExecutesBothCommandAndActionLevelHooks()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        var args = new[] { "hooktest", "withhooks", "testdata" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(HooksCommand.WasCalled);
        Assert.Equal("withhooks", HooksCommand.LastAction);
        Assert.Equal("testdata", HooksCommand.CapturedData);

        // Command-level hooks should execute
        Assert.True(LoggingBeforeHook.WasCalled);
        Assert.True(LoggingAfterHook.WasCalled);

        // Action-level hooks should also execute
        Assert.True(ValidationBeforeHook.WasCalled);
        Assert.True(CleanupAfterHook.WasCalled);

        // Verify hook context data was populated
        Assert.NotNull(LoggingBeforeHook.LastContext);
        Assert.Equal("hooktest", LoggingBeforeHook.LastContext.CommandName);
        Assert.Equal("withhooks", LoggingBeforeHook.LastContext.ActionName);
    }

    [Fact]
    public async Task CancellableAction_CanBeCancelledByHook()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        CancellingBeforeHook.ShouldCancel = true;
        var args = new[] { "hooktest", "cancellable" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CancellingBeforeHook.WasCalled);
        Assert.False(HooksCommand.WasCalled); // Action should not execute
        Assert.Null(HooksCommand.LastAction);
    }

    [Fact]
    public async Task CancellableAction_ExecutesWhenNotCancelled()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        CancellingBeforeHook.ShouldCancel = false;
        var args = new[] { "hooktest", "cancellable" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CancellingBeforeHook.WasCalled);
        Assert.True(HooksCommand.WasCalled); // Action should execute
        Assert.Equal("cancellable", HooksCommand.LastAction);
    }

    [Fact]
    public async Task ValidationHook_CancelsActionWhenValidationFails()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        ValidationBeforeHook.ShouldReject = true;
        var args = new[] { "hooktest", "withhooks", "testdata" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationBeforeHook.WasCalled);
        Assert.False(HooksCommand.WasCalled); // Action should not execute
    }

    [Fact]
    public async Task ErrorAction_ErrorHookCanHandleException()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        ErrorHandlerHook.ShouldHandle = true;
        var args = new[] { "hooktest", "error", "Test error message" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(HooksCommand.WasCalled);
        Assert.Equal("error", HooksCommand.LastAction);
        Assert.True(ErrorHandlerHook.WasCalled);
        Assert.NotNull(ErrorHandlerHook.LastException);
        Assert.IsType<InvalidOperationException>(ErrorHandlerHook.LastException);
        Assert.Equal("Test error message", ErrorHandlerHook.LastException.Message);
    }

    [Fact]
    public async Task ErrorAction_ExceptionPropagatesWhenNotHandled()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        ErrorHandlerHook.ShouldHandle = false;
        var args = new[] { "hooktest", "error", "Unhandled error" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Equal("Unhandled error", exception.Message);
        Assert.True(HooksCommand.WasCalled);
        Assert.True(ErrorHandlerHook.WasCalled);
    }

    [Fact]
    public async Task MultiErrorAction_FirstHandlerThatHandlesStopsChain()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        ErrorHandlerHook.ShouldHandle = true;
        var args = new[] { "hooktest", "multierror" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(HooksCommand.WasCalled);
        Assert.True(ErrorLoggingHook.WasCalled);
        Assert.True(ErrorHandlerHook.WasCalled);
        Assert.NotNull(ErrorLoggingHook.LastException);
        Assert.NotNull(ErrorHandlerHook.LastException);
    }

    [Fact]
    public async Task OrderedAction_HooksExecuteInOrderSpecified()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        var args = new[] { "hooktest", "ordered" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(HooksCommand.WasCalled);
        Assert.True(OrderedHook1.WasCalled);
        Assert.True(OrderedHook2.WasCalled);
        Assert.True(OrderedHook3.WasCalled);

        // Verify execution order (plus command-level hooks)
        // Command-level LoggingBeforeHook executes first (Order = 0 by default)
        // Then OrderedHook1 (Order = 1), OrderedHook2 (Order = 2), OrderedHook3 (Order = 3)
        Assert.True(OrderedHook1.ExecutionOrder < OrderedHook2.ExecutionOrder);
        Assert.True(OrderedHook2.ExecutionOrder < OrderedHook3.ExecutionOrder);
    }

    [Fact]
    public async Task NoExtraHooks_OnlyInheritsCommandLevelHooks()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        var args = new[] { "hooktest", "noextra", "--name", "custom" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(HooksCommand.WasCalled);
        Assert.Equal("noextra", HooksCommand.LastAction);
        Assert.Equal("custom", HooksCommand.CapturedData);

        // Command-level hooks should execute
        Assert.True(LoggingBeforeHook.WasCalled);
        Assert.True(LoggingAfterHook.WasCalled);

        // Action-level hooks should not execute
        Assert.False(ValidationBeforeHook.WasCalled);
        Assert.False(CleanupAfterHook.WasCalled);
    }

    [Fact]
    public async Task NoHooksCommand_ExecutesWithoutAnyHooks()
    {
        // Arrange
        ResetAllHooks();
        NoHooksCommand.Reset();
        var args = new[] { "nohooks", "test" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NoHooksCommand.WasCalled);

        // No hooks should execute
        Assert.False(LoggingBeforeHook.WasCalled);
        Assert.False(LoggingAfterHook.WasCalled);
        Assert.False(ValidationBeforeHook.WasCalled);
        Assert.False(ErrorHandlerHook.WasCalled);
    }

    [Fact]
    public async Task ErrorHandlingCommand_SafeAction_HooksAvailableButNotInvoked()
    {
        // Arrange
        ResetAllHooks();
        ErrorHandlingCommand.Reset();
        var args = new[] { "errorhandling", "safe" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ErrorHandlingCommand.WasCalled);
        Assert.Equal("safe", ErrorHandlingCommand.LastAction);

        // Error hooks should not be invoked for successful execution
        Assert.False(ErrorHandlerHook.WasCalled);
    }

    [Fact]
    public async Task ErrorHandlingCommand_ThrowsAction_ErrorHookHandlesException()
    {
        // Arrange
        ResetAllHooks();
        ErrorHandlingCommand.Reset();
        ErrorHandlerHook.ShouldHandle = true;
        var args = new[] { "errorhandling", "throws", "Command error" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ErrorHandlingCommand.WasCalled);
        Assert.Equal("throws", ErrorHandlingCommand.LastAction);
        Assert.True(ErrorHandlerHook.WasCalled);
        Assert.NotNull(ErrorHandlerHook.LastException);
        Assert.Equal("Command error", ErrorHandlerHook.LastException.Message);
    }

    [Fact]
    public async Task HookContext_ContainsCorrectCommandAndActionInfo()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        var args = new[] { "hooktest", "simple" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(LoggingBeforeHook.LastContext);
        Assert.Equal("hooktest", LoggingBeforeHook.LastContext.CommandName);
        Assert.Equal("simple", LoggingBeforeHook.LastContext.ActionName);
        Assert.Empty(LoggingBeforeHook.LastContext.Arguments); // No args after "simple"
    }

    [Fact]
    public async Task HookContext_ContainsRemainingArguments()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        var args = new[] { "hooktest", "withhooks", "testdata" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(LoggingBeforeHook.LastContext);
        Assert.Single(LoggingBeforeHook.LastContext.Arguments);
        Assert.Equal("testdata", LoggingBeforeHook.LastContext.Arguments[0]);
    }

    [Fact]
    public async Task HookContext_DataDictionary_SharedBetweenHooks()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        var args = new[] { "hooktest", "withhooks", "test" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(LoggingBeforeHook.LastContext);
        Assert.NotNull(LoggingAfterHook.LastContext);

        // Data should be shared (both hooks add to the same context)
        Assert.True(LoggingBeforeHook.LastContext.Data.ContainsKey("LoggingBeforeHook"));
        Assert.True(ValidationBeforeHook.WasCalled);

        // After hooks should see data from before hooks
        Assert.True(LoggingAfterHook.LastContext.Data.ContainsKey("LoggingAfterHook"));
    }

    [Fact]
    public async Task BeforeHook_CancellationPreventsAfterHooks()
    {
        // Arrange
        ResetAllHooks();
        HooksCommand.Reset();
        ValidationBeforeHook.ShouldReject = true;
        var args = new[] { "hooktest", "withhooks", "test" };

        // Act
        await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationBeforeHook.WasCalled);
        Assert.False(HooksCommand.WasCalled); // Action should not execute

        // After hooks should not execute when action is cancelled
        Assert.False(CleanupAfterHook.WasCalled);
    }

    private void ResetAllHooks()
    {
        LoggingBeforeHook.Reset();
        LoggingAfterHook.Reset();
        CancellingBeforeHook.Reset();
        ValidationBeforeHook.Reset();
        CleanupAfterHook.Reset();
        ErrorHandlerHook.Reset();
        ErrorLoggingHook.Reset();
        OrderedHookCounter.Reset();
        OrderedHook1.Reset();
        OrderedHook2.Reset();
        OrderedHook3.Reset();
    }
}
