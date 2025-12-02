using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeCLI.Output.Formatters;

/// <summary>
/// Formats output as JSON (JavaScript Object Notation).
/// </summary>
public class JsonOutputFormatter : IOutputFormatter
{
    private readonly JsonSerializerOptions _options;

    /// <inheritdoc />
    public OutputFormat Format => OutputFormat.Json;

    /// <inheritdoc />
    public string FileExtension => ".json";

    /// <inheritdoc />
    public string MimeType => "application/json";

    /// <summary>
    /// Gets or sets whether to use indented formatting.
    /// </summary>
    public bool Indent { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonOutputFormatter"/> class.
    /// </summary>
    public JsonOutputFormatter()
    {
        _options = CreateDefaultOptions();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonOutputFormatter"/> class
    /// with custom JSON serializer options.
    /// </summary>
    /// <param name="options">The JSON serializer options to use.</param>
    public JsonOutputFormatter(JsonSerializerOptions options)
    {
        _options = options ?? CreateDefaultOptions();
    }

    private JsonSerializerOptions CreateDefaultOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = Indent,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <inheritdoc />
    public void Format(object? value, TextWriter output)
    {
        if (value == null)
        {
            output.WriteLine("null");
            return;
        }

        var options = new JsonSerializerOptions(_options)
        {
            WriteIndented = Indent
        };

        var json = JsonSerializer.Serialize(value, value.GetType(), options);
        output.WriteLine(json);
    }

    /// <inheritdoc />
    public void FormatCollection(IEnumerable<object> values, TextWriter output)
    {
        var options = new JsonSerializerOptions(_options)
        {
            WriteIndented = Indent
        };

        var list = values?.ToList() ?? new List<object>();
        var json = JsonSerializer.Serialize(list, options);
        output.WriteLine(json);
    }
}

/// <summary>
/// Strongly-typed JSON formatter for a specific type.
/// </summary>
/// <typeparam name="T">The type of object to format.</typeparam>
public class JsonOutputFormatter<T> : JsonOutputFormatter, IOutputFormatter<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonOutputFormatter{T}"/> class.
    /// </summary>
    public JsonOutputFormatter() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonOutputFormatter{T}"/> class
    /// with custom JSON serializer options.
    /// </summary>
    /// <param name="options">The JSON serializer options to use.</param>
    public JsonOutputFormatter(JsonSerializerOptions options) : base(options)
    {
    }

    /// <inheritdoc />
    public void Format(T value, TextWriter output)
    {
        Format((object?)value, output);
    }

    /// <inheritdoc />
    public void FormatCollection(IEnumerable<T> values, TextWriter output)
    {
        FormatCollection(values?.Cast<object>() ?? Enumerable.Empty<object>(), output);
    }
}
