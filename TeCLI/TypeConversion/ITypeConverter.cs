using System;

namespace TeCLI.TypeConversion;

/// <summary>
/// Defines a converter for converting string values from command-line arguments to a specific type.
/// </summary>
/// <typeparam name="T">The type to convert to.</typeparam>
/// <remarks>
/// Implement this interface to create custom type converters for types that don't have
/// built-in conversion support. The converter will be used automatically when specified
/// via the <see cref="TypeConverterAttribute"/>.
/// </remarks>
/// <example>
/// <code>
/// public class EmailAddressConverter : ITypeConverter&lt;EmailAddress&gt;
/// {
///     public EmailAddress Convert(string value)
///     {
///         if (string.IsNullOrWhiteSpace(value))
///         {
///             throw new ArgumentException("Email address cannot be empty");
///         }
///
///         if (!value.Contains("@"))
///         {
///             throw new ArgumentException($"Invalid email address: {value}");
///         }
///
///         return new EmailAddress(value);
///     }
/// }
///
/// // Usage:
/// [Action("send")]
/// public void SendEmail(
///     [Option("to")] [TypeConverter(typeof(EmailAddressConverter))] EmailAddress recipient)
/// {
///     // recipient is automatically converted from string
/// }
/// </code>
/// </example>
public interface ITypeConverter<T>
{
    /// <summary>
    /// Converts a string value from command-line arguments to the target type.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>The converted value of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the value cannot be converted to the target type.
    /// The exception message should clearly describe why the conversion failed.
    /// </exception>
    T Convert(string value);
}
