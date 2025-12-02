using System.Globalization;

namespace TeCLI.Localization
{
    /// <summary>
    /// Provides localized strings for CLI help text and error messages.
    /// </summary>
    public interface ILocalizationProvider
    {
        /// <summary>
        /// Gets the current culture being used for localization.
        /// </summary>
        CultureInfo CurrentCulture { get; }

        /// <summary>
        /// Gets a localized string by its resource key.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <returns>The localized string, or the key itself if not found.</returns>
        string GetString(string key);

        /// <summary>
        /// Gets a localized string with format arguments.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <param name="args">Format arguments.</param>
        /// <returns>The formatted localized string.</returns>
        string GetString(string key, params object[] args);

        /// <summary>
        /// Gets a pluralized string based on count.
        /// </summary>
        /// <param name="singularKey">The resource key for singular form.</param>
        /// <param name="pluralKey">The resource key for plural form.</param>
        /// <param name="count">The count to determine which form to use.</param>
        /// <returns>The appropriate localized string.</returns>
        string GetPluralString(string singularKey, string pluralKey, int count);

        /// <summary>
        /// Gets a pluralized string with format arguments.
        /// </summary>
        /// <param name="singularKey">The resource key for singular form.</param>
        /// <param name="pluralKey">The resource key for plural form.</param>
        /// <param name="count">The count to determine which form to use.</param>
        /// <param name="args">Format arguments.</param>
        /// <returns>The formatted pluralized string.</returns>
        string GetPluralString(string singularKey, string pluralKey, int count, params object[] args);

        /// <summary>
        /// Checks if a resource key exists.
        /// </summary>
        /// <param name="key">The resource key to check.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        bool HasKey(string key);
    }
}
