using System.IO;
using TeCLI.Console;
using Xunit;

namespace TeCLI.Extensions.Console.Tests;

public class ProgressIndicatorTests
{
    private StyledConsole CreateConsole(out StringWriter output)
    {
        output = new StringWriter();
        var error = new StringWriter();
        return new StyledConsole(output, error, supportsColor: false, supportsAnsi: false);
    }

    [Fact]
    public void Constructor_WritesInitialOutput()
    {
        // Arrange
        var console = CreateConsole(out var output);

        // Act
        using var progress = new ProgressIndicator(console, "Loading...");

        // Assert
        Assert.Contains("Loading...", output.ToString());
    }

    [Fact]
    public void Progress_CanBeSet()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var progress = new ProgressIndicator(console);

        // Act
        progress.Progress = 50;

        // Assert
        Assert.Equal(50, progress.Progress);
    }

    [Fact]
    public void Progress_ClampedToZero()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var progress = new ProgressIndicator(console);

        // Act
        progress.Progress = -10;

        // Assert
        Assert.Equal(0, progress.Progress);
    }

    [Fact]
    public void Progress_ClampedTo100()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var progress = new ProgressIndicator(console);

        // Act
        progress.Progress = 150;

        // Assert
        Assert.Equal(100, progress.Progress);
    }

    [Fact]
    public void Message_CanBeSet()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var progress = new ProgressIndicator(console, "Initial");

        // Act
        progress.Message = "Updated";

        // Assert
        Assert.Equal("Updated", progress.Message);
    }

    [Fact]
    public void Report_UpdatesProgress()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var progress = new ProgressIndicator(console);

        // Act
        progress.Report(75);

        // Assert
        Assert.Equal(75, progress.Progress);
    }

    [Fact]
    public void Report_WithMessage_UpdatesBoth()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var progress = new ProgressIndicator(console);

        // Act
        progress.Report(50, "Halfway there");

        // Assert
        Assert.Equal(50, progress.Progress);
        Assert.Equal("Halfway there", progress.Message);
    }

    [Fact]
    public void Complete_SetsProgressTo100()
    {
        // Arrange
        var console = CreateConsole(out var output);
        var progress = new ProgressIndicator(console, "Processing");

        // Act
        progress.Complete("Done!");

        // Assert
        var result = output.ToString();
        Assert.Contains("Done!", result);
    }

    [Fact]
    public void Complete_WithNoMessage_KeepsOriginalMessage()
    {
        // Arrange
        var console = CreateConsole(out var output);
        var progress = new ProgressIndicator(console, "Processing");

        // Act
        progress.Complete();

        // Assert
        var result = output.ToString();
        Assert.Contains("Processing", result);
    }

    [Fact]
    public void Fail_WritesErrorOutput()
    {
        // Arrange
        var console = CreateConsole(out var output);
        var progress = new ProgressIndicator(console, "Processing");

        // Act
        progress.Fail("Operation failed");

        // Assert
        var result = output.ToString();
        Assert.Contains("Operation failed", result);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        // Arrange
        var console = CreateConsole(out _);
        var progress = new ProgressIndicator(console, "Test");

        // Act & Assert (should not throw)
        progress.Dispose();
        progress.Dispose(); // Double dispose should be safe
    }

    [Fact]
    public void BarWidth_CanBeConfigured()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var progress = new ProgressIndicator(console);

        // Act
        progress.BarWidth = 50;

        // Assert
        Assert.Equal(50, progress.BarWidth);
    }

    [Fact]
    public void FilledChar_CanBeConfigured()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var progress = new ProgressIndicator(console);

        // Act
        progress.FilledChar = '#';

        // Assert
        Assert.Equal('#', progress.FilledChar);
    }

    [Fact]
    public void EmptyChar_CanBeConfigured()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var progress = new ProgressIndicator(console);

        // Act
        progress.EmptyChar = '-';

        // Assert
        Assert.Equal('-', progress.EmptyChar);
    }
}
