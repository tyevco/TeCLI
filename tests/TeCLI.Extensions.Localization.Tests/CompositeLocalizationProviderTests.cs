using System.Globalization;
using TeCLI.Localization;
using Xunit;

namespace TeCLI.Extensions.Localization.Tests;

public class CompositeLocalizationProviderTests
{
    [Fact]
    public void GetString_ReturnsFromFirstMatchingProvider()
    {
        var provider1 = new DictionaryLocalizationProvider()
            .AddTranslation("en", "greeting", "Hello from Provider 1");

        var provider2 = new DictionaryLocalizationProvider()
            .AddTranslation("en", "greeting", "Hello from Provider 2")
            .AddTranslation("en", "farewell", "Goodbye from Provider 2");

        var composite = new CompositeLocalizationProvider(provider1, provider2);
        composite.CurrentCulture = new CultureInfo("en");
        provider1.CurrentCulture = new CultureInfo("en");
        provider2.CurrentCulture = new CultureInfo("en");

        // "greeting" exists in both - should return from first
        Assert.Equal("Hello from Provider 1", composite.GetString("greeting"));

        // "farewell" only exists in second
        Assert.Equal("Goodbye from Provider 2", composite.GetString("farewell"));
    }

    [Fact]
    public void GetString_ReturnsKey_WhenNoProviderHasKey()
    {
        var provider1 = new DictionaryLocalizationProvider();
        var provider2 = new DictionaryLocalizationProvider();

        var composite = new CompositeLocalizationProvider(provider1, provider2);

        Assert.Equal("unknown_key", composite.GetString("unknown_key"));
    }

    [Fact]
    public void GetString_WithArgs_FormatsCorrectly()
    {
        var provider = new DictionaryLocalizationProvider()
            .AddTranslation("en", "greeting", "Hello, {0}!");

        var composite = new CompositeLocalizationProvider(provider);
        composite.CurrentCulture = new CultureInfo("en");
        provider.CurrentCulture = new CultureInfo("en");

        Assert.Equal("Hello, World!", composite.GetString("greeting", "World"));
    }

    [Fact]
    public void HasKey_ReturnsTrue_IfAnyProviderHasKey()
    {
        var provider1 = new DictionaryLocalizationProvider();
        var provider2 = new DictionaryLocalizationProvider()
            .AddTranslation("en", "greeting", "Hello");

        var composite = new CompositeLocalizationProvider(provider1, provider2);
        provider2.CurrentCulture = new CultureInfo("en");

        Assert.True(composite.HasKey("greeting"));
    }

    [Fact]
    public void HasKey_ReturnsFalse_IfNoProviderHasKey()
    {
        var provider1 = new DictionaryLocalizationProvider();
        var provider2 = new DictionaryLocalizationProvider();

        var composite = new CompositeLocalizationProvider(provider1, provider2);

        Assert.False(composite.HasKey("unknown"));
    }

    [Fact]
    public void GetPluralString_DelegatesToMatchingProvider()
    {
        var provider = new DictionaryLocalizationProvider()
            .AddTranslation("en", "item_singular", "1 item")
            .AddTranslation("en", "item_plural", "{0} items");

        var composite = new CompositeLocalizationProvider(provider);
        composite.CurrentCulture = new CultureInfo("en");
        provider.CurrentCulture = new CultureInfo("en");

        Assert.Equal("1 item", composite.GetPluralString("item_singular", "item_plural", 1));
        Assert.Equal("{0} items", composite.GetPluralString("item_singular", "item_plural", 5));
    }

    [Fact]
    public void Constructor_AcceptsEnumerable()
    {
        var providers = new List<ILocalizationProvider>
        {
            new DictionaryLocalizationProvider().AddTranslation("en", "a", "A"),
            new DictionaryLocalizationProvider().AddTranslation("en", "b", "B")
        };

        foreach (var p in providers)
        {
            if (p is DictionaryLocalizationProvider dp)
                dp.CurrentCulture = new CultureInfo("en");
        }

        var composite = new CompositeLocalizationProvider(providers);

        Assert.Equal("A", composite.GetString("a"));
        Assert.Equal("B", composite.GetString("b"));
    }

    [Fact]
    public void EmptyComposite_ReturnsKeys()
    {
        var composite = new CompositeLocalizationProvider();

        Assert.Equal("test_key", composite.GetString("test_key"));
        Assert.False(composite.HasKey("test_key"));
    }
}
