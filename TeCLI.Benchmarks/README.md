# TeCLI Benchmarks

This project contains performance benchmarks for TeCLI using BenchmarkDotNet.

## Running Benchmarks

```bash
cd TeCLI.Benchmarks
dotnet run -c Release
```

## What's Being Measured

### CodeBuilderBenchmarks

Tests the performance of the `CodeBuilder` class which is used heavily during source generation:

1. **BuildSimpleClass** (Baseline) - Minimal code generation
2. **BuildCodeWith10Usings** - Tests using directive processing overhead
3. **BuildCodeWithNestedBlocks** - Tests deeply nested scope management
4. **BuildCodeWithManyAppends** - Tests performance with many sequential line appends
5. **BuildCodeWithTryCatch** - Tests try-catch block generation overhead

Each benchmark measures:
- **Mean execution time** - Average time to complete
- **Memory allocation** - Total memory allocated during execution
- **Gen0/Gen1/Gen2** - Garbage collection statistics

## Interpreting Results

- Look for benchmarks that are significantly slower than the baseline
- High Gen0/Gen1/Gen2 values indicate excessive allocations
- Compare results across code changes to detect performance regressions

## Future Benchmarks

Consider adding:
- Parameter extraction from complex Roslyn symbols
- Full source generation end-to-end tests
- Comparison with manual string concatenation approaches
