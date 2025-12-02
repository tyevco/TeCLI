using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TeCLI.Localization
{
    /// <summary>
    /// Provides utilities for detecting and managing culture settings.
    /// </summary>
    public static class CultureDetection
    {
        /// <summary>
        /// Gets the culture based on environment variables and system settings.
        /// Checks in order: CLI arguments, environment variables, system culture.
        /// </summary>
        /// <param name="args">Command line arguments to check for --lang or --locale.</param>
        /// <returns>The detected culture.</returns>
        public static CultureInfo DetectCulture(string[]? args = null)
        {
            // 1. Check command line arguments
            var argCulture = GetCultureFromArgs(args);
            if (argCulture != null)
                return argCulture;

            // 2. Check environment variables (common Unix/Linux patterns)
            var envCulture = GetCultureFromEnvironment();
            if (envCulture != null)
                return envCulture;

            // 3. Fall back to system culture
            return CultureInfo.CurrentUICulture;
        }

        /// <summary>
        /// Parses culture from command line arguments.
        /// Supports: --lang=fr, --locale=fr-FR, -l fr, --language fr
        /// </summary>
        private static CultureInfo? GetCultureFromArgs(string[]? args)
        {
            if (args == null || args.Length == 0)
                return null;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                // Check for --lang=value or --locale=value format
                if (arg.StartsWith("--lang=", StringComparison.OrdinalIgnoreCase))
                    return TryParseCulture(arg.Substring(7));

                if (arg.StartsWith("--locale=", StringComparison.OrdinalIgnoreCase))
                    return TryParseCulture(arg.Substring(9));

                if (arg.StartsWith("--language=", StringComparison.OrdinalIgnoreCase))
                    return TryParseCulture(arg.Substring(11));

                // Check for --lang value or -l value format
                if ((arg == "--lang" || arg == "--locale" || arg == "--language" || arg == "-l") && i + 1 < args.Length)
                    return TryParseCulture(args[i + 1]);
            }

            return null;
        }

        /// <summary>
        /// Gets culture from environment variables.
        /// Checks: LANG, LC_ALL, LC_MESSAGES, LANGUAGE (in order)
        /// </summary>
        private static CultureInfo? GetCultureFromEnvironment()
        {
            // Common environment variables for locale
            var envVars = new[] { "LANG", "LC_ALL", "LC_MESSAGES", "LANGUAGE" };

            foreach (var envVar in envVars)
            {
                var value = Environment.GetEnvironmentVariable(envVar);
                if (!string.IsNullOrEmpty(value))
                {
                    var culture = ParseLocaleString(value);
                    if (culture != null)
                        return culture;
                }
            }

            return null;
        }

        /// <summary>
        /// Parses Unix-style locale strings like "en_US.UTF-8" or "fr_FR@euro".
        /// </summary>
        private static CultureInfo? ParseLocaleString(string locale)
        {
            if (string.IsNullOrEmpty(locale) || locale == "C" || locale == "POSIX")
                return null;

            // Remove encoding suffix (e.g., ".UTF-8")
            var atIndex = locale.IndexOf('@');
            if (atIndex >= 0)
                locale = locale.Substring(0, atIndex);

            var dotIndex = locale.IndexOf('.');
            if (dotIndex >= 0)
                locale = locale.Substring(0, dotIndex);

            // Convert underscore to hyphen (en_US -> en-US)
            locale = locale.Replace('_', '-');

            return TryParseCulture(locale);
        }

        /// <summary>
        /// Safely parses a culture string.
        /// </summary>
        public static CultureInfo? TryParseCulture(string? cultureName)
        {
            if (string.IsNullOrEmpty(cultureName))
                return null;

            try
            {
                return new CultureInfo(cultureName);
            }
            catch (CultureNotFoundException)
            {
                // Try just the language part
                var hyphenIndex = cultureName.IndexOf('-');
                if (hyphenIndex > 0)
                {
                    try
                    {
                        return new CultureInfo(cultureName.Substring(0, hyphenIndex));
                    }
                    catch (CultureNotFoundException)
                    {
                        return null;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets all available cultures that have translations in the provider.
        /// </summary>
        public static IEnumerable<CultureInfo> GetAvailableCultures(ILocalizationProvider provider, IEnumerable<string> testKeys)
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var originalCulture = provider.CurrentCulture;

            try
            {
                foreach (var culture in cultures)
                {
                    if (culture.Equals(CultureInfo.InvariantCulture))
                        continue;

                    // Test if this culture has any of the test keys
                    if (provider is ResourceLocalizationProvider resProvider)
                    {
                        resProvider.CurrentCulture = culture;
                        if (testKeys.Any(key => provider.HasKey(key)))
                        {
                            yield return culture;
                        }
                    }
                }
            }
            finally
            {
                // Restore original culture
                if (provider is ResourceLocalizationProvider resProvider2)
                {
                    resProvider2.CurrentCulture = originalCulture;
                }
            }
        }
    }
}
