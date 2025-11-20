using BenchmarkDotNet.Attributes;
using TeCLI;

namespace TeCLI.Benchmarks;

/// <summary>
/// Benchmarks for CodeBuilder class performance
/// </summary>
[MemoryDiagnoser]
public class CodeBuilderBenchmarks
{
    private const int IterationCount = 100;

    [Benchmark(Description = "CodeBuilder with 10 usings")]
    public string BuildCodeWith10Usings()
    {
        var cb = new CodeBuilder(
            "using System",
            "using System.Linq",
            "using System.Collections.Generic",
            "using System.Text",
            "using System.Threading.Tasks",
            "using Microsoft.CodeAnalysis",
            "using Microsoft.CodeAnalysis.CSharp",
            "using Microsoft.CodeAnalysis.CSharp.Syntax",
            "using TeCLI.Attributes",
            "using TeCLI.Extensions"
        );

        using (cb.AddBlock("namespace TeCLI.Generated"))
        {
            using (cb.AddBlock("public class TestClass"))
            {
                cb.AppendLine("private string _field;");
                cb.AddBlankLine();

                using (cb.AddBlock("public void TestMethod()"))
                {
                    cb.AppendLine("Console.WriteLine(\"Hello\");");
                }
            }
        }

        return cb.ToString();
    }

    [Benchmark(Description = "CodeBuilder with nested blocks")]
    public string BuildCodeWithNestedBlocks()
    {
        var cb = new CodeBuilder("using System");

        using (cb.AddBlock("namespace Test"))
        {
            using (cb.AddBlock("public class Outer"))
            {
                using (cb.AddBlock("public class Inner"))
                {
                    using (cb.AddBlock("public void Method()"))
                    {
                        using (cb.AddBlock("if (true)"))
                        {
                            using (cb.AddBlock("for (int i = 0; i < 10; i++)"))
                            {
                                cb.AppendLine("Console.WriteLine(i);");
                            }
                        }
                    }
                }
            }
        }

        return cb.ToString();
    }

    [Benchmark(Description = "CodeBuilder with many AppendLine calls")]
    public string BuildCodeWithManyAppends()
    {
        var cb = new CodeBuilder("using System");

        using (cb.AddBlock("namespace Test"))
        {
            using (cb.AddBlock("public class ManyLines"))
            {
                for (int i = 0; i < IterationCount; i++)
                {
                    cb.AppendLine($"private int _field{i};");
                }
            }
        }

        return cb.ToString();
    }

    [Benchmark(Description = "CodeBuilder with try-catch blocks")]
    public string BuildCodeWithTryCatch()
    {
        var cb = new CodeBuilder("using System");

        using (cb.AddBlock("namespace Test"))
        {
            using (cb.AddBlock("public class ErrorHandler"))
            {
                using (cb.AddBlock("public void HandleErrors()"))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        using (var tryCatch = cb.AddTry())
                        {
                            cb.AppendLine($"DoWork{i}();");
                            tryCatch.Catch();
                            cb.AppendLine("// Handle error");
                        }
                    }
                }
            }
        }

        return cb.ToString();
    }

    [Benchmark(Baseline = true, Description = "CodeBuilder baseline - simple class")]
    public string BuildSimpleClass()
    {
        var cb = new CodeBuilder("using System");

        using (cb.AddBlock("namespace Test"))
        {
            using (cb.AddBlock("public class Simple"))
            {
                cb.AppendLine("public string Name { get; set; }");
            }
        }

        return cb.ToString();
    }
}
