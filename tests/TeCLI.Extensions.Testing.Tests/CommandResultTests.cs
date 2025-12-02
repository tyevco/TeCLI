using TeCLI.Testing;
using Xunit;

namespace TeCLI.Extensions.Testing.Tests;

public class CommandResultTests
{
    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        // Act
        var result = CommandResult.Success("output", "error", TimeSpan.FromSeconds(1));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(0, result.ExitCode);
        Assert.Null(result.Exception);
        Assert.Equal("output", result.Output);
        Assert.Equal("error", result.Error);
        Assert.Equal(TimeSpan.FromSeconds(1), result.ElapsedTime);
    }

    [Fact]
    public void Failure_CreatesFailedResult()
    {
        // Act
        var result = CommandResult.Failure(42, "output", "error");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(42, result.ExitCode);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void FromException_CreatesFailedResultWithException()
    {
        // Arrange
        var exception = new InvalidOperationException("test error");

        // Act
        var result = CommandResult.FromException(exception, "output", "error");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(1, result.ExitCode);
        Assert.Same(exception, result.Exception);
        Assert.True(result.HasException);
    }

    [Fact]
    public void HasOutput_ReturnsTrue_WhenOutputNotEmpty()
    {
        // Act
        var result = CommandResult.Success("some output");

        // Assert
        Assert.True(result.HasOutput);
    }

    [Fact]
    public void HasOutput_ReturnsFalse_WhenOutputEmpty()
    {
        // Act
        var result = CommandResult.Success("");

        // Assert
        Assert.False(result.HasOutput);
    }

    [Fact]
    public void HasError_ReturnsTrue_WhenErrorNotEmpty()
    {
        // Act
        var result = CommandResult.Success("", "some error");

        // Assert
        Assert.True(result.HasError);
    }

    [Fact]
    public void HasError_ReturnsFalse_WhenErrorEmpty()
    {
        // Act
        var result = CommandResult.Success("", "");

        // Assert
        Assert.False(result.HasError);
    }

    [Fact]
    public void OutputLines_SplitsOnNewlines()
    {
        // Act
        var result = CommandResult.Success("Line1\nLine2\nLine3");

        // Assert
        Assert.Equal(3, result.OutputLines.Length);
        Assert.Equal("Line1", result.OutputLines[0]);
        Assert.Equal("Line2", result.OutputLines[1]);
        Assert.Equal("Line3", result.OutputLines[2]);
    }

    [Fact]
    public void OutputLines_HandlesWindowsNewlines()
    {
        // Act
        var result = CommandResult.Success("Line1\r\nLine2\r\nLine3");

        // Assert
        Assert.Equal(3, result.OutputLines.Length);
    }

    [Fact]
    public void OutputLines_ReturnsEmptyArray_WhenNoOutput()
    {
        // Act
        var result = CommandResult.Success("");

        // Assert
        Assert.Empty(result.OutputLines);
    }

    [Fact]
    public void ErrorLines_SplitsOnNewlines()
    {
        // Act
        var result = CommandResult.Failure(1, error: "Error1\nError2");

        // Assert
        Assert.Equal(2, result.ErrorLines.Length);
    }

    [Fact]
    public void OutputContains_ReturnsTrue_WhenTextPresent()
    {
        // Act
        var result = CommandResult.Success("Hello, World!");

        // Assert
        Assert.True(result.OutputContains("World"));
        Assert.False(result.OutputContains("Goodbye"));
    }

    [Fact]
    public void OutputContains_WithComparison_RespectsCaseInsensitivity()
    {
        // Act
        var result = CommandResult.Success("Hello, World!");

        // Assert
        Assert.True(result.OutputContains("WORLD", StringComparison.OrdinalIgnoreCase));
        Assert.False(result.OutputContains("WORLD", StringComparison.Ordinal));
    }

    [Fact]
    public void ErrorContains_ReturnsTrue_WhenTextPresent()
    {
        // Act
        var result = CommandResult.Failure(1, error: "Error: Something went wrong");

        // Assert
        Assert.True(result.ErrorContains("Something"));
        Assert.False(result.ErrorContains("Nothing"));
    }

    [Fact]
    public void ErrorContains_WithComparison_RespectsCaseInsensitivity()
    {
        // Act
        var result = CommandResult.Failure(1, error: "Error: Something went wrong");

        // Assert
        Assert.True(result.ErrorContains("SOMETHING", StringComparison.OrdinalIgnoreCase));
        Assert.False(result.ErrorContains("SOMETHING", StringComparison.Ordinal));
    }

    [Fact]
    public void ToString_IncludesStatusAndSizes()
    {
        // Act
        var result = CommandResult.Success("Hello, World!", "");

        // Assert
        var str = result.ToString();
        Assert.Contains("Success", str);
        Assert.Contains("Output:", str);
    }

    [Fact]
    public void ToString_IncludesExceptionInfo_WhenPresent()
    {
        // Act
        var result = CommandResult.FromException(new InvalidOperationException());

        // Assert
        var str = result.ToString();
        Assert.Contains("Failed", str);
        Assert.Contains("InvalidOperationException", str);
    }

    [Fact]
    public void NullOutput_ConvertedToEmptyString()
    {
        // Act - internal constructor allows null
        var result = new CommandResult(null!, null!, 0, null, TimeSpan.Zero);

        // Assert
        Assert.Equal(string.Empty, result.Output);
        Assert.Equal(string.Empty, result.Error);
    }
}
