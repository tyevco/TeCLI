using System;
using System.IO;

namespace TeCLI.Attributes.Validation;

/// <summary>
/// Validates that a directory path points to an existing directory.
/// </summary>
/// <remarks>
/// This attribute can be applied to string or DirectoryInfo parameters to ensure
/// the specified directory exists before processing the command.
/// </remarks>
/// <example>
/// <code>
/// [Action("scan")]
/// public void Scan(
///     [Argument(Description = "Source directory")] [DirectoryExists] string sourceDir,
///     [Option("output")] [DirectoryExists] DirectoryInfo? outputDir = null)
/// {
///     // sourceDir must exist
///     // outputDir must exist if provided
/// }
/// </code>
/// Usage: <c>myapp scan /path/to/source --output /path/to/output</c>
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public class DirectoryExistsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the error message to display when validation fails.
    /// If not set, a default message will be generated.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Validates that the specified directory path points to an existing directory.
    /// </summary>
    /// <param name="value">The value to validate (string path or DirectoryInfo).</param>
    /// <param name="parameterName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the directory doesn't exist.</exception>
    public void Validate(object value, string parameterName)
    {
        if (value == null)
        {
            return; // null values are handled by Required validation
        }

        string directoryPath;

        if (value is DirectoryInfo directoryInfo)
        {
            directoryPath = directoryInfo.FullName;
        }
        else if (value is string path)
        {
            directoryPath = path;
        }
        else
        {
            throw new ArgumentException(
                $"DirectoryExists attribute can only be applied to string or DirectoryInfo parameters, but was applied to {value.GetType().Name}.");
        }

        if (!Directory.Exists(directoryPath))
        {
            string message = ErrorMessage ??
                $"Directory '{directoryPath}' specified for '{parameterName}' does not exist.";
            throw new ArgumentException(message);
        }
    }
}
