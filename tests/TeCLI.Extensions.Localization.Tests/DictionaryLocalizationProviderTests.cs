using System.Globalization;
using TeCLI.Localization;
using Xunit;

namespace TeCLI.Extensions.Localization.Tests;

public class DictionaryLocalizationProviderTests
{
    [Fact]
    public void GetString_ReturnsTranslation_WhenKeyExists()
    {
        var provider = new DictionaryLocalizationProvider()
            .AddTranslation("en", "greeting", "Hello")
            .AddTranslation("fr", "greeting", "Bonjour");

        provider.CurrentCulture = new CultureInfo("en");
        Assert.Equal("Hello", provider.GetString("greeting"));

        provider.CurrentCulture = new CultureInfo("fr");
        Assert.Equal("Bonjour", provider.GetString("greeting"));
    }

    [Fact]
    public void GetString_ReturnsKey_WhenKeyNotFound()
    {
        var provider = new DictionaryLocalizationProvider();
        provider.CurrentCulture = new CultureInfo("en");

        Assert.Equal("unknown_key", provider.GetString("unknown_key"));
    }

    [Fact]
    public void GetString_FallsBackToNeutralCulture()
    {
        var provider = new DictionaryLocalizationProvider()
            .AddTranslation("fr", "greeting", "Bonjour");

        // fr-CA should fall back to fr
        provider.CurrentCulture = new CultureInfo("fr-CA");
        Assert.Equal("Bonjour", provider.GetString("greeting"));
    }

    [Fact]
    public void GetString_FallsBackToDefaultCulture()
    {
        var provider = new DictionaryLocalizationProvider("en")
            .AddTranslation("en", "greeting", "Hello");

        // German not defined, should fall back to English
        provider.CurrentCulture = new CultureInfo("de");
        Assert.Equal("Hello", provider.GetString("greeting"));
    }

    [Fact]
    public void GetString_WithArgs_FormatsCorrectly()
    {
        var provider = new DictionaryLocalizationProvider()
            .AddTranslation("en", "greeting", "Hello, {0}!");

        provider.CurrentCulture = new CultureInfo("en");
        Assert.Equal("Hello, World!", provider.GetString("greeting", "World"));
    }

    [Fact]
    public void GetPluralString_ReturnsSingular_WhenCountIsOne()
    {
        var provider = new DictionaryLocalizationProvider()
            .AddTranslation("en", "item_singular", "1 item")
            .AddTranslation("en", "item_plural", "{0} items");

        provider.CurrentCulture = new CultureInfo("en");
        Assert.Equal("1 item", provider.GetPluralString("item_singular", "item_plural", 1));
    }

    [Fact]
    public void GetPluralString_ReturnsPlural_WhenCountIsNotOne()
    {
        var provider = new DictionaryLocalizationProvider()
            .AddTranslation("en", "item_singular", "1 item")
            .AddTranslation("en", "item_plural", "{0} items");

        provider.CurrentCulture = new CultureInfo("en");
        Assert.Equal("{0} items", provider.GetPluralString("item_singular", "item_plural", 5));
    }

    [Fact]
    public void GetPluralString_WithArgs_FormatsCorrectly()
    {
        var provider = new DictionaryLocalizationProvider()
            .AddTranslation("en", "item_singular", "1 item")
            .AddTranslation("en", "item_plural", "{0} items");

        provider.CurrentCulture = new CultureInfo("en");
        Assert.Equal("5 items", provider.GetPluralString("item_singular", "item_plural", 5, 5));
    }

    [Fact]
    public void HasKey_ReturnsTrue_WhenKeyExists()
    {
        var provider = new DictionaryLocalizationProvider()
            .AddTranslation("en", "greeting", "Hello");

        provider.CurrentCulture = new CultureInfo("en");
        Assert.True(provider.HasKey("greeting"));
    }

    [Fact]
    public void HasKey_ReturnsFalse_WhenKeyNotExists()
    {
        var provider = new DictionaryLocalizationProvider();
        provider.CurrentCulture = new CultureInfo("en");

        Assert.False(provider.HasKey("unknown"));
    }

    [Fact]
    public void AddDefault_AddsToFallbackCulture()
    {
        var provider = new DictionaryLocalizationProvider("en")
            .AddDefault("greeting", "Hello");

        provider.CurrentCulture = new CultureInfo("de");
        Assert.Equal("Hello", provider.GetString("greeting"));
    }

    [Fact]
    public void AddTranslations_AddsBulkTranslations()
    {
        var translations = new Dictionary<string, string>
        {
            { "greeting", "Hallo" },
            { "farewell", "Auf Wiedersehen" }
        };

        var provider = new DictionaryLocalizationProvider()
            .AddTranslations("de", translations);

        provider.CurrentCulture = new CultureInfo("de");
        Assert.Equal("Hallo", provider.GetString("greeting"));
        Assert.Equal("Auf Wiedersehen", provider.GetString("farewell"));
    }

    [Fact]
    public void FluentApi_AllowsChaining()
    {
        var provider = new DictionaryLocalizationProvider()
            .AddTranslation("en", "a", "A")
            .AddTranslation("en", "b", "B")
            .AddDefault("c", "C");

        provider.CurrentCulture = new CultureInfo("en");
        Assert.Equal("A", provider.GetString("a"));
        Assert.Equal("B", provider.GetString("b"));
        Assert.Equal("C", provider.GetString("c"));
    }
}
