using TylerCLI.Attributes;

namespace TylerCLI.Example;

public class CommandLineOptions
{
    [Argument]
    public string Name { get; set; }
    
    [Argument]
    public string Subject { get; set; }

    [Option("verbose", ShortName = 'v')]
    public bool Verbose { get; set; } = false;
}