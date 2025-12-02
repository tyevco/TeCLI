using System;
using System.Collections.Generic;
using TeCLI.Output.Formatters;

namespace TeCLI.Output;

/// <summary>
/// Registry for output formatters. Manages available formatters and provides
/// formatter lookup by format type.
/// </summary>
public class OutputFormatterRegistry
{
    private readonly Dictionary<OutputFormat, IOutputFormatter> _formatters = new();
    private static readonly Lazy<OutputFormatterRegistry> _default = new(() => CreateDefault());

    /// <summary>
    /// Gets the default registry with all built-in formatters registered.
    /// </summary>
    public static OutputFormatterRegistry Default => _default.Value;

    /// <summary>
    /// Creates a new <see cref="OutputFormatterRegistry"/> with all built-in formatters.
    /// </summary>
    /// <returns>A new registry with JSON, XML, YAML, and Table formatters.</returns>
    public static OutputFormatterRegistry CreateDefault()
    {
        var registry = new OutputFormatterRegistry();
        registry.Register(new JsonOutputFormatter());
        registry.Register(new XmlOutputFormatter());
        registry.Register(new YamlOutputFormatter());
        registry.Register(new TableOutputFormatter());
        return registry;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutputFormatterRegistry"/> class.
    /// </summary>
    public OutputFormatterRegistry()
    {
    }

    /// <summary>
    /// Registers a formatter for its format type.
    /// </summary>
    /// <param name="formatter">The formatter to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when formatter is null.</exception>
    public void Register(IOutputFormatter formatter)
    {
        if (formatter == null)
            throw new ArgumentNullException(nameof(formatter));

        _formatters[formatter.Format] = formatter;
    }

    /// <summary>
    /// Registers a formatter for a specific format type.
    /// </summary>
    /// <param name="format">The format type.</param>
    /// <param name="formatter">The formatter to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when formatter is null.</exception>
    public void Register(OutputFormat format, IOutputFormatter formatter)
    {
        if (formatter == null)
            throw new ArgumentNullException(nameof(formatter));

        _formatters[format] = formatter;
    }

    /// <summary>
    /// Gets the formatter for the specified format.
    /// </summary>
    /// <param name="format">The output format.</param>
    /// <returns>The formatter for the specified format.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no formatter is registered for the format.</exception>
    public IOutputFormatter GetFormatter(OutputFormat format)
    {
        if (!_formatters.TryGetValue(format, out var formatter))
        {
            throw new KeyNotFoundException($"No formatter registered for format '{format}'.");
        }

        return formatter;
    }

    /// <summary>
    /// Tries to get the formatter for the specified format.
    /// </summary>
    /// <param name="format">The output format.</param>
    /// <param name="formatter">When this method returns, contains the formatter if found; otherwise, null.</param>
    /// <returns>true if a formatter was found; otherwise, false.</returns>
    public bool TryGetFormatter(OutputFormat format, out IOutputFormatter? formatter)
    {
        return _formatters.TryGetValue(format, out formatter);
    }

    /// <summary>
    /// Gets all registered formatters.
    /// </summary>
    /// <returns>An enumerable of all registered formatters.</returns>
    public IEnumerable<IOutputFormatter> GetFormatters()
    {
        return _formatters.Values;
    }

    /// <summary>
    /// Checks if a formatter is registered for the specified format.
    /// </summary>
    /// <param name="format">The output format.</param>
    /// <returns>true if a formatter is registered; otherwise, false.</returns>
    public bool HasFormatter(OutputFormat format)
    {
        return _formatters.ContainsKey(format);
    }

    /// <summary>
    /// Removes the formatter for the specified format.
    /// </summary>
    /// <param name="format">The output format.</param>
    /// <returns>true if the formatter was removed; otherwise, false.</returns>
    public bool Unregister(OutputFormat format)
    {
        return _formatters.Remove(format);
    }

    /// <summary>
    /// Parses a format string to an <see cref="OutputFormat"/> value.
    /// </summary>
    /// <param name="formatString">The format string (e.g., "json", "xml", "yaml", "table").</param>
    /// <returns>The parsed output format.</returns>
    /// <exception cref="ArgumentException">Thrown when the format string is not recognized.</exception>
    public static OutputFormat ParseFormat(string formatString)
    {
        if (string.IsNullOrWhiteSpace(formatString))
            throw new ArgumentException("Format string cannot be null or empty.", nameof(formatString));

        return formatString.ToLowerInvariant() switch
        {
            "json" => OutputFormat.Json,
            "xml" => OutputFormat.Xml,
            "yaml" or "yml" => OutputFormat.Yaml,
            "table" or "tbl" => OutputFormat.Table,
            _ => throw new ArgumentException($"Unknown output format: '{formatString}'. Valid formats are: json, xml, yaml, table.", nameof(formatString))
        };
    }

    /// <summary>
    /// Tries to parse a format string to an <see cref="OutputFormat"/> value.
    /// </summary>
    /// <param name="formatString">The format string.</param>
    /// <param name="format">When this method returns, contains the parsed format if successful; otherwise, default.</param>
    /// <returns>true if the format string was parsed successfully; otherwise, false.</returns>
    public static bool TryParseFormat(string? formatString, out OutputFormat format)
    {
        format = default;

        if (string.IsNullOrWhiteSpace(formatString))
            return false;

        switch (formatString.ToLowerInvariant())
        {
            case "json":
                format = OutputFormat.Json;
                return true;
            case "xml":
                format = OutputFormat.Xml;
                return true;
            case "yaml":
            case "yml":
                format = OutputFormat.Yaml;
                return true;
            case "table":
            case "tbl":
                format = OutputFormat.Table;
                return true;
            default:
                return false;
        }
    }
}
