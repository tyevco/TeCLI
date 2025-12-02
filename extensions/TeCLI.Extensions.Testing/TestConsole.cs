using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TeCLI.Testing;

/// <summary>
/// A mock console that captures stdout/stderr and allows setting stdin for testing.
/// Use this class to intercept and verify console output during command execution.
/// </summary>
public sealed class TestConsole : IDisposable
{
    private readonly TextWriter _originalOut;
    private readonly TextWriter _originalError;
    private readonly TextReader _originalIn;

    private readonly StringWriter _outputWriter;
    private readonly StringWriter _errorWriter;
    private readonly Queue<string> _inputLines;
    private readonly TestInputReader _inputReader;

    /// <summary>
    /// Gets the captured standard output.
    /// </summary>
    public string Output => _outputWriter.ToString();

    /// <summary>
    /// Gets the captured standard error output.
    /// </summary>
    public string Error => _errorWriter.ToString();

    /// <summary>
    /// Gets the captured output split into individual lines.
    /// </summary>
    public string[] OutputLines => SplitLines(Output);

    /// <summary>
    /// Gets the captured error output split into individual lines.
    /// </summary>
    public string[] ErrorLines => SplitLines(Error);

    /// <summary>
    /// Gets a value indicating whether any output was written to stdout.
    /// </summary>
    public bool HasOutput => _outputWriter.GetStringBuilder().Length > 0;

    /// <summary>
    /// Gets a value indicating whether any output was written to stderr.
    /// </summary>
    public bool HasError => _errorWriter.GetStringBuilder().Length > 0;

    /// <summary>
    /// Creates a new TestConsole and redirects Console.Out, Console.Error, and Console.In.
    /// </summary>
    public TestConsole()
    {
        _originalOut = Console.Out;
        _originalError = Console.Error;
        _originalIn = Console.In;

        _outputWriter = new StringWriter();
        _errorWriter = new StringWriter();
        _inputLines = new Queue<string>();
        _inputReader = new TestInputReader(_inputLines);

        Console.SetOut(_outputWriter);
        Console.SetError(_errorWriter);
        Console.SetIn(_inputReader);
    }

    /// <summary>
    /// Queues input that will be returned when Console.ReadLine is called.
    /// </summary>
    /// <param name="line">The line to return from the next ReadLine call.</param>
    /// <returns>This instance for method chaining.</returns>
    public TestConsole SetInput(string line)
    {
        _inputLines.Enqueue(line);
        return this;
    }

    /// <summary>
    /// Queues multiple input lines that will be returned in order when Console.ReadLine is called.
    /// </summary>
    /// <param name="lines">The lines to return from subsequent ReadLine calls.</param>
    /// <returns>This instance for method chaining.</returns>
    public TestConsole SetInputLines(params string[] lines)
    {
        foreach (var line in lines)
        {
            _inputLines.Enqueue(line);
        }
        return this;
    }

    /// <summary>
    /// Clears all captured output and error streams.
    /// </summary>
    public void Clear()
    {
        _outputWriter.GetStringBuilder().Clear();
        _errorWriter.GetStringBuilder().Clear();
    }

    /// <summary>
    /// Checks if the output contains the specified text.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <returns>True if the output contains the text; otherwise, false.</returns>
    public bool OutputContains(string text) =>
        Output.Contains(text);

    /// <summary>
    /// Checks if the error output contains the specified text.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <returns>True if the error output contains the text; otherwise, false.</returns>
    public bool ErrorContains(string text) =>
        Error.Contains(text);

    /// <summary>
    /// Restores the original console streams.
    /// </summary>
    public void Dispose()
    {
        Console.SetOut(_originalOut);
        Console.SetError(_originalError);
        Console.SetIn(_originalIn);

        _outputWriter.Dispose();
        _errorWriter.Dispose();
        _inputReader.Dispose();
    }

    private static string[] SplitLines(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<string>();

        return text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    }

    /// <summary>
    /// A TextReader implementation that returns queued input lines.
    /// </summary>
    private sealed class TestInputReader : TextReader
    {
        private readonly Queue<string> _lines;
        private string? _currentLine;
        private int _currentPosition;

        public TestInputReader(Queue<string> lines)
        {
            _lines = lines;
        }

        public override string? ReadLine()
        {
            if (_lines.Count == 0)
                return null;

            return _lines.Dequeue();
        }

        public override int Read()
        {
            if (_currentLine == null || _currentPosition >= _currentLine.Length)
            {
                _currentLine = ReadLine();
                _currentPosition = 0;

                if (_currentLine == null)
                    return -1;

                _currentLine += Environment.NewLine;
            }

            return _currentLine[_currentPosition++];
        }

        public override int Peek()
        {
            if (_currentLine == null || _currentPosition >= _currentLine.Length)
            {
                if (_lines.Count == 0)
                    return -1;

                // Don't consume, just peek
                var nextLine = _lines.Peek() + Environment.NewLine;
                return nextLine[0];
            }

            return _currentLine[_currentPosition];
        }
    }
}
