using System;
using System.Collections.Generic;
using System.Linq;

namespace TeCLI.Configuration;

/// <summary>
/// Merges multiple configuration dictionaries with a defined precedence strategy.
/// </summary>
public class ConfigurationMerger
{
    private readonly ConfigurationOptions _options;

    /// <summary>
    /// Creates a new configuration merger with the specified options.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    public ConfigurationMerger(ConfigurationOptions? options = null)
    {
        _options = options ?? new ConfigurationOptions();
    }

    /// <summary>
    /// Merges multiple configurations in order of precedence (later items override earlier ones).
    /// </summary>
    /// <param name="configurations">Configurations to merge, in order of increasing precedence.</param>
    /// <returns>The merged configuration.</returns>
    public IDictionary<string, object?> Merge(params IDictionary<string, object?>[] configurations)
    {
        return Merge((IEnumerable<IDictionary<string, object?>>)configurations);
    }

    /// <summary>
    /// Merges multiple configurations in order of precedence (later items override earlier ones).
    /// </summary>
    /// <param name="configurations">Configurations to merge, in order of increasing precedence.</param>
    /// <returns>The merged configuration.</returns>
    public IDictionary<string, object?> Merge(IEnumerable<IDictionary<string, object?>> configurations)
    {
        var comparer = _options.CaseInsensitiveKeys
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        var result = new Dictionary<string, object?>(comparer);

        foreach (var config in configurations)
        {
            if (config == null)
            {
                continue;
            }

            MergeInto(result, config, comparer);
        }

        return result;
    }

    /// <summary>
    /// Merges environment variables into the configuration.
    /// </summary>
    /// <param name="configuration">The base configuration.</param>
    /// <returns>Configuration with environment variable overrides applied.</returns>
    public IDictionary<string, object?> MergeEnvironmentVariables(IDictionary<string, object?> configuration)
    {
        if (!_options.EnvironmentOverridesConfig)
        {
            return configuration;
        }

        var comparer = _options.CaseInsensitiveKeys
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        var result = new Dictionary<string, object?>(comparer);
        MergeInto(result, configuration, comparer);

        var envVars = Environment.GetEnvironmentVariables();
        var prefix = _options.EnvironmentVariablePrefix?.ToUpperInvariant() ?? string.Empty;
        var separator = _options.EnvironmentVariableNestingSeparator;

        foreach (var key in envVars.Keys)
        {
            var keyString = key?.ToString();
            if (string.IsNullOrEmpty(keyString))
            {
                continue;
            }

            // Check if this env var matches our prefix
            if (!string.IsNullOrEmpty(prefix))
            {
                if (!keyString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                keyString = keyString.Substring(prefix.Length);
            }
            else
            {
                // Without prefix, only use env vars that match existing config keys
                if (!HasMatchingKey(result, keyString, separator, comparer))
                {
                    continue;
                }
            }

            var value = envVars[key]?.ToString();
            SetNestedValue(result, keyString, ParseEnvironmentValue(value), separator, comparer);
        }

        return result;
    }

    private static void MergeInto(
        IDictionary<string, object?> target,
        IDictionary<string, object?> source,
        StringComparer comparer)
    {
        foreach (var kvp in source)
        {
            if (target.TryGetValue(kvp.Key, out var existingValue) &&
                existingValue is IDictionary<string, object?> existingDict &&
                kvp.Value is IDictionary<string, object?> newDict)
            {
                // Recursively merge nested dictionaries
                var mergedDict = new Dictionary<string, object?>(comparer);
                MergeInto(mergedDict, existingDict, comparer);
                MergeInto(mergedDict, newDict, comparer);
                target[kvp.Key] = mergedDict;
            }
            else
            {
                // Override with new value
                target[kvp.Key] = kvp.Value;
            }
        }
    }

    private static bool HasMatchingKey(
        IDictionary<string, object?> config,
        string envKey,
        string separator,
        StringComparer comparer)
    {
        var parts = envKey.Split(new[] { separator }, StringSplitOptions.None);
        var current = config;

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            var matchingKey = current.Keys.FirstOrDefault(k =>
                comparer.Equals(k, part) ||
                comparer.Equals(k.Replace("-", "_").Replace(".", "_"), part));

            if (matchingKey == null)
            {
                return false;
            }

            if (i < parts.Length - 1)
            {
                if (current[matchingKey] is IDictionary<string, object?> nested)
                {
                    current = nested;
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static void SetNestedValue(
        IDictionary<string, object?> config,
        string key,
        object? value,
        string separator,
        StringComparer comparer)
    {
        var parts = key.Split(new[] { separator }, StringSplitOptions.None);
        var current = config;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            var part = parts[i];
            var normalizedPart = part.ToLowerInvariant().Replace("_", "-");

            // Find existing key that matches
            var matchingKey = current.Keys.FirstOrDefault(k =>
                comparer.Equals(k, part) ||
                comparer.Equals(k, normalizedPart) ||
                comparer.Equals(k.Replace("-", "_"), part));

            if (matchingKey != null && current[matchingKey] is IDictionary<string, object?> nested)
            {
                current = nested;
            }
            else
            {
                var newDict = new Dictionary<string, object?>(comparer);
                current[matchingKey ?? normalizedPart] = newDict;
                current = newDict;
            }
        }

        var finalPart = parts[parts.Length - 1];
        var finalNormalized = finalPart.ToLowerInvariant().Replace("_", "-");
        var finalMatchingKey = current.Keys.FirstOrDefault(k =>
            comparer.Equals(k, finalPart) ||
            comparer.Equals(k, finalNormalized) ||
            comparer.Equals(k.Replace("-", "_"), finalPart));

        current[finalMatchingKey ?? finalNormalized] = value;
    }

    private static object? ParseEnvironmentValue(string? value)
    {
        if (value == null)
        {
            return null;
        }

        // Try boolean
        if (bool.TryParse(value, out var boolValue))
        {
            return boolValue;
        }

        // Try integer
        if (int.TryParse(value, out var intValue))
        {
            return intValue;
        }

        // Try long
        if (long.TryParse(value, out var longValue))
        {
            return longValue;
        }

        // Try double
        if (double.TryParse(value, out var doubleValue))
        {
            return doubleValue;
        }

        // Return as string
        return value;
    }
}
