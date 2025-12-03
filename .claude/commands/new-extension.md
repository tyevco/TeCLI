Guide for creating a new TeCLI extension package.

To create a new extension `TeCLI.Extensions.{Name}`:

1. **Create the extension project**:
```bash
mkdir -p extensions/TeCLI.Extensions.{Name}
```

2. **Create the project file** at `extensions/TeCLI.Extensions.{Name}/TeCLI.Extensions.{Name}.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>TeCLI.Extensions.{Name}</PackageId>
    <Description>{Description}</Description>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\TeCLI\TeCLI.csproj" />
  </ItemGroup>
</Project>
```

3. **Create the main class** implementing the extension functionality

4. **Create README.md** with usage documentation

5. **Create example project**:
```bash
mkdir -p examples/TeCLI.Example.{Name}
```

6. **Create test project**:
```bash
mkdir -p tests/TeCLI.Extensions.{Name}.Tests
```

7. **Add to solution**:
```bash
dotnet sln TeCLI.sln add extensions/TeCLI.Extensions.{Name}/TeCLI.Extensions.{Name}.csproj
dotnet sln TeCLI.sln add examples/TeCLI.Example.{Name}/TeCLI.Example.{Name}.csproj
dotnet sln TeCLI.sln add tests/TeCLI.Extensions.{Name}.Tests/TeCLI.Extensions.{Name}.Tests.csproj
```

What extension would you like to create? Please describe:
- The feature/functionality
- Integration points with TeCLI core
- Any external dependencies needed
