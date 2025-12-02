using Microsoft.CodeAnalysis;

namespace TeCLI.Generators;

/// <summary>
/// Generates embedded attribute definitions for TeCLI.
/// This allows users to use TeCLI attributes without referencing a separate attributes assembly.
/// </summary>
public partial class CommandLineArgsGenerator
{
    /// <summary>
    /// Registers embedded attribute definitions with the generator initialization context.
    /// </summary>
    private static void RegisterEmbeddedAttributes(IncrementalGeneratorInitializationContext context)
    {
        // Core attributes
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("CommandAttribute.g.cs", CommandAttributeSource);
            ctx.AddSource("ActionAttribute.g.cs", ActionAttributeSource);
            ctx.AddSource("PrimaryAttribute.g.cs", PrimaryAttributeSource);
            ctx.AddSource("OptionAttribute.g.cs", OptionAttributeSource);
            ctx.AddSource("ArgumentAttribute.g.cs", ArgumentAttributeSource);
            ctx.AddSource("GlobalOptionsAttribute.g.cs", GlobalOptionsAttributeSource);

            // Hook attributes
            ctx.AddSource("BeforeExecuteAttribute.g.cs", BeforeExecuteAttributeSource);
            ctx.AddSource("AfterExecuteAttribute.g.cs", AfterExecuteAttributeSource);
            ctx.AddSource("OnErrorAttribute.g.cs", OnErrorAttributeSource);

            // Type conversion
            ctx.AddSource("TypeConverterAttribute.g.cs", TypeConverterAttributeSource);

            // Validation attributes
            ctx.AddSource("FileExistsAttribute.g.cs", FileExistsAttributeSource);
            ctx.AddSource("DirectoryExistsAttribute.g.cs", DirectoryExistsAttributeSource);
            ctx.AddSource("RangeAttribute.g.cs", RangeAttributeSource);
            ctx.AddSource("RegularExpressionAttribute.g.cs", RegularExpressionAttributeSource);
        });
    }

    private const string CommandAttributeSource = """
        using System;

        namespace TeCLI.Attributes
        {
            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
            internal class CommandAttribute : Attribute
            {
                public string Name { get; }
                public string? Description { get; set; }
                public string[]? Aliases { get; set; }

                public CommandAttribute(string name)
                {
                    Name = name;
                }
            }
        }
        """;

    private const string ActionAttributeSource = """
        using System;

        namespace TeCLI.Attributes
        {
            [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
            internal class ActionAttribute : Attribute
            {
                public string Name { get; }
                public string? Description { get; set; }
                public string[]? Aliases { get; set; }

                public ActionAttribute(string name)
                {
                    Name = name;
                }
            }
        }
        """;

    private const string PrimaryAttributeSource = """
        using System;

        namespace TeCLI.Attributes
        {
            [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
            internal class PrimaryAttribute : Attribute
            {
                public PrimaryAttribute() { }
            }
        }
        """;

    private const string OptionAttributeSource = """
        using System;

        namespace TeCLI.Attributes
        {
            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
            internal class OptionAttribute : Attribute
            {
                public string Name { get; }
                public char ShortName { get; set; } = '\0';
                public string? Description { get; set; }
                public bool Required { get; set; } = false;
                public string? EnvVar { get; set; }
                public string? Prompt { get; set; }
                public bool SecurePrompt { get; set; } = false;

                public OptionAttribute(string name)
                {
                    Name = name;
                }
            }
        }
        """;

    private const string ArgumentAttributeSource = """
        using System;

        namespace TeCLI.Attributes
        {
            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
            internal class ArgumentAttribute : Attribute
            {
                public string? Description { get; set; }
                public string? Prompt { get; set; }
                public bool SecurePrompt { get; set; } = false;

                public ArgumentAttribute() { }
            }
        }
        """;

    private const string GlobalOptionsAttributeSource = """
        using System;

        namespace TeCLI.Attributes
        {
            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            internal class GlobalOptionsAttribute : Attribute
            {
            }
        }
        """;

    private const string BeforeExecuteAttributeSource = """
        using System;

        namespace TeCLI.Attributes
        {
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
            internal class BeforeExecuteAttribute : Attribute
            {
                public Type HookType { get; }
                public int Order { get; set; } = 0;

                public BeforeExecuteAttribute(Type hookType)
                {
                    HookType = hookType ?? throw new ArgumentNullException(nameof(hookType));
                }
            }
        }
        """;

    private const string AfterExecuteAttributeSource = """
        using System;

        namespace TeCLI.Attributes
        {
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
            internal class AfterExecuteAttribute : Attribute
            {
                public Type HookType { get; }
                public int Order { get; set; } = 0;

                public AfterExecuteAttribute(Type hookType)
                {
                    HookType = hookType ?? throw new ArgumentNullException(nameof(hookType));
                }
            }
        }
        """;

    private const string OnErrorAttributeSource = """
        using System;

        namespace TeCLI.Attributes
        {
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
            internal class OnErrorAttribute : Attribute
            {
                public Type HookType { get; }
                public int Order { get; set; } = 0;

                public OnErrorAttribute(Type hookType)
                {
                    HookType = hookType ?? throw new ArgumentNullException(nameof(hookType));
                }
            }
        }
        """;

    private const string TypeConverterAttributeSource = """
        using System;

        namespace TeCLI.TypeConversion
        {
            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
            internal class TypeConverterAttribute : Attribute
            {
                public Type ConverterType { get; }

                public TypeConverterAttribute(Type converterType)
                {
                    ConverterType = converterType ?? throw new ArgumentNullException(nameof(converterType));
                }
            }
        }
        """;

    private const string FileExistsAttributeSource = """
        using System;
        using System.IO;

        namespace TeCLI.Attributes.Validation
        {
            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
            internal class FileExistsAttribute : Attribute
            {
                public string? ErrorMessage { get; set; }

                public void Validate(object value, string parameterName)
                {
                    if (value == null) return;

                    string filePath;
                    if (value is FileInfo fileInfo)
                        filePath = fileInfo.FullName;
                    else if (value is string path)
                        filePath = path;
                    else
                        throw new ArgumentException($"FileExists can only be applied to string or FileInfo.");

                    if (!File.Exists(filePath))
                    {
                        string message = ErrorMessage ?? $"File '{filePath}' specified for '{parameterName}' does not exist.";
                        throw new ArgumentException(message);
                    }
                }
            }
        }
        """;

    private const string DirectoryExistsAttributeSource = """
        using System;
        using System.IO;

        namespace TeCLI.Attributes.Validation
        {
            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
            internal class DirectoryExistsAttribute : Attribute
            {
                public string? ErrorMessage { get; set; }

                public void Validate(object value, string parameterName)
                {
                    if (value == null) return;

                    string dirPath;
                    if (value is DirectoryInfo dirInfo)
                        dirPath = dirInfo.FullName;
                    else if (value is string path)
                        dirPath = path;
                    else
                        throw new ArgumentException($"DirectoryExists can only be applied to string or DirectoryInfo.");

                    if (!Directory.Exists(dirPath))
                    {
                        string message = ErrorMessage ?? $"Directory '{dirPath}' specified for '{parameterName}' does not exist.";
                        throw new ArgumentException(message);
                    }
                }
            }
        }
        """;

    private const string RangeAttributeSource = """
        using System;

        namespace TeCLI.Attributes.Validation
        {
            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
            internal class RangeAttribute : Attribute
            {
                public double Minimum { get; }
                public double Maximum { get; }
                public string? ErrorMessage { get; set; }

                public RangeAttribute(double minimum, double maximum)
                {
                    Minimum = minimum;
                    Maximum = maximum;
                }

                public RangeAttribute(int minimum, int maximum)
                {
                    Minimum = minimum;
                    Maximum = maximum;
                }

                public void Validate(object value, string parameterName)
                {
                    if (value == null) return;

                    double numValue = Convert.ToDouble(value);
                    if (numValue < Minimum || numValue > Maximum)
                    {
                        string message = ErrorMessage ?? $"Value {numValue} for '{parameterName}' must be between {Minimum} and {Maximum}.";
                        throw new ArgumentException(message);
                    }
                }
            }
        }
        """;

    private const string RegularExpressionAttributeSource = """
        using System;
        using System.Text.RegularExpressions;

        namespace TeCLI.Attributes.Validation
        {
            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
            internal class RegularExpressionAttribute : Attribute
            {
                public string Pattern { get; }
                public string? ErrorMessage { get; set; }

                public RegularExpressionAttribute(string pattern)
                {
                    Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
                }

                public void Validate(object value, string parameterName)
                {
                    if (value == null) return;

                    string stringValue = value.ToString() ?? string.Empty;
                    if (!Regex.IsMatch(stringValue, Pattern))
                    {
                        string message = ErrorMessage ?? $"Value '{stringValue}' for '{parameterName}' does not match pattern '{Pattern}'.";
                        throw new ArgumentException(message);
                    }
                }
            }
        }
        """;
}
