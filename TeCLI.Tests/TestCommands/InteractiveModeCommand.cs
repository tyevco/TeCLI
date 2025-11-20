using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

[Command("interactive", Description = "Test command for interactive mode prompts")]
public class InteractiveModeCommand
{
    public static bool WasCalled { get; private set; }
    public static string? LastEnvironment { get; private set; }
    public static string? LastRegion { get; private set; }
    public static string? LastUsername { get; private set; }
    public static string? LastPassword { get; private set; }
    public static int? LastPort { get; private set; }

    [Action("deploy")]
    public void Deploy(
        [Argument(Prompt = "Enter deployment environment")] string environment,
        [Option("region", Prompt = "Select deployment region")] string region = "us-west")
    {
        WasCalled = true;
        LastEnvironment = environment;
        LastRegion = region;
    }

    [Action("login")]
    public void Login(
        [Argument(Prompt = "Enter username")] string username,
        [Argument(Prompt = "Enter password", SecurePrompt = true)] string password)
    {
        WasCalled = true;
        LastUsername = username;
        LastPassword = password;
    }

    [Action("connect")]
    public void Connect(
        [Option("port", Prompt = "Enter port number")] int port = 8080)
    {
        WasCalled = true;
        LastPort = port;
    }

    [Action("secure-option")]
    public void SecureOption(
        [Option("api-key", Prompt = "Enter API key", SecurePrompt = true)] string apiKey)
    {
        WasCalled = true;
        LastPassword = apiKey; // Reuse LastPassword for testing
    }

    public static void Reset()
    {
        WasCalled = false;
        LastEnvironment = null;
        LastRegion = null;
        LastUsername = null;
        LastPassword = null;
        LastPort = null;
    }
}
