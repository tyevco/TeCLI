using TeCLI.Attributes;
using System.Collections.Generic;

namespace TeCLI.Tests.TestCommands;

[Command("collection", Description = "Test command with collection parameters")]
public class CollectionCommand
{
    public static bool WasCalled { get; private set; }
    public static string[]? CapturedFiles { get; private set; }
    public static List<int>? CapturedPorts { get; private set; }
    public static IEnumerable<string>? CapturedTags { get; private set; }
    public static string[]? CapturedArgs { get; private set; }

    [Action("process")]
    public void Process(
        [Option("files", ShortName = 'f')] string[] files,
        [Option("ports", ShortName = 'p')] List<int>? ports = null,
        [Option("tags")] IEnumerable<string>? tags = null)
    {
        WasCalled = true;
        CapturedFiles = files;
        CapturedPorts = ports;
        CapturedTags = tags;
    }

    [Action("build")]
    public void Build([Argument] string[] sources)
    {
        WasCalled = true;
        CapturedArgs = sources;
    }

    [Action("copy")]
    public void Copy(
        [Argument] string source,
        [Argument] string[] destinations)
    {
        WasCalled = true;
        CapturedFiles = new[] { source };
        CapturedArgs = destinations;
    }

    public static void Reset()
    {
        WasCalled = false;
        CapturedFiles = null;
        CapturedPorts = null;
        CapturedTags = null;
        CapturedArgs = null;
    }
}
