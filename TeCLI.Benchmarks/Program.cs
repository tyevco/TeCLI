using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace TeCLI.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            // Run all benchmarks with a switcher for interactive selection
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
            return;
        }

        var benchmarkName = args[0].ToLowerInvariant();

        switch (benchmarkName)
        {
            case "codebuilder":
            case "cb":
                BenchmarkRunner.Run<CodeBuilderBenchmarks>();
                break;

            case "dispatch":
            case "command":
            case "cmd":
                BenchmarkRunner.Run<CommandDispatchBenchmarks>();
                break;

            case "large":
            case "largeset":
            case "100":
                BenchmarkRunner.Run<LargeCommandSetBenchmarks>();
                break;

            case "params":
            case "parameters":
            case "parsing":
                BenchmarkRunner.Run<ParameterParsingBenchmarks>();
                break;

            case "compare":
            case "comparison":
            case "vs":
                BenchmarkRunner.Run<ComparisonBenchmarks>();
                break;

            case "all":
                // Run all benchmarks sequentially
                var config = DefaultConfig.Instance;
                BenchmarkRunner.Run<CodeBuilderBenchmarks>(config);
                BenchmarkRunner.Run<CommandDispatchBenchmarks>(config);
                BenchmarkRunner.Run<LargeCommandSetBenchmarks>(config);
                BenchmarkRunner.Run<ParameterParsingBenchmarks>(config);
                BenchmarkRunner.Run<ComparisonBenchmarks>(config);
                break;

            default:
                Console.WriteLine("TeCLI Benchmarks");
                Console.WriteLine("================");
                Console.WriteLine();
                Console.WriteLine("Usage: dotnet run -c Release [benchmark]");
                Console.WriteLine();
                Console.WriteLine("Available benchmarks:");
                Console.WriteLine("  codebuilder, cb      - CodeBuilder class performance");
                Console.WriteLine("  dispatch, command    - Command dispatch performance");
                Console.WriteLine("  large, largeset, 100 - Large command set (100+ commands)");
                Console.WriteLine("  params, parameters   - Parameter parsing performance");
                Console.WriteLine("  compare, comparison  - Comparison with other CLI libraries");
                Console.WriteLine("  all                  - Run all benchmarks");
                Console.WriteLine();
                Console.WriteLine("No argument: Interactive benchmark selector");
                break;
        }
    }
}
