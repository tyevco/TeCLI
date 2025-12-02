using TeCLI.Attributes;
using TeCLI.Attributes.Validation;

namespace TeCLI.Example.Advanced;

/// <summary>
/// Deployment command demonstrating advanced TeCLI features:
/// - Enums
/// - Validation (Range, RegularExpression)
/// - Collections/arrays
/// - Required options
/// - Environment variables
/// - Command aliases
/// </summary>
[Command("deploy", Description = "Application deployment management", Aliases = new[] { "dpl" })]
public class DeployCommand
{
    /// <summary>
    /// Deploy the application to an environment
    /// </summary>
    [Primary]
    [Action("start", Description = "Start a deployment")]
    public Task StartAsync(
        [Argument(Description = "Target environment")]
        Environment environment,

        [Option("version", ShortName = 'v', Description = "Version to deploy (semver format)", Required = true)]
        [RegularExpression(@"^\d+\.\d+\.\d+(-[\w.]+)?$", ErrorMessage = "Version must be in semver format (e.g., 1.2.3 or 1.2.3-beta)")]
        string version,

        [Option("replicas", ShortName = 'r', Description = "Number of replicas (1-100)")]
        [Range(1, 100)]
        int replicas = 3,

        [Option("regions", Description = "Regions to deploy to (comma-separated)")]
        string[]? regions = null,

        [Option("tags", ShortName = 't', Description = "Deployment tags")]
        string[]? tags = null,

        [Option("force", ShortName = 'f', Description = "Force deployment without confirmation")]
        bool force = false,

        [Option("log-level", Description = "Logging verbosity")]
        LogLevel logLevel = LogLevel.Info)
    {
        Console.WriteLine($"Starting deployment...");
        Console.WriteLine($"  Environment: {environment}");
        Console.WriteLine($"  Version: {version}");
        Console.WriteLine($"  Replicas: {replicas}");
        Console.WriteLine($"  Log Level: {logLevel}");
        Console.WriteLine($"  Force: {force}");

        if (regions?.Length > 0)
        {
            Console.WriteLine($"  Regions: {string.Join(", ", regions)}");
        }

        if (tags?.Length > 0)
        {
            Console.WriteLine($"  Tags: {string.Join(", ", tags)}");
        }

        Console.WriteLine("Deployment initiated successfully!");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Check deployment status
    /// </summary>
    [Action("status", Description = "Check deployment status", Aliases = new[] { "st" })]
    public void Status(
        [Argument(Description = "Deployment ID")]
        string deploymentId,

        [Option("format", ShortName = 'o', Description = "Output format")]
        OutputFormat format = OutputFormat.Text,

        [Option("watch", ShortName = 'w', Description = "Watch for changes")]
        bool watch = false)
    {
        Console.WriteLine($"Checking status for deployment: {deploymentId}");
        Console.WriteLine($"Output format: {format}");

        if (watch)
        {
            Console.WriteLine("Watching for changes... (press Ctrl+C to stop)");
        }

        // Simulated status output
        if (format == OutputFormat.Json)
        {
            Console.WriteLine($"{{ \"id\": \"{deploymentId}\", \"status\": \"running\", \"progress\": 75 }}");
        }
        else
        {
            Console.WriteLine($"Status: Running (75% complete)");
        }
    }

    /// <summary>
    /// Rollback to a previous version
    /// </summary>
    [Action("rollback", Description = "Rollback to a previous version", Aliases = new[] { "rb", "undo" })]
    public Task RollbackAsync(
        [Argument(Description = "Target environment")]
        Environment environment,

        [Option("to-version", ShortName = 'v', Description = "Version to rollback to")]
        string? toVersion = null,

        [Option("steps", ShortName = 's', Description = "Number of versions to rollback")]
        [Range(1, 10)]
        int steps = 1)
    {
        if (toVersion != null)
        {
            Console.WriteLine($"Rolling back {environment} to version {toVersion}...");
        }
        else
        {
            Console.WriteLine($"Rolling back {environment} by {steps} version(s)...");
        }

        Console.WriteLine("Rollback completed successfully!");
        return Task.CompletedTask;
    }

    /// <summary>
    /// List deployment history
    /// </summary>
    [Action("history", Description = "Show deployment history")]
    public void History(
        [Argument(Description = "Target environment")]
        Environment environment,

        [Option("limit", ShortName = 'n', Description = "Number of entries to show")]
        [Range(1, 100)]
        int limit = 10,

        [Option("format", ShortName = 'o', Description = "Output format")]
        OutputFormat format = OutputFormat.Text)
    {
        Console.WriteLine($"Deployment history for {environment} (last {limit} entries):");
        Console.WriteLine();

        // Simulated history
        var versions = new[] { "1.2.3", "1.2.2", "1.2.1", "1.2.0", "1.1.9" };
        for (int i = 0; i < Math.Min(limit, versions.Length); i++)
        {
            if (format == OutputFormat.Json)
            {
                Console.WriteLine($"  {{ \"version\": \"{versions[i]}\", \"date\": \"2024-01-{15 - i:D2}\" }}");
            }
            else
            {
                Console.WriteLine($"  {i + 1}. v{versions[i]} - deployed 2024-01-{15 - i:D2}");
            }
        }
    }
}
