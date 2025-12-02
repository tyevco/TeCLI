using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace TeCLI.Output.Formatters;

/// <summary>
/// Formats output as a table using Spectre.Console.
/// Provides rich terminal output with colors, borders, and alignment.
/// </summary>
public class TableOutputFormatter : IOutputFormatter
{
    /// <inheritdoc />
    public OutputFormat Format => OutputFormat.Table;

    /// <inheritdoc />
    public string FileExtension => ".txt";

    /// <inheritdoc />
    public string MimeType => "text/plain";

    /// <summary>
    /// Gets or sets the table border style.
    /// </summary>
    public TableBorder Border { get; set; } = TableBorder.Rounded;

    /// <summary>
    /// Gets or sets whether to show table headers.
    /// </summary>
    public bool ShowHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets the header style.
    /// </summary>
    public Style HeaderStyle { get; set; } = new Style(Color.Blue, decoration: Decoration.Bold);

    /// <summary>
    /// Gets or sets whether to expand the table to fill the available width.
    /// </summary>
    public bool Expand { get; set; } = false;

    /// <summary>
    /// Gets or sets the title of the table.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the column configurations.
    /// Key is the property name, value is the configuration.
    /// </summary>
    public Dictionary<string, TableColumnConfig> ColumnConfigs { get; set; } = new();

    /// <inheritdoc />
    public void Format(object? value, TextWriter output)
    {
        if (value == null)
        {
            output.WriteLine("(null)");
            return;
        }

        // For a single object, display as key-value pairs
        var type = value.GetType();

        if (IsSimpleType(type))
        {
            output.WriteLine(value.ToString());
            return;
        }

        var table = new Table()
            .Border(Border);

        if (Title != null)
        {
            table.Title(Title);
        }

        table.AddColumn(new TableColumn("Property").Centered());
        table.AddColumn(new TableColumn("Value"));

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

        foreach (var prop in properties)
        {
            try
            {
                var propValue = prop.GetValue(value);
                table.AddRow(
                    new Markup($"[blue]{Markup.Escape(prop.Name)}[/]"),
                    new Markup(Markup.Escape(FormatValue(propValue)))
                );
            }
            catch
            {
                // Skip properties that throw exceptions
            }
        }

        // Write to a StringWriter first, then to the output
        using var stringWriter = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(stringWriter)
        });
        console.Write(table);
        output.Write(stringWriter.ToString());
    }

    /// <inheritdoc />
    public void FormatCollection(IEnumerable<object> values, TextWriter output)
    {
        var list = values?.ToList() ?? new List<object>();

        if (list.Count == 0)
        {
            output.WriteLine("(no items)");
            return;
        }

        // Get properties from the first non-null item
        var firstItem = list.FirstOrDefault(x => x != null);
        if (firstItem == null)
        {
            output.WriteLine("(no items)");
            return;
        }

        var type = firstItem.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .ToList();

        if (properties.Count == 0)
        {
            // Simple type collection
            FormatSimpleCollection(list, output);
            return;
        }

        var table = new Table()
            .Border(Border);

        if (Title != null)
        {
            table.Title(Title);
        }

        if (Expand)
        {
            table.Expand();
        }

        // Add columns
        foreach (var prop in properties)
        {
            var column = new TableColumn(prop.Name);

            if (ColumnConfigs.TryGetValue(prop.Name, out var config))
            {
                if (config.Alignment.HasValue)
                {
                    column = config.Alignment.Value switch
                    {
                        ColumnAlignment.Left => column.LeftAligned(),
                        ColumnAlignment.Right => column.RightAligned(),
                        ColumnAlignment.Center => column.Centered(),
                        _ => column
                    };
                }

                if (config.Width.HasValue)
                {
                    column.Width = config.Width.Value;
                }
            }
            else
            {
                // Auto-align numbers to the right
                if (IsNumericType(prop.PropertyType))
                {
                    column = column.RightAligned();
                }
            }

            if (ShowHeaders)
            {
                column.Header = new Markup($"[blue bold]{Markup.Escape(prop.Name)}[/]");
            }

            table.AddColumn(column);
        }

        // Add rows
        foreach (var item in list)
        {
            if (item == null)
            {
                table.AddRow(properties.Select(_ => new Markup("(null)")).ToArray());
                continue;
            }

            var cells = new List<IRenderable>();
            foreach (var prop in properties)
            {
                try
                {
                    var propValue = prop.GetValue(item);
                    cells.Add(new Markup(Markup.Escape(FormatValue(propValue))));
                }
                catch
                {
                    cells.Add(new Markup("(error)"));
                }
            }
            table.AddRow(cells);
        }

        // Write to a StringWriter first, then to the output
        using var stringWriter = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(stringWriter)
        });
        console.Write(table);
        output.Write(stringWriter.ToString());
    }

    private void FormatSimpleCollection(IEnumerable<object> values, TextWriter output)
    {
        var table = new Table()
            .Border(Border);

        if (Title != null)
        {
            table.Title(Title);
        }

        table.AddColumn(new TableColumn("Value"));

        foreach (var value in values)
        {
            table.AddRow(new Markup(Markup.Escape(FormatValue(value))));
        }

        using var stringWriter = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(stringWriter)
        });
        console.Write(table);
        output.Write(stringWriter.ToString());
    }

    private static string FormatValue(object? value)
    {
        if (value == null)
            return "(null)";

        return value switch
        {
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            DateTimeOffset dto => dto.ToString("yyyy-MM-dd HH:mm:ss zzz"),
            bool b => b ? "Yes" : "No",
            IEnumerable enumerable when !(value is string) => string.Join(", ", enumerable.Cast<object>().Select(x => x?.ToString() ?? "(null)")),
            _ => value.ToString() ?? "(null)"
        };
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

    private static bool IsNumericType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType == typeof(byte)
               || underlyingType == typeof(sbyte)
               || underlyingType == typeof(short)
               || underlyingType == typeof(ushort)
               || underlyingType == typeof(int)
               || underlyingType == typeof(uint)
               || underlyingType == typeof(long)
               || underlyingType == typeof(ulong)
               || underlyingType == typeof(float)
               || underlyingType == typeof(double)
               || underlyingType == typeof(decimal);
    }
}

/// <summary>
/// Configuration for a table column.
/// </summary>
public class TableColumnConfig
{
    /// <summary>
    /// Gets or sets the column alignment.
    /// </summary>
    public ColumnAlignment? Alignment { get; set; }

    /// <summary>
    /// Gets or sets the column width.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets a custom header text.
    /// </summary>
    public string? Header { get; set; }
}

/// <summary>
/// Specifies the alignment of a table column.
/// </summary>
public enum ColumnAlignment
{
    /// <summary>Left alignment.</summary>
    Left,
    /// <summary>Right alignment.</summary>
    Right,
    /// <summary>Center alignment.</summary>
    Center
}

/// <summary>
/// Strongly-typed table formatter for a specific type.
/// </summary>
/// <typeparam name="T">The type of object to format.</typeparam>
public class TableOutputFormatter<T> : TableOutputFormatter, IOutputFormatter<T>
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
