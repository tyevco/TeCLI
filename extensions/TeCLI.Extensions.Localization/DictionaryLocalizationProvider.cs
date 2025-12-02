using System;
using System.Collections.Generic;
using System.Globalization;

namespace TeCLI.Localization
{
    /// <summary>
    /// A simple localization provider that uses in-memory dictionaries.
    /// Useful for testing, small applications, or when you don't want to use .resx files.
    /// </summary>
    public class DictionaryLocalizationProvider : ILocalizationProvider
    {
        private readonly Dictionary<string, Dictionary<string, string>> _translations;
        private CultureInfo _currentCulture;
        private readonly string _fallbackCulture;

        /// <inheritdoc/>
        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set => _currentCulture = value ?? CultureInfo.CurrentUICulture;
        }

        /// <summary>
        /// Creates a new DictionaryLocalizationProvider.
        /// </summary>
        /// <param name="fallbackCulture">The culture name to fall back to (e.g., "en"). Default is "en".</param>
        public DictionaryLocalizationProvider(string fallbackCulture = "en")
        {
            _translations = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            _currentCulture = CultureInfo.CurrentUICulture;
            _fallbackCulture = fallbackCulture ?? "en";
        }

        /// <summary>
        /// Adds a translation for a specific culture.
        /// </summary>
        /// <param name="culture">The culture name (e.g., "en", "fr", "de").</param>
        /// <param name="key">The resource key.</param>
        /// <param name="value">The translated value.</param>
        /// <returns>This provider for fluent chaining.</returns>
        public DictionaryLocalizationProvider AddTranslation(string culture, string key, string value)
        {
            if (!_translations.TryGetValue(culture, out var dict))
            {
                dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _translations[culture] = dict;
            }
            dict[key] = value;
            return this;
        }

        /// <summary>
        /// Adds multiple translations for a specific culture.
        /// </summary>
        /// <param name="culture">The culture name (e.g., "en", "fr", "de").</param>
        /// <param name="translations">The translations to add.</param>
        /// <returns>This provider for fluent chaining.</returns>
        public DictionaryLocalizationProvider AddTranslations(string culture, IDictionary<string, string> translations)
        {
            if (translations == null)
                return this;

            foreach (var kvp in translations)
            {
                AddTranslation(culture, kvp.Key, kvp.Value);
            }
            return this;
        }

        /// <summary>
        /// Adds translations for the default fallback culture.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <param name="value">The translated value.</param>
        /// <returns>This provider for fluent chaining.</returns>
        public DictionaryLocalizationProvider AddDefault(string key, string value)
        {
            return AddTranslation(_fallbackCulture, key, value);
        }

        private string? TryGetString(string key)
        {
            // Try exact culture match (e.g., "fr-CA")
            var cultureName = _currentCulture.Name;
            if (_translations.TryGetValue(cultureName, out var dict) && dict.TryGetValue(key, out var value))
                return value;

            // Try neutral culture (e.g., "fr")
            var twoLetterName = _currentCulture.TwoLetterISOLanguageName;
            if (twoLetterName != cultureName && _translations.TryGetValue(twoLetterName, out dict) && dict.TryGetValue(key, out value))
                return value;

            // Try fallback culture
            if (_translations.TryGetValue(_fallbackCulture, out dict) && dict.TryGetValue(key, out value))
                return value;

            return null;
        }

        /// <inheritdoc/>
        public string GetString(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;

            return TryGetString(key) ?? key;
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
            return TryGetString(key) != null;
        }
    }
}
