# TeCLI Benchmarks

This project contains comprehensive performance benchmarks for TeCLI using BenchmarkDotNet.

## Running Benchmarks

```bash
cd TeCLI.Benchmarks
dotnet run -c Release
```

### Running Specific Benchmarks

```bash
# Run specific benchmark suite
dotnet run -c Release -- dispatch      # Command dispatch benchmarks
dotnet run -c Release -- large         # Large command set benchmarks
dotnet run -c Release -- params        # Parameter parsing benchmarks
dotnet run -c Release -- compare       # Comparison with other CLI libraries
dotnet run -c Release -- codebuilder   # CodeBuilder benchmarks
dotnet run -c Release -- all           # Run all benchmarks

# Interactive mode (no arguments)
dotnet run -c Release
```

## Benchmark Suites

### 1. CodeBuilderBenchmarks

Tests the performance of the `CodeBuilder` class used during source generation:

| Benchmark | Description |
|-----------|-------------|
| BuildSimpleClass | Baseline - minimal code generation |
| BuildCodeWith10Usings | Tests using directive processing |
| BuildCodeWithNestedBlocks | Tests deeply nested scope management |
| BuildCodeWithManyAppends | Tests 100 sequential line appends |
| BuildCodeWithTryCatch | Tests try-catch block generation |

### 2. CommandDispatchBenchmarks

Tests command dispatch performance with the generated `CommandDispatcher`:

| Benchmark | Description |
|-----------|-------------|
| EmptyArgsDispatch | Baseline - no arguments |
| HelpDispatch | --help flag handling |
| SimpleCommandDispatch | Single command dispatch |
| CommandWithOptionsDispatch | Command with basic options |
| CommandWithManyOptionsDispatch | Command with 6+ options |
| SubcommandDispatch | Parent -> Child dispatch |
| DeepNestedCommandDispatch | 3-level nested commands |
| ActionDispatch | Command action dispatch |
| UnknownCommandDispatch | Error path with suggestions |

### 3. LargeCommandSetBenchmarks

Tests performance with a large command set (100 commands):

| Benchmark | Description |
|-----------|-------------|
| FirstCommandDispatch | First command in switch |
| MiddleCommandDispatch | Middle command lookup |
| LastCommandDispatch | Last command lookup |
| NestedSubcommandDispatch | Subcommand within large set |
| CommandAliasDispatch | Alias resolution |
| TypoCommandDispatch | Suggestion search for typos |

### 4. ParameterParsingBenchmarks

Tests various parameter parsing scenarios:

| Benchmark | Description |
|-----------|-------------|
| NoParameters | Baseline - no parameter parsing |
| SingleStringParameter | Single positional argument |
| MultipleStringParameters | Multiple positional arguments |
| NumericTypeParsing | int, long, double, decimal |
| BooleanFlagParsing | Boolean switches |
| EnumParsing | Enum value parsing |
| DateTimeParsing | DateTime and TimeSpan |
| CollectionParsing | Array/collection parameters |
| MixedArgsAndOptions | Combined args and options |
| WithValidation | Range and Regex validation |
| ComplexAllTypes | All parameter types combined |
| ShortNameOptions | Short name option parsing |

### 5. ComparisonBenchmarks

Compares TeCLI with other popular CLI libraries:

**Libraries Compared:**
- **TeCLI** - Source-generated CLI framework
- **System.CommandLine** - Microsoft's official CLI library
- **CommandLineParser** - Popular community library

**Comparison Categories:**

| Category | Description |
|----------|-------------|
| Simple Command | Basic command execution |
| Options Parsing | Parsing named options |
| Complex Parsing | Multiple args, options, enums, arrays |
| Help Generation | --help flag handling |
| Parser Construction | Initial setup overhead |

## Interpreting Results

Each benchmark measures:
- **Mean execution time** - Average time to complete
- **Memory allocation** - Total memory allocated during execution
- **Gen0/Gen1/Gen2** - Garbage collection statistics

### What to Look For

1. **Mean Time**: Lower is better. Compare against baseline benchmarks.
2. **Memory**: Lower allocation means less GC pressure.
3. **GC Generations**: High values indicate allocation issues.
4. **Comparison Results**: See how TeCLI compares to alternatives.

### Example Output

```
|                Method |      Mean |    Error |   StdDev |  Gen0 | Allocated |
|---------------------- |----------:|---------:|---------:|------:|----------:|
|    EmptyArgsDispatch  |  1.234 μs | 0.012 μs | 0.011 μs |  0.15 |     320 B |
|  SimpleCommandDispatch|  2.456 μs | 0.024 μs | 0.022 μs |  0.23 |     480 B |
```

## Adding New Benchmarks

1. Create a new class with `[MemoryDiagnoser]` attribute
2. Add benchmark methods with `[Benchmark]` attribute
3. Add test commands in a `#region` at the bottom
4. Update `Program.cs` to include the new benchmark
5. Update this README

## Dependencies

- BenchmarkDotNet 0.13.12
- System.CommandLine 2.0.0-beta4 (for comparison)
- CommandLineParser 2.9.1 (for comparison)
