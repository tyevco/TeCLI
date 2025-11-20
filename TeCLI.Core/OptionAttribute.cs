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
    /// Gets or sets a value indicating whether this option is required.
    /// </summary>
    /// <value>
    /// <c>true</c> if the option must be provided by the user; otherwise, <c>false</c>.
    /// Default is <c>false</c>.
    /// </value>
    public bool Required { get; set; } = false;

    /// <summary>
    /// Gets or sets the environment variable name to use as a fallback value.
    /// </summary>
    /// <value>
    /// The name of an environment variable to read if the option is not provided on the command line.
    /// When specified, the precedence is: CLI option > environment variable > default value.
    /// Default is <c>null</c> (no environment variable fallback).
    /// </value>
    /// <example>
    /// <code>
    /// [Action("connect")]
    /// public void Connect(
    ///     [Option("api-key", EnvVar = "API_KEY")] string apiKey,
    ///     [Option("timeout", EnvVar = "TIMEOUT")] int timeout = 30)
    /// {
    ///     // Can be set via --api-key OR API_KEY environment variable
    /// }
    /// </code>
    /// </example>
    public string? EnvVar { get; set; }

    /// <summary>
    /// Gets or sets a prompt message to display when the option is not provided.
    /// </summary>
    /// <value>
    /// The prompt message to display to the user. When specified, the user will be
    /// prompted interactively for the value if it's not provided on the command line
    /// or via environment variable. Prompting applies when the option is required or
    /// when a default value would otherwise be used.
    /// Precedence: CLI option > environment variable > interactive prompt > default value.
    /// Default is <c>null</c> (no prompting).
    /// </value>
    /// <example>
    /// <code>
    /// [Action("deploy")]
    /// public void Deploy(
    ///     [Option("region", Prompt = "Select deployment region")] string region = "us-west")
    /// {
    ///     // User will be prompted if region is not provided via CLI or env var
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
    ///     [Option("username", Prompt = "Enter username")] string username,
    ///     [Option("password", Prompt = "Enter password", SecurePrompt = true)] string password)
    /// {
    ///     // password input will be masked
    /// }
    /// </code>
    /// </example>
    public bool SecurePrompt { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionAttribute"/> class.
    /// </summary>
    /// <param name="name">The long name of the option (without --).</param>
    public OptionAttribute(string name)
    {
        Name = name;
    }
}