using System;

namespace TeCLI.Attributes;

/// <summary>
/// Specifies a hook that executes before the action
/// </summary>
/// <example>
/// <code>
/// [Command("deploy")]
/// [BeforeExecute(typeof(AuthenticationHook))]
/// public class DeployCommand
/// {
///     [Action("production")]
///     public void Production() { }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class BeforeExecuteAttribute : Attribute
{
    /// <summary>
    /// The type of the hook to execute. Must implement IBeforeExecuteHook
    /// </summary>
    public Type HookType { get; }

    /// <summary>
    /// Optional order for hook execution (lower values execute first)
    /// </summary>
    public int Order { get; set; } = 0;

    public BeforeExecuteAttribute(Type hookType)
    {
        HookType = hookType ?? throw new ArgumentNullException(nameof(hookType));
    }
}

/// <summary>
/// Specifies a hook that executes after the action completes successfully
/// </summary>
/// <example>
/// <code>
/// [Command("process")]
/// [AfterExecute(typeof(LoggingHook))]
/// public class ProcessCommand
/// {
///     [Action("data")]
///     public void Data() { }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class AfterExecuteAttribute : Attribute
{
    /// <summary>
    /// The type of the hook to execute. Must implement IAfterExecuteHook
    /// </summary>
    public Type HookType { get; }

    /// <summary>
    /// Optional order for hook execution (lower values execute first)
    /// </summary>
    public int Order { get; set; } = 0;

    public AfterExecuteAttribute(Type hookType)
    {
        HookType = hookType ?? throw new ArgumentNullException(nameof(hookType));
    }
}

/// <summary>
/// Specifies a hook that executes when an error occurs during action execution
/// </summary>
/// <example>
/// <code>
/// [Command("api")]
/// [OnError(typeof(ErrorLoggingHook))]
/// public class ApiCommand
/// {
///     [Action("call")]
///     public void Call() { }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class OnErrorAttribute : Attribute
{
    /// <summary>
    /// The type of the hook to execute. Must implement IOnErrorHook
    /// </summary>
    public Type HookType { get; }

    /// <summary>
    /// Optional order for hook execution (lower values execute first)
    /// </summary>
    public int Order { get; set; } = 0;

    public OnErrorAttribute(Type hookType)
    {
        HookType = hookType ?? throw new ArgumentNullException(nameof(hookType));
    }
}
