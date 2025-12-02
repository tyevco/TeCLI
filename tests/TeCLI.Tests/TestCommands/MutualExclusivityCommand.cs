using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

[Command("mutualexclusive", Description = "Test command with mutually exclusive options")]
public class MutualExclusivityCommand
{
    public static bool WasCalled { get; private set; }
    public static bool CapturedJson { get; private set; }
    public static bool CapturedXml { get; private set; }
    public static bool CapturedYaml { get; private set; }
    public static string? CapturedFormat { get; private set; }
    public static string? CapturedEncoding { get; private set; }
    public static bool CapturedCompact { get; private set; }
    public static bool CapturedPretty { get; private set; }

    /// <summary>
    /// Test action with mutually exclusive boolean switches for format selection
    /// </summary>
    [Action("output")]
    public void Output(
        [Option("json", ShortName = 'j', MutuallyExclusiveSet = "format")] bool json = false,
        [Option("xml", ShortName = 'x', MutuallyExclusiveSet = "format")] bool xml = false,
        [Option("yaml", ShortName = 'y', MutuallyExclusiveSet = "format")] bool yaml = false)
    {
        WasCalled = true;
        CapturedJson = json;
        CapturedXml = xml;
        CapturedYaml = yaml;
    }

    /// <summary>
    /// Test action with mutually exclusive value options
    /// </summary>
    [Action("convert")]
    public void Convert(
        [Option("format", ShortName = 'f', MutuallyExclusiveSet = "outputType")] string? format = null,
        [Option("encoding", ShortName = 'e', MutuallyExclusiveSet = "outputType")] string? encoding = null)
    {
        WasCalled = true;
        CapturedFormat = format;
        CapturedEncoding = encoding;
    }

    /// <summary>
    /// Test action with multiple mutually exclusive sets
    /// </summary>
    [Action("process")]
    public void Process(
        [Option("json", MutuallyExclusiveSet = "format")] bool json = false,
        [Option("xml", MutuallyExclusiveSet = "format")] bool xml = false,
        [Option("compact", MutuallyExclusiveSet = "style")] bool compact = false,
        [Option("pretty", MutuallyExclusiveSet = "style")] bool pretty = false)
    {
        WasCalled = true;
        CapturedJson = json;
        CapturedXml = xml;
        CapturedCompact = compact;
        CapturedPretty = pretty;
    }

    /// <summary>
    /// Test action with some options in exclusive set and some not
    /// </summary>
    [Action("export")]
    public void Export(
        [Option("json", MutuallyExclusiveSet = "format")] bool json = false,
        [Option("xml", MutuallyExclusiveSet = "format")] bool xml = false,
        [Option("verbose", ShortName = 'v')] bool verbose = false)
    {
        WasCalled = true;
        CapturedJson = json;
        CapturedXml = xml;
    }

    public static void Reset()
    {
        WasCalled = false;
        CapturedJson = false;
        CapturedXml = false;
        CapturedYaml = false;
        CapturedFormat = null;
        CapturedEncoding = null;
        CapturedCompact = false;
        CapturedPretty = false;
    }
}
