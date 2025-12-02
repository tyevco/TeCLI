using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TeCLI.Output;

/// <summary>
/// Provides a unified interface for outputting data in various formats.
/// Use this class to easily format and write data to the console or files.
/// </summary>
/// <example>
/// <code>
/// var context = new OutputContext(OutputFormat.Json, Console.Out);
/// context.Write(users);  // Outputs users as JSON
///
/// // Or with fluent API:
/// OutputContext.Create()
///     .WithFormat(OutputFormat.Table)
///     .WriteTo(Console.Out)
///     .Write(users);
/// </code>
/// </example>
public class OutputContext
{
    private readonly OutputFormatterRegistry _registry;
    private TextWriter _output;
    private OutputFormat _format;

    /// <summary>
    /// Gets the current output format.
    /// </summary>
    public OutputFormat Format => _format;

    /// <summary>
    /// Gets the output writer.
    /// </summary>
    public TextWriter Output => _output;

    /// <summary>
    /// Gets the formatter registry.
    /// </summary>
    public OutputFormatterRegistry Registry => _registry;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutputContext"/> class
    /// with default settings.
    /// </summary>
    public OutputContext()
        : this(OutputFormat.Table, Console.Out, OutputFormatterRegistry.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutputContext"/> class
    /// with the specified format and output.
    /// </summary>
    /// <param name="format">The output format to use.</param>
    /// <param name="output">The text writer to write to.</param>
    public OutputContext(OutputFormat format, TextWriter output)
        : this(format, output, OutputFormatterRegistry.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutputContext"/> class
    /// with the specified format, output, and registry.
    /// </summary>
    /// <param name="format">The output format to use.</param>
    /// <param name="output">The text writer to write to.</param>
    /// <param name="registry">The formatter registry to use.</param>
    public OutputContext(OutputFormat format, TextWriter output, OutputFormatterRegistry registry)
    {
        _format = format;
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    /// <summary>
    /// Creates a new <see cref="OutputContextBuilder"/> for fluent configuration.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static OutputContextBuilder Create()
    {
        return new OutputContextBuilder();
    }

    /// <summary>
    /// Sets the output format.
    /// </summary>
    /// <param name="format">The output format.</param>
    /// <returns>This instance for chaining.</returns>
    public OutputContext WithFormat(OutputFormat format)
    {
        _format = format;
        return this;
    }

    /// <summary>
    /// Sets the output format from a string.
    /// </summary>
    /// <param name="formatString">The format string (e.g., "json", "xml").</param>
    /// <returns>This instance for chaining.</returns>
    public OutputContext WithFormat(string formatString)
    {
        _format = OutputFormatterRegistry.ParseFormat(formatString);
        return this;
    }

    /// <summary>
    /// Sets the output writer.
    /// </summary>
    /// <param name="output">The text writer to write to.</param>
    /// <returns>This instance for chaining.</returns>
    public OutputContext WriteTo(TextWriter output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        return this;
    }

    /// <summary>
    /// Writes a single object to the output using the current format.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void Write(object? value)
    {
        var formatter = _registry.GetFormatter(_format);

        if (value == null)
        {
            formatter.Format(null, _output);
            return;
        }

        // Check if it's an enumerable (but not a string)
        if (value is IEnumerable enumerable && !(value is string))
        {
            var items = enumerable.Cast<object>().ToList();
            formatter.FormatCollection(items, _output);
        }
        else
        {
            formatter.Format(value, _output);
        }
    }

    /// <summary>
    /// Writes a collection of objects to the output using the current format.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="values">The collection to write.</param>
    public void WriteCollection<T>(IEnumerable<T> values)
    {
        var formatter = _registry.GetFormatter(_format);
        var items = values?.Cast<object>() ?? Enumerable.Empty<object>();
        formatter.FormatCollection(items, _output);
    }

    /// <summary>
    /// Formats a single object to a string using the current format.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The formatted string.</returns>
    public string FormatToString(object? value)
    {
        using var writer = new StringWriter();
        var context = new OutputContext(_format, writer, _registry);
        context.Write(value);
        return writer.ToString();
    }

    /// <summary>
    /// Formats a collection to a string using the current format.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="values">The collection to format.</param>
    /// <returns>The formatted string.</returns>
    public string FormatCollectionToString<T>(IEnumerable<T> values)
    {
        using var writer = new StringWriter();
        var context = new OutputContext(_format, writer, _registry);
        context.WriteCollection(values);
        return writer.ToString();
    }
}

/// <summary>
/// Fluent builder for creating <see cref="OutputContext"/> instances.
/// </summary>
public class OutputContextBuilder
{
    private OutputFormat _format = OutputFormat.Table;
    private TextWriter _output = Console.Out;
    private OutputFormatterRegistry _registry = OutputFormatterRegistry.Default;

    /// <summary>
    /// Sets the output format.
    /// </summary>
    /// <param name="format">The output format.</param>
    /// <returns>This builder for chaining.</returns>
    public OutputContextBuilder WithFormat(OutputFormat format)
    {
        _format = format;
        return this;
    }

    /// <summary>
    /// Sets the output format from a string.
    /// </summary>
    /// <param name="formatString">The format string (e.g., "json", "xml").</param>
    /// <returns>This builder for chaining.</returns>
    public OutputContextBuilder WithFormat(string formatString)
    {
        _format = OutputFormatterRegistry.ParseFormat(formatString);
        return this;
    }

    /// <summary>
    /// Sets the output writer.
    /// </summary>
    /// <param name="output">The text writer to write to.</param>
    /// <returns>This builder for chaining.</returns>
    public OutputContextBuilder WriteTo(TextWriter output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        return this;
    }

    /// <summary>
    /// Sets the formatter registry.
    /// </summary>
    /// <param name="registry">The formatter registry.</param>
    /// <returns>This builder for chaining.</returns>
    public OutputContextBuilder WithRegistry(OutputFormatterRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        return this;
    }

    /// <summary>
    /// Builds the <see cref="OutputContext"/>.
    /// </summary>
    /// <returns>The configured output context.</returns>
    public OutputContext Build()
    {
        return new OutputContext(_format, _output, _registry);
    }

    /// <summary>
    /// Builds and immediately writes a value.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void Write(object? value)
    {
        Build().Write(value);
    }

    /// <summary>
    /// Builds and immediately writes a collection.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="values">The collection to write.</param>
    public void WriteCollection<T>(IEnumerable<T> values)
    {
        Build().WriteCollection(values);
    }
}
