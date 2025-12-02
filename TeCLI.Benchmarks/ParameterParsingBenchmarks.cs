using BenchmarkDotNet.Attributes;
using TeCLI.Attributes;
using TeCLI.Attributes.Validation;

namespace TeCLI.Benchmarks;

/// <summary>
/// Benchmarks for complex parameter parsing scenarios.
/// Tests various parameter types, validations, and parsing complexity.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class ParameterParsingBenchmarks
{
    private CommandDispatcher _dispatcher = null!;

    // Pre-allocated argument arrays
    private string[] _noParametersArgs = null!;
    private string[] _singleStringArgs = null!;
    private string[] _multipleStringsArgs = null!;
    private string[] _numericTypesArgs = null!;
    private string[] _booleanFlagsArgs = null!;
    private string[] _enumParsingArgs = null!;
    private string[] _dateTimeParsingArgs = null!;
    private string[] _collectionParsingArgs = null!;
    private string[] _mixedOptionsAndArgsArgs = null!;
    private string[] _validationRequiredArgs = null!;
    private string[] _allParameterTypesArgs = null!;
    private string[] _shortNameOptionsArgs = null!;

    [GlobalSetup]
    public void Setup()
    {
        _dispatcher = new CommandDispatcher();

        // No parameters
        _noParametersArgs = new[] { "param-none" };

        // Single string parameter
        _singleStringArgs = new[] { "param-string", "hello" };

        // Multiple string parameters
        _multipleStringsArgs = new[] { "param-strings", "first", "second", "third" };

        // Numeric types (int, long, double, decimal)
        _numericTypesArgs = new[] { "param-numeric", "--int", "42", "--long", "9999999999", "--double", "3.14159", "--decimal", "123.456" };

        // Boolean flags (various forms)
        _booleanFlagsArgs = new[] { "param-flags", "-v", "--force", "-d" };

        // Enum parsing
        _enumParsingArgs = new[] { "param-enum", "--level", "Warning", "--format", "Json" };

        // DateTime parsing
        _dateTimeParsingArgs = new[] { "param-datetime", "--date", "2024-06-15", "--time", "14:30:00", "--duration", "01:30:00" };

        // Collection/array parsing
        _collectionParsingArgs = new[] { "param-collection", "--items", "a,b,c,d,e", "--numbers", "1,2,3,4,5" };

        // Mixed positional arguments and named options
        _mixedOptionsAndArgsArgs = new[] { "param-mixed", "source.txt", "dest.txt", "--verbose", "-f", "--retries", "3" };

        // Validation with Range and Regex
        _validationRequiredArgs = new[] { "param-validation", "--port", "8080", "--email", "test@example.com", "--count", "50" };

        // All parameter types combined
        _allParameterTypesArgs = new[]
        {
            "param-complex",
            "inputfile.txt",
            "--output", "outputfile.txt",
            "--count", "100",
            "--rate", "0.95",
            "--enabled",
            "-v",
            "--level", "Error",
            "--tags", "prod,release,v2",
            "--timeout", "00:05:00"
        };

        // Short name options
        _shortNameOptionsArgs = new[] { "param-short", "-n", "test", "-c", "42", "-r", "3.14", "-e", "-v" };
    }

    [Benchmark(Baseline = true, Description = "No parameters")]
    public async Task<int> NoParameters()
    {
        return await _dispatcher.DispatchAsync(_noParametersArgs);
    }

    [Benchmark(Description = "Single string argument")]
    public async Task<int> SingleStringParameter()
    {
        return await _dispatcher.DispatchAsync(_singleStringArgs);
    }

    [Benchmark(Description = "Multiple string arguments")]
    public async Task<int> MultipleStringParameters()
    {
        return await _dispatcher.DispatchAsync(_multipleStringsArgs);
    }

    [Benchmark(Description = "Numeric type parsing")]
    public async Task<int> NumericTypeParsing()
    {
        return await _dispatcher.DispatchAsync(_numericTypesArgs);
    }

    [Benchmark(Description = "Boolean flags")]
    public async Task<int> BooleanFlagParsing()
    {
        return await _dispatcher.DispatchAsync(_booleanFlagsArgs);
    }

    [Benchmark(Description = "Enum parsing")]
    public async Task<int> EnumParsing()
    {
        return await _dispatcher.DispatchAsync(_enumParsingArgs);
    }

    [Benchmark(Description = "DateTime/TimeSpan parsing")]
    public async Task<int> DateTimeParsing()
    {
        return await _dispatcher.DispatchAsync(_dateTimeParsingArgs);
    }

    [Benchmark(Description = "Collection/array parsing")]
    public async Task<int> CollectionParsing()
    {
        return await _dispatcher.DispatchAsync(_collectionParsingArgs);
    }

    [Benchmark(Description = "Mixed args and options")]
    public async Task<int> MixedArgsAndOptions()
    {
        return await _dispatcher.DispatchAsync(_mixedOptionsAndArgsArgs);
    }

    [Benchmark(Description = "With validation")]
    public async Task<int> WithValidation()
    {
        return await _dispatcher.DispatchAsync(_validationRequiredArgs);
    }

    [Benchmark(Description = "Complex all types")]
    public async Task<int> ComplexAllTypes()
    {
        return await _dispatcher.DispatchAsync(_allParameterTypesArgs);
    }

    [Benchmark(Description = "Short name options")]
    public async Task<int> ShortNameOptions()
    {
        return await _dispatcher.DispatchAsync(_shortNameOptionsArgs);
    }
}

#region Parameter Parsing Test Commands

/// <summary>
/// Command with no parameters
/// </summary>
[Command("param-none", Description = "No parameters command")]
public class ParamNoneCommand
{
    [Primary]
    [Action("execute")]
    public void Execute()
    {
        // No-op
    }
}

/// <summary>
/// Command with single string argument
/// </summary>
[Command("param-string", Description = "Single string parameter")]
public class ParamStringCommand
{
    [Primary]
    [Action("execute")]
    public void Execute([Argument(Description = "Input value")] string input)
    {
        // No-op
    }
}

/// <summary>
/// Command with multiple string arguments
/// </summary>
[Command("param-strings", Description = "Multiple string parameters")]
public class ParamStringsCommand
{
    [Primary]
    [Action("execute")]
    public void Execute(
        [Argument(Description = "First value")] string first,
        [Argument(Description = "Second value")] string second,
        [Argument(Description = "Third value")] string third)
    {
        // No-op
    }
}

/// <summary>
/// Command with various numeric types
/// </summary>
[Command("param-numeric", Description = "Numeric type parameters")]
public class ParamNumericCommand
{
    [Primary]
    [Action("execute")]
    public void Execute(
        [Option("int", ShortName = 'i', Description = "Int value")] int intValue = 0,
        [Option("long", ShortName = 'l', Description = "Long value")] long longValue = 0L,
        [Option("double", ShortName = 'd', Description = "Double value")] double doubleValue = 0.0,
        [Option("decimal", ShortName = 'm', Description = "Decimal value")] decimal decimalValue = 0m)
    {
        // No-op
    }
}

/// <summary>
/// Command with boolean flags
/// </summary>
[Command("param-flags", Description = "Boolean flag parameters")]
public class ParamFlagsCommand
{
    [Primary]
    [Action("execute")]
    public void Execute(
        [Option("verbose", ShortName = 'v', Description = "Verbose output")] bool verbose = false,
        [Option("force", ShortName = 'f', Description = "Force operation")] bool force = false,
        [Option("debug", ShortName = 'd', Description = "Debug mode")] bool debug = false,
        [Option("quiet", ShortName = 'q', Description = "Quiet mode")] bool quiet = false)
    {
        // No-op
    }
}

/// <summary>
/// Enum types for testing
/// </summary>
public enum LogLevelBench { Debug, Info, Warning, Error, Critical }
public enum OutputFormatBench { Text, Json, Xml, Yaml }

/// <summary>
/// Command with enum parameters
/// </summary>
[Command("param-enum", Description = "Enum parameter parsing")]
public class ParamEnumCommand
{
    [Primary]
    [Action("execute")]
    public void Execute(
        [Option("level", ShortName = 'l', Description = "Log level")] LogLevelBench level = LogLevelBench.Info,
        [Option("format", ShortName = 'f', Description = "Output format")] OutputFormatBench format = OutputFormatBench.Text)
    {
        // No-op
    }
}

/// <summary>
/// Command with DateTime, TimeSpan parameters
/// </summary>
[Command("param-datetime", Description = "DateTime/TimeSpan parsing")]
public class ParamDateTimeCommand
{
    [Primary]
    [Action("execute")]
    public void Execute(
        [Option("date", ShortName = 'd', Description = "Date value")] DateTime? date = null,
        [Option("time", Description = "Time value")] TimeSpan? time = null,
        [Option("duration", Description = "Duration value")] TimeSpan? duration = null)
    {
        // No-op
    }
}

/// <summary>
/// Command with collection/array parameters
/// </summary>
[Command("param-collection", Description = "Collection parameter parsing")]
public class ParamCollectionCommand
{
    [Primary]
    [Action("execute")]
    public void Execute(
        [Option("items", ShortName = 'i', Description = "String items")] string[]? items = null,
        [Option("numbers", ShortName = 'n', Description = "Number items")] int[]? numbers = null)
    {
        // No-op
    }
}

/// <summary>
/// Command with mixed positional arguments and named options
/// </summary>
[Command("param-mixed", Description = "Mixed args and options")]
public class ParamMixedCommand
{
    [Primary]
    [Action("execute")]
    public void Execute(
        [Argument(Description = "Source file")] string source,
        [Argument(Description = "Destination file")] string destination,
        [Option("verbose", ShortName = 'v', Description = "Verbose output")] bool verbose = false,
        [Option("force", ShortName = 'f', Description = "Force overwrite")] bool force = false,
        [Option("retries", ShortName = 'r', Description = "Number of retries")] int retries = 1)
    {
        // No-op
    }
}

/// <summary>
/// Command with validation attributes
/// </summary>
[Command("param-validation", Description = "Parameter validation")]
public class ParamValidationCommand
{
    [Primary]
    [Action("execute")]
    public void Execute(
        [Option("port", ShortName = 'p', Description = "Port number")]
        [Range(1, 65535)]
        int port = 80,

        [Option("email", ShortName = 'e', Description = "Email address")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        string? email = null,

        [Option("count", ShortName = 'c', Description = "Count value")]
        [Range(1, 100)]
        int count = 10)
    {
        // No-op
    }
}

/// <summary>
/// Command with complex combination of all parameter types
/// </summary>
[Command("param-complex", Description = "Complex parameter combination")]
public class ParamComplexCommand
{
    [Primary]
    [Action("execute")]
    public void Execute(
        [Argument(Description = "Input file")] string inputFile,
        [Option("output", ShortName = 'o', Description = "Output file")] string output = "output.txt",
        [Option("count", ShortName = 'c', Description = "Count")] int count = 1,
        [Option("rate", ShortName = 'r', Description = "Rate")] double rate = 1.0,
        [Option("enabled", ShortName = 'e', Description = "Enabled flag")] bool enabled = false,
        [Option("verbose", ShortName = 'v', Description = "Verbose flag")] bool verbose = false,
        [Option("level", ShortName = 'l', Description = "Log level")] LogLevelBench level = LogLevelBench.Info,
        [Option("tags", ShortName = 't', Description = "Tags")] string[]? tags = null,
        [Option("timeout", Description = "Timeout duration")] TimeSpan? timeout = null)
    {
        // No-op
    }
}

/// <summary>
/// Command with short name options
/// </summary>
[Command("param-short", Description = "Short name options")]
public class ParamShortCommand
{
    [Primary]
    [Action("execute")]
    public void Execute(
        [Option("name", ShortName = 'n', Description = "Name")] string name = "default",
        [Option("count", ShortName = 'c', Description = "Count")] int count = 0,
        [Option("rate", ShortName = 'r', Description = "Rate")] double rate = 0.0,
        [Option("enabled", ShortName = 'e', Description = "Enabled")] bool enabled = false,
        [Option("verbose", ShortName = 'v', Description = "Verbose")] bool verbose = false)
    {
        // No-op
    }
}

#endregion
