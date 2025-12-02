using BenchmarkDotNet.Attributes;
using TeCLI.Attributes;

namespace TeCLI.Benchmarks;

/// <summary>
/// Benchmarks for command dispatch performance.
/// Tests the generated CommandDispatcher's ability to parse and route commands.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class CommandDispatchBenchmarks
{
    private CommandDispatcher _dispatcher = null!;

    // Pre-allocated argument arrays to avoid allocation overhead in benchmarks
    private string[] _emptyArgs = null!;
    private string[] _helpArgs = null!;
    private string[] _simpleCommandArgs = null!;
    private string[] _commandWithOptionsArgs = null!;
    private string[] _commandWithManyOptionsArgs = null!;
    private string[] _subcommandArgs = null!;
    private string[] _deepNestedArgs = null!;
    private string[] _actionArgs = null!;
    private string[] _unknownCommandArgs = null!;

    [GlobalSetup]
    public void Setup()
    {
        _dispatcher = new CommandDispatcher();

        // Pre-allocate all argument arrays
        _emptyArgs = Array.Empty<string>();
        _helpArgs = new[] { "--help" };
        _simpleCommandArgs = new[] { "bench-simple" };
        _commandWithOptionsArgs = new[] { "bench-options", "--name", "test", "-v" };
        _commandWithManyOptionsArgs = new[]
        {
            "bench-options",
            "--name", "test",
            "--count", "42",
            "--rate", "3.14",
            "--enabled",
            "-v",
            "--tags", "tag1,tag2,tag3"
        };
        _subcommandArgs = new[] { "bench-parent", "child" };
        _deepNestedArgs = new[] { "bench-parent", "child", "grandchild" };
        _actionArgs = new[] { "bench-actions", "run", "taskname" };
        _unknownCommandArgs = new[] { "nonexistent-command" };
    }

    [Benchmark(Baseline = true, Description = "Empty args dispatch")]
    public async Task<int> EmptyArgsDispatch()
    {
        return await _dispatcher.DispatchAsync(_emptyArgs);
    }

    [Benchmark(Description = "Help flag dispatch")]
    public async Task<int> HelpDispatch()
    {
        return await _dispatcher.DispatchAsync(_helpArgs);
    }

    [Benchmark(Description = "Simple command dispatch")]
    public async Task<int> SimpleCommandDispatch()
    {
        return await _dispatcher.DispatchAsync(_simpleCommandArgs);
    }

    [Benchmark(Description = "Command with options")]
    public async Task<int> CommandWithOptionsDispatch()
    {
        return await _dispatcher.DispatchAsync(_commandWithOptionsArgs);
    }

    [Benchmark(Description = "Command with many options")]
    public async Task<int> CommandWithManyOptionsDispatch()
    {
        return await _dispatcher.DispatchAsync(_commandWithManyOptionsArgs);
    }

    [Benchmark(Description = "Subcommand dispatch")]
    public async Task<int> SubcommandDispatch()
    {
        return await _dispatcher.DispatchAsync(_subcommandArgs);
    }

    [Benchmark(Description = "Deep nested command")]
    public async Task<int> DeepNestedCommandDispatch()
    {
        return await _dispatcher.DispatchAsync(_deepNestedArgs);
    }

    [Benchmark(Description = "Action dispatch")]
    public async Task<int> ActionDispatch()
    {
        return await _dispatcher.DispatchAsync(_actionArgs);
    }

    [Benchmark(Description = "Unknown command (error path)")]
    public async Task<int> UnknownCommandDispatch()
    {
        return await _dispatcher.DispatchAsync(_unknownCommandArgs);
    }
}

#region Benchmark Test Commands

/// <summary>
/// Simple command with no parameters for baseline dispatch testing
/// </summary>
[Command("bench-simple", Description = "Simple benchmark command")]
public class BenchSimpleCommand
{
    [Primary]
    [Action("default")]
    public void Execute()
    {
        // No-op for benchmarking
    }
}

/// <summary>
/// Command with various option types for parameter parsing benchmarks
/// </summary>
[Command("bench-options", Description = "Command with options")]
public class BenchOptionsCommand
{
    [Primary]
    [Action("default")]
    public void Execute(
        [Option("name", ShortName = 'n', Description = "Name option")] string name = "default",
        [Option("count", ShortName = 'c', Description = "Count option")] int count = 0,
        [Option("rate", ShortName = 'r', Description = "Rate option")] double rate = 1.0,
        [Option("enabled", ShortName = 'e', Description = "Enabled flag")] bool enabled = false,
        [Option("verbose", ShortName = 'v', Description = "Verbose flag")] bool verbose = false,
        [Option("tags", ShortName = 't', Description = "Tags array")] string[]? tags = null)
    {
        // No-op for benchmarking
    }
}

/// <summary>
/// Parent command with nested subcommands for hierarchy testing
/// </summary>
[Command("bench-parent", Description = "Parent command")]
public class BenchParentCommand
{
    [Primary]
    [Action("default")]
    public void Execute()
    {
        // No-op for benchmarking
    }

    /// <summary>
    /// Child subcommand
    /// </summary>
    [Command("child", Description = "Child subcommand")]
    public class ChildCommand
    {
        [Primary]
        [Action("default")]
        public void Execute()
        {
            // No-op for benchmarking
        }

        /// <summary>
        /// Grandchild subcommand for deep nesting
        /// </summary>
        [Command("grandchild", Description = "Grandchild subcommand")]
        public class GrandchildCommand
        {
            [Primary]
            [Action("default")]
            public void Execute()
            {
                // No-op for benchmarking
            }
        }
    }
}

/// <summary>
/// Command with multiple actions for action dispatch testing
/// </summary>
[Command("bench-actions", Description = "Command with multiple actions")]
public class BenchActionsCommand
{
    [Primary]
    [Action("list", Description = "List action")]
    public void List()
    {
        // No-op for benchmarking
    }

    [Action("run", Description = "Run action")]
    public void Run(
        [Argument(Description = "Task name")] string taskName)
    {
        // No-op for benchmarking
    }

    [Action("status", Description = "Status action")]
    public void Status(
        [Option("verbose", ShortName = 'v')] bool verbose = false)
    {
        // No-op for benchmarking
    }

    [Action("delete", Description = "Delete action")]
    public void Delete(
        [Argument(Description = "Task name")] string taskName,
        [Option("force", ShortName = 'f')] bool force = false)
    {
        // No-op for benchmarking
    }
}

#endregion
