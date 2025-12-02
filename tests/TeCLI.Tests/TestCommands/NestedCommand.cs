using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

/// <summary>
/// Test command demonstrating 2-level nested subcommands
/// Models a structure like "git remote add"
/// </summary>
[Command("gitcli", Description = "Distributed version control system")]
public class NestedCommand
{
    public static bool WasCalled { get; private set; }
    public static string? LastAction { get; private set; }
    public static string? CapturedName { get; private set; }
    public static string? CapturedUrl { get; private set; }
    public static string? CapturedPath { get; private set; }
    public static string? CapturedBranch { get; private set; }

    [Action("status")]
    public void Status()
    {
        WasCalled = true;
        LastAction = "status";
    }

    [Action("commit", Description = "Record changes to the repository")]
    public void Commit(
        [Option("message", ShortName = 'm')] string message)
    {
        WasCalled = true;
        LastAction = "commit";
        CapturedName = message;
    }

    /// <summary>
    /// Nested subcommand for managing remotes
    /// </summary>
    [Command("remote", Description = "Manage remote repositories", Aliases = new[] { "rem" })]
    public class RemoteCommand
    {
        [Action("add", Description = "Add a remote repository")]
        public void Add(
            [Argument(Description = "Name of the remote")] string name,
            [Argument(Description = "URL of the remote")] string url)
        {
            WasCalled = true;
            LastAction = "remote-add";
            CapturedName = name;
            CapturedUrl = url;
        }

        [Action("remove", Description = "Remove a remote repository", Aliases = new[] { "rm" })]
        public void Remove(
            [Argument(Description = "Name of the remote")] string name)
        {
            WasCalled = true;
            LastAction = "remote-remove";
            CapturedName = name;
        }

        [Action("list", Description = "List all remotes")]
        public void List(
            [Option("verbose", ShortName = 'v')] bool verbose = false)
        {
            WasCalled = true;
            LastAction = "remote-list";
            CapturedName = verbose.ToString();
        }
    }

    /// <summary>
    /// Nested subcommand for managing branches
    /// </summary>
    [Command("branch", Description = "Manage branches")]
    public class BranchCommand
    {
        [Action("create", Description = "Create a new branch")]
        public void Create(
            [Argument(Description = "Name of the branch")] string name)
        {
            WasCalled = true;
            LastAction = "branch-create";
            CapturedBranch = name;
        }

        [Action("delete", Description = "Delete a branch", Aliases = new[] { "del" })]
        public void Delete(
            [Argument(Description = "Name of the branch")] string name,
            [Option("force", ShortName = 'f')] bool force = false)
        {
            WasCalled = true;
            LastAction = "branch-delete";
            CapturedBranch = name;
            CapturedName = force.ToString();
        }

        [Action("list", Description = "List all branches")]
        public void List()
        {
            WasCalled = true;
            LastAction = "branch-list";
        }
    }

    /// <summary>
    /// Nested subcommand with its own nested subcommand (3-level nesting)
    /// </summary>
    [Command("config", Description = "Configure git settings")]
    public class ConfigCommand
    {
        [Action("get", Description = "Get a config value")]
        public void Get(
            [Argument(Description = "Config key")] string key)
        {
            WasCalled = true;
            LastAction = "config-get";
            CapturedName = key;
        }

        [Action("set", Description = "Set a config value")]
        public void Set(
            [Argument(Description = "Config key")] string key,
            [Argument(Description = "Config value")] string value)
        {
            WasCalled = true;
            LastAction = "config-set";
            CapturedName = key;
            CapturedUrl = value;
        }

        /// <summary>
        /// 3-level nested subcommand
        /// </summary>
        [Command("user", Description = "Manage user settings")]
        public class UserCommand
        {
            [Action("name", Description = "Set user name")]
            public void Name(
                [Argument(Description = "User name")] string name)
            {
                WasCalled = true;
                LastAction = "config-user-name";
                CapturedName = name;
            }

            [Action("email", Description = "Set user email")]
            public void Email(
                [Argument(Description = "User email")] string email)
            {
                WasCalled = true;
                LastAction = "config-user-email";
                CapturedUrl = email;
            }
        }
    }

    public static void Reset()
    {
        WasCalled = false;
        LastAction = null;
        CapturedName = null;
        CapturedUrl = null;
        CapturedPath = null;
        CapturedBranch = null;
    }
}
