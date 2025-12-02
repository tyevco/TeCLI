using TeCLI.Configuration;

// Example: Using configuration files with TeCLI
//
// This example demonstrates how to use the TeCLI.Extensions.Configuration
// package to load CLI options from configuration files.
//
// Run with different options:
//   dotnet run -- deploy                    # Uses config file defaults
//   dotnet run -- deploy --profile prod     # Uses "prod" profile
//   dotnet run -- deploy --environment dev  # CLI overrides config
//   dotnet run -- deploy --verbose          # CLI adds extra options

// Load configuration and merge with CLI arguments
var mergedArgs = args.WithConfiguration(appName: "tecli-example");

// Dispatch with merged arguments
await CommandDispatcher.DispatchAsync(mergedArgs);

[Command("deploy", Description = "Deploy the application")]
public class DeployCommand
{
    [Primary(Description = "Deploy to the specified environment")]
    public void Execute(
        [Option("environment", ShortName = 'e', Description = "Target environment")]
        string environment = "development",

        [Option("region", ShortName = 'r', Description = "Deployment region")]
        string region = "us-east",

        [Option("verbose", ShortName = 'v', Description = "Enable verbose output")]
        bool verbose = false,

        [Option("timeout", ShortName = 't', Description = "Deployment timeout in seconds")]
        int timeout = 300,

        [Option("profile", ShortName = 'p', Description = "Configuration profile to use")]
        string? profile = null)
    {
        Console.WriteLine("=== Deployment Configuration ===");
        Console.WriteLine($"Environment: {environment}");
        Console.WriteLine($"Region:      {region}");
        Console.WriteLine($"Verbose:     {verbose}");
        Console.WriteLine($"Timeout:     {timeout}s");
        if (profile != null)
        {
            Console.WriteLine($"Profile:     {profile}");
        }
        Console.WriteLine();
        Console.WriteLine("Deploying...");

        if (verbose)
        {
            Console.WriteLine("  - Initializing deployment...");
            Console.WriteLine($"  - Connecting to {region}...");
            Console.WriteLine($"  - Deploying to {environment}...");
            Console.WriteLine("  - Verifying deployment...");
        }

        Console.WriteLine("Deployment complete!");
    }
}

[Command("config", Description = "Configuration utilities")]
public class ConfigCommand
{
    [Action("show", Description = "Show current configuration")]
    public void Show(
        [Option("format", ShortName = 'f', Description = "Output format")]
        string format = "text")
    {
        var loader = new ConfigurationLoader();
        var config = loader.Load("tecli-example");

        Console.WriteLine("=== Current Configuration ===");
        Console.WriteLine($"Format: {format}");
        Console.WriteLine();

        PrintConfig(config, 0);
    }

    [Action("profiles", Description = "List available profiles")]
    public void ListProfiles()
    {
        var loader = new ConfigurationLoader();
        var config = loader.Load("tecli-example");
        var resolver = new ProfileResolver();
        var profiles = resolver.ExtractProfiles(config);

        Console.WriteLine("=== Available Profiles ===");

        if (profiles.Count == 0)
        {
            Console.WriteLine("No profiles defined.");
            return;
        }

        foreach (var profile in profiles)
        {
            Console.WriteLine($"\n[{profile.Key}]");
            if (!string.IsNullOrEmpty(profile.Value.Inherits))
            {
                Console.WriteLine($"  inherits: {profile.Value.Inherits}");
            }
            foreach (var value in profile.Value.Values)
            {
                Console.WriteLine($"  {value.Key}: {value.Value}");
            }
        }
    }

    private static void PrintConfig(IDictionary<string, object?> config, int indent)
    {
        var prefix = new string(' ', indent * 2);

        foreach (var kvp in config)
        {
            if (kvp.Value is IDictionary<string, object?> nested)
            {
                Console.WriteLine($"{prefix}{kvp.Key}:");
                PrintConfig(nested, indent + 1);
            }
            else if (kvp.Value is List<object?> list)
            {
                Console.WriteLine($"{prefix}{kvp.Key}:");
                foreach (var item in list)
                {
                    Console.WriteLine($"{prefix}  - {item}");
                }
            }
            else
            {
                Console.WriteLine($"{prefix}{kvp.Key}: {kvp.Value}");
            }
        }
    }
}
