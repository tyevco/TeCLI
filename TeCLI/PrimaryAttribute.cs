using System;

namespace TeCLI.Attributes;

/// <summary>
/// Marks a method as the primary (default) action for a command.
/// </summary>
/// <remarks>
/// When a command is invoked without specifying an action name, the primary action
/// is executed. Only one method per command class can be marked as primary.
/// The primary action can be combined with <see cref="ActionAttribute"/> to allow
/// it to be called either as default or by name.
/// </remarks>
/// <example>
/// <code>
/// [Command("status")]
/// public class StatusCommand
/// {
///     [Primary]
///     [Action("show")]
///     public void ShowStatus()
///     {
///         Console.WriteLine("Status: OK");
///     }
/// }
/// </code>
/// Both <c>myapp status</c> and <c>myapp status show</c> will work.
/// </example>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class PrimaryAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrimaryAttribute"/> class.
    /// </summary>
    public PrimaryAttribute()
    {
    }
}