using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TeCLI.Output.Formatters;

/// <summary>
/// Formats output as YAML (YAML Ain't Markup Language).
/// This is a lightweight YAML implementation without external dependencies.
/// </summary>
public class YamlOutputFormatter : IOutputFormatter
{
    /// <inheritdoc />
    public OutputFormat Format => OutputFormat.Yaml;

    /// <inheritdoc />
    public string FileExtension => ".yaml";

    /// <inheritdoc />
    public string MimeType => "application/x-yaml";

    /// <summary>
    /// Gets or sets the number of spaces per indentation level.
    /// </summary>
    public int IndentSize { get; set; } = 2;

    /// <inheritdoc />
    public void Format(object? value, TextWriter output)
    {
        if (value == null)
        {
            output.WriteLine("null");
            return;
        }

        var builder = new StringBuilder();
        WriteValue(builder, value, 0, false);
        output.Write(builder.ToString());
    }

    /// <inheritdoc />
    public void FormatCollection(IEnumerable<object> values, TextWriter output)
    {
        var list = values?.ToList() ?? new List<object>();

        if (list.Count == 0)
        {
            output.WriteLine("[]");
            return;
        }

        var builder = new StringBuilder();
        foreach (var item in list)
        {
            builder.Append("- ");
            WriteValue(builder, item, 1, true);
        }

        output.Write(builder.ToString());
    }

    private void WriteValue(StringBuilder builder, object? value, int indentLevel, bool isArrayItem)
    {
        if (value == null)
        {
            builder.AppendLine("null");
            return;
        }

        var type = value.GetType();

        if (IsSimpleType(type))
        {
            builder.AppendLine(FormatSimpleValue(value));
            return;
        }

        if (value is IEnumerable enumerable && !(value is string))
        {
            var items = enumerable.Cast<object>().ToList();
            if (items.Count == 0)
            {
                builder.AppendLine("[]");
                return;
            }

            builder.AppendLine();
            foreach (var item in items)
            {
                builder.Append(GetIndent(indentLevel));
                builder.Append("- ");
                WriteValue(builder, item, indentLevel + 1, true);
            }
            return;
        }

        // Complex object
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .ToList();

        if (properties.Count == 0)
        {
            builder.AppendLine("{}");
            return;
        }

        if (!isArrayItem)
        {
            builder.AppendLine();
        }
        else
        {
            // For first property of array item, don't add newline
        }

        bool first = true;
        foreach (var prop in properties)
        {
            try
            {
                var propValue = prop.GetValue(value);

                if (first && isArrayItem)
                {
                    // First property in array item - already indented by "- "
                    builder.Append(ToCamelCase(prop.Name));
                    builder.Append(": ");
                    WriteValue(builder, propValue, indentLevel, false);
                    first = false;
                }
                else
                {
                    builder.Append(GetIndent(indentLevel));
                    builder.Append(ToCamelCase(prop.Name));
                    builder.Append(": ");
                    WriteValue(builder, propValue, indentLevel + 1, false);
                }
            }
            catch
            {
                // Skip properties that throw exceptions
            }
        }
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

    private static string FormatSimpleValue(object value)
    {
        return value switch
        {
            string s => NeedsQuoting(s) ? $"\"{EscapeString(s)}\"" : s,
            bool b => b.ToString().ToLowerInvariant(),
            DateTime dt => dt.ToString("O"),
            DateTimeOffset dto => dto.ToString("O"),
            TimeSpan ts => ts.ToString(),
            Guid g => g.ToString(),
            Enum e => e.ToString(),
            _ => value.ToString() ?? "null"
        };
    }

    private static bool NeedsQuoting(string s)
    {
        if (string.IsNullOrEmpty(s))
            return true;

        // Quote if starts with special characters
        if (s.StartsWith(" ") || s.EndsWith(" "))
            return true;

        // Quote if contains special characters
        var specialChars = new[] { ':', '#', '[', ']', '{', '}', ',', '&', '*', '!', '|', '>', '\'', '"', '%', '@', '`' };
        if (s.IndexOfAny(specialChars) >= 0)
            return true;

        // Quote if looks like a number but should be a string
        if (double.TryParse(s, out _) || s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("false", StringComparison.OrdinalIgnoreCase) || s.Equals("null", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private static string EscapeString(string s)
    {
        return s.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
    }

    private string GetIndent(int level)
    {
        return new string(' ', level * IndentSize);
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        if (name.Length == 1)
            return name.ToLowerInvariant();

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}

/// <summary>
/// Strongly-typed YAML formatter for a specific type.
/// </summary>
/// <typeparam name="T">The type of object to format.</typeparam>
public class YamlOutputFormatter<T> : YamlOutputFormatter, IOutputFormatter<T>
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
