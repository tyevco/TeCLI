using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TeCLI.Configuration.Parsers;

/// <summary>
/// Configuration parser for TOML files.
/// Provides a lightweight TOML parser for configuration files without external dependencies.
/// </summary>
public class TomlConfigurationParser : IConfigurationParser
{
    /// <inheritdoc />
    public IEnumerable<string> SupportedExtensions => new[] { ".toml" };

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

        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var currentSection = result;
        var currentSectionPath = new List<string>();

        var lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
            {
                continue;
            }

            // Handle section headers [section] or [section.subsection]
            if (line.StartsWith("[") && line.EndsWith("]") && !line.StartsWith("[["))
            {
                var sectionName = line.Substring(1, line.Length - 2).Trim();
                currentSection = GetOrCreateSection(result, sectionName);
                currentSectionPath = sectionName.Split('.').ToList();
                continue;
            }

            // Handle array of tables [[array]]
            if (line.StartsWith("[[") && line.EndsWith("]]"))
            {
                var arrayName = line.Substring(2, line.Length - 4).Trim();
                currentSection = GetOrCreateArrayTable(result, arrayName);
                currentSectionPath = arrayName.Split('.').ToList();
                continue;
            }

            // Handle key-value pairs
            var equalsIndex = line.IndexOf('=');
            if (equalsIndex > 0)
            {
                var key = line.Substring(0, equalsIndex).Trim();
                var valuePart = line.Substring(equalsIndex + 1).Trim();

                // Remove inline comments
                valuePart = RemoveInlineComment(valuePart);

                currentSection[key] = ParseValue(valuePart);
            }
        }

        return result;
    }

    /// <inheritdoc />
    public IDictionary<string, object?> Parse(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }

    private static IDictionary<string, object?> GetOrCreateSection(
        IDictionary<string, object?> root,
        string path)
    {
        var parts = path.Split('.');
        var current = root;

        foreach (var part in parts)
        {
            if (!current.TryGetValue(part, out var existing))
            {
                var newSection = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                current[part] = newSection;
                current = newSection;
            }
            else if (existing is IDictionary<string, object?> dict)
            {
                current = dict;
            }
            else
            {
                // Conflict - overwrite with section
                var newSection = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                current[part] = newSection;
                current = newSection;
            }
        }

        return current;
    }

    private static IDictionary<string, object?> GetOrCreateArrayTable(
        IDictionary<string, object?> root,
        string path)
    {
        var parts = path.Split('.');
        var current = root;

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            var isLast = i == parts.Length - 1;

            if (isLast)
            {
                // Create array entry
                if (!current.TryGetValue(part, out var existing))
                {
                    var newArray = new List<object?>();
                    var newEntry = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    newArray.Add(newEntry);
                    current[part] = newArray;
                    return newEntry;
                }
                else if (existing is List<object?> list)
                {
                    var newEntry = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    list.Add(newEntry);
                    return newEntry;
                }
                else
                {
                    var newArray = new List<object?>();
                    var newEntry = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    newArray.Add(newEntry);
                    current[part] = newArray;
                    return newEntry;
                }
            }
            else
            {
                if (!current.TryGetValue(part, out var existing))
                {
                    var newSection = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    current[part] = newSection;
                    current = newSection;
                }
                else if (existing is IDictionary<string, object?> dict)
                {
                    current = dict;
                }
                else if (existing is List<object?> list && list.Count > 0 &&
                         list[list.Count - 1] is IDictionary<string, object?> lastDict)
                {
                    current = lastDict;
                }
            }
        }

        return current;
    }

    private static string RemoveInlineComment(string value)
    {
        var inString = false;
        var stringChar = '\0';
        var escaped = false;

        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (!inString && (c == '"' || c == '\''))
            {
                inString = true;
                stringChar = c;
            }
            else if (inString && c == stringChar)
            {
                inString = false;
            }
            else if (!inString && c == '#')
            {
                return value.Substring(0, i).TrimEnd();
            }
        }

        return value;
    }

    private static object? ParseValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // Handle multiline strings (basic)
        if (value.StartsWith("\"\"\""))
        {
            var endIndex = value.IndexOf("\"\"\"", 3);
            if (endIndex > 3)
            {
                return value.Substring(3, endIndex - 3);
            }
        }

        // Handle basic strings
        if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
        {
            return UnescapeString(value.Substring(1, value.Length - 2));
        }

        // Handle literal strings
        if (value.StartsWith("'") && value.EndsWith("'") && value.Length >= 2)
        {
            return value.Substring(1, value.Length - 2);
        }

        // Handle booleans
        if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Handle integers (including hex, octal, binary)
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(value.Substring(2).Replace("_", ""), NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out var hexValue))
            {
                return hexValue;
            }
        }

        if (value.StartsWith("0o", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return Convert.ToInt32(value.Substring(2).Replace("_", ""), 8);
            }
            catch { }
        }

        if (value.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return Convert.ToInt32(value.Substring(2).Replace("_", ""), 2);
            }
            catch { }
        }

        // Handle regular integers
        var cleanValue = value.Replace("_", "");
        if (int.TryParse(cleanValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
        {
            return intValue;
        }

        if (long.TryParse(cleanValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
        {
            return longValue;
        }

        // Handle floats
        if (double.TryParse(cleanValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
        {
            return doubleValue;
        }

        // Handle special floats
        if (value.Equals("inf", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("+inf", StringComparison.OrdinalIgnoreCase))
        {
            return double.PositiveInfinity;
        }

        if (value.Equals("-inf", StringComparison.OrdinalIgnoreCase))
        {
            return double.NegativeInfinity;
        }

        if (value.Equals("nan", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("+nan", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("-nan", StringComparison.OrdinalIgnoreCase))
        {
            return double.NaN;
        }

        // Handle arrays
        if (value.StartsWith("[") && value.EndsWith("]"))
        {
            return ParseArray(value);
        }

        // Handle inline tables
        if (value.StartsWith("{") && value.EndsWith("}"))
        {
            return ParseInlineTable(value);
        }

        // Handle dates/times
        if (TryParseDateTime(value, out var dateTime))
        {
            return dateTime;
        }

        // Return as string
        return value;
    }

    private static string UnescapeString(string value)
    {
        var result = new StringBuilder();
        var i = 0;

        while (i < value.Length)
        {
            if (value[i] == '\\' && i + 1 < value.Length)
            {
                var next = value[i + 1];
                switch (next)
                {
                    case 'n':
                        result.Append('\n');
                        i += 2;
                        break;
                    case 'r':
                        result.Append('\r');
                        i += 2;
                        break;
                    case 't':
                        result.Append('\t');
                        i += 2;
                        break;
                    case '\\':
                        result.Append('\\');
                        i += 2;
                        break;
                    case '"':
                        result.Append('"');
                        i += 2;
                        break;
                    default:
                        result.Append(value[i]);
                        i++;
                        break;
                }
            }
            else
            {
                result.Append(value[i]);
                i++;
            }
        }

        return result.ToString();
    }

    private static List<object?> ParseArray(string value)
    {
        var result = new List<object?>();
        var content = value.Substring(1, value.Length - 2).Trim();

        if (string.IsNullOrEmpty(content))
        {
            return result;
        }

        var items = SplitArrayItems(content);
        foreach (var item in items)
        {
            var trimmed = item.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                result.Add(ParseValue(trimmed));
            }
        }

        return result;
    }

    private static IDictionary<string, object?> ParseInlineTable(string value)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var content = value.Substring(1, value.Length - 2).Trim();

        if (string.IsNullOrEmpty(content))
        {
            return result;
        }

        var items = SplitArrayItems(content);
        foreach (var item in items)
        {
            var trimmed = item.Trim();
            var equalsIndex = trimmed.IndexOf('=');
            if (equalsIndex > 0)
            {
                var key = trimmed.Substring(0, equalsIndex).Trim();
                var valuePart = trimmed.Substring(equalsIndex + 1).Trim();
                result[key] = ParseValue(valuePart);
            }
        }

        return result;
    }

    private static IEnumerable<string> SplitArrayItems(string content)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var depth = 0;
        var inString = false;
        var stringChar = '\0';
        var escaped = false;

        foreach (var c in content)
        {
            if (escaped)
            {
                current.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                current.Append(c);
                escaped = true;
                continue;
            }

            if (!inString && (c == '"' || c == '\''))
            {
                inString = true;
                stringChar = c;
                current.Append(c);
            }
            else if (inString && c == stringChar)
            {
                inString = false;
                current.Append(c);
            }
            else if (!inString && (c == '[' || c == '{'))
            {
                depth++;
                current.Append(c);
            }
            else if (!inString && (c == ']' || c == '}'))
            {
                depth--;
                current.Append(c);
            }
            else if (!inString && depth == 0 && c == ',')
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

    private static bool TryParseDateTime(string value, out object result)
    {
        result = value;

        // Full datetime with offset
        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out var dateTimeOffset))
        {
            result = dateTimeOffset;
            return true;
        }

        // Local datetime
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out var dateTime))
        {
            result = dateTime;
            return true;
        }

        return false;
    }
}
