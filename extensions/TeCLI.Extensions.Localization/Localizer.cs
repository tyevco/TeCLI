using System;
using System.Globalization;
using System.Threading;

namespace TeCLI.Localization
{
    /// <summary>
    /// Static entry point for TeCLI localization.
    /// Provides a simple way to configure and access localization throughout the application.
    /// </summary>
    public static class Localizer
    {
        private static ILocalizationProvider? _provider;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the current localization provider.
        /// If not configured, returns a default provider that returns keys as-is.
        /// </summary>
        public static ILocalizationProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    lock (_lock)
                    {
                        if (_provider == null)
                        {
                            _provider = new NullLocalizationProvider();
                        }
                    }
                }
                return _provider;
            }
        }

        /// <summary>
        /// Gets or sets the current culture for localization.
        /// </summary>
        public static CultureInfo CurrentCulture
        {
            get => Provider.CurrentCulture;
            set
            {
                if (_provider != null)
                {
                    // Set culture on provider if it supports it
                    if (_provider is ResourceLocalizationProvider resProvider)
                        resProvider.CurrentCulture = value;
                    else if (_provider is DictionaryLocalizationProvider dictProvider)
                        dictProvider.CurrentCulture = value;
                }
            }
        }

        /// <summary>
        /// Configures the localization provider.
        /// Should be called early in application startup.
        /// </summary>
        /// <param name="provider">The localization provider to use.</param>
        public static void Configure(ILocalizationProvider provider)
        {
            lock (_lock)
            {
                _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            }
        }

        /// <summary>
        /// Configures localization using a resource type.
        /// </summary>
        /// <param name="resourceType">The type containing the resources (typically the generated Resources class).</param>
        /// <param name="culture">The culture to use. If null, uses current culture.</param>
        public static void Configure(Type resourceType, CultureInfo? culture = null)
        {
            Configure(new ResourceLocalizationProvider(resourceType, culture));
        }

        /// <summary>
        /// Configures localization using a dictionary provider with fluent API.
        /// </summary>
        /// <param name="configureProvider">Action to configure translations.</param>
        public static void Configure(Action<DictionaryLocalizationProvider> configureProvider)
        {
            var provider = new DictionaryLocalizationProvider();
            configureProvider(provider);
            Configure(provider);
        }

        /// <summary>
        /// Initializes localization with automatic culture detection.
        /// </summary>
        /// <param name="provider">The localization provider to use.</param>
        /// <param name="args">Command line arguments for culture detection.</param>
        public static void Initialize(ILocalizationProvider provider, string[]? args = null)
        {
            Configure(provider);
            var culture = CultureDetection.DetectCulture(args);
            CurrentCulture = culture;

            // Also set thread cultures for consistency
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        /// <summary>
        /// Gets a localized string by key.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <returns>The localized string, or the key if not found.</returns>
        public static string GetString(string key) => Provider.GetString(key);

        /// <summary>
        /// Gets a localized string with format arguments.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <param name="args">Format arguments.</param>
        /// <returns>The formatted localized string.</returns>
        public static string GetString(string key, params object[] args) => Provider.GetString(key, args);

        /// <summary>
        /// Gets a pluralized string.
        /// </summary>
        /// <param name="singularKey">The resource key for singular form.</param>
        /// <param name="pluralKey">The resource key for plural form.</param>
        /// <param name="count">The count to determine which form to use.</param>
        /// <returns>The appropriate localized string.</returns>
        public static string GetPluralString(string singularKey, string pluralKey, int count)
            => Provider.GetPluralString(singularKey, pluralKey, count);

        /// <summary>
        /// Creates a new LocalizedHelpRenderer using the current provider.
        /// </summary>
        /// <returns>A new LocalizedHelpRenderer.</returns>
        public static LocalizedHelpRenderer CreateHelpRenderer()
        {
            return new LocalizedHelpRenderer(Provider);
        }

        /// <summary>
        /// Renders localized help for a command type.
        /// </summary>
        /// <param name="commandType">The command type to render help for.</param>
        /// <param name="actionName">Optional action name to show specific action help.</param>
        public static void ShowHelp(Type commandType, string? actionName = null)
        {
            CreateHelpRenderer().RenderCommandHelp(commandType, actionName);
        }

        /// <summary>
        /// Renders localized help for a command type.
        /// </summary>
        /// <typeparam name="TCommand">The command type to render help for.</typeparam>
        /// <param name="actionName">Optional action name to show specific action help.</param>
        public static void ShowHelp<TCommand>(string? actionName = null)
        {
            ShowHelp(typeof(TCommand), actionName);
        }

        /// <summary>
        /// A null implementation that returns keys as-is.
        /// Used as default when no provider is configured.
        /// </summary>
        private class NullLocalizationProvider : ILocalizationProvider
        {
            public CultureInfo CurrentCulture { get; set; } = CultureInfo.CurrentUICulture;

            public string GetString(string key) => key;

            public string GetString(string key, params object[] args)
            {
                try
                {
                    return string.Format(CurrentCulture, key, args);
                }
                catch
                {
                    return key;
                }
            }

            public string GetPluralString(string singularKey, string pluralKey, int count)
                => count == 1 ? singularKey : pluralKey;

            public string GetPluralString(string singularKey, string pluralKey, int count, params object[] args)
            {
                var key = count == 1 ? singularKey : pluralKey;
                return GetString(key, args);
            }

            public bool HasKey(string key) => false;
        }
    }
}
