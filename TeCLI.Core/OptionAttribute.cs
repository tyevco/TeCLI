using System;

namespace TeCLI.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class OptionAttribute : Attribute
{
    public string Name { get; }

    public char ShortName { get; set; } = '\0'; // Optional short name

    public string? Description { get; set; }

    public OptionAttribute(string name)
    {
        Name = name;
    }
}