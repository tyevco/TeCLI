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
    /// Gets or sets a prompt message to display when the argument is not provided.
    /// </summary>
    /// <value>
    /// The prompt message to display to the user. When specified, the user will be
    /// prompted interactively for the value if it's not provided on the command line.
    /// Default is <c>null</c> (no prompting).
    /// </value>
    /// <example>
    /// <code>
    /// [Action("deploy")]
    /// public void Deploy(
    ///     [Argument(Prompt = "Enter deployment environment")] string environment)
    /// {
    ///     // User will be prompted if environment is not provided
    /// }
    /// </code>
    /// </example>
    public string? Prompt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether input should be hidden (for sensitive data like passwords).
    /// </summary>
    /// <value>
    /// <c>true</c> if the input should be masked/hidden; otherwise, <c>false</c>.
    /// Only applies when <see cref="Prompt"/> is specified.
    /// Default is <c>false</c>.
    /// </value>
    /// <example>
    /// <code>
    /// [Action("login")]
    /// public void Login(
    ///     [Argument(Prompt = "Enter username")] string username,
    ///     [Argument(Prompt = "Enter password", SecurePrompt = true)] string password)
    /// {
    ///     // password input will be masked
    /// }
    /// </code>
    /// </example>
    public bool SecurePrompt { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentAttribute"/> class.
    /// </summary>
    public ArgumentAttribute()
    {
    }
}