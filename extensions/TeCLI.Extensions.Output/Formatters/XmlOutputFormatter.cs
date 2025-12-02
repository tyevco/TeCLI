using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace TeCLI.Output.Formatters;

/// <summary>
/// Formats output as XML (eXtensible Markup Language).
/// </summary>
public class XmlOutputFormatter : IOutputFormatter
{
    /// <inheritdoc />
    public OutputFormat Format => OutputFormat.Xml;

    /// <inheritdoc />
    public string FileExtension => ".xml";

    /// <inheritdoc />
    public string MimeType => "application/xml";

    /// <summary>
    /// Gets or sets whether to use indented formatting.
    /// </summary>
    public bool Indent { get; set; } = true;

    /// <summary>
    /// Gets or sets the root element name for collections.
    /// </summary>
    public string RootElementName { get; set; } = "Items";

    /// <summary>
    /// Gets or sets the item element name for collection items.
    /// </summary>
    public string ItemElementName { get; set; } = "Item";

    /// <inheritdoc />
    public void Format(object? value, TextWriter output)
    {
        if (value == null)
        {
            output.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            output.WriteLine("<null />");
            return;
        }

        var settings = new XmlWriterSettings
        {
            Indent = Indent,
            IndentChars = "  ",
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        };

        using var stringWriter = new StringWriter();
        using (var writer = XmlWriter.Create(stringWriter, settings))
        {
            writer.WriteStartDocument();
            WriteElement(writer, GetElementName(value.GetType()), value);
            writer.WriteEndDocument();
        }

        output.WriteLine(stringWriter.ToString());
    }

    /// <inheritdoc />
    public void FormatCollection(IEnumerable<object> values, TextWriter output)
    {
        var settings = new XmlWriterSettings
        {
            Indent = Indent,
            IndentChars = "  ",
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        };

        var list = values?.ToList() ?? new List<object>();

        using var stringWriter = new StringWriter();
        using (var writer = XmlWriter.Create(stringWriter, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(RootElementName);

            foreach (var item in list)
            {
                var elementName = item != null ? GetElementName(item.GetType()) : ItemElementName;
                WriteElement(writer, elementName, item);
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        output.WriteLine(stringWriter.ToString());
    }

    private void WriteElement(XmlWriter writer, string elementName, object? value)
    {
        if (value == null)
        {
            writer.WriteStartElement(elementName);
            writer.WriteAttributeString("nil", "true");
            writer.WriteEndElement();
            return;
        }

        var type = value.GetType();

        if (IsSimpleType(type))
        {
            writer.WriteStartElement(elementName);
            writer.WriteString(FormatValue(value));
            writer.WriteEndElement();
            return;
        }

        if (value is IEnumerable enumerable && !(value is string))
        {
            writer.WriteStartElement(elementName);
            foreach (var item in enumerable)
            {
                var itemElementName = item != null ? GetElementName(item.GetType()) : ItemElementName;
                WriteElement(writer, itemElementName, item);
            }
            writer.WriteEndElement();
            return;
        }

        // Complex object
        writer.WriteStartElement(elementName);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

        foreach (var prop in properties)
        {
            try
            {
                var propValue = prop.GetValue(value);
                if (propValue != null)
                {
                    WriteElement(writer, prop.Name, propValue);
                }
            }
            catch
            {
                // Skip properties that throw exceptions
            }
        }
        writer.WriteEndElement();
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive
               || type.IsEnum
               || type == typeof(string)
               || type == typeof(decimal)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(TimeSpan)
               || type == typeof(Guid);
    }

    private static string FormatValue(object value)
    {
        return value switch
        {
            DateTime dt => dt.ToString("O"),
            DateTimeOffset dto => dto.ToString("O"),
            bool b => b.ToString().ToLowerInvariant(),
            _ => value.ToString() ?? string.Empty
        };
    }

    private string GetElementName(Type type)
    {
        var name = type.Name;

        // Handle generic types
        var backtickIndex = name.IndexOf('`');
        if (backtickIndex > 0)
        {
            name = name.Substring(0, backtickIndex);
        }

        // Handle anonymous types
        if (name.StartsWith("<>") || name.Contains("AnonymousType"))
        {
            return ItemElementName;
        }

        return name;
    }
}

/// <summary>
/// Strongly-typed XML formatter for a specific type.
/// </summary>
/// <typeparam name="T">The type of object to format.</typeparam>
public class XmlOutputFormatter<T> : XmlOutputFormatter, IOutputFormatter<T>
{
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
