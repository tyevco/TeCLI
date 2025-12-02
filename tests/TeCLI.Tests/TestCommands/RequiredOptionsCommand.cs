using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

[Command("required", Description = "Test command with required options")]
public class RequiredOptionsCommand
{
    public static bool WasCalled { get; private set; }
    public static string? CapturedEnvironment { get; private set; }
    public static string? CapturedRegion { get; private set; }
    public static bool CapturedVerbose { get; private set; }
    public static string[]? CapturedTags { get; private set; }
    public static string? CapturedApiKey { get; private set; }

    [Action("deploy")]
    public void Deploy(
        [Option("environment", ShortName = 'e', Required = true)] string environment,
        [Option("region")] string region = "us-west",
        [Option("verbose", ShortName = 'v')] bool verbose = false)
    {
        WasCalled = true;
        CapturedEnvironment = environment;
        CapturedRegion = region;
        CapturedVerbose = verbose;
    }

    [Action("process")]
    public void Process(
        [Option("tags", ShortName = 't', Required = true)] string[] tags,
        [Option("verbose", ShortName = 'v')] bool verbose = false)
    {
        WasCalled = true;
        CapturedTags = tags;
        CapturedVerbose = verbose;
    }

    [Action("connect")]
    public void Connect(
        [Option("api-key", Required = true)] string apiKey,
        [Option("timeout")] int timeout = 30)
    {
        WasCalled = true;
        CapturedApiKey = apiKey;
    }

    public static void Reset()
    {
        WasCalled = false;
        CapturedEnvironment = null;
        CapturedRegion = null;
        CapturedVerbose = false;
        CapturedTags = null;
        CapturedApiKey = null;
    }
}
