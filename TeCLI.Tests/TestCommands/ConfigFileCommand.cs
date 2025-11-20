using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

/// <summary>
/// Test command for configuration file support
/// </summary>
[Command("config", Description = "Test command for configuration file support")]
public class ConfigFileCommand
{
    public static bool WasCalled { get; private set; }
    public static string? LastAction { get; private set; }
    public static string? CapturedEnvironment { get; private set; }
    public static string? CapturedRegion { get; private set; }
    public static bool? CapturedVerbose { get; private set; }
    public static int? CapturedPort { get; private set; }
    public static string? CapturedHost { get; private set; }
    public static string? CapturedOutput { get; private set; }

    [Action("deploy", Description = "Deploy with config file support")]
    public void Deploy(
        [Option("environment")] string? environment = null,
        [Option("region")] string? region = null,
        [Option("verbose")] bool verbose = false)
    {
        WasCalled = true;
        LastAction = "deploy";
        CapturedEnvironment = environment;
        CapturedRegion = region;
        CapturedVerbose = verbose;
    }

    [Action("connect", Description = "Connect with config file support")]
    public void Connect(
        [Option("host")] string? host = null,
        [Option("port")] int port = 8080,
        [Option("verbose")] bool verbose = false)
    {
        WasCalled = true;
        LastAction = "connect";
        CapturedHost = host;
        CapturedPort = port;
        CapturedVerbose = verbose;
    }

    [Action("build", Description = "Build with config file support")]
    public void Build(
        [Option("output")] string? output = null,
        [Option("verbose")] bool verbose = false)
    {
        WasCalled = true;
        LastAction = "build";
        CapturedOutput = output;
        CapturedVerbose = verbose;
    }

    public static void Reset()
    {
        WasCalled = false;
        LastAction = null;
        CapturedEnvironment = null;
        CapturedRegion = null;
        CapturedVerbose = null;
        CapturedPort = null;
        CapturedHost = null;
        CapturedOutput = null;
    }
}
