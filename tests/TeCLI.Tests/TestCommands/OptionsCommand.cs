using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

[Command("options", Description = "Test command with options")]
public class OptionsCommand
{
    public static bool WasCalled { get; private set; }
    public static string? CapturedEnvironment { get; private set; }
    public static bool CapturedForce { get; private set; }
    public static int CapturedTimeout { get; private set; }

    [Action("deploy")]
    public void Deploy(
        [Option("environment", ShortName = 'e')] string environment = "dev",
        [Option("force", ShortName = 'f')] bool force = false,
        [Option("timeout", ShortName = 't')] int timeout = 30)
    {
        WasCalled = true;
        CapturedEnvironment = environment;
        CapturedForce = force;
        CapturedTimeout = timeout;
    }

    public static void Reset()
    {
        WasCalled = false;
        CapturedEnvironment = null;
        CapturedForce = false;
        CapturedTimeout = 0;
    }
}
