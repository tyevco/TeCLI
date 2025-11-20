using System;

namespace TeCLI.Attributes;

/// <summary>
/// Marks a parameter or property as a positional command-line argument.
/// </summary>
/// <remarks>
/// Arguments are required positional parameters that must appear in a specific order
/// on the command line (after the command and action names). Unlike options, arguments
/// are not prefixed with -- or -.
/// </remarks>
/// <example>
/// <code>
/// [Action("copy")]
/// public void CopyFile(
///     [Argument(Description = "Source file path")]
///     string source,
///     [Argument(Description = "Destination file path")]
///     string destination,
///     [Option("overwrite")]
///     bool overwrite = false)
/// {
///     // Implementation
/// }
/// </code>
/// Usage: <c>myapp copy file1.txt file2.txt --overwrite</c>
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class ArgumentAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a description of what this argument represents.
    /// </summary>
    /// <value>
    /// A user-friendly description shown in help text. Can be null.
    /// </value>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentAttribute"/> class.
    /// </summary>
    public ArgumentAttribute()
    {
    }
}