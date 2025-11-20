using System;

namespace TeCLI.Attributes;

/// <summary>
/// Marks a class as containing global options that are available across all commands.
/// Global options are parsed before command dispatch and can be injected into any action method.
/// </summary>
/// <example>
/// <code>
/// [GlobalOptions]
/// public class GlobalOptions
/// {
///     [Option("verbose", ShortName = 'v')]
///     public bool Verbose { get; set; }
///
///     [Option("config")]
///     public string? ConfigFile { get; set; }
/// }
///
/// [Command("build")]
/// public class BuildCommand
/// {
///     [Primary]
///     public void Build(GlobalOptions globals, [Option("output")] string output)
///     {
///         if (globals.Verbose)
///             Console.WriteLine($"Building to {output}...");
///     }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class GlobalOptionsAttribute : Attribute
{
}
