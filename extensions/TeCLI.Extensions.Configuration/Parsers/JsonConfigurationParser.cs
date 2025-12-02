using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TeCLI.Configuration.Parsers;

/// <summary>
/// Configuration parser for JSON files.
/// </summary>
public class JsonConfigurationParser : IConfigurationParser
{
    /// <inheritdoc />
    public IEnumerable<string> SupportedExtensions => new[] { ".json" };

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

        using var document = JsonDocument.Parse(content, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        return ConvertJsonElement(document.RootElement);
    }

    /// <inheritdoc />
    public IDictionary<string, object?> Parse(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }

    private static IDictionary<string, object?> ConvertJsonElement(JsonElement element)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (element.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (var property in element.EnumerateObject())
        {
            result[property.Name] = ConvertValue(property.Value);
        }

        return result;
    }

    private static object? ConvertValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertJsonElement(element),
            JsonValueKind.Array => ConvertArray(element),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => GetNumber(element),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

    private static object GetNumber(JsonElement element)
    {
        if (element.TryGetInt64(out var longValue))
        {
            if (longValue >= int.MinValue && longValue <= int.MaxValue)
            {
                return (int)longValue;
            }
            return longValue;
        }

        if (element.TryGetDouble(out var doubleValue))
        {
            return doubleValue;
        }

        return element.GetDecimal();
    }

    private static List<object?> ConvertArray(JsonElement element)
    {
        var result = new List<object?>();
        foreach (var item in element.EnumerateArray())
        {
            result.Add(ConvertValue(item));
        }
        return result;
    }
}
