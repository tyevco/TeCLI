using TeCLI.Attributes;
using TeCLI.Tests.TestHooks;

namespace TeCLI.Tests.TestCommands;

/// <summary>
/// Test command with command-level hooks (inherited by all actions)
/// </summary>
[Command("hooktest", Description = "Test command for hooks system")]
[BeforeExecute(typeof(LoggingBeforeHook))]
[AfterExecute(typeof(LoggingAfterHook))]
public class HooksCommand
{
    public static bool WasCalled { get; private set; }
    public static string? LastAction { get; private set; }
    public static string? CapturedData { get; private set; }

    /// <summary>
    /// Simple action that inherits command-level hooks
    /// </summary>
    [Action("simple", Description = "Simple action with inherited hooks")]
    public void SimpleAction()
    {
        WasCalled = true;
        LastAction = "simple";
    }

    /// <summary>
    /// Action with additional action-level hooks
    /// </summary>
    [Action("withhooks", Description = "Action with additional hooks")]
    [BeforeExecute(typeof(ValidationBeforeHook), Order = 10)]
    [AfterExecute(typeof(CleanupAfterHook), Order = 5)]
    public void ActionWithHooks([Argument] string data)
    {
        WasCalled = true;
        LastAction = "withhooks";
        CapturedData = data;
    }

    /// <summary>
    /// Action that can be cancelled by a hook
    /// </summary>
    [Action("cancellable", Description = "Action that can be cancelled")]
    [BeforeExecute(typeof(CancellingBeforeHook))]
    public void CancellableAction()
    {
        WasCalled = true;
        LastAction = "cancellable";
    }

    /// <summary>
    /// Action that throws an error to test error hooks
    /// </summary>
    [Action("error", Description = "Action that throws an error")]
    [OnError(typeof(ErrorHandlerHook))]
    public void ErrorAction([Argument] string message)
    {
        WasCalled = true;
        LastAction = "error";
        throw new InvalidOperationException(message);
    }

    /// <summary>
    /// Action with multiple error hooks
    /// </summary>
    [Action("multierror", Description = "Action with multiple error hooks")]
    [OnError(typeof(ErrorLoggingHook), Order = 1)]
    [OnError(typeof(ErrorHandlerHook), Order = 2)]
    public void MultiErrorAction()
    {
        WasCalled = true;
        LastAction = "multierror";
        throw new InvalidOperationException("Test error");
    }

    /// <summary>
    /// Action with ordered hooks to test execution sequence
    /// </summary>
    [Action("ordered", Description = "Action with ordered hooks")]
    [BeforeExecute(typeof(OrderedHook3), Order = 3)]
    [BeforeExecute(typeof(OrderedHook1), Order = 1)]
    [BeforeExecute(typeof(OrderedHook2), Order = 2)]
    public void OrderedAction()
    {
        WasCalled = true;
        LastAction = "ordered";
    }

    /// <summary>
    /// Action without any additional hooks
    /// </summary>
    [Action("noextra", Description = "Action without extra hooks")]
    public void NoExtraHooks([Option("name")] string name = "default")
    {
        WasCalled = true;
        LastAction = "noextra";
        CapturedData = name;
    }

    public static void Reset()
    {
        WasCalled = false;
        LastAction = null;
        CapturedData = null;
    }
}

/// <summary>
/// Test command without any hooks
/// </summary>
[Command("nohooks", Description = "Test command without hooks")]
public class NoHooksCommand
{
    public static bool WasCalled { get; private set; }

    [Action("test", Description = "Simple test action")]
    public void TestAction()
    {
        WasCalled = true;
    }

    public static void Reset()
    {
        WasCalled = false;
    }
}

/// <summary>
/// Command with only error hooks at command level
/// </summary>
[Command("errorhandling", Description = "Command with error handling hooks")]
[OnError(typeof(ErrorHandlerHook))]
public class ErrorHandlingCommand
{
    public static bool WasCalled { get; private set; }
    public static string? LastAction { get; private set; }

    [Action("safe", Description = "Safe action that doesn't throw")]
    public void SafeAction()
    {
        WasCalled = true;
        LastAction = "safe";
    }

    [Action("throws", Description = "Action that throws an error")]
    public void ThrowsAction([Argument] string message)
    {
        WasCalled = true;
        LastAction = "throws";
        throw new InvalidOperationException(message);
    }

    public static void Reset()
    {
        WasCalled = false;
        LastAction = null;
    }
}
