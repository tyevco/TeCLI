using System.IO;
using TeCLI.Attributes;

namespace TeCLI.Example.Advanced;

/// <summary>
/// Stream operations command demonstrating:
/// - Pipeline support (stdin/stdout)
/// - Stream, TextReader, TextWriter parameters
/// - Special "-" handling for stdin/stdout
/// - File path to stream conversion
///
/// Usage examples:
///   cat input.txt | myapp stream transform --output result.txt
///   myapp stream transform --input data.txt --output -
///   echo "hello" | myapp stream uppercase
///   myapp stream count --input file.txt
/// </summary>
[Command("stream", Description = "Stream and pipeline operations")]
public class StreamCommand
{
    /// <summary>
    /// Transform text from input to output with customizable operations.
    /// Demonstrates dual stream parameters for full pipeline support.
    /// </summary>
    /// <example>
    /// cat input.txt | myapp stream transform | tee output.txt
    /// myapp stream transform -i input.txt -o output.txt
    /// </example>
    [Primary]
    [Action("transform", Description = "Transform text from input to output")]
    public void Transform(
        [Option("input", ShortName = 'i', Description = "Input file path or - for stdin")]
        TextReader input,

        [Option("output", ShortName = 'o', Description = "Output file path or - for stdout")]
        TextWriter output,

        [Option("uppercase", ShortName = 'u', Description = "Convert to uppercase")]
        bool uppercase = false,

        [Option("lowercase", ShortName = 'l', Description = "Convert to lowercase")]
        bool lowercase = false,

        [Option("trim", ShortName = 't', Description = "Trim whitespace from each line")]
        bool trim = false)
    {
        string? line;
        while ((line = input.ReadLine()) != null)
        {
            if (trim)
                line = line.Trim();

            if (uppercase)
                line = line.ToUpperInvariant();
            else if (lowercase)
                line = line.ToLowerInvariant();

            output.WriteLine(line);
        }

        output.Flush();
    }

    /// <summary>
    /// Convert input to uppercase.
    /// Demonstrates simple stdin-to-stdout transformation.
    /// </summary>
    /// <example>
    /// echo "hello world" | myapp stream uppercase
    /// myapp stream uppercase --input file.txt
    /// </example>
    [Action("uppercase", Description = "Convert input to uppercase", Aliases = new[] { "upper", "uc" })]
    public void Uppercase(
        [Option("input", ShortName = 'i', Description = "Input file or stdin")]
        TextReader input)
    {
        var content = input.ReadToEnd();
        Console.Write(content.ToUpperInvariant());
    }

    /// <summary>
    /// Convert input to lowercase.
    /// </summary>
    [Action("lowercase", Description = "Convert input to lowercase", Aliases = new[] { "lower", "lc" })]
    public void Lowercase(
        [Option("input", ShortName = 'i', Description = "Input file or stdin")]
        TextReader input)
    {
        var content = input.ReadToEnd();
        Console.Write(content.ToLowerInvariant());
    }

    /// <summary>
    /// Count lines, words, and characters in input.
    /// Similar to the Unix 'wc' command.
    /// </summary>
    /// <example>
    /// cat document.txt | myapp stream count
    /// myapp stream count --input document.txt --lines --words
    /// </example>
    [Action("count", Description = "Count lines, words, and characters", Aliases = new[] { "wc" })]
    public void Count(
        [Option("input", ShortName = 'i', Description = "Input file or stdin")]
        TextReader input,

        [Option("lines", ShortName = 'l', Description = "Count lines only")]
        bool linesOnly = false,

        [Option("words", ShortName = 'w', Description = "Count words only")]
        bool wordsOnly = false,

        [Option("chars", ShortName = 'c', Description = "Count characters only")]
        bool charsOnly = false)
    {
        var content = input.ReadToEnd();

        var lines = content.Split('\n').Length;
        var words = content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        var chars = content.Length;

        // If no specific flag, show all
        var showAll = !linesOnly && !wordsOnly && !charsOnly;

        if (showAll)
        {
            Console.WriteLine($"  Lines: {lines}");
            Console.WriteLine($"  Words: {words}");
            Console.WriteLine($"  Chars: {chars}");
        }
        else
        {
            if (linesOnly) Console.WriteLine(lines);
            if (wordsOnly) Console.WriteLine(words);
            if (charsOnly) Console.WriteLine(chars);
        }
    }

    /// <summary>
    /// Filter lines matching a pattern.
    /// Similar to Unix 'grep' command.
    /// </summary>
    /// <example>
    /// cat log.txt | myapp stream filter "ERROR"
    /// myapp stream filter "TODO" --input source.cs --ignore-case
    /// </example>
    [Action("filter", Description = "Filter lines matching a pattern", Aliases = new[] { "grep" })]
    public void Filter(
        [Argument(Description = "Pattern to match")]
        string pattern,

        [Option("input", ShortName = 'i', Description = "Input file or stdin")]
        TextReader input,

        [Option("output", ShortName = 'o', Description = "Output file or stdout")]
        TextWriter output,

        [Option("ignore-case", ShortName = 'c', Description = "Case-insensitive matching")]
        bool ignoreCase = false,

        [Option("invert", ShortName = 'v', Description = "Invert match (exclude matching lines)")]
        bool invert = false,

        [Option("line-numbers", ShortName = 'n', Description = "Show line numbers")]
        bool lineNumbers = false)
    {
        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var lineNum = 0;
        string? line;

        while ((line = input.ReadLine()) != null)
        {
            lineNum++;
            var matches = line.Contains(pattern, comparison);

            if (matches != invert)
            {
                if (lineNumbers)
                    output.WriteLine($"{lineNum}: {line}");
                else
                    output.WriteLine(line);
            }
        }

        output.Flush();
    }

    /// <summary>
    /// Process binary data from a stream.
    /// Demonstrates raw Stream type for binary operations.
    /// </summary>
    /// <example>
    /// cat image.png | myapp stream binary --analyze
    /// myapp stream binary --input data.bin --hex
    /// </example>
    [Action("binary", Description = "Process binary data")]
    public void Binary(
        [Option("input", ShortName = 'i', Description = "Input file or stdin")]
        Stream input,

        [Option("hex", Description = "Output as hexadecimal")]
        bool hex = false,

        [Option("analyze", ShortName = 'a', Description = "Show file analysis")]
        bool analyze = false,

        [Option("limit", ShortName = 'l', Description = "Limit bytes to read")]
        int limit = 1024)
    {
        var buffer = new byte[Math.Min(limit, 8192)];
        var bytesRead = input.Read(buffer, 0, buffer.Length);

        if (analyze)
        {
            Console.WriteLine($"Bytes read: {bytesRead}");
            Console.WriteLine($"First byte: 0x{buffer[0]:X2}");

            // Simple file type detection
            if (bytesRead >= 4)
            {
                var magic = $"{buffer[0]:X2}{buffer[1]:X2}{buffer[2]:X2}{buffer[3]:X2}";
                var fileType = magic switch
                {
                    "89504E47" => "PNG image",
                    "FFD8FFE0" or "FFD8FFE1" => "JPEG image",
                    "25504446" => "PDF document",
                    "504B0304" => "ZIP archive",
                    "7F454C46" => "ELF executable",
                    _ => "Unknown"
                };
                Console.WriteLine($"File type: {fileType}");
            }
        }

        if (hex)
        {
            for (var i = 0; i < bytesRead; i++)
            {
                Console.Write($"{buffer[i]:X2} ");
                if ((i + 1) % 16 == 0)
                    Console.WriteLine();
            }
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Reverse lines in input.
    /// </summary>
    [Action("reverse", Description = "Reverse lines in input")]
    public void Reverse(
        [Option("input", ShortName = 'i', Description = "Input file or stdin")]
        TextReader input,

        [Option("output", ShortName = 'o', Description = "Output file or stdout")]
        TextWriter output,

        [Option("chars", ShortName = 'c', Description = "Reverse characters in each line instead of line order")]
        bool reverseChars = false)
    {
        var lines = new List<string>();
        string? line;

        while ((line = input.ReadLine()) != null)
        {
            lines.Add(line);
        }

        if (reverseChars)
        {
            foreach (var l in lines)
            {
                var chars = l.ToCharArray();
                Array.Reverse(chars);
                output.WriteLine(new string(chars));
            }
        }
        else
        {
            lines.Reverse();
            foreach (var l in lines)
            {
                output.WriteLine(l);
            }
        }

        output.Flush();
    }
}
