using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace TeCLI.Localization
{
    /// <summary>
    /// A localization provider that uses .NET resource files (.resx).
    /// </summary>
    public class ResourceLocalizationProvider : ILocalizationProvider
    {
        private readonly ResourceManager _resourceManager;
        private CultureInfo _currentCulture;

        /// <inheritdoc/>
        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set => _currentCulture = value ?? CultureInfo.CurrentUICulture;
        }

        /// <summary>
        /// Creates a new ResourceLocalizationProvider using the specified ResourceManager.
        /// </summary>
        /// <param name="resourceManager">The ResourceManager to use for string lookup.</param>
        /// <param name="culture">The culture to use. If null, uses CurrentUICulture.</param>
        public ResourceLocalizationProvider(ResourceManager resourceManager, CultureInfo? culture = null)
        {
            _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
            _currentCulture = culture ?? CultureInfo.CurrentUICulture;
        }

        /// <summary>
        /// Creates a new ResourceLocalizationProvider using the specified resource type.
        /// </summary>
        /// <param name="resourceType">The type containing the resources (typically the generated Resources class).</param>
        /// <param name="culture">The culture to use. If null, uses CurrentUICulture.</param>
        public ResourceLocalizationProvider(Type resourceType, CultureInfo? culture = null)
        {
            if (resourceType == null)
                throw new ArgumentNullException(nameof(resourceType));

            _resourceManager = new ResourceManager(resourceType);
            _currentCulture = culture ?? CultureInfo.CurrentUICulture;
        }

        /// <summary>
        /// Creates a new ResourceLocalizationProvider using the specified base name and assembly.
        /// </summary>
        /// <param name="baseName">The root name of the resource file (e.g., "MyApp.Resources.Strings").</param>
        /// <param name="assembly">The assembly containing the resources.</param>
        /// <param name="culture">The culture to use. If null, uses CurrentUICulture.</param>
        public ResourceLocalizationProvider(string baseName, System.Reflection.Assembly assembly, CultureInfo? culture = null)
        {
            if (string.IsNullOrEmpty(baseName))
                throw new ArgumentNullException(nameof(baseName));
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            _resourceManager = new ResourceManager(baseName, assembly);
            _currentCulture = culture ?? CultureInfo.CurrentUICulture;
        }

        /// <inheritdoc/>
        public string GetString(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;

            try
            {
                var value = _resourceManager.GetString(key, _currentCulture);
                return value ?? key;
            }
            catch (MissingManifestResourceException)
            {
                return key;
            }
        }

        /// <inheritdoc/>
        public string GetString(string key, params object[] args)
        {
            var format = GetString(key);
            if (args == null || args.Length == 0)
                return format;

            try
            {
                return string.Format(_currentCulture, format, args);
            }
            catch (FormatException)
            {
                return format;
            }
        }

        /// <inheritdoc/>
        public string GetPluralString(string singularKey, string pluralKey, int count)
        {
            // Simple English-style pluralization: 1 = singular, anything else = plural
            // For more complex pluralization rules, override this method
            return GetString(count == 1 ? singularKey : pluralKey);
        }

        /// <inheritdoc/>
        public string GetPluralString(string singularKey, string pluralKey, int count, params object[] args)
        {
            var format = GetPluralString(singularKey, pluralKey, count);
            if (args == null || args.Length == 0)
                return format;

            try
            {
                return string.Format(_currentCulture, format, args);
            }
            catch (FormatException)
            {
                return format;
            }
        }

        /// <inheritdoc/>
        public bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            try
            {
                var value = _resourceManager.GetString(key, _currentCulture);
                return value != null;
            }
            catch (MissingManifestResourceException)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A composite localization provider that chains multiple providers together.
    /// Useful for combining application-specific resources with framework defaults.
    /// </summary>
    public class CompositeLocalizationProvider : ILocalizationProvider
    {
        private readonly IReadOnlyList<ILocalizationProvider> _providers;
        private CultureInfo _currentCulture;

        /// <inheritdoc/>
        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                _currentCulture = value ?? CultureInfo.CurrentUICulture;
                // Note: We don't propagate to child providers as they may have their own culture settings
            }
        }

        /// <summary>
        /// Creates a composite provider from multiple providers.
        /// Providers are checked in order, first match wins.
        /// </summary>
        /// <param name="providers">The providers to chain together.</param>
        public CompositeLocalizationProvider(params ILocalizationProvider[] providers)
        {
            _providers = providers ?? Array.Empty<ILocalizationProvider>();
            _currentCulture = CultureInfo.CurrentUICulture;
        }

        /// <summary>
        /// Creates a composite provider from a list of providers.
        /// </summary>
        /// <param name="providers">The providers to chain together.</param>
        public CompositeLocalizationProvider(IEnumerable<ILocalizationProvider> providers)
        {
            _providers = new List<ILocalizationProvider>(providers ?? Array.Empty<ILocalizationProvider>());
            _currentCulture = CultureInfo.CurrentUICulture;
        }

        /// <inheritdoc/>
        public string GetString(string key)
        {
            foreach (var provider in _providers)
            {
                if (provider.HasKey(key))
                {
                    return provider.GetString(key);
                }
            }
            return key;
        }

        /// <inheritdoc/>
        public string GetString(string key, params object[] args)
        {
            foreach (var provider in _providers)
            {
                if (provider.HasKey(key))
                {
                    return provider.GetString(key, args);
                }
            }

            if (args == null || args.Length == 0)
                return key;

            try
            {
                return string.Format(_currentCulture, key, args);
            }
            catch (FormatException)
            {
                return key;
            }
        }

        /// <inheritdoc/>
        public string GetPluralString(string singularKey, string pluralKey, int count)
        {
            // Check both keys exist in the same provider
            foreach (var provider in _providers)
            {
                if (provider.HasKey(singularKey) || provider.HasKey(pluralKey))
                {
                    return provider.GetPluralString(singularKey, pluralKey, count);
                }
            }
            return count == 1 ? singularKey : pluralKey;
        }

        /// <inheritdoc/>
        public string GetPluralString(string singularKey, string pluralKey, int count, params object[] args)
        {
            foreach (var provider in _providers)
            {
                if (provider.HasKey(singularKey) || provider.HasKey(pluralKey))
                {
                    return provider.GetPluralString(singularKey, pluralKey, count, args);
                }
            }

            var format = count == 1 ? singularKey : pluralKey;
            if (args == null || args.Length == 0)
                return format;

            try
            {
                return string.Format(_currentCulture, format, args);
            }
            catch (FormatException)
            {
                return format;
            }
        }

        /// <inheritdoc/>
        public bool HasKey(string key)
        {
            foreach (var provider in _providers)
            {
                if (provider.HasKey(key))
                    return true;
            }
            return false;
        }
    }
}
