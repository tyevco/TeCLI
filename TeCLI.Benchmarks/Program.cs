using BenchmarkDotNet.Running;

namespace TeCLI.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<CodeBuilderBenchmarks>();
    }
}
