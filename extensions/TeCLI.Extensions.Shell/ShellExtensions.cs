using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeCLI.Shell;

/// <summary>
/// Extension methods and utilities for shell integration.
/// </summary>
public static class ShellExtensions
{
    /// <summary>
    /// Creates a shell host with the specified command handler.
    /// </summary>
    /// <param name="options">Shell options.</param>
    /// <param name="commandHandler">The command handler.</param>
    /// <returns>A configured shell host.</returns>
    public static ShellHost CreateShell(
        ShellOptions options,
        Func<ShellHost, string[], Task<int>> commandHandler)
    {
        var host = new ShellHost(options);
        host.CommandHandler += commandHandler;
        return host;
    }

    /// <summary>
    /// Creates a shell host with the specified command handler (synchronous).
    /// </summary>
    /// <param name="options">Shell options.</param>
    /// <param name="commandHandler">The command handler.</param>
    /// <returns>A configured shell host.</returns>
    public static ShellHost CreateShell(
        ShellOptions options,
        Func<ShellHost, string[], int> commandHandler)
    {
        var host = new ShellHost(options);
        host.CommandHandler += (h, args) => Task.FromResult(commandHandler(h, args));
        return host;
    }

    /// <summary>
    /// Creates a shell host from an attribute.
    /// </summary>
    /// <param name="attribute">The shell attribute.</param>
    /// <param name="commandHandler">The command handler.</param>
    /// <returns>A configured shell host.</returns>
    public static ShellHost CreateShell(
        ShellAttribute attribute,
        Func<ShellHost, string[], Task<int>> commandHandler)
    {
        var options = ShellOptions.FromAttribute(attribute);
        return CreateShell(options, commandHandler);
    }

    /// <summary>
    /// Creates an action-based shell that routes commands to action handlers.
    /// </summary>
    /// <param name="options">Shell options.</param>
    /// <returns>An action shell builder.</returns>
    public static ActionShellBuilder CreateActionShell(ShellOptions? options = null)
    {
        return new ActionShellBuilder(options ?? new ShellOptions());
    }
}

/// <summary>
/// Builder for creating action-based shells.
/// </summary>
public class ActionShellBuilder
{
    private readonly ShellOptions _options;
    private readonly Dictionary<string, ActionInfo> _actions;
    private Func<string[], int>? _defaultAction;

    internal ActionShellBuilder(ShellOptions options)
    {
        _options = options;
        _actions = new Dictionary<string, ActionInfo>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Registers an action handler.
    /// </summary>
    /// <param name="name">The action name.</param>
    /// <param name="description">The action description.</param>
    /// <param name="handler">The action handler.</param>
    /// <returns>This builder.</returns>
    public ActionShellBuilder WithAction(string name, string description, Func<string[], int> handler)
    {
        _actions[name] = new ActionInfo(name, description, (args) => Task.FromResult(handler(args)));
        return this;
    }

    /// <summary>
    /// Registers an async action handler.
    /// </summary>
    /// <param name="name">The action name.</param>
    /// <param name="description">The action description.</param>
    /// <param name="handler">The action handler.</param>
    /// <returns>This builder.</returns>
    public ActionShellBuilder WithActionAsync(string name, string description, Func<string[], Task<int>> handler)
    {
        _actions[name] = new ActionInfo(name, description, handler);
        return this;
    }

    /// <summary>
    /// Sets the default action for unknown commands.
    /// </summary>
    /// <param name="handler">The default handler.</param>
    /// <returns>This builder.</returns>
    public ActionShellBuilder WithDefaultAction(Func<string[], int> handler)
    {
        _defaultAction = handler;
        return this;
    }

    /// <summary>
    /// Builds the shell host.
    /// </summary>
    /// <returns>A configured shell host.</returns>
    public ShellHost Build()
    {
        var host = new ShellHost(_options);

        // Register action help command
        host.RegisterCommand("actions", "Show available actions", (h, _) =>
        {
            Console.WriteLine("Available actions:");
            foreach (var action in _actions.Values.OrderBy(a => a.Name))
            {
                Console.WriteLine($"  {action.Name,-20} {action.Description}");
            }
            return 0;
        });

        host.CommandHandler += async (h, args) =>
        {
            if (args.Length == 0)
                return 0;

            var actionName = args[0];
            var actionArgs = args.Skip(1).ToArray();

            if (_actions.TryGetValue(actionName, out var action))
            {
                return await action.Handler(actionArgs);
            }

            if (_defaultAction != null)
            {
                return _defaultAction(args);
            }

            Console.Error.WriteLine($"Unknown action: {actionName}");
            Console.Error.WriteLine("Type 'actions' to see available actions, or 'help' for shell commands.");
            return 1;
        };

        host.AutoCompleteHandler += (input) =>
        {
            var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var isFirstWord = parts.Length <= 1 && !input.EndsWith(" ");

            if (isFirstWord)
            {
                var prefix = parts.Length > 0 ? parts[0] : string.Empty;
                return _actions.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            }

            return Enumerable.Empty<string>();
        };

        return host;
    }

    private class ActionInfo
    {
        public string Name { get; }
        public string Description { get; }
        public Func<string[], Task<int>> Handler { get; }

        public ActionInfo(string name, string description, Func<string[], Task<int>> handler)
        {
            Name = name;
            Description = description;
            Handler = handler;
        }
    }
}
