using System.Globalization;
using TeCLI.Localization;
using Xunit;

namespace TeCLI.Extensions.Localization.Tests;

public class CultureDetectionTests
{
    [Theory]
    [InlineData("--lang=fr", "fr")]
    [InlineData("--lang=de-DE", "de-DE")]
    [InlineData("--locale=es", "es")]
    [InlineData("--language=it", "it")]
    public void DetectCulture_ParsesLangArgWithEquals(string arg, string expectedCulture)
    {
        var args = new[] { arg };
        var culture = CultureDetection.DetectCulture(args);

        Assert.Equal(expectedCulture, culture.Name);
    }

    [Theory]
    [InlineData("--lang", "fr", "fr")]
    [InlineData("--locale", "de", "de")]
    [InlineData("--language", "es", "es")]
    [InlineData("-l", "it", "it")]
    public void DetectCulture_ParsesLangArgWithSpace(string flag, string value, string expectedCulture)
    {
        var args = new[] { flag, value };
        var culture = CultureDetection.DetectCulture(args);

        Assert.Equal(expectedCulture, culture.Name);
    }

    [Fact]
    public void DetectCulture_FallsBackToCurrentUICulture_WhenNoArgsOrEnv()
    {
        var culture = CultureDetection.DetectCulture(Array.Empty<string>());

        // Should return some valid culture (the current UI culture)
        Assert.NotNull(culture);
    }

    [Fact]
    public void DetectCulture_HandlesNullArgs()
    {
        var culture = CultureDetection.DetectCulture(null);

        Assert.NotNull(culture);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    [InlineData("de")]
    public void TryParseCulture_ReturnsValidCulture(string cultureName)
    {
        var culture = CultureDetection.TryParseCulture(cultureName);

        Assert.NotNull(culture);
        Assert.Equal(cultureName, culture!.Name);
    }

    [Fact]
    public void TryParseCulture_ReturnsNull_ForInvalidCulture()
    {
        var culture = CultureDetection.TryParseCulture("invalid-culture-xyz");

        Assert.Null(culture);
    }

    [Fact]
    public void TryParseCulture_ReturnsNull_ForEmptyString()
    {
        Assert.Null(CultureDetection.TryParseCulture(""));
        Assert.Null(CultureDetection.TryParseCulture(null));
    }

    [Fact]
    public void DetectCulture_IgnoresOtherArgs()
    {
        var args = new[] { "--verbose", "--output", "file.txt", "--lang=fr" };
        var culture = CultureDetection.DetectCulture(args);

        Assert.Equal("fr", culture.Name);
    }

    [Fact]
    public void DetectCulture_UsesFirstLangArg()
    {
        var args = new[] { "--lang=fr", "--lang=de" };
        var culture = CultureDetection.DetectCulture(args);

        Assert.Equal("fr", culture.Name);
    }
}
