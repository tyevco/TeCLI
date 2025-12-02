using System.IO;
using Xunit;
using TeCLI.Console;

namespace TeCLI.Extensions.Console.Tests;

/// <summary>
/// Unit tests for ProgressContext and ProgressBar
/// </summary>
public class ProgressContextTests
{
    [Fact]
    public void Constructor_WithoutParameters_ShouldCreateWithDefaultConsole()
    {
        // Act
        var context = new ProgressContext();

        // Assert
        Assert.NotNull(context.Console);
    }

    [Fact]
    public void Constructor_WithConsole_ShouldUseProvidedConsole()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error);

        // Act
        var context = new ProgressContext(console);

        // Assert
        Assert.Same(console, context.Console);
    }

    [Fact]
    public void CreateProgressBar_ShouldReturnProgressBar()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, false, false);
        var context = new ProgressContext(console);

        // Act
        using var bar = context.CreateProgressBar("Test");

        // Assert
        Assert.NotNull(bar);
        Assert.IsType<ProgressBar>(bar);
    }

    [Fact]
    public void CreateSpinner_ShouldReturnSpinner()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, false, false);
        var context = new ProgressContext(console);

        // Act
        using var spinner = context.CreateSpinner("Test");

        // Assert
        Assert.NotNull(spinner);
        Assert.IsAssignableFrom<ISpinner>(spinner);
    }

    [Fact]
    public void CreateProgress_ShouldReturnProgressIndicator()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, false, false);
        var context = new ProgressContext(console);

        // Act
        using var indicator = context.CreateProgress("Test");

        // Assert
        Assert.NotNull(indicator);
        Assert.IsAssignableFrom<IProgressIndicator>(indicator);
    }
}

/// <summary>
/// Unit tests for ProgressBar
/// </summary>
public class ProgressBarTests
{
    private static (ProgressBar bar, StringWriter output) CreateProgressBar(string? message = null, double maxValue = 100)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, false, false);
        var bar = new ProgressBar(console, message, maxValue);
        return (bar, output);
    }

    [Fact]
    public void Constructor_ShouldSetInitialValues()
    {
        // Act
        var (bar, _) = CreateProgressBar("Loading...", 50);

        // Assert
        Assert.Equal(0, bar.Value);
        Assert.Equal(50, bar.MaxValue);
        Assert.Equal("Loading...", bar.Message);

        bar.Dispose();
    }

    [Fact]
    public void Value_WhenSet_ShouldUpdateProgress()
    {
        // Arrange
        var (bar, _) = CreateProgressBar(maxValue: 100);

        // Act
        bar.Value = 25;

        // Assert
        Assert.Equal(25, bar.Value);

        bar.Dispose();
    }

    [Fact]
    public void Value_WhenExceedsMax_ShouldClampToMax()
    {
        // Arrange
        var (bar, _) = CreateProgressBar(maxValue: 100);

        // Act
        bar.Value = 150;

        // Assert
        Assert.Equal(100, bar.Value);

        bar.Dispose();
    }

    [Fact]
    public void Value_WhenNegative_ShouldClampToZero()
    {
        // Arrange
        var (bar, _) = CreateProgressBar(maxValue: 100);

        // Act
        bar.Value = -10;

        // Assert
        Assert.Equal(0, bar.Value);

        bar.Dispose();
    }

    [Fact]
    public void Increment_ShouldAddToValue()
    {
        // Arrange
        var (bar, _) = CreateProgressBar(maxValue: 100);
        bar.Value = 10;

        // Act
        bar.Increment(5);

        // Assert
        Assert.Equal(15, bar.Value);

        bar.Dispose();
    }

    [Fact]
    public void Increment_WithDefaultAmount_ShouldAddOne()
    {
        // Arrange
        var (bar, _) = CreateProgressBar(maxValue: 100);

        // Act
        bar.Increment();

        // Assert
        Assert.Equal(1, bar.Value);

        bar.Dispose();
    }

    [Fact]
    public void Report_ShouldSetValue()
    {
        // Arrange
        var (bar, _) = CreateProgressBar(maxValue: 100);

        // Act
        bar.Report(75);

        // Assert
        Assert.Equal(75, bar.Value);

        bar.Dispose();
    }

    [Fact]
    public void Report_WithMessage_ShouldSetValueAndMessage()
    {
        // Arrange
        var (bar, _) = CreateProgressBar(maxValue: 100);

        // Act
        bar.Report(50, "Halfway there");

        // Assert
        Assert.Equal(50, bar.Value);
        Assert.Equal("Halfway there", bar.Message);

        bar.Dispose();
    }

    [Fact]
    public void MaxValue_WhenChanged_ShouldAffectPercentageCalculation()
    {
        // Arrange
        var (bar, _) = CreateProgressBar(maxValue: 100);
        bar.Value = 50; // 50% of 100

        // Act
        bar.MaxValue = 200; // Now 50 is 25% of 200

        // Assert
        Assert.Equal(50, bar.Value);
        Assert.Equal(200, bar.MaxValue);

        bar.Dispose();
    }

    [Fact]
    public void Message_WhenSet_ShouldUpdateMessage()
    {
        // Arrange
        var (bar, _) = CreateProgressBar("Initial");

        // Act
        bar.Message = "Updated";

        // Assert
        Assert.Equal("Updated", bar.Message);

        bar.Dispose();
    }

    [Fact]
    public void Complete_ShouldSetValueToMax()
    {
        // Arrange
        var (bar, _) = CreateProgressBar(maxValue: 100);
        bar.Value = 50;

        // Act
        bar.Complete();

        // Assert
        Assert.Equal(100, bar.Value);

        // Note: bar is disposed by Complete() internally via the indicator
    }

    [Fact]
    public void Complete_WithMessage_ShouldSetFinalMessage()
    {
        // Arrange
        var (bar, output) = CreateProgressBar("Processing", maxValue: 100);

        // Act
        bar.Complete("All done!");

        // Assert
        var text = output.ToString();
        Assert.Contains("All done!", text);
    }
}
