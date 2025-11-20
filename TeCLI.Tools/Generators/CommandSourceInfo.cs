using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace TeCLI.Generators
{
    /// <summary>
    /// Information about a command class, including any nested subcommands
    /// </summary>
    public class CommandSourceInfo
    {
        /// <summary>
        /// The type symbol for this command class
        /// </summary>
        public INamedTypeSymbol? TypeSymbol { get; set; }

        /// <summary>
        /// The command name (from CommandAttribute)
        /// </summary>
        public string? CommandName { get; set; }

        /// <summary>
        /// Command aliases
        /// </summary>
        public List<string> Aliases { get; set; } = new();

        /// <summary>
        /// Description of the command
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// List of nested subcommands (nested classes with CommandAttribute)
        /// </summary>
        public List<CommandSourceInfo> Subcommands { get; set; } = new();

        /// <summary>
        /// List of actions defined in this command
        /// </summary>
        public List<ActionSourceInfo> Actions { get; set; } = new();

        /// <summary>
        /// The parent command (null for top-level commands)
        /// </summary>
        public CommandSourceInfo? Parent { get; set; }

        /// <summary>
        /// The depth level in the hierarchy (0 for top-level)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Whether this command has any subcommands
        /// </summary>
        public bool HasSubcommands => Subcommands.Count > 0;

        /// <summary>
        /// Whether this command has any actions
        /// </summary>
        public bool HasActions => Actions.Count > 0;

        /// <summary>
        /// Hooks applied at the command level (inherited by all actions)
        /// </summary>
        public List<HookInfo> BeforeExecuteHooks { get; set; } = new();
        public List<HookInfo> AfterExecuteHooks { get; set; } = new();
        public List<HookInfo> OnErrorHooks { get; set; } = new();

        /// <summary>
        /// Full command path (e.g., "git remote")
        /// </summary>
        public string GetFullCommandPath()
        {
            if (Parent == null)
                return CommandName ?? "";

            return $"{Parent.GetFullCommandPath()} {CommandName}";
        }
    }
}
