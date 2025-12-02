using System.Collections.Generic;
using System.IO;

namespace TeCLI.Configuration;

/// <summary>
/// Interface for configuration file parsers.
/// </summary>
public interface IConfigurationParser
{
    /// <summary>
    /// Gets the file extensions supported by this parser.
    /// </summary>
    IEnumerable<string> SupportedExtensions { get; }

    /// <summary>
    /// Parses the configuration content and returns a dictionary representation.
    /// </summary>
    /// <param name="content">The raw configuration file content.</param>
    /// <returns>A dictionary representing the configuration.</returns>
    IDictionary<string, object?> Parse(string content);

    /// <summary>
    /// Parses the configuration from a stream.
    /// </summary>
    /// <param name="stream">The stream containing configuration content.</param>
    /// <returns>A dictionary representing the configuration.</returns>
    IDictionary<string, object?> Parse(Stream stream);

    /// <summary>
    /// Checks if this parser can handle the given file extension.
    /// </summary>
    /// <param name="extension">The file extension (including the dot).</param>
    /// <returns>True if this parser supports the extension.</returns>
    bool CanParse(string extension);
}
