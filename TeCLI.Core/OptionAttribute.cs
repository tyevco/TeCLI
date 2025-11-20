using System;

namespace TeCLI.Attributes;

/// <summary>
/// Marks a parameter or property as a command-line option (flag).
/// </summary>
/// <remarks>
/// Options are optional named parameters prefixed with -- (or - for short names).
/// They can appear in any order on the command line. Boolean options act as switches
/// (their presence sets them to true). Other types require a value after the option name.
/// </remarks>
/// <example>
/// <code>
/// [Action("deploy")]
/// public void Deploy(
///     [Option("environment", ShortName = 'e', Description = "Target environment")]
///     string environment = "dev",
///     [Option("force", ShortName = 'f')]
///     bool force = false,
///     [Option("timeout")]
///     int timeout = 30)
/// {
///     // Implementation
/// }
/// </code>
/// Usage: <c>myapp deploy -e prod --force --timeout 60</c>
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class OptionAttribute : Attribute
{
    /// <summary>
    /// Gets the long name of the option as it appears on the command line (without --).
    /// </summary>
    /// <value>
    /// The option name. Should contain only letters, numbers, and hyphens.
    /// </value>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the short name (single character) for this option.
    /// </summary>
    /// <value>
    /// A single character that can be used with - instead of --Name.
    /// Default is '\0' (no short name).
    /// </value>
    public char ShortName { get; set; } = '\0';

    /// <summary>
    /// Gets or sets a description of what this option does.
    /// </summary>
    /// <value>
    /// A user-friendly description shown in help text. Can be null.
    /// </value>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionAttribute"/> class.
    /// </summary>
    /// <param name="name">The long name of the option (without --).</param>
    public OptionAttribute(string name)
    {
        Name = name;
    }
}