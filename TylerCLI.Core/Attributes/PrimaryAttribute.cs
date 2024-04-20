using System;

namespace TylerCLI.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class PrimaryAttribute : Attribute
{
    public PrimaryAttribute()
    {
    }
}