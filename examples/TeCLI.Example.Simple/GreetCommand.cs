using TeCLI.Attributes;

namespace TeCLI.Example.Simple;

/// <summary>
/// A greeting command demonstrating:
/// - Simple string arguments
/// - Boolean flags (switches)
/// - Optional parameters with defaults
/// - Async actions
/// </summary>
[Command("greet", Description = "Greeting utilities")]
public class GreetCommand
{
    /// <summary>
    /// Say hello to someone
    /// </summary>
    [Primary]
    [Action("hello", Description = "Say hello to someone")]
    public void Hello(
        [Argument(Description = "Name of the person to greet")] string name,
        [Option("excited", ShortName = 'e', Description = "Use excited greeting")] bool excited = false,
        [Option("times", ShortName = 't', Description = "Number of times to greet")] int times = 1)
    {
        var greeting = excited ? $"Hello, {name}!!!" : $"Hello, {name}.";

        for (int i = 0; i < times; i++)
        {
            Console.WriteLine(greeting);
        }
    }

    /// <summary>
    /// Say goodbye to someone
    /// </summary>
    [Action("goodbye", Description = "Say goodbye to someone")]
    public void Goodbye(
        [Argument(Description = "Name of the person")] string name,
        [Option("formal", ShortName = 'f', Description = "Use formal goodbye")] bool formal = false)
    {
        var farewell = formal
            ? $"Farewell, {name}. Until we meet again."
            : $"Bye, {name}!";
        Console.WriteLine(farewell);
    }

    /// <summary>
    /// Generate a personalized message
    /// </summary>
    [Action("message", Description = "Send a custom message")]
    public async Task MessageAsync(
        [Argument(Description = "Recipient name")] string recipient,
        [Argument(Description = "Message content")] string content,
        [Option("delay", ShortName = 'd', Description = "Delay in milliseconds before sending")] int delay = 0)
    {
        if (delay > 0)
        {
            Console.WriteLine($"Sending message in {delay}ms...");
            await Task.Delay(delay);
        }

        Console.WriteLine($"To: {recipient}");
        Console.WriteLine($"Message: {content}");
        Console.WriteLine("Message sent!");
    }
}
