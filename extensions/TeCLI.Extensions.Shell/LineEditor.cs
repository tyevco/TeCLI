using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TeCLI.Shell;

/// <summary>
/// Provides readline-like line editing functionality with history support.
/// </summary>
public class LineEditor
{
    private readonly CommandHistory _history;
    private readonly StringBuilder _buffer;
    private readonly Func<string, IEnumerable<string>>? _autoCompleteProvider;
    private int _cursorPosition;
    private string _savedLine = string.Empty;

    /// <summary>
    /// Gets or sets the prompt to display.
    /// </summary>
    public string Prompt { get; set; } = "> ";

    /// <summary>
    /// Gets or sets the text input reader for testability.
    /// </summary>
    public TextReader? InputReader { get; set; }

    /// <summary>
    /// Gets or sets the text output writer for testability.
    /// </summary>
    public TextWriter? OutputWriter { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LineEditor"/> class.
    /// </summary>
    /// <param name="history">The command history to use.</param>
    /// <param name="autoCompleteProvider">Optional auto-complete provider.</param>
    public LineEditor(CommandHistory? history = null, Func<string, IEnumerable<string>>? autoCompleteProvider = null)
    {
        _history = history ?? new CommandHistory();
        _buffer = new StringBuilder();
        _autoCompleteProvider = autoCompleteProvider;
        _cursorPosition = 0;
    }

    /// <summary>
    /// Reads a line of input with editing support.
    /// </summary>
    /// <returns>The entered line, or null if end of input.</returns>
    public string? ReadLine()
    {
        _buffer.Clear();
        _cursorPosition = 0;
        _history.ResetPosition();
        _savedLine = string.Empty;

        WritePrompt();

        while (true)
        {
            var keyInfo = ReadKey();
            if (keyInfo == null)
                return null;

            var key = keyInfo.Value;

            // Handle special keys
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    WriteLine();
                    var result = _buffer.ToString();
                    return result;

                case ConsoleKey.Backspace:
                    HandleBackspace();
                    break;

                case ConsoleKey.Delete:
                    HandleDelete();
                    break;

                case ConsoleKey.LeftArrow:
                    if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                        MoveToPreviousWord();
                    else
                        MoveLeft();
                    break;

                case ConsoleKey.RightArrow:
                    if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                        MoveToNextWord();
                    else
                        MoveRight();
                    break;

                case ConsoleKey.UpArrow:
                    HandleHistoryPrevious();
                    break;

                case ConsoleKey.DownArrow:
                    HandleHistoryNext();
                    break;

                case ConsoleKey.Home:
                    MoveToStart();
                    break;

                case ConsoleKey.End:
                    MoveToEnd();
                    break;

                case ConsoleKey.Tab:
                    HandleAutoComplete();
                    break;

                case ConsoleKey.Escape:
                    ClearLine();
                    break;

                case ConsoleKey.C:
                    if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        WriteLine();
                        return string.Empty;
                    }
                    InsertCharacter(key.KeyChar);
                    break;

                case ConsoleKey.D:
                    if ((key.Modifiers & ConsoleModifiers.Control) != 0 && _buffer.Length == 0)
                    {
                        return null; // EOF
                    }
                    InsertCharacter(key.KeyChar);
                    break;

                case ConsoleKey.U:
                    if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        DeleteToStart();
                    }
                    else
                    {
                        InsertCharacter(key.KeyChar);
                    }
                    break;

                case ConsoleKey.K:
                    if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        DeleteToEnd();
                    }
                    else
                    {
                        InsertCharacter(key.KeyChar);
                    }
                    break;

                case ConsoleKey.A:
                    if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        MoveToStart();
                    }
                    else
                    {
                        InsertCharacter(key.KeyChar);
                    }
                    break;

                case ConsoleKey.E:
                    if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        MoveToEnd();
                    }
                    else
                    {
                        InsertCharacter(key.KeyChar);
                    }
                    break;

                case ConsoleKey.W:
                    if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        DeletePreviousWord();
                    }
                    else
                    {
                        InsertCharacter(key.KeyChar);
                    }
                    break;

                case ConsoleKey.L:
                    if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        ClearScreen();
                    }
                    else
                    {
                        InsertCharacter(key.KeyChar);
                    }
                    break;

                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        InsertCharacter(key.KeyChar);
                    }
                    break;
            }
        }
    }

    private ConsoleKeyInfo? ReadKey()
    {
        try
        {
            return Console.ReadKey(intercept: true);
        }
        catch
        {
            return null;
        }
    }

    private void WritePrompt()
    {
        Write(Prompt);
    }

    private void Write(string text)
    {
        if (OutputWriter != null)
            OutputWriter.Write(text);
        else
            Console.Write(text);
    }

    private void Write(char c)
    {
        if (OutputWriter != null)
            OutputWriter.Write(c);
        else
            Console.Write(c);
    }

    private void WriteLine()
    {
        if (OutputWriter != null)
            OutputWriter.WriteLine();
        else
            Console.WriteLine();
    }

    private void InsertCharacter(char c)
    {
        if (c == '\0')
            return;

        _buffer.Insert(_cursorPosition, c);
        _cursorPosition++;

        // Redraw from cursor position
        var remaining = _buffer.ToString(_cursorPosition - 1, _buffer.Length - _cursorPosition + 1);
        Write(remaining);

        // Move cursor back to correct position
        var moveBack = _buffer.Length - _cursorPosition;
        if (moveBack > 0)
        {
            Console.CursorLeft -= moveBack;
        }
    }

    private void HandleBackspace()
    {
        if (_cursorPosition > 0)
        {
            _buffer.Remove(_cursorPosition - 1, 1);
            _cursorPosition--;

            // Move cursor back and redraw
            Console.CursorLeft--;
            var remaining = _buffer.ToString(_cursorPosition, _buffer.Length - _cursorPosition) + " ";
            Write(remaining);
            Console.CursorLeft -= remaining.Length;
        }
    }

    private void HandleDelete()
    {
        if (_cursorPosition < _buffer.Length)
        {
            _buffer.Remove(_cursorPosition, 1);

            // Redraw from cursor position
            var remaining = _buffer.ToString(_cursorPosition, _buffer.Length - _cursorPosition) + " ";
            Write(remaining);
            Console.CursorLeft -= remaining.Length;
        }
    }

    private void MoveLeft()
    {
        if (_cursorPosition > 0)
        {
            _cursorPosition--;
            Console.CursorLeft--;
        }
    }

    private void MoveRight()
    {
        if (_cursorPosition < _buffer.Length)
        {
            _cursorPosition++;
            Console.CursorLeft++;
        }
    }

    private void MoveToStart()
    {
        Console.CursorLeft -= _cursorPosition;
        _cursorPosition = 0;
    }

    private void MoveToEnd()
    {
        var delta = _buffer.Length - _cursorPosition;
        Console.CursorLeft += delta;
        _cursorPosition = _buffer.Length;
    }

    private void MoveToPreviousWord()
    {
        if (_cursorPosition == 0)
            return;

        // Skip whitespace
        while (_cursorPosition > 0 && char.IsWhiteSpace(_buffer[_cursorPosition - 1]))
        {
            _cursorPosition--;
            Console.CursorLeft--;
        }

        // Skip word characters
        while (_cursorPosition > 0 && !char.IsWhiteSpace(_buffer[_cursorPosition - 1]))
        {
            _cursorPosition--;
            Console.CursorLeft--;
        }
    }

    private void MoveToNextWord()
    {
        if (_cursorPosition >= _buffer.Length)
            return;

        // Skip word characters
        while (_cursorPosition < _buffer.Length && !char.IsWhiteSpace(_buffer[_cursorPosition]))
        {
            _cursorPosition++;
            Console.CursorLeft++;
        }

        // Skip whitespace
        while (_cursorPosition < _buffer.Length && char.IsWhiteSpace(_buffer[_cursorPosition]))
        {
            _cursorPosition++;
            Console.CursorLeft++;
        }
    }

    private void DeleteToStart()
    {
        if (_cursorPosition > 0)
        {
            var deleteCount = _cursorPosition;
            _buffer.Remove(0, deleteCount);
            Console.CursorLeft -= deleteCount;
            _cursorPosition = 0;

            // Redraw
            var text = _buffer.ToString() + new string(' ', deleteCount);
            Write(text);
            Console.CursorLeft -= text.Length;
        }
    }

    private void DeleteToEnd()
    {
        if (_cursorPosition < _buffer.Length)
        {
            var deleteCount = _buffer.Length - _cursorPosition;
            _buffer.Remove(_cursorPosition, deleteCount);

            // Clear the rest of the line
            Write(new string(' ', deleteCount));
            Console.CursorLeft -= deleteCount;
        }
    }

    private void DeletePreviousWord()
    {
        if (_cursorPosition == 0)
            return;

        var endPos = _cursorPosition;

        // Skip whitespace
        while (_cursorPosition > 0 && char.IsWhiteSpace(_buffer[_cursorPosition - 1]))
        {
            _cursorPosition--;
        }

        // Skip word characters
        while (_cursorPosition > 0 && !char.IsWhiteSpace(_buffer[_cursorPosition - 1]))
        {
            _cursorPosition--;
        }

        var deleteCount = endPos - _cursorPosition;
        _buffer.Remove(_cursorPosition, deleteCount);

        Console.CursorLeft -= deleteCount;
        var text = _buffer.ToString(_cursorPosition, _buffer.Length - _cursorPosition) + new string(' ', deleteCount);
        Write(text);
        Console.CursorLeft -= text.Length;
    }

    private void HandleHistoryPrevious()
    {
        // Save current line if we're at the end
        if (_history.Position == _history.Count)
        {
            _savedLine = _buffer.ToString();
        }

        var previous = _history.Previous();
        if (previous != null)
        {
            ReplaceLine(previous);
        }
    }

    private void HandleHistoryNext()
    {
        var next = _history.Next();
        if (next != null)
        {
            ReplaceLine(next);
        }
        else if (_history.Position >= _history.Count)
        {
            // Restore saved line
            ReplaceLine(_savedLine);
        }
    }

    private void ReplaceLine(string newLine)
    {
        // Clear current line
        Console.CursorLeft -= _cursorPosition;
        Write(new string(' ', _buffer.Length));
        Console.CursorLeft -= _buffer.Length;

        // Set new line
        _buffer.Clear();
        _buffer.Append(newLine);
        _cursorPosition = newLine.Length;
        Write(newLine);
    }

    private void ClearLine()
    {
        Console.CursorLeft -= _cursorPosition;
        Write(new string(' ', _buffer.Length));
        Console.CursorLeft -= _buffer.Length;

        _buffer.Clear();
        _cursorPosition = 0;
    }

    private void ClearScreen()
    {
        Console.Clear();
        WritePrompt();
        Write(_buffer.ToString());
        Console.CursorLeft = Prompt.Length + _cursorPosition;
    }

    private void HandleAutoComplete()
    {
        if (_autoCompleteProvider == null)
            return;

        var currentText = _buffer.ToString(0, _cursorPosition);
        var completions = new List<string>(_autoCompleteProvider(currentText));

        if (completions.Count == 0)
            return;

        if (completions.Count == 1)
        {
            // Single completion - apply it
            var completion = completions[0];
            var suffix = completion.Substring(currentText.Length);
            foreach (var c in suffix)
            {
                InsertCharacter(c);
            }

            // Add a space after completion
            InsertCharacter(' ');
        }
        else
        {
            // Multiple completions - show them
            WriteLine();
            foreach (var completion in completions)
            {
                Write("  ");
                Write(completion);
                WriteLine();
            }

            // Redraw prompt and current input
            WritePrompt();
            Write(_buffer.ToString());
            Console.CursorLeft = Prompt.Length + _cursorPosition;
        }
    }
}
