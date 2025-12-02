namespace TeCLI.Tests.TestTypes;

/// <summary>
/// Example custom type for testing type converters
/// </summary>
public class EmailAddress
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is EmailAddress other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
}
