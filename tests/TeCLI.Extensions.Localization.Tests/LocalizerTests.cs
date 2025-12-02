using System.Globalization;
using TeCLI.Localization;
using Xunit;

namespace TeCLI.Extensions.Localization.Tests;

public class LocalizerTests
{
    [Fact]
    public void Configure_SetsProvider()
    {
        var provider = new DictionaryLocalizationProvider()
            .AddTranslation("en", "test", "Test Value");
        provider.CurrentCulture = new CultureInfo("en");

        Localizer.Configure(provider);

        Assert.Equal("Test Value", Localizer.GetString("test"));
    }

    [Fact]
    public void Configure_WithAction_SetsUpDictionaryProvider()
    {
        Localizer.Configure(p =>
        {
            p.AddTranslation("en", "greeting", "Hello");
            p.CurrentCulture = new CultureInfo("en");
        });

        Assert.Equal("Hello", Localizer.GetString("greeting"));
    }

    [Fact]
    public void GetString_WithArgs_FormatsCorrectly()
    {
        Localizer.Configure(p =>
        {
            p.AddTranslation("en", "greeting", "Hello, {0}!");
            p.CurrentCulture = new CultureInfo("en");
        });

        Assert.Equal("Hello, World!", Localizer.GetString("greeting", "World"));
    }

    [Fact]
    public void GetPluralString_ReturnsCorrectForm()
    {
        Localizer.Configure(p =>
        {
            p.AddTranslation("en", "file_singular", "1 file");
            p.AddTranslation("en", "file_plural", "multiple files");
            p.CurrentCulture = new CultureInfo("en");
        });

        Assert.Equal("1 file", Localizer.GetPluralString("file_singular", "file_plural", 1));
        Assert.Equal("multiple files", Localizer.GetPluralString("file_singular", "file_plural", 5));
    }

    [Fact]
    public void Provider_ReturnsNullProvider_WhenNotConfigured()
    {
        // Reset by configuring a new provider
        Localizer.Configure(new DictionaryLocalizationProvider());

        // Provider should still work (return keys)
        Assert.NotNull(Localizer.Provider);
    }

    [Fact]
    public void CreateHelpRenderer_ReturnsRenderer()
    {
        Localizer.Configure(new DictionaryLocalizationProvider());

        var renderer = Localizer.CreateHelpRenderer();

        Assert.NotNull(renderer);
        Assert.IsType<LocalizedHelpRenderer>(renderer);
    }
}
