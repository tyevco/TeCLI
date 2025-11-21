using TeCLI.Hooks;

namespace TeCLI.Tests.TestHooks;

/// <summary>
/// Test hook that logs before action execution
/// </summary>
public class LoggingBeforeHook : IBeforeExecuteHook
{
    public static bool WasCalled { get; private set; }
    public static HookContext? LastContext { get; private set; }

    public Task BeforeExecuteAsync(HookContext context)
    {
        WasCalled = true;
        LastContext = context;
        context.Data["LoggingBeforeHook"] = "executed";
        return Task.CompletedTask;
    }

    public static void Reset()
    {
        WasCalled = false;
        LastContext = null;
    }
}

/// <summary>
/// Test hook that cancels action execution based on a condition
/// </summary>
public class CancellingBeforeHook : IBeforeExecuteHook
{
    public static bool WasCalled { get; private set; }
    public static bool ShouldCancel { get; set; }

    public Task BeforeExecuteAsync(HookContext context)
    {
        WasCalled = true;
        if (ShouldCancel)
        {
            context.IsCancelled = true;
            context.CancellationMessage = "Action cancelled by CancellingBeforeHook";
        }
        return Task.CompletedTask;
    }

    public static void Reset()
    {
        WasCalled = false;
        ShouldCancel = false;
    }
}

/// <summary>
/// Test hook that validates arguments before execution
/// </summary>
public class ValidationBeforeHook : IBeforeExecuteHook
{
    public static bool WasCalled { get; private set; }
    public static bool ShouldReject { get; set; }

    public Task BeforeExecuteAsync(HookContext context)
    {
        WasCalled = true;
        if (ShouldReject)
        {
            context.IsCancelled = true;
            context.CancellationMessage = "Validation failed";
        }
        else
        {
            context.Data["ValidationBeforeHook"] = "validated";
        }
        return Task.CompletedTask;
    }

    public static void Reset()
    {
        WasCalled = false;
        ShouldReject = false;
    }
}

/// <summary>
/// Test hook that logs after action execution
/// </summary>
public class LoggingAfterHook : IAfterExecuteHook
{
    public static bool WasCalled { get; private set; }
    public static HookContext? LastContext { get; private set; }
    public static object? LastResult { get; private set; }

    public Task AfterExecuteAsync(HookContext context, object? result)
    {
        WasCalled = true;
        LastContext = context;
        LastResult = result;
        context.Data["LoggingAfterHook"] = "executed";
        return Task.CompletedTask;
    }

    public static void Reset()
    {
        WasCalled = false;
        LastContext = null;
        LastResult = null;
    }
}

/// <summary>
/// Test hook that performs cleanup after execution
/// </summary>
public class CleanupAfterHook : IAfterExecuteHook
{
    public static bool WasCalled { get; private set; }
    public static int ExecutionOrder { get; private set; }
    private static int _counter = 0;

    public Task AfterExecuteAsync(HookContext context, object? result)
    {
        WasCalled = true;
        ExecutionOrder = ++_counter;
        context.Data["CleanupAfterHook"] = "cleaned up";
        return Task.CompletedTask;
    }

    public static void Reset()
    {
        WasCalled = false;
        ExecutionOrder = 0;
        _counter = 0;
    }
}

/// <summary>
/// Test hook that handles errors gracefully
/// </summary>
public class ErrorHandlerHook : IOnErrorHook
{
    public static bool WasCalled { get; private set; }
    public static Exception? LastException { get; private set; }
    public static bool ShouldHandle { get; set; }

    public Task<bool> OnErrorAsync(HookContext context, Exception exception)
    {
        WasCalled = true;
        LastException = exception;
        context.Data["ErrorHandlerHook"] = exception.Message;
        return Task.FromResult(ShouldHandle);
    }

    public static void Reset()
    {
        WasCalled = false;
        LastException = null;
        ShouldHandle = false;
    }
}

/// <summary>
/// Test hook that logs errors but doesn't handle them
/// </summary>
public class ErrorLoggingHook : IOnErrorHook
{
    public static bool WasCalled { get; private set; }
    public static Exception? LastException { get; private set; }

    public Task<bool> OnErrorAsync(HookContext context, Exception exception)
    {
        WasCalled = true;
        LastException = exception;
        context.Data["ErrorLoggingHook"] = "logged";
        // Return false to not handle the exception (let it propagate)
        return Task.FromResult(false);
    }

    public static void Reset()
    {
        WasCalled = false;
        LastException = null;
    }
}

/// <summary>
/// Test hook with specific order for testing hook execution sequence
/// </summary>
public class OrderedHook1 : IBeforeExecuteHook
{
    public static bool WasCalled { get; private set; }
    public static int ExecutionOrder { get; private set; }
    private static int _counter = 0;

    public Task BeforeExecuteAsync(HookContext context)
    {
        WasCalled = true;
        ExecutionOrder = ++_counter;
        return Task.CompletedTask;
    }

    public static void Reset()
    {
        WasCalled = false;
        ExecutionOrder = 0;
        _counter = 0;
    }
}

/// <summary>
/// Test hook with specific order for testing hook execution sequence
/// </summary>
public class OrderedHook2 : IBeforeExecuteHook
{
    public static bool WasCalled { get; private set; }
    public static int ExecutionOrder { get; private set; }
    private static int _counter = 0;

    public Task BeforeExecuteAsync(HookContext context)
    {
        WasCalled = true;
        ExecutionOrder = ++_counter;
        return Task.CompletedTask;
    }

    public static void Reset()
    {
        WasCalled = false;
        ExecutionOrder = 0;
        _counter = 0;
    }
}

/// <summary>
/// Test hook with specific order for testing hook execution sequence
/// </summary>
public class OrderedHook3 : IBeforeExecuteHook
{
    public static bool WasCalled { get; private set; }
    public static int ExecutionOrder { get; private set; }
    private static int _counter = 0;

    public Task BeforeExecuteAsync(HookContext context)
    {
        WasCalled = true;
        ExecutionOrder = ++_counter;
        return Task.CompletedTask;
    }

    public static void Reset()
    {
        WasCalled = false;
        ExecutionOrder = 0;
        _counter = 0;
    }
}
