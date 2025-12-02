using System;

namespace TeCLI.Output;

/// <summary>
/// Enables structured output formatting for a command action.
/// When applied to a method, the framework automatically adds an --output option
/// that allows users to specify the output format (json, xml, yaml, table).
/// </summary>
/// <remarks>
/// The method should return data (single object or IEnumerable) that will be
/// formatted according to the selected output format. If no --output option
/// is specified, the default format will be used.
/// </remarks>
/// <example>
/// <code>
/// [Command("list")]
/// public class ListCommand
/// {
///     [Action("users")]
///     [OutputFormat]  // Enables --output json|xml|table|yaml
///     public IEnumerable&lt;User&gt; ListUsers()
///     {
///         return _userService.GetAll();
///     }
/// }
///
/// // Usage:
/// // myapp list users --output json
/// // myapp list users --output table
/// // myapp list users -o yaml
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class OutputFormatAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the default output format when --output is not specified.
    /// Defaults to <see cref="OutputFormat.Table"/>.
    /// </summary>
    public OutputFormat DefaultFormat { get; set; } = OutputFormat.Table;

    /// <summary>
    /// Gets or sets whether to use indented/pretty formatting for JSON and XML output.
    /// Defaults to true.
    /// </summary>
    public bool Indent { get; set; } = true;

    /// <summary>
    /// Gets or sets the long option name. Defaults to "output".
    /// </summary>
    public string OptionName { get; set; } = "output";

    /// <summary>
    /// Gets or sets the short option name. Defaults to 'o'.
    /// </summary>
    public char ShortName { get; set; } = 'o';

    /// <summary>
    /// Gets or sets the description for the output option.
    /// </summary>
    public string Description { get; set; } = "Output format (json, xml, yaml, table)";

    /// <summary>
    /// Gets or sets which formats are available for this action.
    /// If null, all formats are available.
    /// </summary>
    public OutputFormat[]? AvailableFormats { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutputFormatAttribute"/> class
    /// with default settings.
    /// </summary>
    public OutputFormatAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutputFormatAttribute"/> class
    /// with a specified default format.
    /// </summary>
    /// <param name="defaultFormat">The default output format.</param>
    public OutputFormatAttribute(OutputFormat defaultFormat)
    {
        DefaultFormat = defaultFormat;
    }
}
