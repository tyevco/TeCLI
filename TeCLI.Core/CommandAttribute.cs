using System;

namespace TeCLI.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
    public string Name { get; }

    public string? Description { get; set; }

    public CommandAttribute(string name)
    {
        Name = name;
    }
}