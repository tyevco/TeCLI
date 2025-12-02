using TeCLI.Attributes;
using TeCLI.Attributes.Validation;

namespace TeCLI.Example.Advanced;

/// <summary>
/// Configuration command with nested subcommands demonstrating:
/// - Nested command structure (git-style)
/// - Environment variables for options
/// - Prompts for missing values
/// </summary>
[Command("config", Description = "Configuration management")]
public class ConfigCommand
{
    /// <summary>
    /// Show current configuration
    /// </summary>
    [Primary]
    [Action("show", Description = "Display current configuration")]
    public void Show(
        [Option("section", ShortName = 's', Description = "Configuration section to show")]
        string? section = null,

        [Option("format", ShortName = 'o', Description = "Output format")]
        OutputFormat format = OutputFormat.Text)
    {
        Console.WriteLine("Current Configuration:");
        Console.WriteLine();

        if (format == OutputFormat.Json)
        {
            Console.WriteLine("{");
            Console.WriteLine("  \"app\": { \"name\": \"MyApp\", \"version\": \"1.0.0\" },");
            Console.WriteLine("  \"database\": { \"host\": \"localhost\", \"port\": 5432 },");
            Console.WriteLine("  \"logging\": { \"level\": \"Info\" }");
            Console.WriteLine("}");
        }
        else
        {
            if (section == null || section == "app")
            {
                Console.WriteLine("[app]");
                Console.WriteLine("  name = MyApp");
                Console.WriteLine("  version = 1.0.0");
                Console.WriteLine();
            }

            if (section == null || section == "database")
            {
                Console.WriteLine("[database]");
                Console.WriteLine("  host = localhost");
                Console.WriteLine("  port = 5432");
                Console.WriteLine();
            }

            if (section == null || section == "logging")
            {
                Console.WriteLine("[logging]");
                Console.WriteLine("  level = Info");
            }
        }
    }

    /// <summary>
    /// Set a configuration value
    /// </summary>
    [Action("set", Description = "Set a configuration value")]
    public void Set(
        [Argument(Description = "Configuration key (e.g., app.name)")]
        [RegularExpression(@"^[\w]+\.[\w]+$", ErrorMessage = "Key must be in format 'section.key'")]
        string key,

        [Argument(Description = "Value to set")]
        string value,

        [Option("global", ShortName = 'g', Description = "Set globally (not just for current project)")]
        bool global = false)
    {
        var scope = global ? "global" : "local";
        Console.WriteLine($"Setting {key} = {value} ({scope})");
        Console.WriteLine("Configuration updated successfully!");
    }

    /// <summary>
    /// Get a configuration value
    /// </summary>
    [Action("get", Description = "Get a configuration value")]
    public void Get(
        [Argument(Description = "Configuration key")]
        string key)
    {
        Console.WriteLine($"{key} = example-value");
    }

    /// <summary>
    /// Database configuration subcommand
    /// </summary>
    [Command("database", Description = "Database configuration", Aliases = new[] { "db" })]
    public class DatabaseConfig
    {
        /// <summary>
        /// Configure database connection
        /// </summary>
        [Primary]
        [Action("setup", Description = "Setup database connection")]
        public void Setup(
            [Option("host", ShortName = 'h', Description = "Database host", EnvVar = "DB_HOST")]
            string host = "localhost",

            [Option("port", ShortName = 'p', Description = "Database port", EnvVar = "DB_PORT")]
            [Range(1, 65535)]
            int port = 5432,

            [Option("name", ShortName = 'n', Description = "Database name", Required = true)]
            string databaseName = default!,

            [Option("user", ShortName = 'u', Description = "Username", EnvVar = "DB_USER")]
            string? username = null,

            [Option("ssl", Description = "Enable SSL connection")]
            bool useSsl = false)
        {
            Console.WriteLine("Database Configuration:");
            Console.WriteLine($"  Host: {host}");
            Console.WriteLine($"  Port: {port}");
            Console.WriteLine($"  Database: {databaseName}");
            Console.WriteLine($"  User: {username ?? "(not set)"}");
            Console.WriteLine($"  SSL: {useSsl}");
        }

        /// <summary>
        /// Test database connection
        /// </summary>
        [Action("test", Description = "Test database connection")]
        public async Task TestAsync(
            [Option("timeout", ShortName = 't', Description = "Connection timeout in seconds")]
            [Range(1, 60)]
            int timeout = 5)
        {
            Console.WriteLine($"Testing database connection (timeout: {timeout}s)...");
            await Task.Delay(500); // Simulate connection test
            Console.WriteLine("Connection successful!");
        }

        /// <summary>
        /// Run database migrations
        /// </summary>
        [Action("migrate", Description = "Run database migrations")]
        public void Migrate(
            [Option("direction", ShortName = 'd', Description = "Migration direction")]
            MigrationDirection direction = MigrationDirection.Up,

            [Option("steps", ShortName = 's', Description = "Number of migrations to run")]
            [Range(1, 100)]
            int? steps = null,

            [Option("dry-run", Description = "Show what would be done without making changes")]
            bool dryRun = false)
        {
            var stepText = steps.HasValue ? $"{steps} migration(s)" : "all pending migrations";
            var action = dryRun ? "[DRY RUN] Would run" : "Running";

            Console.WriteLine($"{action} {stepText} ({direction})...");

            if (!dryRun)
            {
                Console.WriteLine("  Applied: 001_create_users_table");
                Console.WriteLine("  Applied: 002_add_email_to_users");
                Console.WriteLine("Migrations completed!");
            }
        }
    }

    /// <summary>
    /// Cache configuration subcommand
    /// </summary>
    [Command("cache", Description = "Cache configuration")]
    public class CacheConfig
    {
        /// <summary>
        /// Configure cache settings
        /// </summary>
        [Primary]
        [Action("setup", Description = "Setup cache configuration")]
        public void Setup(
            [Option("provider", ShortName = 'p', Description = "Cache provider")]
            CacheProvider provider = CacheProvider.Memory,

            [Option("ttl", ShortName = 't', Description = "Default TTL in seconds")]
            [Range(1, 86400)]
            int ttl = 3600,

            [Option("max-size", ShortName = 's', Description = "Maximum cache size in MB")]
            [Range(1, 10240)]
            int maxSizeMb = 256)
        {
            Console.WriteLine("Cache Configuration:");
            Console.WriteLine($"  Provider: {provider}");
            Console.WriteLine($"  TTL: {ttl}s");
            Console.WriteLine($"  Max Size: {maxSizeMb}MB");
        }

        /// <summary>
        /// Clear the cache
        /// </summary>
        [Action("clear", Description = "Clear the cache")]
        public void Clear(
            [Option("pattern", ShortName = 'p', Description = "Key pattern to clear (supports wildcards)")]
            string pattern = "*",

            [Option("force", ShortName = 'f', Description = "Skip confirmation")]
            bool force = false)
        {
            Console.WriteLine($"Clearing cache keys matching: {pattern}");
            Console.WriteLine("Cache cleared successfully!");
        }
    }
}

public enum MigrationDirection
{
    Up,
    Down
}

public enum CacheProvider
{
    Memory,
    Redis,
    Memcached
}
