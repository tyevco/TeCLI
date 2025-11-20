using System.Collections.Generic;
using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

[Command("envvar", Description = "Test command with environment variable binding")]
public class EnvVarCommand
{
    public static bool WasCalled { get; private set; }
    public static string? CapturedApiKey { get; private set; }
    public static int CapturedPort { get; private set; }
    public static string? CapturedEnvironment { get; private set; }
    public static bool CapturedVerbose { get; private set; }
    public static int CapturedTimeout { get; private set; }
    public static string[]? CapturedTags { get; private set; }
    public static string? CapturedRegion { get; private set; }

    [Action("connect")]
    public void Connect(
        [Option("api-key", EnvVar = "API_KEY", Required = true)] string apiKey,
        [Option("port", EnvVar = "PORT")] int port = 8080,
        [Option("timeout", EnvVar = "TIMEOUT")] int timeout = 30)
    {
        WasCalled = true;
        CapturedApiKey = apiKey;
        CapturedPort = port;
        CapturedTimeout = timeout;
    }

    [Action("deploy")]
    public void Deploy(
        [Option("environment", ShortName = 'e', EnvVar = "DEPLOY_ENV")] string environment = "dev",
        [Option("verbose", ShortName = 'v', EnvVar = "VERBOSE")] bool verbose = false,
        [Option("region", EnvVar = "REGION")] string? region = null)
    {
        WasCalled = true;
        CapturedEnvironment = environment;
        CapturedVerbose = verbose;
        CapturedRegion = region;
    }

    [Action("process")]
    public void Process(
        [Option("tags", ShortName = 't', EnvVar = "TAGS")] string[]? tags = null)
    {
        WasCalled = true;
        CapturedTags = tags;
    }

    public static void Reset()
    {
        WasCalled = false;
        CapturedApiKey = null;
        CapturedPort = 0;
        CapturedEnvironment = null;
        CapturedVerbose = false;
        CapturedTimeout = 0;
        CapturedTags = null;
        CapturedRegion = null;
    }
}
