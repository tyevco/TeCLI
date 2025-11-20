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
    }
}
