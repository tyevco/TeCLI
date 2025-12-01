using System;
using System.IO;

namespace TeCLI.Attributes.Validation;

/// <summary>
/// Validates that a file path points to an existing file.
/// </summary>
/// <remarks>
/// This attribute can be applied to string or FileInfo parameters to ensure
/// the specified file exists before processing the command.
/// </remarks>
/// <example>
/// <code>
/// [Action("process")]
/// public void Process(
///     [Argument(Description = "Input file")] [FileExists] string inputFile,
///     [Option("config")] [FileExists] FileInfo? configFile = null)
/// {
///     // inputFile must exist
///     // configFile must exist if provided
/// }
/// </code>
/// Usage: <c>myapp process data.txt --config settings.json</c>
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public class FileExistsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the error message to display when validation fails.
    /// If not set, a default message will be generated.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Validates that the specified file path points to an existing file.
    /// </summary>
    /// <param name="value">The value to validate (string path or FileInfo).</param>
    /// <param name="parameterName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the file doesn't exist.</exception>
    public void Validate(object value, string parameterName)
    {
        if (value == null)
        {
            return; // null values are handled by Required validation
        }

        string filePath;

        if (value is FileInfo fileInfo)
        {
            filePath = fileInfo.FullName;
        }
        else if (value is string path)
        {
            filePath = path;
        }
        else
        {
            throw new ArgumentException(
                $"FileExists attribute can only be applied to string or FileInfo parameters, but was applied to {value.GetType().Name}.");
        }

        if (!File.Exists(filePath))
        {
            string message = ErrorMessage ??
                $"File '{filePath}' specified for '{parameterName}' does not exist.";
            throw new ArgumentException(message);
        }
    }
}
