using System;

namespace TeCLI.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class ArgumentAttribute : Attribute
{
    public string? Description { get; set; }

    public ArgumentAttribute()
    {
    }
}