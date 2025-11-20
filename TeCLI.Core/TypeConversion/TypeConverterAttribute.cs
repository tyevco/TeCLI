using System;

namespace TeCLI.TypeConversion;

/// <summary>
/// Specifies a custom type converter to use for converting command-line argument values.
/// </summary>
/// <remarks>
/// Apply this attribute to parameters or properties that use custom types requiring
/// special conversion logic. The converter type must implement <see cref="ITypeConverter{T}"/>
/// where T matches the parameter's type.
/// </remarks>
/// <example>
/// <code>
/// public class EmailAddress
/// {
///     public string Value { get; }
///     public EmailAddress(string value) => Value = value;
/// }
///
/// public class EmailAddressConverter : ITypeConverter&lt;EmailAddress&gt;
/// {
///     public EmailAddress Convert(string value)
///     {
///         if (!value.Contains("@"))
///             throw new ArgumentException($"Invalid email: {value}");
///         return new EmailAddress(value);
///     }
/// }
///
/// [Action("send")]
/// public void SendEmail(
///     [Option("to")] [TypeConverter(typeof(EmailAddressConverter))] EmailAddress recipient)
/// {
///     // recipient is automatically converted using EmailAddressConverter
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public class TypeConverterAttribute : Attribute
{
    /// <summary>
    /// Gets the type of the converter to use.
    /// </summary>
    /// <value>
    /// A type that implements <see cref="ITypeConverter{T}"/>.
    /// </value>
    public Type ConverterType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeConverterAttribute"/> class.
    /// </summary>
    /// <param name="converterType">
    /// The type of the converter. Must implement <see cref="ITypeConverter{T}"/>
    /// where T matches the type of the parameter this attribute is applied to.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="converterType"/> is null.
    /// </exception>
    public TypeConverterAttribute(Type converterType)
    {
        ConverterType = converterType ?? throw new ArgumentNullException(nameof(converterType));
    }
}
