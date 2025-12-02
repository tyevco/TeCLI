using TeCLI.Testing;
using Xunit;

namespace TeCLI.Extensions.Testing.Tests;

public class CommandResultAssertionsTests
{
    [Fact]
    public void ShouldSucceed_Passes_WhenResultIsSuccess()
    {
        // Arrange
        var result = CommandResult.Success("output");

        // Act & Assert - should not throw
        result.ShouldSucceed();
    }

    [Fact]
    public void ShouldSucceed_Throws_WhenResultHasException()
    {
        // Arrange
        var result = CommandResult.FromException(new InvalidOperationException("test error"));

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldSucceed());
    }

    [Fact]
    public void ShouldSucceed_Throws_WhenResultHasNonZeroExitCode()
    {
        // Arrange
        var result = CommandResult.Failure(1);

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldSucceed());
    }

    [Fact]
    public void ShouldFail_Passes_WhenResultHasException()
    {
        // Arrange
        var result = CommandResult.FromException(new InvalidOperationException());

        // Act & Assert - should not throw
        result.ShouldFail();
    }

    [Fact]
    public void ShouldFail_Passes_WhenResultHasNonZeroExitCode()
    {
        // Arrange
        var result = CommandResult.Failure(1);

        // Act & Assert - should not throw
        result.ShouldFail();
    }

    [Fact]
    public void ShouldFail_Throws_WhenResultIsSuccess()
    {
        // Arrange
        var result = CommandResult.Success();

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldFail());
    }

    [Fact]
    public void ShouldHaveExitCode_Passes_WhenExitCodeMatches()
    {
        // Arrange
        var result = CommandResult.Failure(42);

        // Act & Assert - should not throw
        result.ShouldHaveExitCode(42);
    }

    [Fact]
    public void ShouldHaveExitCode_Throws_WhenExitCodeDoesNotMatch()
    {
        // Arrange
        var result = CommandResult.Failure(1);

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldHaveExitCode(0));
    }

    [Fact]
    public void ShouldContainOutput_Passes_WhenOutputContainsText()
    {
        // Arrange
        var result = CommandResult.Success("Hello, World!");

        // Act & Assert - should not throw
        result.ShouldContainOutput("World");
    }

    [Fact]
    public void ShouldContainOutput_Throws_WhenOutputDoesNotContainText()
    {
        // Arrange
        var result = CommandResult.Success("Hello, World!");

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldContainOutput("Goodbye"));
    }

    [Fact]
    public void ShouldContainOutputIgnoreCase_Passes_WhenOutputContainsTextIgnoringCase()
    {
        // Arrange
        var result = CommandResult.Success("Hello, World!");

        // Act & Assert - should not throw
        result.ShouldContainOutputIgnoreCase("WORLD");
    }

    [Fact]
    public void ShouldContainOutputIgnoreCase_Throws_WhenOutputDoesNotContainText()
    {
        // Arrange
        var result = CommandResult.Success("Hello, World!");

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldContainOutputIgnoreCase("GOODBYE"));
    }

    [Fact]
    public void ShouldNotContainOutput_Passes_WhenOutputDoesNotContainText()
    {
        // Arrange
        var result = CommandResult.Success("Hello, World!");

        // Act & Assert - should not throw
        result.ShouldNotContainOutput("Goodbye");
    }

    [Fact]
    public void ShouldNotContainOutput_Throws_WhenOutputContainsText()
    {
        // Arrange
        var result = CommandResult.Success("Hello, World!");

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldNotContainOutput("World"));
    }

    [Fact]
    public void ShouldMatchOutput_Passes_WhenOutputMatchesRegex()
    {
        // Arrange
        var result = CommandResult.Success("Version: 1.2.3");

        // Act & Assert - should not throw
        result.ShouldMatchOutput(@"Version: \d+\.\d+\.\d+");
    }

    [Fact]
    public void ShouldMatchOutput_Throws_WhenOutputDoesNotMatchRegex()
    {
        // Arrange
        var result = CommandResult.Success("Version: abc");

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldMatchOutput(@"Version: \d+\.\d+\.\d+"));
    }

    [Fact]
    public void ShouldContainError_Passes_WhenErrorContainsText()
    {
        // Arrange
        var result = CommandResult.Failure(1, error: "Error: File not found");

        // Act & Assert - should not throw
        result.ShouldContainError("not found");
    }

    [Fact]
    public void ShouldContainError_Throws_WhenErrorDoesNotContainText()
    {
        // Arrange
        var result = CommandResult.Failure(1, error: "Error: File not found");

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldContainError("permission denied"));
    }

    [Fact]
    public void ShouldNotContainError_Passes_WhenErrorDoesNotContainText()
    {
        // Arrange
        var result = CommandResult.Failure(1, error: "Error: File not found");

        // Act & Assert - should not throw
        result.ShouldNotContainError("permission denied");
    }

    [Fact]
    public void ShouldNotContainError_Throws_WhenErrorContainsText()
    {
        // Arrange
        var result = CommandResult.Failure(1, error: "Error: File not found");

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldNotContainError("not found"));
    }

    [Fact]
    public void ShouldHaveNoError_Passes_WhenNoError()
    {
        // Arrange
        var result = CommandResult.Success("output", error: "");

        // Act & Assert - should not throw
        result.ShouldHaveNoError();
    }

    [Fact]
    public void ShouldHaveNoError_Throws_WhenErrorPresent()
    {
        // Arrange
        var result = CommandResult.Success("output", error: "some error");

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldHaveNoError());
    }

    [Fact]
    public void ShouldHaveNoOutput_Passes_WhenNoOutput()
    {
        // Arrange
        var result = CommandResult.Success("");

        // Act & Assert - should not throw
        result.ShouldHaveNoOutput();
    }

    [Fact]
    public void ShouldHaveNoOutput_Throws_WhenOutputPresent()
    {
        // Arrange
        var result = CommandResult.Success("some output");

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldHaveNoOutput());
    }

    [Fact]
    public void ShouldThrow_Passes_WhenCorrectExceptionType()
    {
        // Arrange
        var result = CommandResult.FromException(new InvalidOperationException("test"));

        // Act & Assert - should not throw and return the exception
        var exception = result.ShouldThrow<InvalidOperationException>();
        Assert.Equal("test", exception.Message);
    }

    [Fact]
    public void ShouldThrow_Throws_WhenNoException()
    {
        // Arrange
        var result = CommandResult.Success();

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldThrow<InvalidOperationException>());
    }

    [Fact]
    public void ShouldThrow_Throws_WhenDifferentExceptionType()
    {
        // Arrange
        var result = CommandResult.FromException(new ArgumentException("test"));

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldThrow<InvalidOperationException>());
    }

    [Fact]
    public void ShouldNotThrow_Passes_WhenNoException()
    {
        // Arrange
        var result = CommandResult.Success();

        // Act & Assert - should not throw
        result.ShouldNotThrow();
    }

    [Fact]
    public void ShouldNotThrow_Throws_WhenExceptionPresent()
    {
        // Arrange
        var result = CommandResult.FromException(new InvalidOperationException());

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldNotThrow());
    }

    [Fact]
    public void ShouldHaveOutput_Passes_WhenOutputMatches()
    {
        // Arrange
        var result = CommandResult.Success("Expected output");

        // Act & Assert - should not throw
        result.ShouldHaveOutput("Expected output");
    }

    [Fact]
    public void ShouldHaveOutput_Throws_WhenOutputDoesNotMatch()
    {
        // Arrange
        var result = CommandResult.Success("Actual output");

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldHaveOutput("Expected output"));
    }

    [Fact]
    public void ShouldCompleteWithin_Passes_WhenWithinTimeout()
    {
        // Arrange
        var result = new CommandResult("", "", 0, null, TimeSpan.FromMilliseconds(50));

        // Act & Assert - should not throw
        result.ShouldCompleteWithin(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ShouldCompleteWithin_Throws_WhenExceedsTimeout()
    {
        // Arrange
        var result = new CommandResult("", "", 0, null, TimeSpan.FromSeconds(2));

        // Act & Assert
        Assert.Throws<CommandAssertionException>(() => result.ShouldCompleteWithin(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void AssertionMethods_CanBeChained()
    {
        // Arrange
        var result = CommandResult.Success("Hello, World!", error: "");

        // Act & Assert - all should pass and chain correctly
        result
            .ShouldSucceed()
            .ShouldContainOutput("Hello")
            .ShouldNotContainOutput("Goodbye")
            .ShouldHaveNoError()
            .ShouldNotThrow();
    }

    [Fact]
    public void CustomMessage_IncludedInException()
    {
        // Arrange
        var result = CommandResult.Failure(1);
        const string customMessage = "Custom error context";

        // Act
        var exception = Assert.Throws<CommandAssertionException>(() =>
            result.ShouldSucceed(customMessage));

        // Assert
        Assert.Contains(customMessage, exception.Message);
    }
}
