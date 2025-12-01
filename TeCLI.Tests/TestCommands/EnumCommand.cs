using TeCLI.Attributes;
using System;
using System.Collections.Generic;

namespace TeCLI.Tests.TestCommands;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

[Flags]
public enum FilePermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4,
    ReadWrite = Read | Write,
    All = Read | Write | Execute
}

public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}

[Command("enum", Description = "Test command with enum parameters")]
public class EnumCommand
{
    public static bool WasCalled { get; private set; }
    public static LogLevel CapturedLogLevel { get; private set; }
    public static FilePermissions CapturedPermissions { get; private set; }
    public static Priority CapturedPriority { get; private set; }
    public static LogLevel[] CapturedLevels { get; private set; }
    public static List<Priority> CapturedPriorities { get; private set; }

    [Action("run")]
    public void Run(
        [Option("log-level", ShortName = 'l')] LogLevel level = LogLevel.Info,
        [Option("permissions", ShortName = 'p')] FilePermissions permissions = FilePermissions.Read)
    {
        WasCalled = true;
        CapturedLogLevel = level;
        CapturedPermissions = permissions;
    }

    [Action("process")]
    public void Process([Argument] Priority priority)
    {
        WasCalled = true;
        CapturedPriority = priority;
    }

    [Action("batch")]
    public void Batch(
        [Option("levels")] LogLevel[] levels,
        [Option("priorities")] List<Priority>? priorities = null)
    {
        WasCalled = true;
        CapturedLevels = levels;
        CapturedPriorities = priorities;
    }

    [Action("multi")]
    public void Multi([Argument] Priority[] priorities)
    {
        WasCalled = true;
        CapturedPriorities = new List<Priority>(priorities);
    }

    public static void Reset()
    {
        WasCalled = false;
        CapturedLogLevel = default;
        CapturedPermissions = default;
        CapturedPriority = default;
        CapturedLevels = null;
        CapturedPriorities = null;
    }
}
