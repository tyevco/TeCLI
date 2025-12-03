using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeCLI.Configuration.Parsers;

namespace TeCLI.Configuration;

/// <summary>
/// Main entry point for loading CLI configuration from files.
/// Supports multiple formats, profiles, and merge strategies.
/// </summary>
public class ConfigurationLoader
{
    private readonly List<IConfigurationParser> _parsers;
    private readonly ConfigurationDiscovery _discovery;
    private readonly ConfigurationMerger _merger;
    private readonly ProfileResolver _profileResolver;
    private readonly ConfigurationOptions _options;

    /// <summary>
    /// Creates a new configuration loader with default settings.
    /// </summary>
    public ConfigurationLoader() : this(null)
    {
    }

    /// <summary>
    /// Creates a new configuration loader with the specified options.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    public ConfigurationLoader(ConfigurationOptions? options)
    {
        _options = options ?? new ConfigurationOptions();
        _parsers = new List<IConfigurationParser>
        {
            new JsonConfigurationParser(),
            new YamlConfigurationParser(),
            new TomlConfigurationParser(),
            new IniConfigurationParser()
        };
        _discovery = new ConfigurationDiscovery(_options);
        _merger = new ConfigurationMerger(_options);
        _profileResolver = new ProfileResolver(_merger);
    }

    /// <summary>
    /// Adds a custom configuration parser.
    /// </summary>
    /// <param name="parser">The parser to add.</param>
    /// <returns>This loader for method chaining.</returns>
    public ConfigurationLoader AddParser(IConfigurationParser parser)
    {
        _parsers.Add(parser);
        return this;
    }

    /// <summary>
    /// Loads and merges configuration from all discovered files.
    /// </summary>
    /// <param name="appName">Optional application name for app-specific config files.</param>
    /// <returns>The merged configuration dictionary.</returns>
    public IDictionary<string, object?> Load(string? appName = null)
    {
        var configFiles = _discovery.DiscoverConfigurationFiles(appName);
        var configurations = new List<IDictionary<string, object?>>();
        IDictionary<string, object?>? rawConfig = null;

        foreach (var filePath in configFiles)
        {
            var config = LoadFile(filePath);
            if (config != null)
            {
                configurations.Add(config);
                rawConfig = config; // Keep track of latest for profile extraction
            }
        }

        // Merge all configurations
        var merged = _merger.Merge(configurations);

        // Apply profile if specified
        if (!string.IsNullOrEmpty(_options.ProfileName) && rawConfig != null)
        {
            merged = _profileResolver.ApplyProfile(merged, rawConfig, _options.ProfileName);
        }

        // Apply environment variable overrides
        if (_options.EnvironmentOverridesConfig)
        {
            merged = _merger.MergeEnvironmentVariables(merged);
        }

        return merged;
    }

    /// <summary>
    /// Loads configuration from a specific file.
    /// </summary>
    /// <param name="filePath">Path to the configuration file.</param>
    /// <returns>The parsed configuration, or null if the file cannot be parsed.</returns>
    public IDictionary<string, object?>? LoadFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
        {
            // Try to detect format from content
            extension = DetectFormat(filePath);
        }

        var parser = _parsers.FirstOrDefault(p => p.CanParse(extension));
        if (parser == null)
        {
            return null;
        }

        try
        {
            var content = File.ReadAllText(filePath);
            return parser.Parse(content);
        }
        catch
        {
            // Failed to parse, return null
            return null;
        }
    }

    /// <summary>
    /// Gets the configuration section for a specific command.
    /// </summary>
    /// <param name="configuration">The full configuration.</param>
    /// <param name="commandName">The command name.</param>
    /// <returns>The command-specific configuration merged with root config.</returns>
    public IDictionary<string, object?> GetCommandConfiguration(
        IDictionary<string, object?> configuration,
        string commandName)
    {
        // Start with non-command configuration
        var rootConfig = configuration
            .Where(kvp => !kvp.Key.Equals("profiles", StringComparison.OrdinalIgnoreCase))
            .Where(kvp => kvp.Value is not IDictionary<string, object?> dict ||
                         !dict.ContainsKey("$command"))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);

        // Look for command-specific section
        if (configuration.TryGetValue(commandName, out var commandSection) &&
            commandSection is IDictionary<string, object?> commandDict)
        {
            return _merger.Merge(rootConfig, commandDict);
        }

        // Also check for commands section
        if (configuration.TryGetValue("commands", out var commandsSection) &&
            commandsSection is IDictionary<string, object?> commandsDict)
        {
            if (commandsDict.TryGetValue(commandName, out var cmdSection) &&
                cmdSection is IDictionary<string, object?> cmdDict)
            {
                return _merger.Merge(rootConfig, cmdDict);
            }
        }

        return rootConfig;
    }

    /// <summary>
    /// Gets a typed value from the configuration.
    /// </summary>
    /// <typeparam name="T">The expected value type.</typeparam>
    /// <param name="configuration">The configuration dictionary.</param>
    /// <param name="key">The configuration key (supports dot notation for nesting).</param>
    /// <param name="defaultValue">Default value if key not found.</param>
    /// <returns>The configuration value.</returns>
    public T GetValue<T>(IDictionary<string, object?> configuration, string key, T defaultValue = default!)
    {
        var value = GetNestedValue(configuration, key);

        if (value == null)
        {
            return defaultValue;
        }

        return ConvertValue<T>(value, defaultValue);
    }

    /// <summary>
    /// Checks if a configuration key exists.
    /// </summary>
    /// <param name="configuration">The configuration dictionary.</param>
    /// <param name="key">The configuration key (supports dot notation for nesting).</param>
    /// <returns>True if the key exists.</returns>
    public bool HasValue(IDictionary<string, object?> configuration, string key)
    {
        return GetNestedValue(configuration, key) != null;
    }

    private static object? GetNestedValue(IDictionary<string, object?> configuration, string key)
    {
        var parts = key.Split('.');
        object? current = configuration;

        foreach (var part in parts)
        {
            if (current is IDictionary<string, object?> dict)
            {
                if (!dict.TryGetValue(part, out current))
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        return current;
    }

    private static T ConvertValue<T>(object value, T defaultValue)
    {
        var targetType = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (value is T typedValue)
            {
                return typedValue;
            }

            if (underlyingType == typeof(string))
            {
                return (T)(object)value.ToString()!;
            }

            if (underlyingType == typeof(bool))
            {
                if (value is bool b) return (T)(object)b;
                if (bool.TryParse(value.ToString(), out var parsed)) return (T)(object)parsed;
            }

            if (underlyingType == typeof(int))
            {
                if (value is int i) return (T)(object)i;
                if (value is long l) return (T)(object)(int)l;
                if (int.TryParse(value.ToString(), out var parsed)) return (T)(object)parsed;
            }

            if (underlyingType == typeof(long))
            {
                if (value is long l) return (T)(object)l;
                if (value is int i) return (T)(object)(long)i;
                if (long.TryParse(value.ToString(), out var parsed)) return (T)(object)parsed;
            }

            if (underlyingType == typeof(double))
            {
                if (value is double d) return (T)(object)d;
                if (double.TryParse(value.ToString(), out var parsed)) return (T)(object)parsed;
            }

            if (underlyingType.IsEnum)
            {
                var stringValue = value.ToString();
                if (!string.IsNullOrEmpty(stringValue))
                {
                    try
                    {
                        var enumValue = Enum.Parse(underlyingType, stringValue, ignoreCase: true);
                        return (T)enumValue;
                    }
                    catch (ArgumentException)
                    {
                        // Invalid enum value, fall through to Convert
                    }
                }
            }

            // Try Convert for other types
            return (T)Convert.ChangeType(value, underlyingType);
        }
        catch
        {
            return defaultValue;
        }
    }

    private static string DetectFormat(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath).TrimStart();

            // JSON starts with { or [
            if (content.StartsWith("{") || content.StartsWith("["))
            {
                return ".json";
            }

            // TOML has [section] or key = value pattern
            if (content.Contains(" = ") || content.Contains("[") && content.Contains("]"))
            {
                var lines = content.Split('\n');
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]") && !trimmed.StartsWith("[["))
                    {
                        // Check for TOML-style assignment
                        return ".toml";
                    }
                }
            }

            // INI-style with ; comments
            if (content.Contains(";") || (content.Contains("=") && !content.Contains(":")))
            {
                return ".ini";
            }

            // Default to YAML for anything else
            return ".yaml";
        }
        catch
        {
            return ".json";
        }
    }
}
