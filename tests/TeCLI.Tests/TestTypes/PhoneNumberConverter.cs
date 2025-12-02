using System;
using System.Linq;
using TeCLI.TypeConversion;

namespace TeCLI.Tests.TestTypes;

/// <summary>
/// Example custom type converter for testing with collections
/// </summary>
public class PhoneNumberConverter : ITypeConverter<PhoneNumber>
{
    public PhoneNumber Convert(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Phone number cannot be empty");
        }

        // Remove common formatting characters
        var cleaned = new string(value.Where(c => char.IsDigit(c) || c == '+').ToArray());

        if (cleaned.Length < 10)
        {
            throw new ArgumentException($"Invalid phone number: {value}. Phone number must have at least 10 digits.");
        }

        return new PhoneNumber(cleaned);
    }
}
