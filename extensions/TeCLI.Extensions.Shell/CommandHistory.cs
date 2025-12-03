using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TeCLI.Shell;

/// <summary>
/// Manages command history with navigation support and optional persistence.
/// </summary>
public class CommandHistory
{
    private readonly List<string> _history;
    private readonly int _maxSize;
    private readonly string? _historyFile;
    private int _position;

    /// <summary>
    /// Gets the number of entries in the history.
    /// </summary>
    public int Count => _history.Count;

    /// <summary>
    /// Gets all history entries.
    /// </summary>
    public IReadOnlyList<string> Entries => _history.AsReadOnly();

    /// <summary>
    /// Gets or sets the current position in the history.
    /// </summary>
    public int Position
    {
        get => _position;
        set => _position = Math.Max(-1, Math.Min(value, _history.Count));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHistory"/> class.
    /// </summary>
    /// <param name="maxSize">Maximum number of entries to retain.</param>
    /// <param name="historyFile">Optional file path for persistence.</param>
    public CommandHistory(int maxSize = 100, string? historyFile = null)
    {
        _maxSize = maxSize;
        _historyFile = historyFile;
        _history = new List<string>();
        _position = -1;

        if (!string.IsNullOrEmpty(_historyFile))
        {
            Load();
        }
    }

    /// <summary>
    /// Adds a command to the history.
    /// </summary>
    /// <param name="command">The command to add.</param>
    public void Add(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return;

        // Don't add duplicates of the last command
        if (_history.Count > 0 && _history[_history.Count - 1] == command)
            return;

        _history.Add(command);

        // Trim if over max size
        while (_history.Count > _maxSize)
        {
            _history.RemoveAt(0);
        }

        // Reset position to end
        _position = _history.Count;

        if (!string.IsNullOrEmpty(_historyFile))
        {
            Save();
        }
    }

    /// <summary>
    /// Moves to the previous command in history.
    /// </summary>
    /// <returns>The previous command, or null if at the beginning.</returns>
    public string? Previous()
    {
        if (_history.Count == 0)
            return null;

        if (_position > 0)
        {
            _position--;
        }

        return _position >= 0 && _position < _history.Count ? _history[_position] : null;
    }

    /// <summary>
    /// Moves to the next command in history.
    /// </summary>
    /// <returns>The next command, or null if at the end.</returns>
    public string? Next()
    {
        if (_history.Count == 0)
            return null;

        if (_position < _history.Count)
        {
            _position++;
        }

        return _position >= 0 && _position < _history.Count ? _history[_position] : null;
    }

    /// <summary>
    /// Resets the position to the end of history.
    /// </summary>
    public void ResetPosition()
    {
        _position = _history.Count;
    }

    /// <summary>
    /// Gets the command at the current position.
    /// </summary>
    /// <returns>The current command, or null if position is invalid.</returns>
    public string? Current()
    {
        return _position >= 0 && _position < _history.Count ? _history[_position] : null;
    }

    /// <summary>
    /// Clears all history entries.
    /// </summary>
    public void Clear()
    {
        _history.Clear();
        _position = -1;

        if (!string.IsNullOrEmpty(_historyFile) && File.Exists(_historyFile))
        {
            try
            {
                File.Delete(_historyFile);
            }
            catch
            {
                // Ignore file deletion errors
            }
        }
    }

    /// <summary>
    /// Searches history for commands containing the specified text.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <returns>Matching history entries.</returns>
    public IEnumerable<string> Search(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
            return _history;

        return _history.Where(h => h.Contains(searchText));
    }

    /// <summary>
    /// Searches backwards from current position for a command starting with the prefix.
    /// </summary>
    /// <param name="prefix">The prefix to search for.</param>
    /// <returns>The matching command, or null if not found.</returns>
    public string? SearchBackward(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return Previous();

        for (int i = _position - 1; i >= 0; i--)
        {
            if (_history[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                _position = i;
                return _history[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Searches forwards from current position for a command starting with the prefix.
    /// </summary>
    /// <param name="prefix">The prefix to search for.</param>
    /// <returns>The matching command, or null if not found.</returns>
    public string? SearchForward(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return Next();

        for (int i = _position + 1; i < _history.Count; i++)
        {
            if (_history[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                _position = i;
                return _history[i];
            }
        }

        return null;
    }

    private void Load()
    {
        if (string.IsNullOrEmpty(_historyFile) || !File.Exists(_historyFile))
            return;

        try
        {
            var lines = File.ReadAllLines(_historyFile);
            foreach (var line in lines.Skip(Math.Max(0, lines.Length - _maxSize)))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    _history.Add(line);
                }
            }
            _position = _history.Count;
        }
        catch
        {
            // Ignore file read errors
        }
    }

    private void Save()
    {
        if (string.IsNullOrEmpty(_historyFile))
            return;

        try
        {
            var directory = Path.GetDirectoryName(_historyFile);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllLines(_historyFile, _history.Skip(Math.Max(0, _history.Count - _maxSize)));
        }
        catch
        {
            // Ignore file write errors
        }
    }
}
