using System;

namespace TeCLI.Attributes.Validation;

/// <summary>
/// Validates that a numeric value falls within a specified range.
/// </summary>
/// <remarks>
/// This attribute can be applied to numeric parameters (int, long, double, float, decimal)
/// to ensure they fall within the specified minimum and maximum bounds (inclusive).
/// </remarks>
/// <example>
/// <code>
/// [Action("connect")]
/// public void Connect(
///     [Option("port")] [Range(1, 65535)] int port,
///     [Option("timeout")] [Range(0, 3600)] int timeoutSeconds = 30)
/// {
///     // port must be between 1 and 65535 (inclusive)
///     // timeoutSeconds must be between 0 and 3600 (inclusive)
/// }
/// </code>
/// Usage: <c>myapp connect --port 8080 --timeout 60</c>
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public class RangeAttribute : Attribute
{
    /// <summary>
    /// Gets the minimum allowed value (inclusive).
    /// </summary>
    public double Minimum { get; }

    /// <summary>
    /// Gets the maximum allowed value (inclusive).
    /// </summary>
    public double Maximum { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeAttribute"/> class.
    /// </summary>
    /// <param name="minimum">The minimum allowed value (inclusive).</param>
    /// <param name="maximum">The maximum allowed value (inclusive).</param>
    /// <exception cref="ArgumentException">Thrown when minimum is greater than maximum.</exception>
    public RangeAttribute(double minimum, double maximum)
    {
        if (minimum > maximum)
        {
            throw new ArgumentException($"Minimum value ({minimum}) cannot be greater than maximum value ({maximum}).");
        }

        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Validates that the specified value is within the allowed range.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the value is outside the allowed range.</exception>
    public void Validate(object value, string parameterName)
    {
        if (value == null)
        {
            return; // null values are handled by Required validation
        }

        double numericValue = Convert.ToDouble(value);

        if (numericValue < Minimum || numericValue > Maximum)
        {
            throw new ArgumentException(
                $"Value {numericValue} for '{parameterName}' is outside the allowed range [{Minimum}, {Maximum}].");
        }
    }
}
