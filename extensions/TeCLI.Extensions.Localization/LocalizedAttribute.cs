using System;

namespace TeCLI.Localization
{
    /// <summary>
    /// Specifies that a command, action, option, or argument should use localized descriptions.
    /// This attribute works alongside the standard TeCLI attributes to provide resource-based descriptions.
    /// </summary>
    /// <example>
    /// <code>
    /// [Command("greet")]
    /// [LocalizedDescription("GreetCommand_Description")]
    /// public class GreetCommand
    /// {
    ///     [Action("hello")]
    ///     [LocalizedDescription("GreetCommand_Hello_Description")]
    ///     public void Hello(
    ///         [Argument]
    ///         [LocalizedDescription("GreetCommand_Hello_Name_Description")]
    ///         string name) { }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class LocalizedDescriptionAttribute : Attribute
    {
        /// <summary>
        /// Gets the resource key for the description.
        /// </summary>
        public string ResourceKey { get; }

        /// <summary>
        /// Gets or sets the resource type containing the localized strings.
        /// If not specified, uses the default resource provider.
        /// </summary>
        public Type? ResourceType { get; set; }

        /// <summary>
        /// Creates a new LocalizedDescriptionAttribute with the specified resource key.
        /// </summary>
        /// <param name="resourceKey">The resource key for the description.</param>
        public LocalizedDescriptionAttribute(string resourceKey)
        {
            ResourceKey = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey));
        }
    }

    /// <summary>
    /// Specifies the resource key for an option's prompt text.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class LocalizedPromptAttribute : Attribute
    {
        /// <summary>
        /// Gets the resource key for the prompt text.
        /// </summary>
        public string ResourceKey { get; }

        /// <summary>
        /// Gets or sets the resource type containing the localized strings.
        /// </summary>
        public Type? ResourceType { get; set; }

        /// <summary>
        /// Creates a new LocalizedPromptAttribute with the specified resource key.
        /// </summary>
        /// <param name="resourceKey">The resource key for the prompt.</param>
        public LocalizedPromptAttribute(string resourceKey)
        {
            ResourceKey = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey));
        }
    }

    /// <summary>
    /// Specifies the resource key for an error message.
    /// Can be used on validation attributes or custom validators.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = false)]
    public class LocalizedErrorMessageAttribute : Attribute
    {
        /// <summary>
        /// Gets the resource key for the error message.
        /// </summary>
        public string ResourceKey { get; }

        /// <summary>
        /// Gets or sets the resource type containing the localized strings.
        /// </summary>
        public Type? ResourceType { get; set; }

        /// <summary>
        /// Creates a new LocalizedErrorMessageAttribute with the specified resource key.
        /// </summary>
        /// <param name="resourceKey">The resource key for the error message.</param>
        public LocalizedErrorMessageAttribute(string resourceKey)
        {
            ResourceKey = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey));
        }
    }
}
