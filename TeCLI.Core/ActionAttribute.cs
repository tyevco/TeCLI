using System;

namespace TeCLI.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ActionAttribute : Attribute
{
    public string Name { get; }
    public string? Description { get; set; }

    public ActionAttribute(string name)
    {
        Name = name;
    }
}