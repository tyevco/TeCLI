using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TeCLI.Configuration.Parsers;

/// <summary>
/// Configuration parser for YAML files.
/// Provides a lightweight YAML parser for configuration files without external dependencies.
/// </summary>
public class YamlConfigurationParser : IConfigurationParser
{
    /// <inheritdoc />
    public IEnumerable<string> SupportedExtensions => new[] { ".yaml", ".yml" };

    /// <inheritdoc />
    public bool CanParse(string extension)
    {
        return SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public IDictionary<string, object?> Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        var lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        return ParseYaml(lines, 0, lines.Length, 0);
    }

    /// <inheritdoc />
    public IDictionary<string, object?> Parse(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }

    private IDictionary<string, object?> ParseYaml(string[] lines, int startIndex, int endIndex, int expectedIndent)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var i = startIndex;

        while (i < endIndex)
        {
            var line = lines[i];

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
            {
                i++;
                continue;
            }

            var currentIndent = GetIndentation(line);

            // If we've moved back to a lower indentation, we're done with this block
            if (currentIndent < expectedIndent)
            {
                break;
            }

            // Skip lines with greater indentation (they're part of a nested value we've already processed)
            if (currentIndent > expectedIndent && result.Count > 0)
            {
                i++;
                continue;
            }

            var trimmed = line.Trim();

            // Handle list items at this level
            if (trimmed.StartsWith("-"))
            {
                // This is a list - find the end and parse it
                var listEnd = FindBlockEnd(lines, i, endIndex, currentIndent);
                var listItems = ParseList(lines, i, listEnd, currentIndent);
                // Lists at root level are unusual, but we handle them
                i = listEnd;
                continue;
            }

            // Parse key-value pair
            var colonIndex = trimmed.IndexOf(':');
            if (colonIndex <= 0)
            {
                i++;
                continue;
            }

            var key = trimmed.Substring(0, colonIndex).Trim();
            var valuePart = colonIndex < trimmed.Length - 1
                ? trimmed.Substring(colonIndex + 1).Trim()
                : string.Empty;

            if (string.IsNullOrEmpty(valuePart))
            {
                // Check if next line starts a nested block or list
                var nextLineIndex = i + 1;
                while (nextLineIndex < endIndex && string.IsNullOrWhiteSpace(lines[nextLineIndex]))
                {
                    nextLineIndex++;
                }

                if (nextLineIndex < endIndex)
                {
                    var nextLine = lines[nextLineIndex];
                    var nextIndent = GetIndentation(nextLine);
                    var nextTrimmed = nextLine.Trim();

                    if (nextIndent > currentIndent)
                    {
                        var blockEnd = FindBlockEnd(lines, nextLineIndex, endIndex, nextIndent);

                        if (nextTrimmed.StartsWith("-"))
                        {
                            // It's a list
                            result[key] = ParseList(lines, nextLineIndex, blockEnd, nextIndent);
                        }
                        else
                        {
                            // It's a nested object
                            result[key] = ParseYaml(lines, nextLineIndex, blockEnd, nextIndent);
                        }
                        i = blockEnd;
                        continue;
                    }
                }

                // Empty value
                result[key] = null;
            }
            else
            {
                // Inline value
                result[key] = ParseValue(valuePart);
            }

            i++;
        }

        return result;
    }

    private List<object?> ParseList(string[] lines, int startIndex, int endIndex, int listIndent)
    {
        var result = new List<object?>();
        var i = startIndex;

        while (i < endIndex)
        {
            var line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
            {
                i++;
                continue;
            }

            var currentIndent = GetIndentation(line);

            if (currentIndent < listIndent)
            {
                break;
            }

            if (currentIndent > listIndent)
            {
                i++;
                continue;
            }

            var trimmed = line.Trim();

            if (!trimmed.StartsWith("-"))
            {
                break;
            }

            var itemValue = trimmed.Substring(1).Trim();

            if (string.IsNullOrEmpty(itemValue))
            {
                // Check for nested content
                var nextLineIndex = i + 1;
                while (nextLineIndex < endIndex && string.IsNullOrWhiteSpace(lines[nextLineIndex]))
                {
                    nextLineIndex++;
                }

                if (nextLineIndex < endIndex)
                {
                    var nextIndent = GetIndentation(lines[nextLineIndex]);
                    if (nextIndent > currentIndent)
                    {
                        var blockEnd = FindBlockEnd(lines, nextLineIndex, endIndex, nextIndent);
                        var nestedTrimmed = lines[nextLineIndex].Trim();

                        if (nestedTrimmed.StartsWith("-"))
                        {
                            result.Add(ParseList(lines, nextLineIndex, blockEnd, nextIndent));
                        }
                        else
                        {
                            result.Add(ParseYaml(lines, nextLineIndex, blockEnd, nextIndent));
                        }
                        i = blockEnd;
                        continue;
                    }
                }

                result.Add(null);
            }
            else if (itemValue.Contains(":"))
            {
                // Inline object in list item
                var colonIndex = itemValue.IndexOf(':');
                var key = itemValue.Substring(0, colonIndex).Trim();
                var value = colonIndex < itemValue.Length - 1
                    ? itemValue.Substring(colonIndex + 1).Trim()
                    : string.Empty;

                var itemDict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

                if (!string.IsNullOrEmpty(value))
                {
                    itemDict[key] = ParseValue(value);
                }
                else
                {
                    // Check for nested content under this list item
                    var nextLineIndex = i + 1;
                    while (nextLineIndex < endIndex && string.IsNullOrWhiteSpace(lines[nextLineIndex]))
                    {
                        nextLineIndex++;
                    }

                    if (nextLineIndex < endIndex)
                    {
                        var nextIndent = GetIndentation(lines[nextLineIndex]);
                        if (nextIndent > currentIndent)
                        {
                            var blockEnd = FindBlockEnd(lines, nextLineIndex, endIndex, nextIndent);
                            itemDict[key] = ParseYaml(lines, nextLineIndex, blockEnd, nextIndent);
                            i = blockEnd;
                            result.Add(itemDict);
                            continue;
                        }
                    }
                    itemDict[key] = null;
                }

                result.Add(itemDict);
            }
            else
            {
                result.Add(ParseValue(itemValue));
            }

            i++;
        }

        return result;
    }

    private int FindBlockEnd(string[] lines, int startIndex, int maxEnd, int blockIndent)
    {
        var i = startIndex;

        while (i < maxEnd)
        {
            var line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
            {
                i++;
                continue;
            }

            var indent = GetIndentation(line);
            if (indent < blockIndent)
            {
                break;
            }

            i++;
        }

        return i;
    }

    private static int GetIndentation(string line)
    {
        var count = 0;
        foreach (var c in line)
        {
            if (c == ' ')
            {
                count++;
            }
            else if (c == '\t')
            {
                count += 2; // Treat tab as 2 spaces
            }
            else
            {
                break;
            }
        }
        return count;
    }

    private static object? ParseValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // Handle quoted strings
        if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
            (value.StartsWith("'") && value.EndsWith("'")))
        {
            return value.Substring(1, value.Length - 2);
        }

        // Handle null
        if (value.Equals("null", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("~", StringComparison.Ordinal))
        {
            return null;
        }

        // Handle booleans
        if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("on", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Handle numbers
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
        {
            return intValue;
        }

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
        {
            return longValue;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
        {
            return doubleValue;
        }

        // Handle inline arrays [a, b, c]
        if (value.StartsWith("[") && value.EndsWith("]"))
        {
            return ParseInlineArray(value);
        }

        // Return as string
        return value;
    }

    private static List<object?> ParseInlineArray(string value)
    {
        var result = new List<object?>();
        var content = value.Substring(1, value.Length - 2).Trim();

        if (string.IsNullOrEmpty(content))
        {
            return result;
        }

        var items = SplitRespectingQuotes(content, ',');
        foreach (var item in items)
        {
            result.Add(ParseValue(item.Trim()));
        }

        return result;
    }

    private static IEnumerable<string> SplitRespectingQuotes(string input, char separator)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        var quoteChar = '\0';

        foreach (var c in input)
        {
            if (!inQuotes && (c == '"' || c == '\''))
            {
                inQuotes = true;
                quoteChar = c;
                current.Append(c);
            }
            else if (inQuotes && c == quoteChar)
            {
                inQuotes = false;
                current.Append(c);
            }
            else if (!inQuotes && c == separator)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result;
    }
}
