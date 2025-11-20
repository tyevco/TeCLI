using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

[Command("remove", Aliases = new[] { "rm", "delete" }, Description = "Test command with aliases")]
public class AliasesCommand
{
    public static bool WasCalled { get; private set; }
    public static string? CapturedMethod { get; private set; }
    public static string? CapturedPath { get; private set; }
    public static bool CapturedForce { get; private set; }

    [Action("file", Aliases = new[] { "f" }, Description = "Remove a file")]
    public void RemoveFile([Option("path", ShortName = 'p')] string path, [Option("force")] bool force = false)
    {
        WasCalled = true;
        CapturedMethod = "RemoveFile";
        CapturedPath = path;
        CapturedForce = force;
    }

    [Action("directory", Aliases = new[] { "dir", "d", "folder" }, Description = "Remove a directory")]
    public void RemoveDirectory([Option("path")] string path, [Option("recursive", ShortName = 'r')] bool recursive = false)
    {
        WasCalled = true;
        CapturedMethod = "RemoveDirectory";
        CapturedPath = path;
        CapturedForce = recursive; // Reusing the flag for simplicity
    }

    [Action("list", Aliases = new[] { "ls", "show" })]
    public void List()
    {
        WasCalled = true;
        CapturedMethod = "List";
    }

    public static void Reset()
    {
        WasCalled = false;
        CapturedMethod = null;
        CapturedPath = null;
        CapturedForce = false;
    }
}
