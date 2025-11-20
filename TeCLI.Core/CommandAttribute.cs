using System;

namespace TeCLI.Attributes;

/// <summary>
/// Marks a class as a CLI command that can be invoked from the command line.
/// </summary>
/// <remarks>
/// Classes marked with this attribute will have command-line parsing and dispatch
/// logic automatically generated. The command can contain multiple actions (methods
/// marked with <see cref="ActionAttribute"/>) and one optional primary action
/// (method marked with <see cref="PrimaryAttribute"/>).
/// </remarks>
/// <example>
/// <code>
/// [Command("config", Description = "Manage application configuration")]
/// public class ConfigCommand
/// {
///     [Primary]
///     public void Show() { }
///
///     [Action("set")]
///     public void Set([Argument] string key, [Argument] string value) { }
/// }
/// </code>
/// Usage: <c>myapp config</c> (calls Show) or <c>myapp config set key value</c>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the command as it appears on the command line.
    /// </summary>
    /// <value>
    /// The command name. Should contain only letters, numbers, and hyphens.
    /// </value>
    public string Name { get; }

    /// <summary>
    /// Gets or sets a description of what this command does.
    /// </summary>
    /// <value>
    /// A user-friendly description shown in help text. Can be null.
    /// </value>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the command as it appears on the command line.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
    public CommandAttribute(string name)
    {
        Name = name;
    }
}