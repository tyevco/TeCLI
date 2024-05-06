using System;

namespace TeCLI.Extensions;

public static class AttributeExtensions
{
    // Single generic type
    public static string[] GetAttributeNamesForComparison<T1>()
        where T1 : Attribute
    {
        return GetNames([typeof(T1)]);
    }

    // Two generic types
    public static string[] GetAttributeNamesForComparison<T1, T2>()
        where T1 : Attribute
        where T2 : Attribute
    {
        return GetNames([typeof(T1), typeof(T2)]);
    }

    // Three generic types
    public static string[] GetAttributeNamesForComparison<T1, T2, T3>()
        where T1 : Attribute
        where T2 : Attribute
        where T3 : Attribute
    {
        return GetNames([typeof(T1), typeof(T2), typeof(T3)]);
    }

    // Four generic types
    public static string[] GetAttributeNamesForComparison<T1, T2, T3, T4>()
        where T1 : Attribute
        where T2 : Attribute
        where T3 : Attribute
        where T4 : Attribute
    {
        return GetNames([typeof(T1), typeof(T2), typeof(T3), typeof(T4)]);
    }

    // Helper method to extract attribute names
    private static string[] GetNames(Type[] types)
    {
        var names = new string[types.Length * 2];
        for (int i = 0, j = 0; i < types.Length; i++)
        {
            string name = types[i].Name;
            names[j++] = name;
            names[j++] = name.EndsWith("Attribute") ? name.Substring(0, name.Length - 9) : $"{name}Attribute";
        }
        return names;
    }
}
