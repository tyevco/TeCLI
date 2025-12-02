using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TeCLI.Localization
{
    /// <summary>
    /// Renders localized help text at runtime by reading command metadata via reflection.
    /// </summary>
    public class LocalizedHelpRenderer
    {
        private readonly ILocalizationProvider _localization;
        private readonly TextWriter _output;

        /// <summary>
        /// Standard resource keys for framework messages.
        /// </summary>
        public static class ResourceKeys
        {
            public const string Usage = "Help_Usage";
            public const string Commands = "Help_Commands";
            public const string Actions = "Help_Actions";
            public const string Options = "Help_Options";
            public const string Arguments = "Help_Arguments";
            public const string Subcommands = "Help_Subcommands";
            public const string GlobalOptions = "Help_GlobalOptions";
            public const string Description = "Help_Description";
            public const string Command = "Help_Command";
            public const string Aliases = "Help_Aliases";
            public const string HelpOption = "Help_HelpOption";
            public const string VersionOption = "Help_VersionOption";
            public const string Required = "Help_Required";
            public const string Default = "Help_Default";
        }

        /// <summary>
        /// Creates a new LocalizedHelpRenderer.
        /// </summary>
        /// <param name="localization">The localization provider to use.</param>
        /// <param name="output">The output writer. If null, uses Console.Out.</param>
        public LocalizedHelpRenderer(ILocalizationProvider localization, TextWriter? output = null)
        {
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
            _output = output ?? Console.Out;
        }

        /// <summary>
        /// Renders help for a command type.
        /// </summary>
        /// <param name="commandType">The command class type.</param>
        /// <param name="actionName">Optional specific action to show help for.</param>
        public void RenderCommandHelp(Type commandType, string? actionName = null)
        {
            var commandAttr = GetCommandAttribute(commandType);
            if (commandAttr == null)
            {
                _output.WriteLine($"Type {commandType.Name} is not a TeCLI command.");
                return;
            }

            var commandName = GetAttributeProperty<string>(commandAttr, "Name") ?? commandType.Name.ToLowerInvariant();
            var description = GetLocalizedDescription(commandType) ?? GetAttributeProperty<string>(commandAttr, "Description");
            var aliases = GetAttributeProperty<string[]>(commandAttr, "Aliases");

            // Command header
            var commandLabel = _localization.HasKey(ResourceKeys.Command)
                ? _localization.GetString(ResourceKeys.Command)
                : "Command";

            if (aliases != null && aliases.Length > 0)
            {
                var aliasLabel = _localization.HasKey(ResourceKeys.Aliases)
                    ? _localization.GetString(ResourceKeys.Aliases)
                    : "aliases";
                _output.WriteLine($"{commandLabel}: {commandName} ({aliasLabel}: {string.Join(", ", aliases)})");
            }
            else
            {
                _output.WriteLine($"{commandLabel}: {commandName}");
            }

            if (!string.IsNullOrEmpty(description))
            {
                var descLabel = _localization.HasKey(ResourceKeys.Description)
                    ? _localization.GetString(ResourceKeys.Description)
                    : "Description";
                _output.WriteLine($"{descLabel}: {description}");
            }
            _output.WriteLine();

            // Get actions
            var actions = GetActions(commandType);
            var primaryMethod = GetPrimaryMethod(commandType);

            // Usage section
            var usageLabel = _localization.HasKey(ResourceKeys.Usage)
                ? _localization.GetString(ResourceKeys.Usage)
                : "Usage";
            _output.WriteLine($"{usageLabel}:");

            if (primaryMethod != null)
            {
                _output.WriteLine($"  {BuildUsageLine(commandName, null, primaryMethod)}");
            }

            foreach (var (method, attr) in actions)
            {
                var actionNameValue = GetAttributeProperty<string>(attr, "Name") ?? method.Name.ToLowerInvariant();
                _output.WriteLine($"  {BuildUsageLine(commandName, actionNameValue, method)}");
            }
            _output.WriteLine();

            // Actions section
            if (actions.Count > 0)
            {
                var actionsLabel = _localization.HasKey(ResourceKeys.Actions)
                    ? _localization.GetString(ResourceKeys.Actions)
                    : "Actions";
                _output.WriteLine($"{actionsLabel}:");

                foreach (var (method, attr) in actions)
                {
                    var actionNameValue = GetAttributeProperty<string>(attr, "Name") ?? method.Name.ToLowerInvariant();
                    var actionDesc = GetLocalizedDescription(method) ?? GetAttributeProperty<string>(attr, "Description");
                    var actionAliases = GetAttributeProperty<string[]>(attr, "Aliases");

                    var display = actionNameValue;
                    if (actionAliases != null && actionAliases.Length > 0)
                    {
                        display = $"{actionNameValue} ({string.Join(", ", actionAliases)})";
                    }

                    if (!string.IsNullOrEmpty(actionDesc))
                    {
                        _output.WriteLine($"  {display.PadRight(28)} {actionDesc}");
                    }
                    else
                    {
                        _output.WriteLine($"  {display}");
                    }
                }
                _output.WriteLine();
            }

            // Options section
            var allMethods = new List<MethodInfo>();
            if (primaryMethod != null) allMethods.Add(primaryMethod);
            allMethods.AddRange(actions.Select(a => a.Method));

            var allOptions = allMethods.SelectMany(m => GetOptions(m)).Distinct().ToList();
            if (allOptions.Count > 0)
            {
                var optionsLabel = _localization.HasKey(ResourceKeys.Options)
                    ? _localization.GetString(ResourceKeys.Options)
                    : "Options";
                _output.WriteLine($"{optionsLabel}:");

                foreach (var (param, attr) in allOptions)
                {
                    RenderOption(param, attr);
                }
                _output.WriteLine();
            }

            // Arguments section
            var allArgs = allMethods.SelectMany(m => GetArguments(m)).Distinct().ToList();
            if (allArgs.Count > 0)
            {
                var argsLabel = _localization.HasKey(ResourceKeys.Arguments)
                    ? _localization.GetString(ResourceKeys.Arguments)
                    : "Arguments";
                _output.WriteLine($"{argsLabel}:");

                foreach (var (param, attr) in allArgs)
                {
                    RenderArgument(param, attr);
                }
                _output.WriteLine();
            }

            // Global options
            var globalLabel = _localization.HasKey(ResourceKeys.GlobalOptions)
                ? _localization.GetString(ResourceKeys.GlobalOptions)
                : "Global Options";
            _output.WriteLine($"{globalLabel}:");

            var helpOptionText = _localization.HasKey(ResourceKeys.HelpOption)
                ? _localization.GetString(ResourceKeys.HelpOption)
                : "Display this help message";
            _output.WriteLine($"  --help, -h               {helpOptionText}");

            var versionOptionText = _localization.HasKey(ResourceKeys.VersionOption)
                ? _localization.GetString(ResourceKeys.VersionOption)
                : "Display version information";
            _output.WriteLine($"  --version                {versionOptionText}");
            _output.WriteLine();
        }

        private void RenderOption(ParameterInfo param, Attribute attr)
        {
            var name = GetAttributeProperty<string>(attr, "Name") ?? param.Name ?? "option";
            var shortName = GetAttributeProperty<char>(attr, "ShortName");
            var description = GetLocalizedDescription(param) ?? GetAttributeProperty<string>(attr, "Description");
            var required = GetAttributeProperty<bool>(attr, "Required");

            var sb = new StringBuilder("  --");
            sb.Append(name);
            if (shortName != default(char) && shortName != '\0')
            {
                sb.Append($", -{shortName}");
            }

            var optionDisplay = sb.ToString().PadRight(25);

            var descParts = new List<string>();
            if (!string.IsNullOrEmpty(description))
                descParts.Add(description);
            if (required)
            {
                var requiredText = _localization.HasKey(ResourceKeys.Required)
                    ? _localization.GetString(ResourceKeys.Required)
                    : "(Required)";
                descParts.Add(requiredText);
            }
            if (param.HasDefaultValue && param.DefaultValue != null)
            {
                var defaultText = _localization.HasKey(ResourceKeys.Default)
                    ? _localization.GetString(ResourceKeys.Default, param.DefaultValue)
                    : $"(Default: {param.DefaultValue})";
                descParts.Add(defaultText);
            }

            _output.WriteLine($"{optionDisplay} {string.Join(" ", descParts)}");
        }

        private void RenderArgument(ParameterInfo param, Attribute attr)
        {
            var description = GetLocalizedDescription(param) ?? GetAttributeProperty<string>(attr, "Description");
            var name = param.Name?.ToUpperInvariant() ?? "ARG";

            var display = param.HasDefaultValue ? $"[{name}]" : $"<{name}>";
            display = $"  {display}".PadRight(25);

            var descParts = new List<string>();
            if (!string.IsNullOrEmpty(description))
                descParts.Add(description);
            if (param.HasDefaultValue && param.DefaultValue != null)
            {
                var defaultText = _localization.HasKey(ResourceKeys.Default)
                    ? _localization.GetString(ResourceKeys.Default, param.DefaultValue)
                    : $"(Default: {param.DefaultValue})";
                descParts.Add(defaultText);
            }

            _output.WriteLine($"{display} {string.Join(" ", descParts)}");
        }

        private string BuildUsageLine(string commandName, string? actionName, MethodInfo method)
        {
            var sb = new StringBuilder(commandName);
            if (!string.IsNullOrEmpty(actionName))
            {
                sb.Append(' ');
                sb.Append(actionName);
            }

            var hasOptions = method.GetParameters().Any(p => HasAttribute(p, "OptionAttribute"));
            if (hasOptions)
            {
                sb.Append(" [options]");
            }

            foreach (var param in method.GetParameters())
            {
                if (HasAttribute(param, "ArgumentAttribute"))
                {
                    var argName = param.Name?.ToUpperInvariant() ?? "ARG";
                    if (param.HasDefaultValue)
                    {
                        sb.Append($" [{argName}]");
                    }
                    else
                    {
                        sb.Append($" <{argName}>");
                    }
                }
            }

            return sb.ToString();
        }

        private string? GetLocalizedDescription(MemberInfo member)
        {
            var attr = member.GetCustomAttributes()
                .FirstOrDefault(a => a.GetType().Name == "LocalizedDescriptionAttribute");
            if (attr == null)
                return null;

            var key = GetAttributeProperty<string>(attr, "ResourceKey");
            if (string.IsNullOrEmpty(key))
                return null;

            return _localization.HasKey(key) ? _localization.GetString(key) : null;
        }

        private string? GetLocalizedDescription(ParameterInfo param)
        {
            var attr = param.GetCustomAttributes()
                .FirstOrDefault(a => a.GetType().Name == "LocalizedDescriptionAttribute");
            if (attr == null)
                return null;

            var key = GetAttributeProperty<string>(attr, "ResourceKey");
            if (string.IsNullOrEmpty(key))
                return null;

            return _localization.HasKey(key) ? _localization.GetString(key) : null;
        }

        private Attribute? GetCommandAttribute(Type type)
        {
            return type.GetCustomAttributes()
                .FirstOrDefault(a => a.GetType().Name == "CommandAttribute") as Attribute;
        }

        private MethodInfo? GetPrimaryMethod(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => HasAttribute(m, "PrimaryAttribute"));
        }

        private List<(MethodInfo Method, Attribute Attr)> GetActions(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(m => (Method: m, Attr: m.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == "ActionAttribute") as Attribute))
                .Where(x => x.Attr != null)
                .Select(x => (x.Method, x.Attr!))
                .ToList();
        }

        private IEnumerable<(ParameterInfo Param, Attribute Attr)> GetOptions(MethodInfo method)
        {
            return method.GetParameters()
                .Select(p => (Param: p, Attr: p.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == "OptionAttribute") as Attribute))
                .Where(x => x.Attr != null)
                .Select(x => (x.Param, x.Attr!));
        }

        private IEnumerable<(ParameterInfo Param, Attribute Attr)> GetArguments(MethodInfo method)
        {
            return method.GetParameters()
                .Select(p => (Param: p, Attr: p.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == "ArgumentAttribute") as Attribute))
                .Where(x => x.Attr != null)
                .Select(x => (x.Param, x.Attr!));
        }

        private bool HasAttribute(MemberInfo member, string attributeName)
        {
            return member.GetCustomAttributes().Any(a => a.GetType().Name == attributeName);
        }

        private bool HasAttribute(ParameterInfo param, string attributeName)
        {
            return param.GetCustomAttributes().Any(a => a.GetType().Name == attributeName);
        }

        private T? GetAttributeProperty<T>(Attribute attr, string propertyName)
        {
            var prop = attr.GetType().GetProperty(propertyName);
            if (prop == null)
                return default;

            var value = prop.GetValue(attr);
            if (value is T typedValue)
                return typedValue;

            return default;
        }
    }
}
