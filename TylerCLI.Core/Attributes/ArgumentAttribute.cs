using System;

namespace TylerCLI.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class ArgumentAttribute : Attribute
{
    public string? Description { get; set; }

    public ArgumentAttribute()
    {
    }
}