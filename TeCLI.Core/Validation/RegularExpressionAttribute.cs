using System;
using System.Text.RegularExpressions;

namespace TeCLI.Attributes.Validation;

/// <summary>
/// Validates that a string value matches a specified regular expression pattern.
/// </summary>
/// <remarks>
/// This attribute can be applied to string parameters to ensure they match
/// a specific pattern defined by a regular expression.
/// </remarks>
/// <example>
/// <code>
/// [Action("create-user")]
/// public void CreateUser(
///     [Option("username")] [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$")] string username,
///     [Option("email")] [RegularExpression(@"^[^@]+@[^@]+\.[^@]+$")] string email)
/// {
///     // username must be 3-20 alphanumeric characters or underscores
///     // email must match basic email format
/// }
/// </code>
/// Usage: <c>myapp create-user --username john_doe --email john@example.com</c>
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public class RegularExpressionAttribute : Attribute
{
    /// <summary>
    /// Gets the regular expression pattern that the value must match.
    /// </summary>
    public string Pattern { get; }

    /// <summary>
    /// Gets or sets the error message to display when validation fails.
    /// If not set, a default message will be generated.
    /// </summary>
    public string? ErrorMessage { get; set; }

    private readonly Regex _regex;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegularExpressionAttribute"/> class.
    /// </summary>
    /// <param name="pattern">The regular expression pattern that the value must match.</param>
    /// <exception cref="ArgumentException">Thrown when the pattern is null or empty.</exception>
    public RegularExpressionAttribute(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Pattern cannot be null or empty.", nameof(pattern));
        }

        Pattern = pattern;
        _regex = new Regex(pattern, RegexOptions.Compiled);
    }

    /// <summary>
    /// Validates that the specified value matches the regular expression pattern.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the value doesn't match the pattern.</exception>
    public void Validate(object value, string parameterName)
    {
        if (value == null)
        {
            return; // null values are handled by Required validation
        }

        string stringValue = value.ToString()!;

        if (!_regex.IsMatch(stringValue))
        {
            string message = ErrorMessage ??
                $"Value '{stringValue}' for '{parameterName}' does not match the required pattern '{Pattern}'.";
            throw new ArgumentException(message);
        }
    }
}
