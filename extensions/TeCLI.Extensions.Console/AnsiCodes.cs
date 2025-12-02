using System;
using System.Collections.Generic;

namespace TeCLI.Console;

/// <summary>
/// Contains ANSI escape codes for terminal styling.
/// </summary>
public static class AnsiCodes
{
    /// <summary>
    /// The escape character used to start ANSI sequences.
    /// </summary>
    public const string Escape = "\u001b[";

    /// <summary>
    /// The reset code that clears all formatting.
    /// </summary>
    public const string Reset = "\u001b[0m";

    /// <summary>
    /// Style codes for text formatting.
    /// </summary>
    public static class Styles
    {
        /// <summary>Bold text.</summary>
        public const string Bold = "\u001b[1m";
        /// <summary>Dim/faint text.</summary>
        public const string Dim = "\u001b[2m";
        /// <summary>Italic text.</summary>
        public const string Italic = "\u001b[3m";
        /// <summary>Underlined text.</summary>
        public const string Underline = "\u001b[4m";
        /// <summary>Blinking text.</summary>
        public const string Blink = "\u001b[5m";
        /// <summary>Inverted foreground/background.</summary>
        public const string Inverse = "\u001b[7m";
        /// <summary>Hidden/invisible text.</summary>
        public const string Hidden = "\u001b[8m";
        /// <summary>Strikethrough text.</summary>
        public const string Strikethrough = "\u001b[9m";

        /// <summary>Reset bold.</summary>
        public const string NoBold = "\u001b[22m";
        /// <summary>Reset dim.</summary>
        public const string NoDim = "\u001b[22m";
        /// <summary>Reset italic.</summary>
        public const string NoItalic = "\u001b[23m";
        /// <summary>Reset underline.</summary>
        public const string NoUnderline = "\u001b[24m";
        /// <summary>Reset blink.</summary>
        public const string NoBlink = "\u001b[25m";
        /// <summary>Reset inverse.</summary>
        public const string NoInverse = "\u001b[27m";
        /// <summary>Reset hidden.</summary>
        public const string NoHidden = "\u001b[28m";
        /// <summary>Reset strikethrough.</summary>
        public const string NoStrikethrough = "\u001b[29m";
    }

    /// <summary>
    /// Foreground color codes.
    /// </summary>
    public static class Foreground
    {
        /// <summary>Black foreground.</summary>
        public const string Black = "\u001b[30m";
        /// <summary>Dark red foreground.</summary>
        public const string DarkRed = "\u001b[31m";
        /// <summary>Dark green foreground.</summary>
        public const string DarkGreen = "\u001b[32m";
        /// <summary>Dark yellow foreground.</summary>
        public const string DarkYellow = "\u001b[33m";
        /// <summary>Dark blue foreground.</summary>
        public const string DarkBlue = "\u001b[34m";
        /// <summary>Dark magenta foreground.</summary>
        public const string DarkMagenta = "\u001b[35m";
        /// <summary>Dark cyan foreground.</summary>
        public const string DarkCyan = "\u001b[36m";
        /// <summary>Gray foreground.</summary>
        public const string Gray = "\u001b[37m";
        /// <summary>Default foreground.</summary>
        public const string Default = "\u001b[39m";

        // Bright colors
        /// <summary>Dark gray foreground.</summary>
        public const string DarkGray = "\u001b[90m";
        /// <summary>Red foreground.</summary>
        public const string Red = "\u001b[91m";
        /// <summary>Green foreground.</summary>
        public const string Green = "\u001b[92m";
        /// <summary>Yellow foreground.</summary>
        public const string Yellow = "\u001b[93m";
        /// <summary>Blue foreground.</summary>
        public const string Blue = "\u001b[94m";
        /// <summary>Magenta foreground.</summary>
        public const string Magenta = "\u001b[95m";
        /// <summary>Cyan foreground.</summary>
        public const string Cyan = "\u001b[96m";
        /// <summary>White foreground.</summary>
        public const string White = "\u001b[97m";
    }

    /// <summary>
    /// Background color codes.
    /// </summary>
    public static class Background
    {
        /// <summary>Black background.</summary>
        public const string Black = "\u001b[40m";
        /// <summary>Dark red background.</summary>
        public const string DarkRed = "\u001b[41m";
        /// <summary>Dark green background.</summary>
        public const string DarkGreen = "\u001b[42m";
        /// <summary>Dark yellow background.</summary>
        public const string DarkYellow = "\u001b[43m";
        /// <summary>Dark blue background.</summary>
        public const string DarkBlue = "\u001b[44m";
        /// <summary>Dark magenta background.</summary>
        public const string DarkMagenta = "\u001b[45m";
        /// <summary>Dark cyan background.</summary>
        public const string DarkCyan = "\u001b[46m";
        /// <summary>Gray background.</summary>
        public const string Gray = "\u001b[47m";
        /// <summary>Default background.</summary>
        public const string Default = "\u001b[49m";

        // Bright colors
        /// <summary>Dark gray background.</summary>
        public const string DarkGray = "\u001b[100m";
        /// <summary>Red background.</summary>
        public const string Red = "\u001b[101m";
        /// <summary>Green background.</summary>
        public const string Green = "\u001b[102m";
        /// <summary>Yellow background.</summary>
        public const string Yellow = "\u001b[103m";
        /// <summary>Blue background.</summary>
        public const string Blue = "\u001b[104m";
        /// <summary>Magenta background.</summary>
        public const string Magenta = "\u001b[105m";
        /// <summary>Cyan background.</summary>
        public const string Cyan = "\u001b[106m";
        /// <summary>White background.</summary>
        public const string White = "\u001b[107m";
    }

    /// <summary>
    /// Cursor control codes.
    /// </summary>
    public static class Cursor
    {
        /// <summary>Hide cursor.</summary>
        public const string Hide = "\u001b[?25l";
        /// <summary>Show cursor.</summary>
        public const string Show = "\u001b[?25h";
        /// <summary>Save cursor position.</summary>
        public const string Save = "\u001b[s";
        /// <summary>Restore cursor position.</summary>
        public const string Restore = "\u001b[u";

        /// <summary>Move cursor up by n lines.</summary>
        public static string Up(int n = 1) => $"\u001b[{n}A";
        /// <summary>Move cursor down by n lines.</summary>
        public static string Down(int n = 1) => $"\u001b[{n}B";
        /// <summary>Move cursor right by n columns.</summary>
        public static string Right(int n = 1) => $"\u001b[{n}C";
        /// <summary>Move cursor left by n columns.</summary>
        public static string Left(int n = 1) => $"\u001b[{n}D";
        /// <summary>Move cursor to beginning of line n lines down.</summary>
        public static string NextLine(int n = 1) => $"\u001b[{n}E";
        /// <summary>Move cursor to beginning of line n lines up.</summary>
        public static string PrevLine(int n = 1) => $"\u001b[{n}F";
        /// <summary>Move cursor to column n.</summary>
        public static string Column(int n) => $"\u001b[{n}G";
        /// <summary>Move cursor to row, column position.</summary>
        public static string Position(int row, int col) => $"\u001b[{row};{col}H";
    }

    /// <summary>
    /// Erase/clear codes.
    /// </summary>
    public static class Erase
    {
        /// <summary>Clear from cursor to end of screen.</summary>
        public const string ToEndOfScreen = "\u001b[0J";
        /// <summary>Clear from cursor to beginning of screen.</summary>
        public const string ToStartOfScreen = "\u001b[1J";
        /// <summary>Clear entire screen.</summary>
        public const string Screen = "\u001b[2J";
        /// <summary>Clear from cursor to end of line.</summary>
        public const string ToEndOfLine = "\u001b[0K";
        /// <summary>Clear from cursor to beginning of line.</summary>
        public const string ToStartOfLine = "\u001b[1K";
        /// <summary>Clear entire line.</summary>
        public const string Line = "\u001b[2K";
    }

    private static readonly Dictionary<ConsoleColor, string> ForegroundMap = new Dictionary<ConsoleColor, string>
    {
        { ConsoleColor.Black, Foreground.Black },
        { ConsoleColor.DarkBlue, Foreground.DarkBlue },
        { ConsoleColor.DarkGreen, Foreground.DarkGreen },
        { ConsoleColor.DarkCyan, Foreground.DarkCyan },
        { ConsoleColor.DarkRed, Foreground.DarkRed },
        { ConsoleColor.DarkMagenta, Foreground.DarkMagenta },
        { ConsoleColor.DarkYellow, Foreground.DarkYellow },
        { ConsoleColor.Gray, Foreground.Gray },
        { ConsoleColor.DarkGray, Foreground.DarkGray },
        { ConsoleColor.Blue, Foreground.Blue },
        { ConsoleColor.Green, Foreground.Green },
        { ConsoleColor.Cyan, Foreground.Cyan },
        { ConsoleColor.Red, Foreground.Red },
        { ConsoleColor.Magenta, Foreground.Magenta },
        { ConsoleColor.Yellow, Foreground.Yellow },
        { ConsoleColor.White, Foreground.White }
    };

    private static readonly Dictionary<ConsoleColor, string> BackgroundMap = new Dictionary<ConsoleColor, string>
    {
        { ConsoleColor.Black, Background.Black },
        { ConsoleColor.DarkBlue, Background.DarkBlue },
        { ConsoleColor.DarkGreen, Background.DarkGreen },
        { ConsoleColor.DarkCyan, Background.DarkCyan },
        { ConsoleColor.DarkRed, Background.DarkRed },
        { ConsoleColor.DarkMagenta, Background.DarkMagenta },
        { ConsoleColor.DarkYellow, Background.DarkYellow },
        { ConsoleColor.Gray, Background.Gray },
        { ConsoleColor.DarkGray, Background.DarkGray },
        { ConsoleColor.Blue, Background.Blue },
        { ConsoleColor.Green, Background.Green },
        { ConsoleColor.Cyan, Background.Cyan },
        { ConsoleColor.Red, Background.Red },
        { ConsoleColor.Magenta, Background.Magenta },
        { ConsoleColor.Yellow, Background.Yellow },
        { ConsoleColor.White, Background.White }
    };

    /// <summary>
    /// Gets the ANSI code for a foreground ConsoleColor.
    /// </summary>
    /// <param name="color">The console color.</param>
    /// <returns>The ANSI escape code.</returns>
    public static string GetForeground(ConsoleColor color)
    {
        return ForegroundMap.TryGetValue(color, out var code) ? code : Foreground.Default;
    }

    /// <summary>
    /// Gets the ANSI code for a background ConsoleColor.
    /// </summary>
    /// <param name="color">The console color.</param>
    /// <returns>The ANSI escape code.</returns>
    public static string GetBackground(ConsoleColor color)
    {
        return BackgroundMap.TryGetValue(color, out var code) ? code : Background.Default;
    }

    /// <summary>
    /// Builds an ANSI escape sequence for the given style.
    /// </summary>
    /// <param name="style">The style to apply.</param>
    /// <returns>The ANSI escape sequence.</returns>
    public static string BuildStyleSequence(ConsoleStyle style)
    {
        var codes = new List<string>();

        if (style.Bold) codes.Add(Styles.Bold);
        if (style.Dim) codes.Add(Styles.Dim);
        if (style.Italic) codes.Add(Styles.Italic);
        if (style.Underline) codes.Add(Styles.Underline);
        if (style.Blink) codes.Add(Styles.Blink);
        if (style.Inverse) codes.Add(Styles.Inverse);
        if (style.Strikethrough) codes.Add(Styles.Strikethrough);

        if (style.Foreground.HasValue)
            codes.Add(GetForeground(style.Foreground.Value));

        if (style.Background.HasValue)
            codes.Add(GetBackground(style.Background.Value));

        return string.Concat(codes);
    }

    /// <summary>
    /// Wraps text with the specified style and reset codes.
    /// </summary>
    /// <param name="text">The text to style.</param>
    /// <param name="style">The style to apply.</param>
    /// <returns>The styled text with reset code.</returns>
    public static string Stylize(string? text, ConsoleStyle style)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        var sequence = BuildStyleSequence(style);
        if (string.IsNullOrEmpty(sequence))
            return text;

        return $"{sequence}{text}{Reset}";
    }

    /// <summary>
    /// Removes all ANSI escape codes from a string.
    /// </summary>
    /// <param name="text">The text containing ANSI codes.</param>
    /// <returns>The text without ANSI codes.</returns>
    public static string Strip(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        // Match ANSI escape sequences: ESC [ ... (letter or ~)
        var result = System.Text.RegularExpressions.Regex.Replace(
            text,
            @"\u001b\[[0-9;]*[a-zA-Z~]",
            string.Empty);

        return result;
    }
}
