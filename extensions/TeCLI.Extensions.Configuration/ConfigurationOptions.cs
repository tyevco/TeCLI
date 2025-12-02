using System;
using System.Collections.Generic;

namespace TeCLI.Configuration;

/// <summary>
/// Options for configuration loading behavior.
/// </summary>
public class ConfigurationOptions
{
    /// <summary>
    /// Gets or sets whether to search for global configuration files (e.g., /etc).
    /// Default is true.
    /// </summary>
    public bool SearchGlobalConfig { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to search for user-level configuration files (e.g., ~/.config).
    /// Default is true.
    /// </summary>
    public bool SearchUserConfig { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to search the current working directory for configuration files.
    /// Default is true.
    /// </summary>
    public bool SearchWorkingDirectory { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to walk up the directory tree from the working directory.
    /// Default is true.
    /// </summary>
    public bool SearchWorkingDirectoryTree { get; set; } = true;

    /// <summary>
    /// Gets or sets an explicit configuration file path to load.
    /// This takes highest precedence when merging configurations.
    /// </summary>
    public string? ExplicitConfigPath { get; set; }

    /// <summary>
    /// Gets or sets the name of the profile to use.
    /// </summary>
    public string? ProfileName { get; set; }

    /// <summary>
    /// Gets or sets additional file names to search for (without extension).
    /// </summary>
    public IEnumerable<string>? AdditionalFileNames { get; set; }

    /// <summary>
    /// Gets or sets the supported file extensions.
    /// Default includes .json, .yaml, .yml, .toml, .ini, .cfg, .conf.
    /// </summary>
    public IEnumerable<string> SupportedExtensions { get; set; } = new[]
    {
        ".json",
        ".yaml",
        ".yml",
        ".toml",
        ".ini",
        ".cfg",
        ".conf"
    };

    /// <summary>
    /// Gets or sets whether environment variables should override configuration file values.
    /// Default is true.
    /// </summary>
    public bool EnvironmentOverridesConfig { get; set; } = true;

    /// <summary>
    /// Gets or sets the prefix for environment variables that override configuration.
    /// For example, if prefix is "MYAPP_", then MYAPP_VERBOSE=true would set "verbose" to true.
    /// </summary>
    public string? EnvironmentVariablePrefix { get; set; }

    /// <summary>
    /// Gets or sets whether to use case-insensitive key matching.
    /// Default is true.
    /// </summary>
    public bool CaseInsensitiveKeys { get; set; } = true;

    /// <summary>
    /// Gets or sets the separator used in environment variable names to represent nesting.
    /// Default is "__" (double underscore).
    /// For example, MYAPP__DEPLOY__REGION would map to deploy:region.
    /// </summary>
    public string EnvironmentVariableNestingSeparator { get; set; } = "__";

    /// <summary>
    /// Creates default configuration options.
    /// </summary>
    public static ConfigurationOptions Default => new ConfigurationOptions();

    /// <summary>
    /// Creates configuration options that only load from explicit path.
    /// </summary>
    /// <param name="path">The configuration file path.</param>
    public static ConfigurationOptions FromFile(string path) => new ConfigurationOptions
    {
        SearchGlobalConfig = false,
        SearchUserConfig = false,
        SearchWorkingDirectory = false,
        SearchWorkingDirectoryTree = false,
        ExplicitConfigPath = path
    };

    /// <summary>
    /// Creates configuration options for a specific profile.
    /// </summary>
    /// <param name="profileName">The profile name to use.</param>
    public static ConfigurationOptions WithProfile(string profileName) => new ConfigurationOptions
    {
        ProfileName = profileName
    };
}
