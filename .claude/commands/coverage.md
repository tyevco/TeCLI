Run tests with code coverage for TeCLI.

Execute tests with coverage collection:
```bash
dotnet test TeCLI.Tests/TeCLI.Tests.csproj --configuration Debug --collect:"XPlat Code Coverage" --results-directory ./coverage
```

After running:
1. Report the coverage results
2. Identify areas with low coverage
3. Suggest which files or methods need more test coverage
