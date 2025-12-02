using TeCLI.Attributes;

namespace TeCLI.Example.PureDI;

[Command("greet")]
public class GreetCommand
{
    [Primary]
    [Action("hello", Description = "Say hello to someone.")]
    public Task HelloAsync([Argument] string name, [Option("excited")] bool excited = false)
    {
        var greeting = excited ? $"Hello, {name}!" : $"Hello, {name}.";
        Console.WriteLine(greeting);
        return Task.CompletedTask;
    }

    [Action("goodbye", Description = "Say goodbye to someone.")]
    public Task GoodbyeAsync([Argument] string name)
    {
        Console.WriteLine($"Goodbye, {name}!");
        return Task.CompletedTask;
    }
}
