

using TeCLI.Attributes;

namespace TeCLI.Example.DependencyInjection;

[Command("config")]
public class ConfigurationCommand
{
    [Primary]
    [Action("setup", Description = "Setup application through interactive prompts.")]
    public Task SetupAsync([Argument] string filename, CommandLineOptions otherParameters)
    {
        return Task.CompletedTask;
    }

    [Action("reload", Description = "Reload the configuration files into memory.")]
    public Task ReloadAsync()
    {
        return Task.CompletedTask;
    }

    [Action("watch", Description = "Watch a file and trigger event upon changes being detected.")]
    public async Task WatchAsync(string filename = "*", string extensionFilter = "*", bool allowDuplicates = false, bool haltOnError = false, string logLevel = "WARN")
    {
        await Task.Delay(2000);
    }

    [Action("finalize", Description = "Finalizes the configuration.")]
    public Task Finalize(
                [Option("serverUrl", ShortName = 's')]
                string url  = "https://localhost",

                [Option("serverName", ShortName = 'n')]
                string? name = null)
    {
        return Task.CompletedTask;
    }

    [Action("sync", Description = "Syncs the clocks.")]
    public void Synchronize()
    {
    }
}

