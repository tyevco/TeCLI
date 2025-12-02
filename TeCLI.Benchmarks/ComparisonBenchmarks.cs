using System.CommandLine;
using System.CommandLine.Parsing;
using BenchmarkDotNet.Attributes;
using CommandLine;
using TeCLI.Attributes;

namespace TeCLI.Benchmarks;

/// <summary>
/// Benchmarks comparing TeCLI with other popular CLI libraries:
/// - System.CommandLine (Microsoft's official CLI library)
/// - CommandLineParser (popular community library)
///
/// All three libraries are tested with equivalent functionality to ensure fair comparison.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class ComparisonBenchmarks
{
    #region Setup

    // TeCLI dispatcher
    private CommandDispatcher _tecliDispatcher = null!;

    // System.CommandLine root command and parser
    private RootCommand _sclRootCommand = null!;
    private Parser _sclParser = null!;

    // CommandLineParser parser
    private CommandLine.Parser _clpParser = null!;

    // Test arguments for different scenarios
    private string[] _simpleArgs = null!;
    private string[] _withOptionsArgs = null!;
    private string[] _complexArgs = null!;
    private string[] _helpArgs = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Initialize TeCLI
        _tecliDispatcher = new CommandDispatcher();

        // Initialize System.CommandLine
        SetupSystemCommandLine();

        // Initialize CommandLineParser
        _clpParser = new CommandLine.Parser(settings =>
        {
            settings.CaseInsensitiveEnumValues = true;
            settings.HelpWriter = TextWriter.Null; // Suppress output
        });

        // Test argument arrays
        _simpleArgs = new[] { "compare-simple", "execute" };
        _withOptionsArgs = new[] { "compare-options", "--name", "test", "--count", "42", "-v" };
        _complexArgs = new[]
        {
            "compare-complex",
            "input.txt",
            "--output", "output.txt",
            "--count", "100",
            "--rate", "0.95",
            "--verbose",
            "--format", "Json",
            "--tags", "a,b,c"
        };
        _helpArgs = new[] { "--help" };
    }

    private void SetupSystemCommandLine()
    {
        // Simple command
        var simpleCommand = new Command("compare-simple", "Simple benchmark command");
        var executeSubCommand = new Command("execute", "Execute action");
        executeSubCommand.SetHandler(() => { });
        simpleCommand.AddCommand(executeSubCommand);

        // Options command
        var optionsCommand = new Command("compare-options", "Command with options");
        var nameOption = new Option<string>("--name", () => "default", "Name option");
        var countOption = new Option<int>("--count", () => 0, "Count option");
        var verboseOption = new Option<bool>(new[] { "-v", "--verbose" }, "Verbose flag");
        optionsCommand.AddOption(nameOption);
        optionsCommand.AddOption(countOption);
        optionsCommand.AddOption(verboseOption);
        optionsCommand.SetHandler((name, count, verbose) => { }, nameOption, countOption, verboseOption);

        // Complex command
        var complexCommand = new Command("compare-complex", "Complex benchmark command");
        var inputArg = new Argument<string>("input", "Input file");
        var outputOption = new Option<string>("--output", () => "output.txt", "Output file");
        var cCountOption = new Option<int>("--count", () => 1, "Count");
        var rateOption = new Option<double>("--rate", () => 1.0, "Rate");
        var cVerboseOption = new Option<bool>("--verbose", "Verbose flag");
        var formatOption = new Option<string>("--format", () => "Text", "Output format");
        var tagsOption = new Option<string[]>("--tags", "Tags");

        complexCommand.AddArgument(inputArg);
        complexCommand.AddOption(outputOption);
        complexCommand.AddOption(cCountOption);
        complexCommand.AddOption(rateOption);
        complexCommand.AddOption(cVerboseOption);
        complexCommand.AddOption(formatOption);
        complexCommand.AddOption(tagsOption);
        complexCommand.SetHandler(
            (input, output, count, rate, verbose, format, tags) => { },
            inputArg, outputOption, cCountOption, rateOption, cVerboseOption, formatOption, tagsOption);

        // Root command
        _sclRootCommand = new RootCommand("Benchmark CLI");
        _sclRootCommand.AddCommand(simpleCommand);
        _sclRootCommand.AddCommand(optionsCommand);
        _sclRootCommand.AddCommand(complexCommand);

        // Create parser
        _sclParser = new Parser(_sclRootCommand);
    }

    #endregion

    #region Simple Command Benchmarks

    [Benchmark(Description = "TeCLI")]
    [BenchmarkCategory("Simple Command")]
    public async Task<int> TeCLI_SimpleCommand()
    {
        return await _tecliDispatcher.DispatchAsync(_simpleArgs);
    }

    [Benchmark(Description = "System.CommandLine")]
    [BenchmarkCategory("Simple Command")]
    public int SystemCommandLine_SimpleCommand()
    {
        return _sclParser.Invoke(_simpleArgs);
    }

    [Benchmark(Description = "CommandLineParser")]
    [BenchmarkCategory("Simple Command")]
    public int CommandLineParser_SimpleCommand()
    {
        var result = _clpParser.ParseArguments<ClpSimpleOptions>(_simpleArgs);
        return result.Tag == ParserResultType.Parsed ? 0 : 1;
    }

    #endregion

    #region Options Parsing Benchmarks

    [Benchmark(Description = "TeCLI")]
    [BenchmarkCategory("Options Parsing")]
    public async Task<int> TeCLI_WithOptions()
    {
        return await _tecliDispatcher.DispatchAsync(_withOptionsArgs);
    }

    [Benchmark(Description = "System.CommandLine")]
    [BenchmarkCategory("Options Parsing")]
    public int SystemCommandLine_WithOptions()
    {
        return _sclParser.Invoke(_withOptionsArgs);
    }

    [Benchmark(Description = "CommandLineParser")]
    [BenchmarkCategory("Options Parsing")]
    public int CommandLineParser_WithOptions()
    {
        var result = _clpParser.ParseArguments<ClpOptionsCommand>(
            new[] { "--name", "test", "--count", "42", "-v" });
        return result.Tag == ParserResultType.Parsed ? 0 : 1;
    }

    #endregion

    #region Complex Parsing Benchmarks

    [Benchmark(Description = "TeCLI")]
    [BenchmarkCategory("Complex Parsing")]
    public async Task<int> TeCLI_Complex()
    {
        return await _tecliDispatcher.DispatchAsync(_complexArgs);
    }

    [Benchmark(Description = "System.CommandLine")]
    [BenchmarkCategory("Complex Parsing")]
    public int SystemCommandLine_Complex()
    {
        return _sclParser.Invoke(_complexArgs);
    }

    [Benchmark(Description = "CommandLineParser")]
    [BenchmarkCategory("Complex Parsing")]
    public int CommandLineParser_Complex()
    {
        var result = _clpParser.ParseArguments<ClpComplexOptions>(
            new[] { "input.txt", "--output", "output.txt", "--count", "100",
                   "--rate", "0.95", "--verbose", "--format", "Json", "--tags", "a", "b", "c" });
        return result.Tag == ParserResultType.Parsed ? 0 : 1;
    }

    #endregion

    #region Help Generation Benchmarks

    [Benchmark(Description = "TeCLI")]
    [BenchmarkCategory("Help Generation")]
    public async Task<int> TeCLI_Help()
    {
        return await _tecliDispatcher.DispatchAsync(_helpArgs);
    }

    [Benchmark(Description = "System.CommandLine")]
    [BenchmarkCategory("Help Generation")]
    public int SystemCommandLine_Help()
    {
        return _sclParser.Invoke(_helpArgs);
    }

    [Benchmark(Description = "CommandLineParser")]
    [BenchmarkCategory("Help Generation")]
    public int CommandLineParser_Help()
    {
        var result = _clpParser.ParseArguments<ClpSimpleOptions>(new[] { "--help" });
        return result.Tag == ParserResultType.Parsed ? 0 : 1;
    }

    #endregion

    #region Parser Construction Benchmarks

    [Benchmark(Description = "TeCLI")]
    [BenchmarkCategory("Parser Construction")]
    public CommandDispatcher TeCLI_Construction()
    {
        return new CommandDispatcher();
    }

    [Benchmark(Description = "System.CommandLine")]
    [BenchmarkCategory("Parser Construction")]
    public Parser SystemCommandLine_Construction()
    {
        var root = new RootCommand();
        var cmd = new Command("test", "Test command");
        cmd.AddOption(new Option<string>("--name"));
        cmd.AddOption(new Option<int>("--count"));
        root.AddCommand(cmd);
        return new Parser(root);
    }

    [Benchmark(Description = "CommandLineParser")]
    [BenchmarkCategory("Parser Construction")]
    public CommandLine.Parser CommandLineParser_Construction()
    {
        return new CommandLine.Parser(settings =>
        {
            settings.CaseInsensitiveEnumValues = true;
        });
    }

    #endregion
}

#region TeCLI Test Commands for Comparison

/// <summary>
/// Simple TeCLI command for comparison
/// </summary>
[Command("compare-simple", Description = "Simple comparison command")]
public class CompareSimpleCommand
{
    [Primary]
    [Action("execute")]
    public void Execute()
    {
        // No-op
    }
}

/// <summary>
/// TeCLI command with options for comparison
/// </summary>
[Command("compare-options", Description = "Options comparison command")]
public class CompareOptionsCommand
{
    [Primary]
    [Action("execute")]
    public void Execute(
        [Option("name", ShortName = 'n', Description = "Name option")] string name = "default",
        [Option("count", ShortName = 'c', Description = "Count option")] int count = 0,
        [Option("verbose", ShortName = 'v', Description = "Verbose flag")] bool verbose = false)
    {
        // No-op
    }
}

/// <summary>
/// Enum for format option
/// </summary>
public enum CompareFormat { Text, Json, Xml }

/// <summary>
/// Complex TeCLI command for comparison
/// </summary>
[Command("compare-complex", Description = "Complex comparison command")]
public class CompareComplexCommand
{
    [Primary]
    [Action("execute")]
    public void Execute(
        [Argument(Description = "Input file")] string input,
        [Option("output", ShortName = 'o', Description = "Output file")] string output = "output.txt",
        [Option("count", ShortName = 'c', Description = "Count")] int count = 1,
        [Option("rate", ShortName = 'r', Description = "Rate")] double rate = 1.0,
        [Option("verbose", ShortName = 'v', Description = "Verbose")] bool verbose = false,
        [Option("format", ShortName = 'f', Description = "Format")] CompareFormat format = CompareFormat.Text,
        [Option("tags", ShortName = 't', Description = "Tags")] string[]? tags = null)
    {
        // No-op
    }
}

#endregion

#region CommandLineParser Options Classes

/// <summary>
/// CommandLineParser options for simple command
/// </summary>
public class ClpSimpleOptions
{
    [CommandLine.Value(0, HelpText = "Action")]
    public string? Action { get; set; }
}

/// <summary>
/// CommandLineParser options for options command
/// </summary>
public class ClpOptionsCommand
{
    [CommandLine.Option('n', "name", Default = "default", HelpText = "Name option")]
    public string Name { get; set; } = "default";

    [CommandLine.Option('c', "count", Default = 0, HelpText = "Count option")]
    public int Count { get; set; }

    [CommandLine.Option('v', "verbose", Default = false, HelpText = "Verbose flag")]
    public bool Verbose { get; set; }
}

/// <summary>
/// CommandLineParser options for complex command
/// </summary>
public class ClpComplexOptions
{
    [CommandLine.Value(0, Required = true, HelpText = "Input file")]
    public string Input { get; set; } = string.Empty;

    [CommandLine.Option('o', "output", Default = "output.txt", HelpText = "Output file")]
    public string Output { get; set; } = "output.txt";

    [CommandLine.Option('c', "count", Default = 1, HelpText = "Count")]
    public int Count { get; set; } = 1;

    [CommandLine.Option('r', "rate", Default = 1.0, HelpText = "Rate")]
    public double Rate { get; set; } = 1.0;

    [CommandLine.Option('v', "verbose", Default = false, HelpText = "Verbose")]
    public bool Verbose { get; set; }

    [CommandLine.Option('f', "format", Default = "Text", HelpText = "Format")]
    public string Format { get; set; } = "Text";

    [CommandLine.Option('t', "tags", Separator = ',', HelpText = "Tags")]
    public IEnumerable<string>? Tags { get; set; }
}

#endregion
