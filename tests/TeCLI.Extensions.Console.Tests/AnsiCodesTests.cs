using TeCLI.Console;
using Xunit;

namespace TeCLI.Extensions.Console.Tests;

public class AnsiCodesTests
{
    [Fact]
    public void Reset_ContainsEscapeSequence()
    {
        // Assert
        Assert.Equal("\u001b[0m", AnsiCodes.Reset);
    }

    [Fact]
    public void Escape_IsCorrect()
    {
        // Assert
        Assert.Equal("\u001b[", AnsiCodes.Escape);
    }

    [Theory]
    [InlineData(ConsoleColor.Black, "\u001b[30m")]
    [InlineData(ConsoleColor.Red, "\u001b[91m")]
    [InlineData(ConsoleColor.Green, "\u001b[92m")]
    [InlineData(ConsoleColor.Yellow, "\u001b[93m")]
    [InlineData(ConsoleColor.Blue, "\u001b[94m")]
    [InlineData(ConsoleColor.Magenta, "\u001b[95m")]
    [InlineData(ConsoleColor.Cyan, "\u001b[96m")]
    [InlineData(ConsoleColor.White, "\u001b[97m")]
    [InlineData(ConsoleColor.DarkGray, "\u001b[90m")]
    public void GetForeground_ReturnsCorrectCode(ConsoleColor color, string expected)
    {
        // Act
        var result = AnsiCodes.GetForeground(color);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ConsoleColor.Black, "\u001b[40m")]
    [InlineData(ConsoleColor.Red, "\u001b[101m")]
    [InlineData(ConsoleColor.Green, "\u001b[102m")]
    [InlineData(ConsoleColor.Blue, "\u001b[104m")]
    [InlineData(ConsoleColor.White, "\u001b[107m")]
    public void GetBackground_ReturnsCorrectCode(ConsoleColor color, string expected)
    {
        // Act
        var result = AnsiCodes.GetBackground(color);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Styles_Bold_IsCorrect()
    {
        // Assert
        Assert.Equal("\u001b[1m", AnsiCodes.Styles.Bold);
    }

    [Fact]
    public void Styles_Dim_IsCorrect()
    {
        // Assert
        Assert.Equal("\u001b[2m", AnsiCodes.Styles.Dim);
    }

    [Fact]
    public void Styles_Italic_IsCorrect()
    {
        // Assert
        Assert.Equal("\u001b[3m", AnsiCodes.Styles.Italic);
    }

    [Fact]
    public void Styles_Underline_IsCorrect()
    {
        // Assert
        Assert.Equal("\u001b[4m", AnsiCodes.Styles.Underline);
    }

    [Fact]
    public void Styles_Strikethrough_IsCorrect()
    {
        // Assert
        Assert.Equal("\u001b[9m", AnsiCodes.Styles.Strikethrough);
    }

    [Fact]
    public void BuildStyleSequence_ForDefaultStyle_ReturnsEmpty()
    {
        // Arrange
        var style = ConsoleStyle.Default;

        // Act
        var result = AnsiCodes.BuildStyleSequence(style);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void BuildStyleSequence_ForColorStyle_ReturnsForegroundCode()
    {
        // Arrange
        var style = ConsoleStyle.Color(ConsoleColor.Red);

        // Act
        var result = AnsiCodes.BuildStyleSequence(style);

        // Assert
        Assert.Contains("\u001b[91m", result); // Red foreground
    }

    [Fact]
    public void BuildStyleSequence_ForBoldStyle_ReturnsBoldCode()
    {
        // Arrange
        var style = ConsoleStyle.BoldStyle;

        // Act
        var result = AnsiCodes.BuildStyleSequence(style);

        // Assert
        Assert.Contains("\u001b[1m", result); // Bold
    }

    [Fact]
    public void BuildStyleSequence_ForCombinedStyle_ReturnsAllCodes()
    {
        // Arrange
        var style = new ConsoleStyle(
            foreground: ConsoleColor.Green,
            background: ConsoleColor.Black,
            bold: true,
            underline: true);

        // Act
        var result = AnsiCodes.BuildStyleSequence(style);

        // Assert
        Assert.Contains("\u001b[1m", result);  // Bold
        Assert.Contains("\u001b[4m", result);  // Underline
        Assert.Contains("\u001b[92m", result); // Green foreground
        Assert.Contains("\u001b[40m", result); // Black background
    }

    [Fact]
    public void Stylize_WithNullText_ReturnsEmptyString()
    {
        // Arrange
        var style = ConsoleStyle.Success;

        // Act
        var result = AnsiCodes.Stylize(null, style);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Stylize_WithEmptyText_ReturnsEmptyString()
    {
        // Arrange
        var style = ConsoleStyle.Success;

        // Act
        var result = AnsiCodes.Stylize(string.Empty, style);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Stylize_WithDefaultStyle_ReturnsOriginalText()
    {
        // Arrange
        var text = "Hello World";
        var style = ConsoleStyle.Default;

        // Act
        var result = AnsiCodes.Stylize(text, style);

        // Assert
        Assert.Equal(text, result);
    }

    [Fact]
    public void Stylize_WithColorStyle_WrapsTextWithCodesAndReset()
    {
        // Arrange
        var text = "Success!";
        var style = ConsoleStyle.Success;

        // Act
        var result = AnsiCodes.Stylize(text, style);

        // Assert
        Assert.StartsWith("\u001b[92m", result); // Green
        Assert.Contains(text, result);
        Assert.EndsWith("\u001b[0m", result); // Reset
    }

    [Fact]
    public void Strip_WithNullText_ReturnsEmptyString()
    {
        // Act
        var result = AnsiCodes.Strip(null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Strip_WithPlainText_ReturnsOriginal()
    {
        // Arrange
        var text = "Hello World";

        // Act
        var result = AnsiCodes.Strip(text);

        // Assert
        Assert.Equal(text, result);
    }

    [Fact]
    public void Strip_WithAnsiCodes_RemovesCodes()
    {
        // Arrange
        var styledText = AnsiCodes.Stylize("Hello", ConsoleStyle.Success);

        // Act
        var result = AnsiCodes.Strip(styledText);

        // Assert
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void Strip_WithMultipleAnsiCodes_RemovesAllCodes()
    {
        // Arrange
        var text = "\u001b[1m\u001b[31mBold Red\u001b[0m normal \u001b[32mgreen\u001b[0m";

        // Act
        var result = AnsiCodes.Strip(text);

        // Assert
        Assert.Equal("Bold Red normal green", result);
    }

    [Fact]
    public void Cursor_Up_GeneratesCorrectCode()
    {
        // Act
        var result = AnsiCodes.Cursor.Up(3);

        // Assert
        Assert.Equal("\u001b[3A", result);
    }

    [Fact]
    public void Cursor_Down_GeneratesCorrectCode()
    {
        // Act
        var result = AnsiCodes.Cursor.Down(2);

        // Assert
        Assert.Equal("\u001b[2B", result);
    }

    [Fact]
    public void Cursor_Right_GeneratesCorrectCode()
    {
        // Act
        var result = AnsiCodes.Cursor.Right(5);

        // Assert
        Assert.Equal("\u001b[5C", result);
    }

    [Fact]
    public void Cursor_Left_GeneratesCorrectCode()
    {
        // Act
        var result = AnsiCodes.Cursor.Left(4);

        // Assert
        Assert.Equal("\u001b[4D", result);
    }

    [Fact]
    public void Cursor_Position_GeneratesCorrectCode()
    {
        // Act
        var result = AnsiCodes.Cursor.Position(10, 20);

        // Assert
        Assert.Equal("\u001b[10;20H", result);
    }

    [Fact]
    public void Cursor_Hide_IsCorrect()
    {
        // Assert
        Assert.Equal("\u001b[?25l", AnsiCodes.Cursor.Hide);
    }

    [Fact]
    public void Cursor_Show_IsCorrect()
    {
        // Assert
        Assert.Equal("\u001b[?25h", AnsiCodes.Cursor.Show);
    }

    [Fact]
    public void Erase_Line_IsCorrect()
    {
        // Assert
        Assert.Equal("\u001b[2K", AnsiCodes.Erase.Line);
    }

    [Fact]
    public void Erase_Screen_IsCorrect()
    {
        // Assert
        Assert.Equal("\u001b[2J", AnsiCodes.Erase.Screen);
    }
}
