using System;

namespace TeCLI.Attributes;

/// <summary>
/// Marks a method as a CLI action that can be invoked as a sub-command.
/// </summary>
/// <remarks>
/// Methods marked with this attribute become callable actions within a command class.
/// The method can accept parameters marked with <see cref="OptionAttribute"/> or
/// <see cref="ArgumentAttribute"/> for command-line input.
/// </remarks>
/// <example>
/// <code>
/// [Command("git")]
/// public class GitCommand
/// {
///     [Action("commit")]
///     public void Commit(
///         [Option("message", ShortName = 'm')] string message,
///         [Option("amend")] bool amend = false)
///     {
///         // Implementation
///     }
/// }
/// </code>
/// Usage: <c>myapp git commit -m "fix bug" --amend</c>
/// </example>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ActionAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the action as it appears on the command line.
    /// </summary>
    /// <value>
    /// The action name. Should contain only letters, numbers, and hyphens.
    /// </value>
    public string Name { get; }

    /// <summary>
    /// Gets or sets a description of what this action does.
    /// </summary>
    /// <value>
    /// A user-friendly description shown in help text. Can be null.
    /// </value>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets alternative names (aliases) for this action.
    /// </summary>
    /// <value>
    /// An array of alternative action names. Can be null or empty if no aliases are needed.
    /// </value>
    /// <example>
    /// <code>
    /// [Action("list", Aliases = new[] { "ls", "show" })]
    /// </code>
    /// This allows the action to be invoked as "list", "ls", or "show".
    /// </example>
    public string[]? Aliases { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the action as it appears on the command line.</param>
    public ActionAttribute(string name)
    {
        Name = name;
    }
}