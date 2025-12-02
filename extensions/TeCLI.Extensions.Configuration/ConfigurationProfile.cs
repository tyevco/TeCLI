using System;
using System.Collections.Generic;
using System.Linq;

namespace TeCLI.Configuration;

/// <summary>
/// Represents a named configuration profile.
/// </summary>
public class ConfigurationProfile
{
    /// <summary>
    /// Gets or sets the profile name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the profile configuration values.
    /// </summary>
    public IDictionary<string, object?> Values { get; set; } =
        new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the name of a parent profile to inherit from.
    /// </summary>
    public string? Inherits { get; set; }
}

/// <summary>
/// Manages configuration profiles and their resolution.
/// </summary>
public class ProfileResolver
{
    private readonly ConfigurationMerger _merger;

    /// <summary>
    /// Creates a new profile resolver.
    /// </summary>
    /// <param name="merger">The configuration merger to use.</param>
    public ProfileResolver(ConfigurationMerger? merger = null)
    {
        _merger = merger ?? new ConfigurationMerger();
    }

    /// <summary>
    /// Extracts profiles from a configuration dictionary.
    /// </summary>
    /// <param name="configuration">The configuration containing profiles.</param>
    /// <returns>Dictionary of profile name to profile.</returns>
    public IDictionary<string, ConfigurationProfile> ExtractProfiles(
        IDictionary<string, object?> configuration)
    {
        var profiles = new Dictionary<string, ConfigurationProfile>(StringComparer.OrdinalIgnoreCase);

        // Look for "profiles" section
        if (!configuration.TryGetValue("profiles", out var profilesSection) ||
            profilesSection is not IDictionary<string, object?> profilesDict)
        {
            return profiles;
        }

        foreach (var kvp in profilesDict)
        {
            if (kvp.Value is IDictionary<string, object?> profileData)
            {
                var profile = new ConfigurationProfile
                {
                    Name = kvp.Key,
                    Values = new Dictionary<string, object?>(
                        profileData.Where(p => !p.Key.Equals("inherits", StringComparison.OrdinalIgnoreCase))
                                  .ToDictionary(p => p.Key, p => p.Value),
                        StringComparer.OrdinalIgnoreCase)
                };

                // Check for inheritance
                if (profileData.TryGetValue("inherits", out var inherits) &&
                    inherits is string inheritsString)
                {
                    profile.Inherits = inheritsString;
                }

                profiles[kvp.Key] = profile;
            }
        }

        return profiles;
    }

    /// <summary>
    /// Resolves a profile by name, including inheritance chain.
    /// </summary>
    /// <param name="profiles">Available profiles.</param>
    /// <param name="profileName">Name of the profile to resolve.</param>
    /// <returns>The resolved profile configuration.</returns>
    public IDictionary<string, object?> ResolveProfile(
        IDictionary<string, ConfigurationProfile> profiles,
        string profileName)
    {
        var resolvedProfiles = new List<IDictionary<string, object?>>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        ResolveProfileChain(profiles, profileName, resolvedProfiles, visited);

        return _merger.Merge(resolvedProfiles);
    }

    /// <summary>
    /// Applies a profile to a base configuration.
    /// </summary>
    /// <param name="baseConfig">The base configuration without profiles.</param>
    /// <param name="configuration">The full configuration including profiles.</param>
    /// <param name="profileName">The profile name to apply.</param>
    /// <returns>Configuration with profile values applied.</returns>
    public IDictionary<string, object?> ApplyProfile(
        IDictionary<string, object?> baseConfig,
        IDictionary<string, object?> configuration,
        string? profileName)
    {
        if (string.IsNullOrEmpty(profileName))
        {
            return baseConfig;
        }

        var profiles = ExtractProfiles(configuration);

        if (!profiles.TryGetValue(profileName, out _))
        {
            // Profile not found, return base config
            return baseConfig;
        }

        var profileValues = ResolveProfile(profiles, profileName);
        return _merger.Merge(baseConfig, profileValues);
    }

    private void ResolveProfileChain(
        IDictionary<string, ConfigurationProfile> profiles,
        string profileName,
        List<IDictionary<string, object?>> resolvedProfiles,
        HashSet<string> visited)
    {
        if (visited.Contains(profileName))
        {
            // Circular reference detected, stop
            return;
        }

        if (!profiles.TryGetValue(profileName, out var profile))
        {
            // Profile not found
            return;
        }

        visited.Add(profileName);

        // First resolve parent profiles
        if (!string.IsNullOrEmpty(profile.Inherits))
        {
            ResolveProfileChain(profiles, profile.Inherits, resolvedProfiles, visited);
        }

        // Then add this profile (child overrides parent)
        resolvedProfiles.Add(profile.Values);
    }
}
