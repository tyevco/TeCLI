using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

/// <summary>
/// Global options class that will be available to all commands
/// </summary>
[GlobalOptions]
public class AppGlobalOptions
{
    [Option("verbose", ShortName = 'v')]
    public bool Verbose { get; set; }

    [Option("config")]
    public string? ConfigFile { get; set; }

    [Option("log-level")]
    public string LogLevel { get; set; } = "info";

    [Option("timeout")]
    public int Timeout { get; set; } = 30;

    [Option("debug", ShortName = 'd')]
    public bool Debug { get; set; }
}

/// <summary>
/// Test command that uses global options
/// </summary>
[Command("globaltest", Description = "Test command for global options")]
public class GlobalOptionsCommand
{
    public static bool WasCalled { get; private set; }
    public static string? LastAction { get; private set; }
    public static bool? CapturedVerbose { get; private set; }
    public static bool? CapturedDebug { get; private set; }
    public static string? CapturedConfig { get; private set; }
    public static string? CapturedLogLevel { get; private set; }
    public static int? CapturedTimeout { get; private set; }
    public static string? CapturedFileName { get; private set; }
    public static int? CapturedCount { get; private set; }

    [Action("process", Description = "Process with global options")]
    public void Process(
        AppGlobalOptions globals,
        [Argument] string fileName)
    {
        WasCalled = true;
        LastAction = "process";
        CapturedVerbose = globals.Verbose;
        CapturedDebug = globals.Debug;
        CapturedConfig = globals.ConfigFile;
        CapturedLogLevel = globals.LogLevel;
        CapturedTimeout = globals.Timeout;
        CapturedFileName = fileName;
    }

    [Action("deploy", Description = "Deploy with global options")]
    public void Deploy(
        AppGlobalOptions globals,
        [Argument] string environment,
        [Option("count")] int count = 1)
    {
        WasCalled = true;
        LastAction = "deploy";
        CapturedVerbose = globals.Verbose;
        CapturedDebug = globals.Debug;
        CapturedConfig = globals.ConfigFile;
        CapturedLogLevel = globals.LogLevel;
        CapturedTimeout = globals.Timeout;
        CapturedFileName = environment;
        CapturedCount = count;
    }

    [Action("simple", Description = "Action with only global options")]
    public void Simple(AppGlobalOptions globals)
    {
        WasCalled = true;
        LastAction = "simple";
        CapturedVerbose = globals.Verbose;
        CapturedDebug = globals.Debug;
        CapturedConfig = globals.ConfigFile;
        CapturedLogLevel = globals.LogLevel;
        CapturedTimeout = globals.Timeout;
    }

    [Action("noopt", Description = "Action without any options")]
    public void NoOpt()
    {
        WasCalled = true;
        LastAction = "noopt";
    }

    [Action("mixed", Description = "Mixed with regular options")]
    public void Mixed(
        AppGlobalOptions globals,
        [Option("name")] string name,
        [Option("age")] int age = 0)
    {
        WasCalled = true;
        LastAction = "mixed";
        CapturedVerbose = globals.Verbose;
        CapturedConfig = name;
        CapturedTimeout = age;
    }

    public static void Reset()
    {
        WasCalled = false;
        LastAction = null;
        CapturedVerbose = null;
        CapturedDebug = null;
        CapturedConfig = null;
        CapturedLogLevel = null;
        CapturedTimeout = null;
        CapturedFileName = null;
        CapturedCount = null;
    }
}
