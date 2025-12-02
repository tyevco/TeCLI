using System;
using System.Collections.Generic;
using System.Text;

namespace TeCLI.Testing;

/// <summary>
/// A fluent builder for constructing command-line arguments for testing.
/// Provides a type-safe way to build argument arrays with proper formatting.
/// </summary>
public sealed class ArgumentBuilder
{
    private readonly List<string> _arguments = new List<string>();

    /// <summary>
    /// Creates a new empty ArgumentBuilder.
    /// </summary>
    public ArgumentBuilder()
    {
    }

    /// <summary>
    /// Creates a new ArgumentBuilder with the specified command.
    /// </summary>
    /// <param name="command">The command name.</param>
    public ArgumentBuilder(string command)
    {
        _arguments.Add(command);
    }

    /// <summary>
    /// Creates a new ArgumentBuilder starting with the specified command.
    /// </summary>
    /// <param name="command">The command name.</param>
    /// <returns>A new ArgumentBuilder instance.</returns>
    public static ArgumentBuilder Command(string command) =>
        new ArgumentBuilder(command);

    /// <summary>
    /// Creates a new empty ArgumentBuilder.
    /// </summary>
    /// <returns>A new ArgumentBuilder instance.</returns>
    public static ArgumentBuilder Create() =>
        new ArgumentBuilder();

    /// <summary>
    /// Adds a subcommand or action to the argument list.
    /// </summary>
    /// <param name="action">The action/subcommand name.</param>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder Action(string action)
    {
        _arguments.Add(action);
        return this;
    }

    /// <summary>
    /// Adds a positional argument to the argument list.
    /// </summary>
    /// <param name="value">The argument value.</param>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder Argument(string value)
    {
        _arguments.Add(value);
        return this;
    }

    /// <summary>
    /// Adds multiple positional arguments to the argument list.
    /// </summary>
    /// <param name="values">The argument values.</param>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder Arguments(params string[] values)
    {
        _arguments.AddRange(values);
        return this;
    }

    /// <summary>
    /// Adds a long option (--name value) to the argument list.
    /// </summary>
    /// <param name="name">The option name (without --).</param>
    /// <param name="value">The option value.</param>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder Option(string name, string value)
    {
        _arguments.Add($"--{name}");
        _arguments.Add(value);
        return this;
    }

    /// <summary>
    /// Adds a long option with a typed value (--name value) to the argument list.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="name">The option name (without --).</param>
    /// <param name="value">The option value.</param>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder Option<T>(string name, T value)
    {
        _arguments.Add($"--{name}");
        _arguments.Add(value?.ToString() ?? string.Empty);
        return this;
    }

    /// <summary>
    /// Adds a short option (-n value) to the argument list.
    /// </summary>
    /// <param name="shortName">The option short name (single character, without -).</param>
    /// <param name="value">The option value.</param>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder ShortOption(char shortName, string value)
    {
        _arguments.Add($"-{shortName}");
        _arguments.Add(value);
        return this;
    }

    /// <summary>
    /// Adds a short option with a typed value (-n value) to the argument list.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="shortName">The option short name (single character, without -).</param>
    /// <param name="value">The option value.</param>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder ShortOption<T>(char shortName, T value)
    {
        _arguments.Add($"-{shortName}");
        _arguments.Add(value?.ToString() ?? string.Empty);
        return this;
    }

    /// <summary>
    /// Adds a boolean flag (--flag) to the argument list.
    /// </summary>
    /// <param name="name">The flag name (without --).</param>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder Flag(string name)
    {
        _arguments.Add($"--{name}");
        return this;
    }

    /// <summary>
    /// Adds a short boolean flag (-f) to the argument list.
    /// </summary>
    /// <param name="shortName">The flag short name (single character, without -).</param>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder ShortFlag(char shortName)
    {
        _arguments.Add($"-{shortName}");
        return this;
    }

    /// <summary>
    /// Conditionally adds an option if the condition is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="name">The option name.</param>
    /// <param name="value">The option value.</param>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder OptionIf(bool condition, string name, string value)
    {
        if (condition)
        {
            Option(name, value);
        }
        return this;
    }

    /// <summary>
    /// Conditionally adds a flag if the condition is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="name">The flag name.</param>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder FlagIf(bool condition, string name)
    {
        if (condition)
        {
            Flag(name);
        }
        return this;
    }

    /// <summary>
    /// Adds the --help flag.
    /// </summary>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder Help() => Flag("help");

    /// <summary>
    /// Adds the --version flag.
    /// </summary>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder Version() => Flag("version");

    /// <summary>
    /// Adds a raw argument without any formatting.
    /// </summary>
    /// <param name="arg">The raw argument to add.</param>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder Raw(string arg)
    {
        _arguments.Add(arg);
        return this;
    }

    /// <summary>
    /// Adds multiple raw arguments without any formatting.
    /// </summary>
    /// <param name="args">The raw arguments to add.</param>
    /// <returns>This builder for method chaining.</returns>
    public ArgumentBuilder Raw(params string[] args)
    {
        _arguments.AddRange(args);
        return this;
    }

    /// <summary>
    /// Builds the argument array.
    /// </summary>
    /// <returns>An array of command-line arguments.</returns>
    public string[] Build() => _arguments.ToArray();

    /// <summary>
    /// Implicitly converts an ArgumentBuilder to a string array.
    /// </summary>
    public static implicit operator string[](ArgumentBuilder builder) =>
        builder.Build();

    /// <summary>
    /// Returns the arguments as a single command-line string.
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var arg in _arguments)
        {
            if (sb.Length > 0)
                sb.Append(' ');

            // Quote arguments containing spaces
            if (arg.Contains(" "))
                sb.Append('"').Append(arg).Append('"');
            else
                sb.Append(arg);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Parses a command-line string into an array of arguments.
    /// Handles quoted strings and escaped characters.
    /// </summary>
    /// <param name="commandLine">The command-line string to parse.</param>
    /// <returns>An array of parsed arguments.</returns>
    public static string[] Parse(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
            return Array.Empty<string>();

        var args = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        var escaped = false;

        foreach (var c in commandLine)
        {
            if (escaped)
            {
                current.Append(c);
                escaped = false;
                continue;
            }

            switch (c)
            {
                case '\\':
                    escaped = true;
                    break;
                case '"':
                    inQuotes = !inQuotes;
                    break;
                case ' ' when !inQuotes:
                    if (current.Length > 0)
                    {
                        args.Add(current.ToString());
                        current.Clear();
                    }
                    break;
                default:
                    current.Append(c);
                    break;
            }
        }

        if (current.Length > 0)
        {
            args.Add(current.ToString());
        }

        return args.ToArray();
    }
}
