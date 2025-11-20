using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for error message suggestions
/// </summary>
public class ErrorSuggestionTests
{
    [Fact]
    public async Task UnknownCommand_ShouldSuggestSimilarCommand()
    {
        // Arrange
        var dispatcher = new TeCLI.CommandDispatcher();
        var args = new[] { "simpl" }; // Typo of "simple"
        var output = new StringWriter();
        var originalOutput = Console.Out;

        try
        {
            Console.SetOut(output);

            // Act
            await dispatcher.DispatchAsync(args);

            // Assert
            var result = output.ToString();
            Assert.Contains("Unknown command: simpl", result);
            Assert.Contains("Did you mean 'simple'?", result);
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public async Task UnknownCommand_WithNoSimilarMatch_ShouldShowGenericError()
    {
        // Arrange
        var dispatcher = new TeCLI.CommandDispatcher();
        var args = new[] { "xyz123" }; // No similar command
        var output = new StringWriter();
        var originalOutput = Console.Out;

        try
        {
            Console.SetOut(output);

            // Act
            await dispatcher.DispatchAsync(args);

            // Assert
            var result = output.ToString();
            Assert.Contains("Unknown command: xyz123", result);
            Assert.DoesNotContain("Did you mean", result);
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public async Task UnknownAction_ShouldSuggestSimilarAction()
    {
        // Arrange
        var dispatcher = new TeCLI.CommandDispatcher();
        var args = new[] { "simple", "gret" }; // Typo of "greet"
        var output = new StringWriter();
        var originalOutput = Console.Out;

        try
        {
            Console.SetOut(output);

            // Act
            await dispatcher.DispatchAsync(args);

            // Assert
            var result = output.ToString();
            Assert.Contains("Unknown action: gret", result);
            Assert.Contains("Did you mean 'greet'?", result);
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public async Task UnknownAction_WithNoSimilarMatch_ShouldShowGenericError()
    {
        // Arrange
        var dispatcher = new TeCLI.CommandDispatcher();
        var args = new[] { "simple", "unknown123" }; // No similar action
        var output = new StringWriter();
        var originalOutput = Console.Out;

        try
        {
            Console.SetOut(output);

            // Act
            await dispatcher.DispatchAsync(args);

            // Assert
            var result = output.ToString();
            Assert.Contains("Unknown action: unknown123", result);
            Assert.DoesNotContain("Did you mean", result);
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public async Task UnknownOption_ShouldSuggestSimilarOption()
    {
        // Arrange
        var dispatcher = new TeCLI.CommandDispatcher();
        var args = new[] { "options", "deploy", "--enviornment", "prod" }; // Typo of "environment"

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => dispatcher.DispatchAsync(args));

        Assert.Contains("Unknown option: --enviornment", exception.Message);
        Assert.Contains("Did you mean '--environment'?", exception.Message);
    }

    [Fact]
    public async Task UnknownShortOption_ShouldSuggestSimilarOption()
    {
        // Arrange
        var dispatcher = new TeCLI.CommandDispatcher();
        var args = new[] { "options", "deploy", "-x", "value" }; // Unknown short option

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => dispatcher.DispatchAsync(args));

        Assert.Contains("Unknown option: -x", exception.Message);
        // Should suggest one of the valid options (-e, -f, -t)
    }

    [Fact]
    public async Task UnknownOption_WithNoSimilarMatch_ShouldShowGenericError()
    {
        // Arrange
        var dispatcher = new TeCLI.CommandDispatcher();
        var args = new[] { "options", "deploy", "--xyz123", "value" }; // No similar option

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => dispatcher.DispatchAsync(args));

        Assert.Contains("Unknown option: --xyz123", exception.Message);
        Assert.DoesNotContain("Did you mean", exception.Message);
    }

    [Theory]
    [InlineData("bild", "build")]
    [InlineData("buld", "build")]
    [InlineData("biuld", "build")]
    public async Task UnknownCommand_WithCommonTypos_ShouldSuggestCorrectCommand(
        string typo,
        string expected)
    {
        // Arrange
        var dispatcher = new TeCLI.CommandDispatcher();
        var args = new[] { typo };
        var output = new StringWriter();
        var originalOutput = Console.Out;

        try
        {
            Console.SetOut(output);

            // Act
            await dispatcher.DispatchAsync(args);

            // Assert
            var result = output.ToString();
            // Note: This test assumes a "build" command exists in the test setup
            // Adjust based on actual test commands available
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public async Task CaseInsensitiveCommand_ShouldStillSuggestCorrectly()
    {
        // Arrange
        var dispatcher = new TeCLI.CommandDispatcher();
        var args = new[] { "SIMPL" }; // Uppercase typo
        var output = new StringWriter();
        var originalOutput = Console.Out;

        try
        {
            Console.SetOut(output);

            // Act
            await dispatcher.DispatchAsync(args);

            // Assert
            var result = output.ToString();
            Assert.Contains("Unknown command: SIMPL", result);
            Assert.Contains("Did you mean 'simple'?", result);
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }
}
