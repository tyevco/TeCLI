using TeCLI;
using Xunit;

namespace TeCLI.Tests;

/// <summary>
/// Tests for the StringSimilarity utility class
/// </summary>
public class StringSimilarityTests
{
    [Theory]
    [InlineData("", "", 0)]
    [InlineData("cat", "cat", 0)]
    [InlineData("cat", "bat", 1)]
    [InlineData("cat", "cut", 1)]
    [InlineData("cat", "cats", 1)]
    [InlineData("build", "buld", 1)]
    [InlineData("build", "biuld", 1)]
    [InlineData("environment", "enviornment", 1)]
    [InlineData("hello", "world", 4)]
    [InlineData("build", "test", 5)]
    public void CalculateLevenshteinDistance_ShouldReturnCorrectDistance(string source, string target, int expected)
    {
        // Act
        int distance = StringSimilarity.CalculateLevenshteinDistance(source, target);

        // Assert
        Assert.Equal(expected, distance);
    }

    [Fact]
    public void CalculateLevenshteinDistance_WithNullSource_ShouldReturnTargetLength()
    {
        // Act
        int distance = StringSimilarity.CalculateLevenshteinDistance(null, "test");

        // Assert
        Assert.Equal(4, distance);
    }

    [Fact]
    public void CalculateLevenshteinDistance_WithNullTarget_ShouldReturnSourceLength()
    {
        // Act
        int distance = StringSimilarity.CalculateLevenshteinDistance("test", null);

        // Assert
        Assert.Equal(4, distance);
    }

    [Fact]
    public void FindMostSimilar_WithTypo_ShouldReturnCorrectMatch()
    {
        // Arrange
        var candidates = new[] { "build", "test", "deploy", "run" };
        var input = "buld";

        // Act
        var result = StringSimilarity.FindMostSimilar(input, candidates);

        // Assert
        Assert.Equal("build", result);
    }

    [Fact]
    public void FindMostSimilar_WithCaseInsensitive_ShouldMatch()
    {
        // Arrange
        var candidates = new[] { "Build", "Test", "Deploy" };
        var input = "bild";

        // Act
        var result = StringSimilarity.FindMostSimilar(input, candidates);

        // Assert
        Assert.Equal("Build", result);
    }

    [Fact]
    public void FindMostSimilar_WithDistanceTooLarge_ShouldReturnNull()
    {
        // Arrange
        var candidates = new[] { "build", "test", "deploy" };
        var input = "xyz";

        // Act
        var result = StringSimilarity.FindMostSimilar(input, candidates);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindMostSimilar_WithEmptyCandidates_ShouldReturnNull()
    {
        // Arrange
        var candidates = Array.Empty<string>();
        var input = "test";

        // Act
        var result = StringSimilarity.FindMostSimilar(input, candidates);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindSimilar_WithMultipleMatches_ShouldReturnSortedList()
    {
        // Arrange
        var candidates = new[] { "build", "built", "guild", "test" };
        var input = "buld";

        // Act
        var results = StringSimilarity.FindSimilar(input, candidates, maxDistance: 2, maxResults: 3);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains("build", results);
    }

    [Fact]
    public void FindSimilar_ShouldLimitResults()
    {
        // Arrange
        var candidates = new[] { "build", "built", "guild", "builds", "builder" };
        var input = "build";

        // Act
        var results = StringSimilarity.FindSimilar(input, candidates, maxDistance: 2, maxResults: 2);

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void FindSimilar_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var candidates = new[] { "test", "deploy", "run" };
        var input = "xyz";

        // Act
        var results = StringSimilarity.FindSimilar(input, candidates, maxDistance: 1);

        // Assert
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("--enviornment", new[] { "--environment", "--verbose", "--force" }, "--environment")]
    [InlineData("--envronment", new[] { "--environment", "--verbose", "--force" }, "--environment")]
    [InlineData("--vebrose", new[] { "--verbose", "--version" }, "--verbose")]
    [InlineData("--versoin", new[] { "--version", "--verbose" }, "--version")]
    public void FindMostSimilar_WithOptions_ShouldSuggestCorrectOption(
        string input,
        string[] candidates,
        string expected)
    {
        // Act
        var result = StringSimilarity.FindMostSimilar(input, candidates);

        // Assert
        Assert.Equal(expected, result);
    }
}
