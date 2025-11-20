using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

[Command("git", Description = "Git version control system")]
public class CompletionTestCommand
{
    public static bool WasCalled { get; private set; }
    public static string? CapturedMethod { get; private set; }
    public static string? CapturedMessage { get; private set; }
    public static string? CapturedBranch { get; private set; }

    [Action("status", Description = "Show the working tree status")]
    public void Status([Option("short", ShortName = 's')] bool showShort = false)
    {
        WasCalled = true;
        CapturedMethod = "Status";
    }

    [Action("commit", Description = "Record changes to the repository")]
    public void Commit(
        [Option("message", ShortName = 'm')] string message,
        [Option("all", ShortName = 'a')] bool all = false)
    {
        WasCalled = true;
        CapturedMethod = "Commit";
        CapturedMessage = message;
    }

    [Action("checkout", Description = "Switch branches or restore working tree files")]
    public void Checkout([Argument] string branch)
    {
        WasCalled = true;
        CapturedMethod = "Checkout";
        CapturedBranch = branch;
    }

    [Command("remote", Description = "Manage remote repositories")]
    public class RemoteCommand
    {
        [Action("add", Description = "Add a remote repository")]
        public void Add(
            [Argument] string name,
            [Argument] string url)
        {
            WasCalled = true;
            CapturedMethod = "RemoteAdd";
        }

        [Action("remove", Aliases = new[] { "rm" }, Description = "Remove a remote repository")]
        public void Remove([Argument] string name)
        {
            WasCalled = true;
            CapturedMethod = "RemoteRemove";
        }

        [Action("list", Aliases = new[] { "ls" }, Description = "List remote repositories")]
        public void List([Option("verbose", ShortName = 'v')] bool verbose = false)
        {
            WasCalled = true;
            CapturedMethod = "RemoteList";
        }
    }

    [Command("config", Description = "Get and set repository options")]
    public class ConfigCommand
    {
        [Action("get", Description = "Get a configuration value")]
        public void Get([Argument] string key)
        {
            WasCalled = true;
            CapturedMethod = "ConfigGet";
        }

        [Action("set", Description = "Set a configuration value")]
        public void Set(
            [Argument] string key,
            [Argument] string value)
        {
            WasCalled = true;
            CapturedMethod = "ConfigSet";
        }
    }

    public static void Reset()
    {
        WasCalled = false;
        CapturedMethod = null;
        CapturedMessage = null;
        CapturedBranch = null;
    }
}
