using System;
using TeCLI.TypeConversion;

namespace TeCLI.Tests.TestTypes;

/// <summary>
/// Example custom type converter for testing
/// </summary>
public class EmailAddressConverter : ITypeConverter<EmailAddress>
{
    public EmailAddress Convert(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email address cannot be empty");
        }

        if (!value.Contains("@"))
        {
            throw new ArgumentException($"Invalid email address: {value}. Email must contain '@'.");
        }

        var parts = value.Split('@');
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            throw new ArgumentException($"Invalid email address format: {value}");
        }

        return new EmailAddress(value);
    }
}
