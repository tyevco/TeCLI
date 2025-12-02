using System.IO;
using TeCLI.Console;
using Xunit;

namespace TeCLI.Extensions.Console.Tests;

public class StyledConsoleTests
{
    [Fact]
    public void Write_WritesTextToOutput()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: false, supportsAnsi: false);

        // Act
        console.Write("Hello");

        // Assert
        Assert.Equal("Hello", output.ToString());
    }

    [Fact]
    public void WriteLine_WritesTextWithNewline()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: false, supportsAnsi: false);

        // Act
        console.WriteLine("Hello");

        // Assert
        Assert.Equal("Hello" + Environment.NewLine, output.ToString());
    }

    [Fact]
    public void WriteLine_WithNoText_WritesNewlineOnly()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: false, supportsAnsi: false);

        // Act
        console.WriteLine();

        // Assert
        Assert.Equal(Environment.NewLine, output.ToString());
    }

    [Fact]
    public void WriteError_WritesToErrorStream()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: false, supportsAnsi: false);

        // Act
        console.WriteError("Error text");

        // Assert
        Assert.Empty(output.ToString());
        Assert.Equal("Error text", error.ToString());
    }

    [Fact]
    public void WriteErrorLine_WritesToErrorStreamWithNewline()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: false, supportsAnsi: false);

        // Act
        console.WriteErrorLine("Error message");

        // Assert
        Assert.Equal("Error message" + Environment.NewLine, error.ToString());
    }

    [Fact]
    public void Write_WithStyle_WhenNoColorSupport_WritesPlainText()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: false, supportsAnsi: false);

        // Act
        console.Write("Styled text", ConsoleStyle.Success);

        // Assert
        Assert.Equal("Styled text", output.ToString());
    }

    [Fact]
    public void Write_WithStyle_WhenAnsiSupported_WritesStyledText()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: true, supportsAnsi: true);

        // Act
        console.Write("Success!", ConsoleStyle.Success);

        // Assert
        var result = output.ToString();
        Assert.Contains("Success!", result);
        Assert.Contains("\u001b[92m", result); // Green
        Assert.Contains("\u001b[0m", result);  // Reset
    }

    [Fact]
    public void WriteLine_WithStyle_WhenAnsiSupported_WritesStyledTextWithNewline()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: true, supportsAnsi: true);

        // Act
        console.WriteLine("Warning!", ConsoleStyle.Warning);

        // Assert
        var result = output.ToString();
        Assert.Contains("Warning!", result);
        Assert.Contains("\u001b[93m", result); // Yellow
        Assert.EndsWith(Environment.NewLine, result);
    }

    [Fact]
    public void WriteSuccess_WritesGreenText()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: true, supportsAnsi: true);

        // Act
        console.WriteSuccess("Operation completed");

        // Assert
        var result = output.ToString();
        Assert.Contains("Operation completed", result);
        Assert.Contains("\u001b[92m", result); // Green
    }

    [Fact]
    public void WriteWarning_WritesYellowText()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: true, supportsAnsi: true);

        // Act
        console.WriteWarning("Cache is stale");

        // Assert
        var result = output.ToString();
        Assert.Contains("Cache is stale", result);
        Assert.Contains("\u001b[93m", result); // Yellow
    }

    [Fact]
    public void WriteErrorMessage_WritesRedTextToStderr()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: true, supportsAnsi: true);

        // Act
        console.WriteErrorMessage("Connection failed");

        // Assert
        var result = error.ToString();
        Assert.Contains("Connection failed", result);
        Assert.Contains("\u001b[91m", result); // Red
        Assert.Empty(output.ToString()); // Should not write to stdout
    }

    [Fact]
    public void WriteInfo_WritesCyanText()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: true, supportsAnsi: true);

        // Act
        console.WriteInfo("Processing started");

        // Assert
        var result = output.ToString();
        Assert.Contains("Processing started", result);
        Assert.Contains("\u001b[96m", result); // Cyan
    }

    [Fact]
    public void WriteDebug_WritesDimText()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: true, supportsAnsi: true);

        // Act
        console.WriteDebug("Debug info");

        // Assert
        var result = output.ToString();
        Assert.Contains("Debug info", result);
        Assert.Contains("\u001b[90m", result); // Dark gray
    }

    [Fact]
    public void SupportsColor_ReflectsConstructorParameter()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();

        // Act
        var consoleWithColor = new StyledConsole(output, error, supportsColor: true, supportsAnsi: false);
        var consoleWithoutColor = new StyledConsole(output, error, supportsColor: false, supportsAnsi: false);

        // Assert
        Assert.True(consoleWithColor.SupportsColor);
        Assert.False(consoleWithoutColor.SupportsColor);
    }

    [Fact]
    public void SupportsAnsi_ReflectsConstructorParameter()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();

        // Act
        var consoleWithAnsi = new StyledConsole(output, error, supportsColor: true, supportsAnsi: true);
        var consoleWithoutAnsi = new StyledConsole(output, error, supportsColor: true, supportsAnsi: false);

        // Assert
        Assert.True(consoleWithAnsi.SupportsAnsi);
        Assert.False(consoleWithoutAnsi.SupportsAnsi);
    }

    [Fact]
    public void Write_WithNullText_DoesNotThrow()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: false, supportsAnsi: false);

        // Act & Assert (should not throw)
        console.Write(null);
        Assert.Empty(output.ToString());
    }

    [Fact]
    public void WriteLine_WithNullText_WritesNewlineOnly()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: false, supportsAnsi: false);

        // Act
        console.WriteLine(null);

        // Assert
        Assert.Equal(Environment.NewLine, output.ToString());
    }

    [Fact]
    public void CreateProgress_ReturnsProgressIndicator()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: false, supportsAnsi: false);

        // Act
        using var progress = console.CreateProgress("Loading...");

        // Assert
        Assert.NotNull(progress);
        Assert.IsType<ProgressIndicator>(progress);
    }

    [Fact]
    public void CreateSpinner_ReturnsSpinner()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();
        var console = new StyledConsole(output, error, supportsColor: false, supportsAnsi: false);

        // Act
        using var spinner = console.CreateSpinner("Processing...");

        // Assert
        Assert.NotNull(spinner);
        Assert.IsType<Spinner>(spinner);
    }

    [Fact]
    public void Constructor_WithNullOutput_ThrowsArgumentNullException()
    {
        // Arrange
        var error = new StringWriter();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StyledConsole(null!, error));
    }

    [Fact]
    public void Constructor_WithNullError_ThrowsArgumentNullException()
    {
        // Arrange
        var output = new StringWriter();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StyledConsole(output, null!));
    }
}
