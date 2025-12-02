namespace TeCLI.Tests.TestTypes;

/// <summary>
/// Example custom type for testing type converters with collections
/// </summary>
public class PhoneNumber
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is PhoneNumber other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
}
