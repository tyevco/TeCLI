namespace TeCLI
{
    public static class Constants
    {
        /// <summary>
        /// Root namespace for generated code
        /// </summary>
        public const string RootNamespace = "TeCLI";

        /// <summary>
        /// Generated class names
        /// </summary>
        public static class GeneratedClasses
        {
            public const string CommandDispatcher = "CommandDispatcher";
        }

        /// <summary>
        /// Generated method names
        /// </summary>
        public static class GeneratedMethods
        {
            public const string DispatchAsync = "DispatchAsync";
            public const string DisplayApplicationHelp = "DisplayApplicationHelp";
            public const string InvokeCommandAction = "InvokeCommandAction";
            public const string InvokeCommandActionAsync = "InvokeCommandActionAsync";
        }

        /// <summary>
        /// Attribute class names (without "Attribute" suffix)
        /// </summary>
        public static class AttributeNames
        {
            public const string Command = "CommandAttribute";
            public const string Action = "ActionAttribute";
            public const string Primary = "PrimaryAttribute";
            public const string Option = "OptionAttribute";
            public const string Argument = "ArgumentAttribute";
        }

        /// <summary>
        /// Build properties
        /// </summary>
        public static class BuildProperties
        {
            public const string InvokerLibrary = $"{nameof(TeCLI)}_{nameof(InvokerLibrary)}";
        }

        /// <summary>
        /// Standard error message templates for generated code
        /// </summary>
        /// <remarks>
        /// Error Handling Strategy:
        /// - Use ArgumentException for user input errors (missing/invalid parameters, options, arguments)
        /// - Use InvalidOperationException for configuration errors (missing primary action, etc.)
        /// - Always include helpful context about what went wrong and how to fix it
        /// </remarks>
        public static class ErrorMessages
        {
            // ArgumentException messages (user input errors)
            public const string RequiredParametersNotProvided = "Required parameters not provided. Use --help for usage information.";
            public const string RequiredOptionNotProvided = "Required option '--{0}' not provided.";
            public const string InvalidOptionValue = "Invalid value provided for option '--{0}'.";
            public const string RequiredArgumentNotProvided = "Required argument '{0}' not provided.";
            public const string InvalidArgumentSyntax = "Invalid syntax provided for argument '{0}'.";

            // InvalidOperationException messages (configuration errors)
            public const string NoPrimaryActionDefined = "No primary action defined for command '{0}'. Use --help for available actions.";

            // Unknown command/action messages with suggestions
            public const string UnknownCommand = "Unknown command: {0}";
            public const string UnknownCommandWithSuggestion = "Unknown command: {0}\nDid you mean '{1}'?";
            public const string UnknownAction = "Unknown action: {0}";
            public const string UnknownActionWithSuggestion = "Unknown action: {0}\nDid you mean '{1}'?";
            public const string UnknownOption = "Unknown option: {0}";
            public const string UnknownOptionWithSuggestion = "Unknown option: {0}\nDid you mean '{1}'?";
        }
    }
}
