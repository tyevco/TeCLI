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
            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.CommandAttribute}.g.cs", CommandAttributeSource);

            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.ActionAttribute}.g.cs", ActionAttributeSource);

            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.PrimaryAttribute}.g.cs", PrimaryAttributeSource);

            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.OptionAttribute}.g.cs", OptionAttributeSource);

            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.ArgumentAttribute}.g.cs", ArgumentAttributeSource);

            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.GlobalOptionsAttribute}.g.cs", GlobalOptionsAttributeSource);

            // Hook attributes
            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.BeforeExecuteAttribute}.g.cs", Hooks.BeforeExecuteAttributeSource);

            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.AfterExecuteAttribute}.g.cs", Hooks.AfterExecuteAttributeSource);

            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.OnErrorAttribute}.g.cs", Hooks.OnErrorAttributeSource);

            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource("HookInterfaces.g.cs", Hooks.HookInterfacesSource);

            // Type conversion
            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.TypeConverterAttribute}.g.cs", TypeConverters.TypeConverterAttributeSource);

            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource("ITypeConverter.g.cs", TypeConverters.TypeConverterInterfaceSource);

            // Validation attributes

            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.FileExistsAttribute}.g.cs", FileExistsAttributeSource);

            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.DirectoryExistsAttribute}.g.cs", DirectoryExistsAttributeSource);

            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.RangeAttribute}.g.cs", RangeAttributeSource);

            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource($"{AttributeNames.RegularExpressionAttribute}.g.cs", RegularExpressionAttributeSource);

            // String Similarity
            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource("StringSimilarity.g.cs", StringSimilaritySource);

            // Exit code support
            ctx.AddSource("ExitCode.g.cs", ExitCodeSource);
            ctx.AddSource($"{AttributeNames.MapExitCodeAttribute}.g.cs", MapExitCodeAttributeSource);
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
                public string? MutuallyExclusiveSet { get; set; }

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

    private static class Hooks
    {
        public const string BeforeExecuteAttributeSource = """
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

        public const string AfterExecuteAttributeSource = """
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

        public const string OnErrorAttributeSource = """
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


        public const string HookInterfacesSource = """
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif

namespace TeCLI.Hooks
{
    /// <summary>
    /// Context information passed to hooks during command execution
    /// </summary>
    public class HookContext
    {
        /// <summary>
        /// The name of the command being executed
        /// </summary>
        public string CommandName { get; init; } = string.Empty;

        /// <summary>
        /// The name of the action being executed
        /// </summary>
        public string ActionName { get; init; } = string.Empty;

        /// <summary>
        /// The arguments passed to the command
        /// </summary>
        public string[] Arguments { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Additional context data that can be shared between hooks
        /// </summary>
        public Dictionary<string, object> Data { get; init; } = new();

        /// <summary>
        /// Whether execution should be cancelled
        /// </summary>
        public bool IsCancelled { get; set; }

        /// <summary>
        /// Optional cancellation message
        /// </summary>
        public string? CancellationMessage { get; set; }

        /// <summary>
        /// Cancellation token for cooperative cancellation of async operations
        /// </summary>
        public CancellationToken CancellationToken { get; init; } = CancellationToken.None;
    }

    /// <summary>
    /// Hook that executes before an action
    /// </summary>
    public interface IBeforeExecuteHook
    {
        /// <summary>
        /// Executes before the action. Can cancel execution by setting context.IsCancelled = true
        /// </summary>
        Task BeforeExecuteAsync(HookContext context);
    }

    /// <summary>
    /// Hook that executes after an action completes successfully
    /// </summary>
    public interface IAfterExecuteHook
    {
        /// <summary>
        /// Executes after the action completes successfully
        /// </summary>
        /// <param name="context">The execution context</param>
        /// <param name="result">The result returned by the action (if any)</param>
        Task AfterExecuteAsync(HookContext context, object? result);
    }

    /// <summary>
    /// Hook that executes when an error occurs during action execution
    /// </summary>
    public interface IOnErrorHook
    {
        /// <summary>
        /// Executes when an exception occurs during action execution
        /// </summary>
        /// <param name="context">The execution context</param>
        /// <param name="exception">The exception that occurred</param>
        /// <returns>True to suppress the exception, false to rethrow</returns>
        Task<bool> OnErrorAsync(HookContext context, Exception exception);
    }
}
            
""";
    }

    private static class TypeConverters
    {

        public const string TypeConverterAttributeSource = """
using System;

namespace TeCLI.TypeConversion;

/// <summary>
/// Specifies a custom type converter to use for converting command-line argument values.
/// </summary>
/// <remarks>
/// Apply this attribute to parameters or properties that use custom types requiring
/// special conversion logic. The converter type must implement <see cref="ITypeConverter{T}"/>
/// where T matches the parameter's type.
/// </remarks>
/// <example>
/// <code>
/// public class EmailAddress
/// {
///     public string Value { get; }
///     public EmailAddress(string value) => Value = value;
/// }
///
/// public class EmailAddressConverter : ITypeConverter&lt;EmailAddress&gt;
/// {
///     public EmailAddress Convert(string value)
///     {
///         if (!value.Contains("@"))
///             throw new ArgumentException($"Invalid email: {value}");
///         return new EmailAddress(value);
///     }
/// }
///
/// [Action("send")]
/// public void SendEmail(
///     [Option("to")] [TypeConverter(typeof(EmailAddressConverter))] EmailAddress recipient)
/// {
///     // recipient is automatically converted using EmailAddressConverter
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public class TypeConverterAttribute : Attribute
{
    /// <summary>
    /// Gets the type of the converter to use.
    /// </summary>
    /// <value>
    /// A type that implements <see cref="ITypeConverter{T}"/>.
    /// </value>
    public Type ConverterType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeConverterAttribute"/> class.
    /// </summary>
    /// <param name="converterType">
    /// The type of the converter. Must implement <see cref="ITypeConverter{T}"/>
    /// where T matches the type of the parameter this attribute is applied to.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="converterType"/> is null.
    /// </exception>
    public TypeConverterAttribute(Type converterType)
    {
        ConverterType = converterType ?? throw new ArgumentNullException(nameof(converterType));
    }
}

""";

        public const string TypeConverterInterfaceSource = """
using System;

namespace TeCLI.TypeConversion;

/// <summary>
/// Defines a converter for converting string values from command-line arguments to a specific type.
/// </summary>
/// <typeparam name="T">The type to convert to.</typeparam>
/// <remarks>
/// Implement this interface to create custom type converters for types that don't have
/// built-in conversion support. The converter will be used automatically when specified
/// via the <see cref="TypeConverterAttribute"/>.
/// </remarks>
/// <example>
/// <code>
/// public class EmailAddressConverter : ITypeConverter&lt;EmailAddress&gt;
/// {
///     public EmailAddress Convert(string value)
///     {
///         if (string.IsNullOrWhiteSpace(value))
///         {
///             throw new ArgumentException("Email address cannot be empty");
///         }
///
///         if (!value.Contains("@"))
///         {
///             throw new ArgumentException($"Invalid email address: {value}");
///         }
///
///         return new EmailAddress(value);
///     }
/// }
///
/// // Usage:
/// [Action("send")]
/// public void SendEmail(
///     [Option("to")] [TypeConverter(typeof(EmailAddressConverter))] EmailAddress recipient)
/// {
///     // recipient is automatically converted from string
/// }
/// </code>
/// </example>
public interface ITypeConverter<T>
{
    /// <summary>
    /// Converts a string value from command-line arguments to the target type.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>The converted value of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the value cannot be converted to the target type.
    /// The exception message should clearly describe why the conversion failed.
    /// </exception>
    T Convert(string value);
}

""";
    }

    private const string ExitCodeSource = """
using System;

namespace TeCLI
{
    /// <summary>
    /// Standard exit codes for CLI applications.
    /// Actions can return this enum (or any int-based enum) to indicate their exit status.
    /// </summary>
    /// <remarks>
    /// Exit codes 0-63 are reserved for general success/error conditions.
    /// Exit codes 64-78 follow BSD sysexits.h conventions.
    /// Applications can define custom exit codes starting at 100.
    /// </remarks>
    public enum ExitCode
    {
        /// <summary>
        /// Successful execution
        /// </summary>
        Success = 0,

        /// <summary>
        /// General error
        /// </summary>
        Error = 1,

        /// <summary>
        /// Invalid command line arguments
        /// </summary>
        InvalidArguments = 2,

        /// <summary>
        /// File not found
        /// </summary>
        FileNotFound = 3,

        /// <summary>
        /// Permission denied
        /// </summary>
        PermissionDenied = 4,

        /// <summary>
        /// Network error
        /// </summary>
        NetworkError = 5,

        /// <summary>
        /// Operation cancelled
        /// </summary>
        Cancelled = 6,

        /// <summary>
        /// Configuration error
        /// </summary>
        ConfigurationError = 7,

        /// <summary>
        /// Resource not available
        /// </summary>
        ResourceUnavailable = 8,

        // BSD sysexits.h compatible codes (64-78)

        /// <summary>
        /// Command line usage error (EX_USAGE)
        /// </summary>
        Usage = 64,

        /// <summary>
        /// Data format error (EX_DATAERR)
        /// </summary>
        DataError = 65,

        /// <summary>
        /// Cannot open input (EX_NOINPUT)
        /// </summary>
        NoInput = 66,

        /// <summary>
        /// Addressee unknown (EX_NOUSER)
        /// </summary>
        NoUser = 67,

        /// <summary>
        /// Host name unknown (EX_NOHOST)
        /// </summary>
        NoHost = 68,

        /// <summary>
        /// Service unavailable (EX_UNAVAILABLE)
        /// </summary>
        Unavailable = 69,

        /// <summary>
        /// Internal software error (EX_SOFTWARE)
        /// </summary>
        Software = 70,

        /// <summary>
        /// System error (EX_OSERR)
        /// </summary>
        OsError = 71,

        /// <summary>
        /// Critical OS file missing (EX_OSFILE)
        /// </summary>
        OsFile = 72,

        /// <summary>
        /// Can't create output file (EX_CANTCREAT)
        /// </summary>
        CantCreate = 73,

        /// <summary>
        /// I/O error (EX_IOERR)
        /// </summary>
        IoError = 74,

        /// <summary>
        /// Temporary failure (EX_TEMPFAIL)
        /// </summary>
        TempFail = 75,

        /// <summary>
        /// Remote error in protocol (EX_PROTOCOL)
        /// </summary>
        Protocol = 76,

        /// <summary>
        /// Permission denied (EX_NOPERM)
        /// </summary>
        NoPerm = 77,

        /// <summary>
        /// Configuration error (EX_CONFIG)
        /// </summary>
        Config = 78
    }
}

""";

    private const string MapExitCodeAttributeSource = """
using System;

namespace TeCLI.Attributes
{
    /// <summary>
    /// Maps an exception type to a specific exit code.
    /// Apply this attribute to actions to customize how exceptions are converted to exit codes.
    /// </summary>
    /// <remarks>
    /// When an exception is thrown during action execution, the framework will check for
    /// MapExitCode attributes and use the corresponding exit code. If no mapping is found,
    /// a default exit code of 1 (Error) is used.
    /// </remarks>
    /// <example>
    /// <code>
    /// [Action("process")]
    /// [MapExitCode(typeof(FileNotFoundException), ExitCode.FileNotFound)]
    /// [MapExitCode(typeof(UnauthorizedAccessException), ExitCode.PermissionDenied)]
    /// public ExitCode Process([Argument] string file)
    /// {
    ///     // If FileNotFoundException is thrown, exit code 3 is returned
    ///     // If UnauthorizedAccessException is thrown, exit code 4 is returned
    ///     return ExitCode.Success;
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    internal class MapExitCodeAttribute : Attribute
    {
        /// <summary>
        /// Gets the exception type that this mapping applies to.
        /// </summary>
        public Type ExceptionType { get; }

        /// <summary>
        /// Gets the exit code to use when the specified exception type is thrown.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapExitCodeAttribute"/> class.
        /// </summary>
        /// <param name="exceptionType">The exception type to map.</param>
        /// <param name="exitCode">The exit code to use.</param>
        public MapExitCodeAttribute(Type exceptionType, int exitCode)
        {
            ExceptionType = exceptionType ?? throw new ArgumentNullException(nameof(exceptionType));
            ExitCode = exitCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapExitCodeAttribute"/> class using an ExitCode enum value.
        /// </summary>
        /// <param name="exceptionType">The exception type to map.</param>
        /// <param name="exitCode">The exit code enum value to use.</param>
        public MapExitCodeAttribute(Type exceptionType, TeCLI.ExitCode exitCode)
            : this(exceptionType, (int)exitCode)
        {
        }
    }
}

""";

    private const string StringSimilaritySource = """
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeCLI;

/// <summary>
/// Provides string similarity calculations for suggesting corrections to user input.
/// </summary>
public static class StringSimilarity
{
    /// <summary>
    /// Calculates the Damerau-Levenshtein distance between two strings.
    /// This is the minimum number of single-character edits (insertions, deletions,
    /// substitutions, or transpositions of adjacent characters) required to change
    /// one string into another.
    /// </summary>
    /// <param name="source">The source string</param>
    /// <param name="target">The target string</param>
    /// <returns>The Damerau-Levenshtein distance between the two strings</returns>
    public static int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return target?.Length ?? 0;

        if (string.IsNullOrEmpty(target))
            return source.Length;

        int sourceLength = source.Length;
        int targetLength = target.Length;

        // Create a 2D array to store distances
        int[,] distance = new int[sourceLength + 1, targetLength + 1];

        // Initialize first column and row
        for (int i = 0; i <= sourceLength; i++)
            distance[i, 0] = i;

        for (int j = 0; j <= targetLength; j++)
            distance[0, j] = j;

        // Calculate distances using Damerau-Levenshtein algorithm
        for (int i = 1; i <= sourceLength; i++)
        {
            for (int j = 1; j <= targetLength; j++)
            {
                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                distance[i, j] = Math.Min(
                    Math.Min(
                        distance[i - 1, j] + 1,      // deletion
                        distance[i, j - 1] + 1),     // insertion
                    distance[i - 1, j - 1] + cost);  // substitution

                // Transposition: check if swapping adjacent characters helps
                if (i > 1 && j > 1 &&
                    source[i - 1] == target[j - 2] &&
                    source[i - 2] == target[j - 1])
                {
                    distance[i, j] = Math.Min(
                        distance[i, j],
                        distance[i - 2, j - 2] + cost); // transposition
                }
            }
        }

        return distance[sourceLength, targetLength];
    }

    /// <summary>
    /// Finds the most similar string from a collection of candidates.
    /// </summary>
    /// <param name="input">The input string to compare</param>
    /// <param name="candidates">Collection of candidate strings</param>
    /// <param name="maxDistance">Maximum Levenshtein distance to consider (default: 3)</param>
    /// <returns>The most similar string, or null if no candidates are within maxDistance</returns>
    public static string? FindMostSimilar(string input, IEnumerable<string> candidates, int maxDistance = 3)
    {
        if (string.IsNullOrEmpty(input) || candidates == null || !candidates.Any())
            return null;

        string? bestMatch = null;
        int bestDistance = int.MaxValue;

        foreach (var candidate in candidates)
        {
            // Case-insensitive comparison
            int distance = CalculateLevenshteinDistance(
                input.ToLowerInvariant(),
                candidate.ToLowerInvariant());

            if (distance < bestDistance && distance <= maxDistance)
            {
                bestDistance = distance;
                bestMatch = candidate;
            }
        }

        return bestMatch;
    }

    /// <summary>
    /// Finds all similar strings from a collection of candidates within a maximum distance.
    /// </summary>
    /// <param name="input">The input string to compare</param>
    /// <param name="candidates">Collection of candidate strings</param>
    /// <param name="maxDistance">Maximum Levenshtein distance to consider (default: 2)</param>
    /// <param name="maxResults">Maximum number of suggestions to return (default: 3)</param>
    /// <returns>List of similar strings sorted by similarity</returns>
    public static List<string> FindSimilar(string input, IEnumerable<string> candidates, int maxDistance = 2, int maxResults = 3)
    {
        if (string.IsNullOrEmpty(input) || candidates == null || !candidates.Any())
            return new List<string>();

        var matches = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Distance = CalculateLevenshteinDistance(
                    input.ToLowerInvariant(),
                    candidate.ToLowerInvariant())
            })
            .Where(x => x.Distance <= maxDistance)
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Candidate)
            .Take(maxResults)
            .Select(x => x.Candidate)
            .ToList();

        return matches;
    }
}

""";
}
