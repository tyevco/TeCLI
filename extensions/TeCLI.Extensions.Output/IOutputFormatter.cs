using System;
using System.Collections.Generic;
using System.IO;

namespace TeCLI.Output;

/// <summary>
/// Defines a formatter for converting objects to a specific output format.
/// </summary>
public interface IOutputFormatter
{
    /// <summary>
    /// Gets the output format this formatter handles.
    /// </summary>
    OutputFormat OutputFormat { get; }

    /// <summary>
    /// Gets the file extension associated with this format (e.g., ".json", ".xml").
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Gets the MIME type associated with this format.
    /// </summary>
    string MimeType { get; }

    /// <summary>
    /// Formats a single object and writes it to the output.
    /// </summary>
    /// <param name="value">The object to format.</param>
    /// <param name="output">The text writer to write to.</param>
    void Format(object? value, TextWriter output);

    /// <summary>
    /// Formats a collection of objects and writes it to the output.
    /// </summary>
    /// <param name="values">The collection of objects to format.</param>
    /// <param name="output">The text writer to write to.</param>
    void FormatCollection(IEnumerable<object> values, TextWriter output);
}

/// <summary>
/// Defines a strongly-typed formatter for converting objects of type <typeparamref name="T"/> to a specific output format.
/// Implement this interface to create custom formatters for specific types.
/// </summary>
/// <typeparam name="T">The type of object to format.</typeparam>
/// <example>
/// <code>
/// public class UserCsvFormatter : IOutputFormatter&lt;User&gt;
/// {
///     public OutputFormat OutputFormat => OutputFormat.Csv;
///     public string FileExtension => ".csv";
///     public string MimeType => "text/csv";
///
///     public void Format(User value, TextWriter output)
///     {
///         output.WriteLine($"{value.Id},{value.Name},{value.Email}");
///     }
///
///     public void FormatCollection(IEnumerable&lt;User&gt; values, TextWriter output)
///     {
///         output.WriteLine("Id,Name,Email");
///         foreach (var user in values)
///         {
///             Format(user, output);
///         }
///     }
/// }
/// </code>
/// </example>
public interface IOutputFormatter<T> : IOutputFormatter
{
    /// <summary>
    /// Formats a single object and writes it to the output.
    /// </summary>
    /// <param name="value">The object to format.</param>
    /// <param name="output">The text writer to write to.</param>
    void Format(T value, TextWriter output);

    /// <summary>
    /// Formats a collection of objects and writes it to the output.
    /// </summary>
    /// <param name="values">The collection of objects to format.</param>
    /// <param name="output">The text writer to write to.</param>
    void FormatCollection(IEnumerable<T> values, TextWriter output);
}
