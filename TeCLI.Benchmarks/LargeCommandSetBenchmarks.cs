using BenchmarkDotNet.Attributes;
using TeCLI.Attributes;

namespace TeCLI.Benchmarks;

/// <summary>
/// Benchmarks for large command set scenarios.
/// Tests command lookup performance with many commands and subcommands.
///
/// Note: TeCLI uses compile-time source generation, so "discovery" happens at build time.
/// These benchmarks test runtime command matching and dispatch with large command sets.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class LargeCommandSetBenchmarks
{
    private CommandDispatcher _dispatcher = null!;

    // Test scenarios for large command sets
    private string[] _firstCommandArgs = null!;
    private string[] _middleCommandArgs = null!;
    private string[] _lastCommandArgs = null!;
    private string[] _nestedSubcommandArgs = null!;
    private string[] _commandWithAliasArgs = null!;
    private string[] _typoCommandArgs = null!;

    [GlobalSetup]
    public void Setup()
    {
        _dispatcher = new CommandDispatcher();

        // Commands at different positions in the generated switch statement
        _firstCommandArgs = new[] { "large-cmd-01", "execute" };
        _middleCommandArgs = new[] { "large-cmd-50", "execute" };
        _lastCommandArgs = new[] { "large-cmd-99", "execute" };
        _nestedSubcommandArgs = new[] { "large-cmd-25", "nested", "action" };
        _commandWithAliasArgs = new[] { "lc75" }; // Alias for large-cmd-75
        _typoCommandArgs = new[] { "large-cnd-50" }; // Typo - triggers suggestion lookup
    }

    [Benchmark(Baseline = true, Description = "First command in set")]
    public async Task<int> FirstCommandDispatch()
    {
        return await _dispatcher.DispatchAsync(_firstCommandArgs);
    }

    [Benchmark(Description = "Middle command in set")]
    public async Task<int> MiddleCommandDispatch()
    {
        return await _dispatcher.DispatchAsync(_middleCommandArgs);
    }

    [Benchmark(Description = "Last command in set")]
    public async Task<int> LastCommandDispatch()
    {
        return await _dispatcher.DispatchAsync(_lastCommandArgs);
    }

    [Benchmark(Description = "Nested subcommand lookup")]
    public async Task<int> NestedSubcommandDispatch()
    {
        return await _dispatcher.DispatchAsync(_nestedSubcommandArgs);
    }

    [Benchmark(Description = "Command alias resolution")]
    public async Task<int> CommandAliasDispatch()
    {
        return await _dispatcher.DispatchAsync(_commandWithAliasArgs);
    }

    [Benchmark(Description = "Typo command (suggestion search)")]
    public async Task<int> TypoCommandDispatch()
    {
        return await _dispatcher.DispatchAsync(_typoCommandArgs);
    }
}

#region Large Command Set (100 Commands)

// Commands 01-25: Simple commands
[Command("large-cmd-01", Description = "Large command set item 01")]
public class LargeCmd01 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-02", Description = "Large command set item 02")]
public class LargeCmd02 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-03", Description = "Large command set item 03")]
public class LargeCmd03 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-04", Description = "Large command set item 04")]
public class LargeCmd04 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-05", Description = "Large command set item 05")]
public class LargeCmd05 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-06", Description = "Large command set item 06")]
public class LargeCmd06 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-07", Description = "Large command set item 07")]
public class LargeCmd07 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-08", Description = "Large command set item 08")]
public class LargeCmd08 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-09", Description = "Large command set item 09")]
public class LargeCmd09 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-10", Description = "Large command set item 10")]
public class LargeCmd10 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-11", Description = "Large command set item 11")]
public class LargeCmd11 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-12", Description = "Large command set item 12")]
public class LargeCmd12 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-13", Description = "Large command set item 13")]
public class LargeCmd13 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-14", Description = "Large command set item 14")]
public class LargeCmd14 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-15", Description = "Large command set item 15")]
public class LargeCmd15 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-16", Description = "Large command set item 16")]
public class LargeCmd16 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-17", Description = "Large command set item 17")]
public class LargeCmd17 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-18", Description = "Large command set item 18")]
public class LargeCmd18 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-19", Description = "Large command set item 19")]
public class LargeCmd19 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-20", Description = "Large command set item 20")]
public class LargeCmd20 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-21", Description = "Large command set item 21")]
public class LargeCmd21 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-22", Description = "Large command set item 22")]
public class LargeCmd22 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-23", Description = "Large command set item 23")]
public class LargeCmd23 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-24", Description = "Large command set item 24")]
public class LargeCmd24 { [Primary][Action("execute")] public void Execute() { } }

// Command 25 with nested subcommand
[Command("large-cmd-25", Description = "Large command set item 25 with subcommand")]
public class LargeCmd25
{
    [Primary][Action("execute")] public void Execute() { }

    [Command("nested", Description = "Nested subcommand")]
    public class NestedCommand
    {
        [Primary][Action("action")] public void Action() { }
    }
}

// Commands 26-50: Simple commands
[Command("large-cmd-26", Description = "Large command set item 26")]
public class LargeCmd26 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-27", Description = "Large command set item 27")]
public class LargeCmd27 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-28", Description = "Large command set item 28")]
public class LargeCmd28 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-29", Description = "Large command set item 29")]
public class LargeCmd29 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-30", Description = "Large command set item 30")]
public class LargeCmd30 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-31", Description = "Large command set item 31")]
public class LargeCmd31 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-32", Description = "Large command set item 32")]
public class LargeCmd32 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-33", Description = "Large command set item 33")]
public class LargeCmd33 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-34", Description = "Large command set item 34")]
public class LargeCmd34 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-35", Description = "Large command set item 35")]
public class LargeCmd35 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-36", Description = "Large command set item 36")]
public class LargeCmd36 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-37", Description = "Large command set item 37")]
public class LargeCmd37 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-38", Description = "Large command set item 38")]
public class LargeCmd38 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-39", Description = "Large command set item 39")]
public class LargeCmd39 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-40", Description = "Large command set item 40")]
public class LargeCmd40 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-41", Description = "Large command set item 41")]
public class LargeCmd41 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-42", Description = "Large command set item 42")]
public class LargeCmd42 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-43", Description = "Large command set item 43")]
public class LargeCmd43 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-44", Description = "Large command set item 44")]
public class LargeCmd44 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-45", Description = "Large command set item 45")]
public class LargeCmd45 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-46", Description = "Large command set item 46")]
public class LargeCmd46 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-47", Description = "Large command set item 47")]
public class LargeCmd47 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-48", Description = "Large command set item 48")]
public class LargeCmd48 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-49", Description = "Large command set item 49")]
public class LargeCmd49 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-50", Description = "Large command set item 50")]
public class LargeCmd50 { [Primary][Action("execute")] public void Execute() { } }

// Commands 51-75 with some aliases
[Command("large-cmd-51", Description = "Large command set item 51")]
public class LargeCmd51 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-52", Description = "Large command set item 52")]
public class LargeCmd52 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-53", Description = "Large command set item 53")]
public class LargeCmd53 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-54", Description = "Large command set item 54")]
public class LargeCmd54 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-55", Description = "Large command set item 55")]
public class LargeCmd55 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-56", Description = "Large command set item 56")]
public class LargeCmd56 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-57", Description = "Large command set item 57")]
public class LargeCmd57 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-58", Description = "Large command set item 58")]
public class LargeCmd58 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-59", Description = "Large command set item 59")]
public class LargeCmd59 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-60", Description = "Large command set item 60")]
public class LargeCmd60 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-61", Description = "Large command set item 61")]
public class LargeCmd61 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-62", Description = "Large command set item 62")]
public class LargeCmd62 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-63", Description = "Large command set item 63")]
public class LargeCmd63 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-64", Description = "Large command set item 64")]
public class LargeCmd64 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-65", Description = "Large command set item 65")]
public class LargeCmd65 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-66", Description = "Large command set item 66")]
public class LargeCmd66 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-67", Description = "Large command set item 67")]
public class LargeCmd67 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-68", Description = "Large command set item 68")]
public class LargeCmd68 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-69", Description = "Large command set item 69")]
public class LargeCmd69 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-70", Description = "Large command set item 70")]
public class LargeCmd70 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-71", Description = "Large command set item 71")]
public class LargeCmd71 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-72", Description = "Large command set item 72")]
public class LargeCmd72 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-73", Description = "Large command set item 73")]
public class LargeCmd73 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-74", Description = "Large command set item 74")]
public class LargeCmd74 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-75", Description = "Large command set item 75", Aliases = new[] { "lc75" })]
public class LargeCmd75 { [Primary][Action("execute")] public void Execute() { } }

// Commands 76-99: Simple commands
[Command("large-cmd-76", Description = "Large command set item 76")]
public class LargeCmd76 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-77", Description = "Large command set item 77")]
public class LargeCmd77 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-78", Description = "Large command set item 78")]
public class LargeCmd78 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-79", Description = "Large command set item 79")]
public class LargeCmd79 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-80", Description = "Large command set item 80")]
public class LargeCmd80 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-81", Description = "Large command set item 81")]
public class LargeCmd81 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-82", Description = "Large command set item 82")]
public class LargeCmd82 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-83", Description = "Large command set item 83")]
public class LargeCmd83 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-84", Description = "Large command set item 84")]
public class LargeCmd84 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-85", Description = "Large command set item 85")]
public class LargeCmd85 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-86", Description = "Large command set item 86")]
public class LargeCmd86 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-87", Description = "Large command set item 87")]
public class LargeCmd87 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-88", Description = "Large command set item 88")]
public class LargeCmd88 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-89", Description = "Large command set item 89")]
public class LargeCmd89 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-90", Description = "Large command set item 90")]
public class LargeCmd90 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-91", Description = "Large command set item 91")]
public class LargeCmd91 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-92", Description = "Large command set item 92")]
public class LargeCmd92 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-93", Description = "Large command set item 93")]
public class LargeCmd93 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-94", Description = "Large command set item 94")]
public class LargeCmd94 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-95", Description = "Large command set item 95")]
public class LargeCmd95 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-96", Description = "Large command set item 96")]
public class LargeCmd96 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-97", Description = "Large command set item 97")]
public class LargeCmd97 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-98", Description = "Large command set item 98")]
public class LargeCmd98 { [Primary][Action("execute")] public void Execute() { } }

[Command("large-cmd-99", Description = "Large command set item 99")]
public class LargeCmd99 { [Primary][Action("execute")] public void Execute() { } }

#endregion
