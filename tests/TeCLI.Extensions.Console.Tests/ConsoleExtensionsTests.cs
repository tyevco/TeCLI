using TeCLI.Console;
using Xunit;

namespace TeCLI.Extensions.Console.Tests;

public class ConsoleExtensionsTests
{
    [Fact]
    public void Default_ReturnsStyledConsole()
    {
        // Act
        var console = ConsoleExtensions.Default;

        // Assert
        Assert.NotNull(console);
        Assert.IsType<StyledConsole>(console);
    }

    [Fact]
    public void Default_ReturnsSameInstance()
    {
        // Act
        var first = ConsoleExtensions.Default;
        var second = ConsoleExtensions.Default;

        // Assert
        Assert.Same(first, second);
    }

    [Fact]
    public void SetDefault_ChangesDefaultInstance()
    {
        // Arrange
        var original = ConsoleExtensions.Default;
        var custom = new StyledConsole(
            new StringWriter(),
            new StringWriter(),
            supportsColor: false,
            supportsAnsi: false);

        try
        {
            // Act
            ConsoleExtensions.SetDefault(custom);

            // Assert
            Assert.Same(custom, ConsoleExtensions.Default);
        }
        finally
        {
            // Cleanup - restore original
            ConsoleExtensions.SetDefault(original);
        }
    }

    [Fact]
    public void SetDefault_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ConsoleExtensions.SetDefault(null!));
    }
}

public class StyledTextExtensionsTests
{
    public StyledTextExtensionsTests()
    {
        // Ensure ANSI is enabled for extension tests
        TerminalCapabilities.SetAnsiSupport(true);
        TerminalCapabilities.SetColorSupport(true);
    }

    [Fact]
    public void Colorize_AppliesColor()
    {
        // Act
        var result = "Hello".Colorize(ConsoleColor.Red);

        // Assert
        Assert.Contains("\u001b[91m", result); // Red
        Assert.Contains("Hello", result);
        Assert.Contains("\u001b[0m", result); // Reset
    }

    [Fact]
    public void Stylize_AppliesStyle()
    {
        // Arrange
        var style = ConsoleStyle.Success.WithBold();

        // Act
        var result = "Success".Stylize(style);

        // Assert
        Assert.Contains("\u001b[1m", result);  // Bold
        Assert.Contains("\u001b[92m", result); // Green
        Assert.Contains("Success", result);
    }

    [Fact]
    public void Bold_MakesTextBold()
    {
        // Act
        var result = "Important".Bold();

        // Assert
        Assert.Contains("\u001b[1m", result);
        Assert.Contains("Important", result);
    }

    [Fact]
    public void Underline_UnderLinesText()
    {
        // Act
        var result = "Link".Underline();

        // Assert
        Assert.Contains("\u001b[4m", result);
        Assert.Contains("Link", result);
    }

    [Fact]
    public void Red_ColorsTextRed()
    {
        // Act
        var result = "Error".Red();

        // Assert
        Assert.Contains("\u001b[91m", result);
        Assert.Contains("Error", result);
    }

    [Fact]
    public void Green_ColorsTextGreen()
    {
        // Act
        var result = "Success".Green();

        // Assert
        Assert.Contains("\u001b[92m", result);
        Assert.Contains("Success", result);
    }

    [Fact]
    public void Yellow_ColorsTextYellow()
    {
        // Act
        var result = "Warning".Yellow();

        // Assert
        Assert.Contains("\u001b[93m", result);
        Assert.Contains("Warning", result);
    }

    [Fact]
    public void Cyan_ColorsTextCyan()
    {
        // Act
        var result = "Info".Cyan();

        // Assert
        Assert.Contains("\u001b[96m", result);
        Assert.Contains("Info", result);
    }

    [Fact]
    public void Blue_ColorsTextBlue()
    {
        // Act
        var result = "Blue text".Blue();

        // Assert
        Assert.Contains("\u001b[94m", result);
        Assert.Contains("Blue text", result);
    }

    [Fact]
    public void Magenta_ColorsTextMagenta()
    {
        // Act
        var result = "Magenta text".Magenta();

        // Assert
        Assert.Contains("\u001b[95m", result);
        Assert.Contains("Magenta text", result);
    }

    [Fact]
    public void White_ColorsTextWhite()
    {
        // Act
        var result = "White text".White();

        // Assert
        Assert.Contains("\u001b[97m", result);
        Assert.Contains("White text", result);
    }

    [Fact]
    public void Gray_ColorsTextGray()
    {
        // Act
        var result = "Debug".Gray();

        // Assert
        Assert.Contains("\u001b[90m", result);
        Assert.Contains("Debug", result);
    }

    [Fact]
    public void Extensions_ReturnPlainText_WhenAnsiNotSupported()
    {
        // Arrange
        TerminalCapabilities.SetAnsiSupport(false);

        try
        {
            // Act
            var result = "Plain".Red();

            // Assert
            Assert.Equal("Plain", result);
            Assert.DoesNotContain("\u001b", result);
        }
        finally
        {
            // Cleanup
            TerminalCapabilities.Refresh();
        }
    }
}
