using TeCLI.Attributes;
using TeCLI.Localization;
using TeCLI.Example.Localization.Resources;

namespace TeCLI.Example.Localization;

/// <summary>
/// Example command demonstrating localization support.
/// Uses LocalizedDescription attributes for i18n descriptions.
/// </summary>
[Command("greet", Aliases = new[] { "g" })]
[LocalizedDescription("GreetCommand_Description")]
public class GreetCommand
{
    /// <summary>
    /// Primary action - say hello to someone.
    /// </summary>
    [Primary]
    [LocalizedDescription("GreetCommand_Hello_Description")]
    public void Hello(
        [Argument]
        [LocalizedDescription("GreetCommand_Hello_Name_Description")]
        string name,

        [Option("formal", ShortName = 'f')]
        [LocalizedDescription("GreetCommand_Hello_Formal_Description")]
        bool formal = false)
    {
        // Use localized output messages
        if (formal)
        {
            Console.WriteLine(Localizer.GetString("Greeting_HelloFormal", name));
        }
        else
        {
            Console.WriteLine(Localizer.GetString("Greeting_Hello", name));
        }
    }

    /// <summary>
    /// Say goodbye to someone.
    /// </summary>
    [Action("goodbye", Aliases = new[] { "bye" })]
    [LocalizedDescription("GreetCommand_Goodbye_Description")]
    public void Goodbye(
        [Argument]
        [LocalizedDescription("GreetCommand_Hello_Name_Description")]
        string name)
    {
        Console.WriteLine(Localizer.GetString("Greeting_Goodbye", name));
    }
}
