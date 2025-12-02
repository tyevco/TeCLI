using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

/// <summary>
/// Test command demonstrating exit code support
/// </summary>
[Command("exitcode", Description = "Exit code test command")]
[MapExitCode(typeof(UnauthorizedAccessException), ExitCode.PermissionDenied)]
public class ExitCodeCommand
{
    public static bool WasCalled { get; private set; }
    public static string? LastAction { get; private set; }
    public static int? LastExitCode { get; private set; }

    public static void Reset()
    {
        WasCalled = false;
        LastAction = null;
        LastExitCode = null;
    }

    /// <summary>
    /// Action that returns ExitCode.Success
    /// </summary>
    [Primary]
    [Action("success", Description = "Returns ExitCode.Success")]
    public ExitCode Success()
    {
        WasCalled = true;
        LastAction = "success";
        return ExitCode.Success;
    }

    /// <summary>
    /// Action that returns ExitCode.Error
    /// </summary>
    [Action("error", Description = "Returns ExitCode.Error")]
    public ExitCode Error()
    {
        WasCalled = true;
        LastAction = "error";
        return ExitCode.Error;
    }

    /// <summary>
    /// Action that returns a specific exit code based on argument
    /// </summary>
    [Action("code", Description = "Returns specified exit code")]
    public ExitCode WithCode([Argument] int code)
    {
        WasCalled = true;
        LastAction = "code";
        LastExitCode = code;
        return (ExitCode)code;
    }

    /// <summary>
    /// Action that returns int directly
    /// </summary>
    [Action("intcode", Description = "Returns int exit code")]
    public int IntExitCode([Argument] int code)
    {
        WasCalled = true;
        LastAction = "intcode";
        LastExitCode = code;
        return code;
    }

    /// <summary>
    /// Action that returns ExitCode.FileNotFound
    /// </summary>
    [Action("filenotfound", Description = "Returns ExitCode.FileNotFound")]
    public ExitCode FileNotFound()
    {
        WasCalled = true;
        LastAction = "filenotfound";
        return ExitCode.FileNotFound;
    }

    /// <summary>
    /// Action that returns void (should result in exit code 0)
    /// </summary>
    [Action("void", Description = "Returns void (exit code 0)")]
    public void VoidAction()
    {
        WasCalled = true;
        LastAction = "void";
    }

    /// <summary>
    /// Async action that returns Task<ExitCode>
    /// </summary>
    [Action("asyncenum", Description = "Async action returning Task<ExitCode>")]
    public async Task<ExitCode> AsyncEnumExitCode([Argument] int code)
    {
        WasCalled = true;
        LastAction = "asyncenum";
        LastExitCode = code;
        await Task.Delay(1);
        return (ExitCode)code;
    }

    /// <summary>
    /// Async action that returns Task<int>
    /// </summary>
    [Action("asyncint", Description = "Async action returning Task<int>")]
    public async Task<int> AsyncIntExitCode([Argument] int code)
    {
        WasCalled = true;
        LastAction = "asyncint";
        LastExitCode = code;
        await Task.Delay(1);
        return code;
    }

    /// <summary>
    /// Async action that returns Task (void, should result in exit code 0)
    /// </summary>
    [Action("asyncvoid", Description = "Async action returning Task")]
    public async Task AsyncVoidAction()
    {
        WasCalled = true;
        LastAction = "asyncvoid";
        await Task.Delay(1);
    }

    /// <summary>
    /// Action with exception mapping that throws
    /// </summary>
    [Action("mapped", Description = "Throws exception mapped to exit code")]
    [MapExitCode(typeof(FileNotFoundException), ExitCode.FileNotFound)]
    public ExitCode MappedException([Argument] string exceptionType)
    {
        WasCalled = true;
        LastAction = "mapped";

        return exceptionType switch
        {
            "filenotfound" => throw new FileNotFoundException("File not found"),
            "unauthorized" => throw new UnauthorizedAccessException("Access denied"),
            _ => ExitCode.Success
        };
    }
}

/// <summary>
/// Test command with custom exit code enum
/// </summary>
[Command("customexit", Description = "Custom exit code enum test")]
public class CustomExitCodeCommand
{
    public static bool WasCalled { get; private set; }
    public static string? LastAction { get; private set; }

    public static void Reset()
    {
        WasCalled = false;
        LastAction = null;
    }

    /// <summary>
    /// Custom exit code enum for this command
    /// </summary>
    public enum CustomExitCode
    {
        Ok = 0,
        Warning = 10,
        Failure = 20,
        Critical = 30
    }

    /// <summary>
    /// Action returning custom enum
    /// </summary>
    [Action("custom", Description = "Returns custom exit code")]
    public CustomExitCode CustomCode([Argument] int code)
    {
        WasCalled = true;
        LastAction = "custom";
        return (CustomExitCode)code;
    }

    /// <summary>
    /// Async action returning custom enum
    /// </summary>
    [Action("asynccustom", Description = "Async returns custom exit code")]
    public async Task<CustomExitCode> AsyncCustomCode([Argument] int code)
    {
        WasCalled = true;
        LastAction = "asynccustom";
        await Task.Delay(1);
        return (CustomExitCode)code;
    }
}
