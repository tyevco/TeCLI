using System;

namespace TeCLI.Console;

/// <summary>
/// Represents styling options for console text output.
/// </summary>
public readonly struct ConsoleStyle : IEquatable<ConsoleStyle>
{
    /// <summary>
    /// Gets the foreground color, or null for default.
    /// </summary>
    public ConsoleColor? Foreground { get; }

    /// <summary>
    /// Gets the background color, or null for default.
    /// </summary>
    public ConsoleColor? Background { get; }

    /// <summary>
    /// Gets a value indicating whether the text should be bold.
    /// </summary>
    public bool Bold { get; }

    /// <summary>
    /// Gets a value indicating whether the text should be dim.
    /// </summary>
    public bool Dim { get; }

    /// <summary>
    /// Gets a value indicating whether the text should be italic.
    /// </summary>
    public bool Italic { get; }

    /// <summary>
    /// Gets a value indicating whether the text should be underlined.
    /// </summary>
    public bool Underline { get; }

    /// <summary>
    /// Gets a value indicating whether the text should blink.
    /// </summary>
    public bool Blink { get; }

    /// <summary>
    /// Gets a value indicating whether foreground and background should be inverted.
    /// </summary>
    public bool Inverse { get; }

    /// <summary>
    /// Gets a value indicating whether the text should be struck through.
    /// </summary>
    public bool Strikethrough { get; }

    /// <summary>
    /// Creates a new console style with the specified options.
    /// </summary>
    public ConsoleStyle(
        ConsoleColor? foreground = null,
        ConsoleColor? background = null,
        bool bold = false,
        bool dim = false,
        bool italic = false,
        bool underline = false,
        bool blink = false,
        bool inverse = false,
        bool strikethrough = false)
    {
        Foreground = foreground;
        Background = background;
        Bold = bold;
        Dim = dim;
        Italic = italic;
        Underline = underline;
        Blink = blink;
        Inverse = inverse;
        Strikethrough = strikethrough;
    }

    /// <summary>
    /// Gets the default (no styling) style.
    /// </summary>
    public static ConsoleStyle Default => new ConsoleStyle();

    /// <summary>
    /// Creates a style with the specified foreground color.
    /// </summary>
    public static ConsoleStyle Color(ConsoleColor color) => new ConsoleStyle(foreground: color);

    /// <summary>
    /// Creates a style from a foreground and background color.
    /// </summary>
    public static ConsoleStyle Colors(ConsoleColor foreground, ConsoleColor background) =>
        new ConsoleStyle(foreground: foreground, background: background);

    /// <summary>
    /// Returns a new style with bold enabled.
    /// </summary>
    public ConsoleStyle WithBold() => new ConsoleStyle(Foreground, Background, true, Dim, Italic, Underline, Blink, Inverse, Strikethrough);

    /// <summary>
    /// Returns a new style with dim enabled.
    /// </summary>
    public ConsoleStyle WithDim() => new ConsoleStyle(Foreground, Background, Bold, true, Italic, Underline, Blink, Inverse, Strikethrough);

    /// <summary>
    /// Returns a new style with italic enabled.
    /// </summary>
    public ConsoleStyle WithItalic() => new ConsoleStyle(Foreground, Background, Bold, Dim, true, Underline, Blink, Inverse, Strikethrough);

    /// <summary>
    /// Returns a new style with underline enabled.
    /// </summary>
    public ConsoleStyle WithUnderline() => new ConsoleStyle(Foreground, Background, Bold, Dim, Italic, true, Blink, Inverse, Strikethrough);

    /// <summary>
    /// Returns a new style with the specified foreground color.
    /// </summary>
    public ConsoleStyle WithForeground(ConsoleColor color) => new ConsoleStyle(color, Background, Bold, Dim, Italic, Underline, Blink, Inverse, Strikethrough);

    /// <summary>
    /// Returns a new style with the specified background color.
    /// </summary>
    public ConsoleStyle WithBackground(ConsoleColor color) => new ConsoleStyle(Foreground, color, Bold, Dim, Italic, Underline, Blink, Inverse, Strikethrough);

    // Pre-defined styles for common use cases
    /// <summary>Success style (green text).</summary>
    public static ConsoleStyle Success => new ConsoleStyle(foreground: ConsoleColor.Green);

    /// <summary>Warning style (yellow text).</summary>
    public static ConsoleStyle Warning => new ConsoleStyle(foreground: ConsoleColor.Yellow);

    /// <summary>Error style (red text).</summary>
    public static ConsoleStyle Error => new ConsoleStyle(foreground: ConsoleColor.Red);

    /// <summary>Info style (cyan text).</summary>
    public static ConsoleStyle Info => new ConsoleStyle(foreground: ConsoleColor.Cyan);

    /// <summary>Debug/dim style (dark gray text).</summary>
    public static ConsoleStyle Debug => new ConsoleStyle(foreground: ConsoleColor.DarkGray);

    /// <summary>Bold style.</summary>
    public static ConsoleStyle BoldStyle => new ConsoleStyle(bold: true);

    /// <summary>Underline style.</summary>
    public static ConsoleStyle UnderlineStyle => new ConsoleStyle(underline: true);

    /// <inheritdoc />
    public bool Equals(ConsoleStyle other) =>
        Foreground == other.Foreground &&
        Background == other.Background &&
        Bold == other.Bold &&
        Dim == other.Dim &&
        Italic == other.Italic &&
        Underline == other.Underline &&
        Blink == other.Blink &&
        Inverse == other.Inverse &&
        Strikethrough == other.Strikethrough;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ConsoleStyle other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + Foreground.GetHashCode();
            hash = hash * 31 + Background.GetHashCode();
            hash = hash * 31 + Bold.GetHashCode();
            hash = hash * 31 + Dim.GetHashCode();
            hash = hash * 31 + Italic.GetHashCode();
            hash = hash * 31 + Underline.GetHashCode();
            hash = hash * 31 + Blink.GetHashCode();
            hash = hash * 31 + Inverse.GetHashCode();
            hash = hash * 31 + Strikethrough.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Determines whether two styles are equal.
    /// </summary>
    public static bool operator ==(ConsoleStyle left, ConsoleStyle right) => left.Equals(right);

    /// <summary>
    /// Determines whether two styles are not equal.
    /// </summary>
    public static bool operator !=(ConsoleStyle left, ConsoleStyle right) => !left.Equals(right);
}
