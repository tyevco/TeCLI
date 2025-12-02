using TeCLI.Testing;
using Xunit;

namespace TeCLI.Extensions.Testing.Tests;

public class TestConsoleTests
{
    [Fact]
    public void TestConsole_CapturesStdout()
    {
        // Arrange & Act
        using var console = new TestConsole();
        Console.WriteLine("Hello, World!");

        // Assert
        Assert.Contains("Hello, World!", console.Output);
        Assert.True(console.HasOutput);
    }

    [Fact]
    public void TestConsole_CapturesStderr()
    {
        // Arrange & Act
        using var console = new TestConsole();
        Console.Error.WriteLine("Error message");

        // Assert
        Assert.Contains("Error message", console.Error);
        Assert.True(console.HasError);
    }

    [Fact]
    public void TestConsole_ReturnsQueuedInput()
    {
        // Arrange
        using var console = new TestConsole();
        console.SetInput("test input");

        // Act
        var result = Console.ReadLine();

        // Assert
        Assert.Equal("test input", result);
    }

    [Fact]
    public void TestConsole_ReturnsMultipleQueuedInputs()
    {
        // Arrange
        using var console = new TestConsole();
        console.SetInputLines("line1", "line2", "line3");

        // Act
        var result1 = Console.ReadLine();
        var result2 = Console.ReadLine();
        var result3 = Console.ReadLine();

        // Assert
        Assert.Equal("line1", result1);
        Assert.Equal("line2", result2);
        Assert.Equal("line3", result3);
    }

    [Fact]
    public void TestConsole_OutputLines_SplitsCorrectly()
    {
        // Arrange & Act
        using var console = new TestConsole();
        Console.WriteLine("Line 1");
        Console.WriteLine("Line 2");
        Console.WriteLine("Line 3");

        // Assert
        var lines = console.OutputLines;
        Assert.Contains("Line 1", lines);
        Assert.Contains("Line 2", lines);
        Assert.Contains("Line 3", lines);
    }

    [Fact]
    public void TestConsole_Clear_RemovesAllCapturedOutput()
    {
        // Arrange
        using var console = new TestConsole();
        Console.WriteLine("Initial output");
        Console.Error.WriteLine("Initial error");

        // Act
        console.Clear();

        // Assert
        Assert.False(console.HasOutput);
        Assert.False(console.HasError);
        Assert.Empty(console.Output);
        Assert.Empty(console.Error);
    }

    [Fact]
    public void TestConsole_OutputContains_ReturnsTrue_WhenTextPresent()
    {
        // Arrange
        using var console = new TestConsole();
        Console.WriteLine("Hello, World!");

        // Assert
        Assert.True(console.OutputContains("World"));
        Assert.False(console.OutputContains("Goodbye"));
    }

    [Fact]
    public void TestConsole_ErrorContains_ReturnsTrue_WhenTextPresent()
    {
        // Arrange
        using var console = new TestConsole();
        Console.Error.WriteLine("Error: Something went wrong");

        // Assert
        Assert.True(console.ErrorContains("Something"));
        Assert.False(console.ErrorContains("Nothing"));
    }

    [Fact]
    public void TestConsole_RestoresOriginalStreams_OnDispose()
    {
        // Arrange
        var originalOut = Console.Out;
        var originalError = Console.Error;
        var originalIn = Console.In;

        // Act
        using (var console = new TestConsole())
        {
            // Console streams should be redirected
            Assert.NotSame(originalOut, Console.Out);
        }

        // Assert - streams should be restored
        Assert.Same(originalOut, Console.Out);
        Assert.Same(originalError, Console.Error);
        Assert.Same(originalIn, Console.In);
    }

    [Fact]
    public void TestConsole_SetInput_ReturnsSelfForChaining()
    {
        // Arrange
        using var console = new TestConsole();

        // Act
        var result = console.SetInput("test");

        // Assert
        Assert.Same(console, result);
    }

    [Fact]
    public void TestConsole_ReturnsNull_WhenNoInputQueued()
    {
        // Arrange
        using var console = new TestConsole();

        // Act
        var result = Console.ReadLine();

        // Assert
        Assert.Null(result);
    }
}
