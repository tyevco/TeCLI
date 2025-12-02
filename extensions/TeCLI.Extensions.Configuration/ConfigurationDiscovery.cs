using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TeCLI.Configuration;

/// <summary>
/// Discovers configuration files in standard locations.
/// </summary>
public class ConfigurationDiscovery
{
    private readonly ConfigurationOptions _options;

    /// <summary>
    /// Default configuration file names to search for (without extension).
    /// </summary>
    public static readonly string[] DefaultFileNames = new[]
    {
        ".teclirc",
        "tecli",
        ".tecli",
        "tecliconfig",
        ".tecliconfig"
    };

    /// <summary>
    /// Creates a new instance of the configuration discovery.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    public ConfigurationDiscovery(ConfigurationOptions? options = null)
    {
        _options = options ?? new ConfigurationOptions();
    }

    /// <summary>
    /// Discovers configuration files in order of precedence (lowest to highest).
    /// </summary>
    /// <param name="appName">Optional application name for app-specific config files.</param>
    /// <returns>List of discovered configuration file paths.</returns>
    public IEnumerable<string> DiscoverConfigurationFiles(string? appName = null)
    {
        var discoveredFiles = new List<string>();

        // 1. Global configuration (lowest precedence)
        if (_options.SearchGlobalConfig)
        {
            var globalFiles = FindGlobalConfigFiles(appName);
            discoveredFiles.AddRange(globalFiles);
        }

        // 2. User home directory configuration
        if (_options.SearchUserConfig)
        {
            var userFiles = FindUserConfigFiles(appName);
            discoveredFiles.AddRange(userFiles);
        }

        // 3. Current directory and parent directories (walking up)
        if (_options.SearchWorkingDirectoryTree)
        {
            var workingDirFiles = FindWorkingDirectoryConfigFiles(appName);
            discoveredFiles.AddRange(workingDirFiles);
        }
        else if (_options.SearchWorkingDirectory)
        {
            var currentDirFiles = FindConfigFilesInDirectory(
                Directory.GetCurrentDirectory(), appName);
            discoveredFiles.AddRange(currentDirFiles);
        }

        // 4. Explicit config file path (highest precedence)
        if (!string.IsNullOrEmpty(_options.ExplicitConfigPath) &&
            File.Exists(_options.ExplicitConfigPath))
        {
            discoveredFiles.Add(_options.ExplicitConfigPath);
        }

        return discoveredFiles.Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private IEnumerable<string> FindGlobalConfigFiles(string? appName)
    {
        var globalPaths = new List<string>();

        // Unix-style /etc
        var etcPath = "/etc";
        if (Directory.Exists(etcPath))
        {
            globalPaths.AddRange(FindConfigFilesInDirectory(etcPath, appName));

            if (!string.IsNullOrEmpty(appName))
            {
                var appEtcPath = Path.Combine(etcPath, appName);
                if (Directory.Exists(appEtcPath))
                {
                    globalPaths.AddRange(FindConfigFilesInDirectory(appEtcPath, appName));
                }
            }
        }

        // Windows ProgramData
        var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        if (!string.IsNullOrEmpty(programData) && Directory.Exists(programData))
        {
            if (!string.IsNullOrEmpty(appName))
            {
                var appDataPath = Path.Combine(programData, appName);
                if (Directory.Exists(appDataPath))
                {
                    globalPaths.AddRange(FindConfigFilesInDirectory(appDataPath, appName));
                }
            }
        }

        return globalPaths;
    }

    private IEnumerable<string> FindUserConfigFiles(string? appName)
    {
        var userPaths = new List<string>();

        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(homeDir) || !Directory.Exists(homeDir))
        {
            return userPaths;
        }

        // Direct home directory files
        userPaths.AddRange(FindConfigFilesInDirectory(homeDir, appName));

        // XDG config directory (~/.config)
        var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (string.IsNullOrEmpty(xdgConfigHome))
        {
            xdgConfigHome = Path.Combine(homeDir, ".config");
        }

        if (Directory.Exists(xdgConfigHome))
        {
            userPaths.AddRange(FindConfigFilesInDirectory(xdgConfigHome, appName));

            if (!string.IsNullOrEmpty(appName))
            {
                var appConfigPath = Path.Combine(xdgConfigHome, appName);
                if (Directory.Exists(appConfigPath))
                {
                    userPaths.AddRange(FindConfigFilesInDirectory(appConfigPath, appName));
                }
            }
        }

        // Windows AppData
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrEmpty(appDataPath) && Directory.Exists(appDataPath) &&
            !string.IsNullOrEmpty(appName))
        {
            var appPath = Path.Combine(appDataPath, appName);
            if (Directory.Exists(appPath))
            {
                userPaths.AddRange(FindConfigFilesInDirectory(appPath, appName));
            }
        }

        return userPaths;
    }

    private IEnumerable<string> FindWorkingDirectoryConfigFiles(string? appName)
    {
        var files = new List<string>();
        var currentDir = Directory.GetCurrentDirectory();
        var visitedDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Walk up the directory tree
        var dir = currentDir;
        var pathsToAdd = new List<string>();

        while (!string.IsNullOrEmpty(dir) && !visitedDirs.Contains(dir))
        {
            visitedDirs.Add(dir);

            var dirFiles = FindConfigFilesInDirectory(dir, appName).ToList();
            if (dirFiles.Any())
            {
                // Insert at beginning (lower precedence first)
                pathsToAdd.InsertRange(0, dirFiles);
            }

            var parent = Directory.GetParent(dir);
            if (parent == null)
            {
                break;
            }

            dir = parent.FullName;

            // Safety limit
            if (visitedDirs.Count > 50)
            {
                break;
            }
        }

        files.AddRange(pathsToAdd);
        return files;
    }

    private IEnumerable<string> FindConfigFilesInDirectory(string directory, string? appName)
    {
        var files = new List<string>();

        if (!Directory.Exists(directory))
        {
            return files;
        }

        var extensions = _options.SupportedExtensions;
        var fileNames = GetFileNamesToSearch(appName);

        foreach (var fileName in fileNames)
        {
            // Check for extensionless files (like .teclirc)
            var noExtPath = Path.Combine(directory, fileName);
            if (File.Exists(noExtPath))
            {
                files.Add(noExtPath);
            }

            // Check for files with extensions
            foreach (var ext in extensions)
            {
                var withExtPath = Path.Combine(directory, fileName + ext);
                if (File.Exists(withExtPath))
                {
                    files.Add(withExtPath);
                }
            }
        }

        return files;
    }

    private IEnumerable<string> GetFileNamesToSearch(string? appName)
    {
        var names = new List<string>(DefaultFileNames);

        if (!string.IsNullOrEmpty(appName))
        {
            var lowerAppName = appName.ToLowerInvariant();
            names.InsertRange(0, new[]
            {
                $".{lowerAppName}rc",
                lowerAppName,
                $".{lowerAppName}",
                $"{lowerAppName}config",
                $".{lowerAppName}config",
                $"{lowerAppName}.config"
            });
        }

        if (_options.AdditionalFileNames != null)
        {
            names.AddRange(_options.AdditionalFileNames);
        }

        return names.Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
