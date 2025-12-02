using System.IO;
using TeCLI.Attributes;
using TeCLI.Attributes.Validation;

namespace TeCLI.Tests.TestCommands;

[Command("validate", Description = "Test command with validation attributes")]
public class ValidationCommand
{
    public static bool WasCalled { get; private set; }
    public static int CapturedPort { get; private set; }
    public static string? CapturedUsername { get; private set; }
    public static string? CapturedInputFile { get; private set; }
    public static string? CapturedOutputDir { get; private set; }
    public static int CapturedTimeout { get; private set; }
    public static double CapturedPercentage { get; private set; }
    public static string? CapturedEmail { get; private set; }
    public static string? CapturedConfigFile { get; private set; }

    [Action("connect")]
    public void Connect(
        [Option("port", ShortName = 'p')] [Range(1, 65535)] int port = 8080,
        [Option("username", ShortName = 'u')] [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$")] string? username = null,
        [Option("connection-timeout")] [Range(0, 3600)] int timeout = 30)
    {
        WasCalled = true;
        CapturedPort = port;
        CapturedUsername = username;
        CapturedTimeout = timeout;
    }

    [Action("process")]
    public void Process(
        [Argument(Description = "Input file path")] [FileExists] string inputFile,
        [Option("output-dir")] [DirectoryExists] string? outputDir = null)
    {
        WasCalled = true;
        CapturedInputFile = inputFile;
        CapturedOutputDir = outputDir;
    }

    [Action("analyze")]
    public void Analyze(
        [Option("percentage")] [Range(0.0, 100.0)] double percentage = 50.0,
        [Option("email")] [RegularExpression(@"^[^@]+@[^@]+\.[^@]+$", ErrorMessage = "Invalid email format")] string? email = null)
    {
        WasCalled = true;
        CapturedPercentage = percentage;
        CapturedEmail = email;
    }

    [Action("run")]
    public void Run(
        [Argument(Description = "Config file")] [FileExists] string configFile)
    {
        WasCalled = true;
        CapturedConfigFile = configFile;
    }

    public static void Reset()
    {
        WasCalled = false;
        CapturedPort = 0;
        CapturedUsername = null;
        CapturedInputFile = null;
        CapturedOutputDir = null;
        CapturedTimeout = 0;
        CapturedPercentage = 0.0;
        CapturedEmail = null;
        CapturedConfigFile = null;
    }
}
