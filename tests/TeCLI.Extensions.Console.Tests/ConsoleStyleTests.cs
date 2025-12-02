using TeCLI.Console;
using Xunit;

namespace TeCLI.Extensions.Console.Tests;

public class ConsoleStyleTests
{
    [Fact]
    public void Default_HasNoStyling()
    {
        // Arrange & Act
        var style = ConsoleStyle.Default;

        // Assert
        Assert.Null(style.Foreground);
        Assert.Null(style.Background);
        Assert.False(style.Bold);
        Assert.False(style.Dim);
        Assert.False(style.Italic);
        Assert.False(style.Underline);
        Assert.False(style.Blink);
        Assert.False(style.Inverse);
        Assert.False(style.Strikethrough);
    }

    [Fact]
    public void Color_CreatesForegroundStyle()
    {
        // Arrange & Act
        var style = ConsoleStyle.Color(ConsoleColor.Red);

        // Assert
        Assert.Equal(ConsoleColor.Red, style.Foreground);
        Assert.Null(style.Background);
    }

    [Fact]
    public void Colors_CreatesForegroundAndBackgroundStyle()
    {
        // Arrange & Act
        var style = ConsoleStyle.Colors(ConsoleColor.White, ConsoleColor.Blue);

        // Assert
        Assert.Equal(ConsoleColor.White, style.Foreground);
        Assert.Equal(ConsoleColor.Blue, style.Background);
    }

    [Fact]
    public void WithBold_AddsBoldToExistingStyle()
    {
        // Arrange
        var baseStyle = ConsoleStyle.Color(ConsoleColor.Green);

        // Act
        var boldStyle = baseStyle.WithBold();

        // Assert
        Assert.Equal(ConsoleColor.Green, boldStyle.Foreground);
        Assert.True(boldStyle.Bold);
    }

    [Fact]
    public void WithDim_AddsDimToExistingStyle()
    {
        // Arrange
        var baseStyle = ConsoleStyle.Default;

        // Act
        var dimStyle = baseStyle.WithDim();

        // Assert
        Assert.True(dimStyle.Dim);
    }

    [Fact]
    public void WithItalic_AddsItalicToExistingStyle()
    {
        // Arrange
        var baseStyle = ConsoleStyle.Default;

        // Act
        var italicStyle = baseStyle.WithItalic();

        // Assert
        Assert.True(italicStyle.Italic);
    }

    [Fact]
    public void WithUnderline_AddsUnderlineToExistingStyle()
    {
        // Arrange
        var baseStyle = ConsoleStyle.Default;

        // Act
        var underlineStyle = baseStyle.WithUnderline();

        // Assert
        Assert.True(underlineStyle.Underline);
    }

    [Fact]
    public void WithForeground_ChangesColor()
    {
        // Arrange
        var baseStyle = ConsoleStyle.Color(ConsoleColor.Red);

        // Act
        var newStyle = baseStyle.WithForeground(ConsoleColor.Blue);

        // Assert
        Assert.Equal(ConsoleColor.Blue, newStyle.Foreground);
    }

    [Fact]
    public void WithBackground_AddsBackgroundColor()
    {
        // Arrange
        var baseStyle = ConsoleStyle.Color(ConsoleColor.White);

        // Act
        var newStyle = baseStyle.WithBackground(ConsoleColor.DarkBlue);

        // Assert
        Assert.Equal(ConsoleColor.White, newStyle.Foreground);
        Assert.Equal(ConsoleColor.DarkBlue, newStyle.Background);
    }

    [Fact]
    public void Success_IsGreen()
    {
        // Arrange & Act
        var style = ConsoleStyle.Success;

        // Assert
        Assert.Equal(ConsoleColor.Green, style.Foreground);
    }

    [Fact]
    public void Warning_IsYellow()
    {
        // Arrange & Act
        var style = ConsoleStyle.Warning;

        // Assert
        Assert.Equal(ConsoleColor.Yellow, style.Foreground);
    }

    [Fact]
    public void Error_IsRed()
    {
        // Arrange & Act
        var style = ConsoleStyle.Error;

        // Assert
        Assert.Equal(ConsoleColor.Red, style.Foreground);
    }

    [Fact]
    public void Info_IsCyan()
    {
        // Arrange & Act
        var style = ConsoleStyle.Info;

        // Assert
        Assert.Equal(ConsoleColor.Cyan, style.Foreground);
    }

    [Fact]
    public void Debug_IsDarkGray()
    {
        // Arrange & Act
        var style = ConsoleStyle.Debug;

        // Assert
        Assert.Equal(ConsoleColor.DarkGray, style.Foreground);
    }

    [Fact]
    public void Equals_ReturnsTrueForIdenticalStyles()
    {
        // Arrange
        var style1 = new ConsoleStyle(ConsoleColor.Red, ConsoleColor.White, bold: true);
        var style2 = new ConsoleStyle(ConsoleColor.Red, ConsoleColor.White, bold: true);

        // Assert
        Assert.Equal(style1, style2);
        Assert.True(style1 == style2);
        Assert.False(style1 != style2);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentStyles()
    {
        // Arrange
        var style1 = ConsoleStyle.Color(ConsoleColor.Red);
        var style2 = ConsoleStyle.Color(ConsoleColor.Blue);

        // Assert
        Assert.NotEqual(style1, style2);
        Assert.False(style1 == style2);
        Assert.True(style1 != style2);
    }

    [Fact]
    public void GetHashCode_IsSameForEqualStyles()
    {
        // Arrange
        var style1 = new ConsoleStyle(ConsoleColor.Green, bold: true, underline: true);
        var style2 = new ConsoleStyle(ConsoleColor.Green, bold: true, underline: true);

        // Assert
        Assert.Equal(style1.GetHashCode(), style2.GetHashCode());
    }

    [Fact]
    public void ChainedModifications_PreserveAllSettings()
    {
        // Arrange & Act
        var style = ConsoleStyle.Color(ConsoleColor.Cyan)
            .WithBold()
            .WithUnderline()
            .WithBackground(ConsoleColor.Black);

        // Assert
        Assert.Equal(ConsoleColor.Cyan, style.Foreground);
        Assert.Equal(ConsoleColor.Black, style.Background);
        Assert.True(style.Bold);
        Assert.True(style.Underline);
        Assert.False(style.Dim);
        Assert.False(style.Italic);
    }
}
