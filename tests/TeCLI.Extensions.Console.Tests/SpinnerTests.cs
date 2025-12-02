using System.IO;
using TeCLI.Console;
using Xunit;

namespace TeCLI.Extensions.Console.Tests;

public class SpinnerTests
{
    private StyledConsole CreateConsole(out StringWriter output)
    {
        output = new StringWriter();
        var error = new StringWriter();
        return new StyledConsole(output, error, supportsColor: false, supportsAnsi: false);
    }

    [Fact]
    public void Constructor_StartsSpinner()
    {
        // Arrange
        var console = CreateConsole(out _);

        // Act
        using var spinner = new Spinner(console, "Loading...");

        // Assert
        Assert.True(spinner.IsRunning);
    }

    [Fact]
    public void Message_CanBeSet()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var spinner = new Spinner(console, "Initial");

        // Act
        spinner.Message = "Updated";

        // Assert
        Assert.Equal("Updated", spinner.Message);
    }

    [Fact]
    public void Update_ChangesMessage()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var spinner = new Spinner(console, "Initial");

        // Act
        spinner.Update("New message");

        // Assert
        Assert.Equal("New message", spinner.Message);
    }

    [Fact]
    public void Stop_StopsSpinner()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var spinner = new Spinner(console, "Loading");

        // Act
        spinner.Stop();

        // Assert
        Assert.False(spinner.IsRunning);
    }

    [Fact]
    public void Start_RestartsStoppedSpinner()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var spinner = new Spinner(console, "Loading");
        spinner.Stop();

        // Act
        spinner.Start();

        // Assert
        Assert.True(spinner.IsRunning);
    }

    [Fact]
    public void Success_StopsSpinnerAndWritesMessage()
    {
        // Arrange
        var console = CreateConsole(out var output);
        var spinner = new Spinner(console, "Processing");

        // Act
        spinner.Success("Completed!");

        // Assert
        Assert.Contains("Completed!", output.ToString());
    }

    [Fact]
    public void Success_WithNoMessage_UsesOriginalMessage()
    {
        // Arrange
        var console = CreateConsole(out var output);
        var spinner = new Spinner(console, "Processing");

        // Act
        spinner.Success();

        // Assert
        Assert.Contains("Processing", output.ToString());
    }

    [Fact]
    public void Fail_StopsSpinnerAndWritesError()
    {
        // Arrange
        var console = CreateConsole(out var output);
        var spinner = new Spinner(console, "Processing");

        // Act
        spinner.Fail("Failed!");

        // Assert
        Assert.Contains("Failed!", output.ToString());
    }

    [Fact]
    public void Warn_StopsSpinnerAndWritesWarning()
    {
        // Arrange
        var console = CreateConsole(out var output);
        var spinner = new Spinner(console, "Processing");

        // Act
        spinner.Warn("Warning message");

        // Assert
        Assert.Contains("Warning message", output.ToString());
    }

    [Fact]
    public void Info_StopsSpinnerAndWritesInfo()
    {
        // Arrange
        var console = CreateConsole(out var output);
        var spinner = new Spinner(console, "Processing");

        // Act
        spinner.Info("Info message");

        // Assert
        Assert.Contains("Info message", output.ToString());
    }

    [Fact]
    public void Dispose_StopsSpinner()
    {
        // Arrange
        var console = CreateConsole(out _);
        var spinner = new Spinner(console, "Loading");

        // Act
        spinner.Dispose();

        // Assert
        Assert.False(spinner.IsRunning);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var console = CreateConsole(out _);
        var spinner = new Spinner(console, "Loading");

        // Act & Assert (should not throw)
        spinner.Dispose();
        spinner.Dispose();
        spinner.Dispose();
    }

    [Fact]
    public void Interval_CanBeConfigured()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var spinner = new Spinner(console);

        // Act
        spinner.Interval = 100;

        // Assert
        Assert.Equal(100, spinner.Interval);
    }

    [Fact]
    public void Start_WhenAlreadyRunning_DoesNothing()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var spinner = new Spinner(console, "Loading");

        // Act (call start again while already running)
        spinner.Start();

        // Assert
        Assert.True(spinner.IsRunning);
    }

    [Fact]
    public void Stop_WhenNotRunning_DoesNothing()
    {
        // Arrange
        var console = CreateConsole(out _);
        using var spinner = new Spinner(console, "Loading");
        spinner.Stop();

        // Act (call stop again while already stopped)
        spinner.Stop();

        // Assert
        Assert.False(spinner.IsRunning);
    }
}
