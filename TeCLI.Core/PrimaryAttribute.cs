using System;

namespace TeCLI.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class PrimaryAttribute : Attribute
{
    public PrimaryAttribute()
    {
    }
}