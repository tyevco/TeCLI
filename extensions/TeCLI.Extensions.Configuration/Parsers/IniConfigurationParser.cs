using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TeCLI.Configuration.Parsers;

/// <summary>
/// Configuration parser for INI files.
/// </summary>
public class IniConfigurationParser : IConfigurationParser
{
    /// <inheritdoc />
    public IEnumerable<string> SupportedExtensions => new[] { ".ini", ".cfg", ".conf" };

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
        IDictionary<string, object?> currentSection = result;

        var lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line) || line.StartsWith(";") || line.StartsWith("#"))
            {
                continue;
            }

            // Handle section headers [section]
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                var sectionName = line.Substring(1, line.Length - 2).Trim();
                currentSection = GetOrCreateSection(result, sectionName);
                continue;
            }

            // Handle key-value pairs (supports = and : as separators)
            var separatorIndex = FindSeparator(line);
            if (separatorIndex > 0)
            {
                var key = line.Substring(0, separatorIndex).Trim();
                var valuePart = line.Substring(separatorIndex + 1).Trim();

                // Remove inline comments
                valuePart = RemoveInlineComment(valuePart);

                // Remove surrounding quotes if present
                valuePart = UnquoteValue(valuePart);

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

    private static int FindSeparator(string line)
    {
        var equalsIndex = line.IndexOf('=');
        var colonIndex = line.IndexOf(':');

        if (equalsIndex < 0) return colonIndex;
        if (colonIndex < 0) return equalsIndex;
        return Math.Min(equalsIndex, colonIndex);
    }

    private static IDictionary<string, object?> GetOrCreateSection(
        IDictionary<string, object?> root,
        string path)
    {
        // Support dotted section names: [section.subsection]
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

    private static string RemoveInlineComment(string value)
    {
        var inQuotes = false;
        var quoteChar = '\0';

        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (!inQuotes && (c == '"' || c == '\''))
            {
                inQuotes = true;
                quoteChar = c;
            }
            else if (inQuotes && c == quoteChar)
            {
                inQuotes = false;
            }
            else if (!inQuotes && (c == ';' || c == '#'))
            {
                return value.Substring(0, i).TrimEnd();
            }
        }

        return value;
    }

    private static string UnquoteValue(string value)
    {
        if (value.Length >= 2)
        {
            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                return value.Substring(1, value.Length - 2);
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

        // Handle booleans (various INI-style representations)
        var lowerValue = value.ToLowerInvariant();
        if (lowerValue == "true" || lowerValue == "yes" || lowerValue == "on" || lowerValue == "1")
        {
            return true;
        }

        if (lowerValue == "false" || lowerValue == "no" || lowerValue == "off" || lowerValue == "0")
        {
            return false;
        }

        // Handle integers
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
        {
            return intValue;
        }

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
        {
            return longValue;
        }

        // Handle floating point
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
        {
            return doubleValue;
        }

        // Handle comma-separated lists
        if (value.Contains(","))
        {
            var items = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(s => ParseValue(s.Trim()))
                             .ToList();
            return items;
        }

        // Return as string
        return value;
    }
}
