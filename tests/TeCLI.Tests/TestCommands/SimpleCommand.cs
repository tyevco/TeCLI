using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

[Command("simple", Description = "A simple test command")]
public class SimpleCommand
{
    public static bool WasCalled { get; private set; }
    public static string? LastMessage { get; private set; }

    [Primary]
    [Action("run")]
    public void Run()
    {
        WasCalled = true;
        LastMessage = "Run called";
    }

    [Action("greet")]
    public void Greet([Argument] string name)
    {
        WasCalled = true;
        LastMessage = $"Hello, {name}!";
    }

    public static void Reset()
    {
        WasCalled = false;
        LastMessage = null;
    }
}
