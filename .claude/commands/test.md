Run all tests for the TeCLI project.

Execute the test suite:
```bash
dotnet test TeCLI.sln --configuration Debug --logger "console;verbosity=normal"
```

If tests fail:
1. Report which tests failed
2. Show the error messages
3. Suggest potential fixes based on the test failures

For running specific test projects, use:
- Main tests: `dotnet test TeCLI.Tests/TeCLI.Tests.csproj`
- Extension tests: `dotnet test tests/TeCLI.Extensions.{Name}.Tests/`
