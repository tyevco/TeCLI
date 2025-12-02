using System;
using System.Collections.Generic;
using System.Linq;

namespace TeCLI.Configuration;

/// <summary>
/// Builds CLI arguments from configuration, respecting the merge strategy:
/// file < environment < CLI arguments.
/// </summary>
public class ConfigurationArgumentBuilder
{
    private readonly ConfigurationLoader _loader;

    /// <summary>
    /// Creates a new argument builder with the specified loader.
    /// </summary>
    /// <param name="loader">The configuration loader to use.</param>
    public ConfigurationArgumentBuilder(ConfigurationLoader? loader = null)
    {
        _loader = loader ?? new ConfigurationLoader();
    }

    /// <summary>
    /// Merges configuration with CLI arguments, where CLI arguments take precedence.
    /// </summary>
    /// <param name="args">Original CLI arguments.</param>
    /// <param name="configuration">The loaded configuration.</param>
    /// <param name="commandName">Optional command name for command-specific config.</param>
    /// <returns>Merged arguments array.</returns>
    public string[] MergeWithArguments(
        string[] args,
        IDictionary<string, object?> configuration,
        string? commandName = null)
    {
        // Get command-specific configuration if command name provided
        var effectiveConfig = commandName != null
            ? _loader.GetCommandConfiguration(configuration, commandName)
            : configuration;

        // Parse existing arguments to find what's already specified
        var existingOptions = ParseExistingOptions(args);

        // Build arguments from config for options not already specified
        var configArgs = BuildConfigArguments(effectiveConfig, existingOptions);

        // Merge: config args first, then original args (original takes precedence)
        var result = new List<string>(configArgs);
        result.AddRange(args);

        return result.ToArray();
    }

    /// <summary>
    /// Builds CLI-style arguments from a configuration dictionary.
    /// </summary>
    /// <param name="configuration">The configuration to convert.</param>
    /// <param name="excludeOptions">Options to exclude (already provided via CLI).</param>
    /// <returns>Array of CLI arguments.</returns>
    public string[] BuildConfigArguments(
        IDictionary<string, object?> configuration,
        HashSet<string>? excludeOptions = null)
    {
        var args = new List<string>();
        excludeOptions ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in configuration)
        {
            // Skip profiles and commands sections
            if (kvp.Key.Equals("profiles", StringComparison.OrdinalIgnoreCase) ||
                kvp.Key.Equals("commands", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Skip if already provided via CLI
            var optionName = ToOptionName(kvp.Key);
            if (excludeOptions.Contains(optionName) ||
                excludeOptions.Contains(kvp.Key))
            {
                continue;
            }

            // Handle nested objects as if they were flattened options
            if (kvp.Value is IDictionary<string, object?> nested)
            {
                // Skip nested command configurations
                continue;
            }

            // Build argument based on value type
            var argValue = BuildArgumentValue(kvp.Value);
            if (argValue != null)
            {
                args.AddRange(argValue);
            }
            else if (kvp.Value is bool boolValue)
            {
                if (boolValue)
                {
                    args.Add($"--{optionName}");
                }
            }
            else if (kvp.Value != null)
            {
                args.Add($"--{optionName}");
                args.Add(kvp.Value.ToString()!);
            }
        }

        return args.ToArray();
    }

    private static HashSet<string> ParseExistingOptions(string[] args)
    {
        var options = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith("--"))
            {
                var optionName = arg.Substring(2);
                var equalsIndex = optionName.IndexOf('=');
                if (equalsIndex > 0)
                {
                    optionName = optionName.Substring(0, equalsIndex);
                }
                options.Add(optionName);
            }
            else if (arg.StartsWith("-") && arg.Length == 2)
            {
                options.Add(arg.Substring(1));
            }
        }

        return options;
    }

    private static string ToOptionName(string key)
    {
        // Convert camelCase or PascalCase to kebab-case
        var result = new List<char>();

        for (int i = 0; i < key.Length; i++)
        {
            var c = key[i];

            if (char.IsUpper(c))
            {
                if (i > 0)
                {
                    result.Add('-');
                }
                result.Add(char.ToLowerInvariant(c));
            }
            else if (c == '_')
            {
                result.Add('-');
            }
            else
            {
                result.Add(c);
            }
        }

        return new string(result.ToArray());
    }

    private static List<string>? BuildArgumentValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is List<object?> list)
        {
            // Handle array values
            var result = new List<string>();
            foreach (var item in list)
            {
                if (item != null)
                {
                    result.Add(item.ToString()!);
                }
            }
            return result.Count > 0 ? result : null;
        }

        return null;
    }
}

/// <summary>
/// Extension methods for integrating configuration with command dispatching.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Loads configuration and merges with the provided arguments.
    /// </summary>
    /// <param name="args">Original CLI arguments.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="appName">Optional application name.</param>
    /// <returns>Arguments with configuration defaults applied.</returns>
    public static string[] WithConfiguration(
        this string[] args,
        ConfigurationOptions? options = null,
        string? appName = null)
    {
        var loader = new ConfigurationLoader(options);
        var config = loader.Load(appName);
        var builder = new ConfigurationArgumentBuilder(loader);

        // Try to detect command name from args
        string? commandName = null;
        if (args.Length > 0 && !args[0].StartsWith("-"))
        {
            commandName = args[0];
        }

        return builder.MergeWithArguments(args, config, commandName);
    }

    /// <summary>
    /// Loads configuration with a specific profile.
    /// </summary>
    /// <param name="args">Original CLI arguments.</param>
    /// <param name="profileName">The profile to use.</param>
    /// <param name="appName">Optional application name.</param>
    /// <returns>Arguments with configuration defaults applied.</returns>
    public static string[] WithProfile(
        this string[] args,
        string profileName,
        string? appName = null)
    {
        var options = ConfigurationOptions.WithProfile(profileName);
        return args.WithConfiguration(options, appName);
    }

    /// <summary>
    /// Loads configuration from a specific file.
    /// </summary>
    /// <param name="args">Original CLI arguments.</param>
    /// <param name="configPath">Path to the configuration file.</param>
    /// <returns>Arguments with configuration defaults applied.</returns>
    public static string[] WithConfigFile(
        this string[] args,
        string configPath)
    {
        var options = ConfigurationOptions.FromFile(configPath);
        return args.WithConfiguration(options);
    }
}
