namespace TeCLI.Output;

/// <summary>
/// Specifies the available output formats for structured data.
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// JSON (JavaScript Object Notation) format.
    /// Human-readable and widely supported.
    /// </summary>
    Json,

    /// <summary>
    /// XML (eXtensible Markup Language) format.
    /// Verbose but well-structured.
    /// </summary>
    Xml,

    /// <summary>
    /// YAML (YAML Ain't Markup Language) format.
    /// Human-friendly data serialization.
    /// </summary>
    Yaml,

    /// <summary>
    /// Table format with aligned columns.
    /// Best for displaying data in the terminal.
    /// </summary>
    Table
}
