using System;
using System.IO;
using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

[Command("types", Description = "Test command with common types")]
public class CommonTypesCommand
{
    public static bool WasCalled { get; private set; }
    public static Uri? CapturedUri { get; private set; }
    public static DateTime? CapturedDateTime { get; private set; }
    public static TimeSpan? CapturedTimeSpan { get; private set; }
    public static Guid? CapturedGuid { get; private set; }
    public static FileInfo? CapturedFileInfo { get; private set; }
    public static DirectoryInfo? CapturedDirectoryInfo { get; private set; }
    public static DateTimeOffset? CapturedDateTimeOffset { get; private set; }

    [Action("uri")]
    public void TestUri([Option("url")] Uri url)
    {
        WasCalled = true;
        CapturedUri = url;
    }

    [Action("datetime")]
    public void TestDateTime([Option("date")] DateTime date)
    {
        WasCalled = true;
        CapturedDateTime = date;
    }

    [Action("timespan")]
    public void TestTimeSpan([Option("duration")] TimeSpan duration)
    {
        WasCalled = true;
        CapturedTimeSpan = duration;
    }

    [Action("guid")]
    public void TestGuid([Option("id")] Guid id)
    {
        WasCalled = true;
        CapturedGuid = id;
    }

    [Action("file")]
    public void TestFileInfo([Option("path")] FileInfo path)
    {
        WasCalled = true;
        CapturedFileInfo = path;
    }

    [Action("directory")]
    public void TestDirectoryInfo([Option("path")] DirectoryInfo path)
    {
        WasCalled = true;
        CapturedDirectoryInfo = path;
    }

    [Action("datetimeoffset")]
    public void TestDateTimeOffset([Option("timestamp")] DateTimeOffset timestamp)
    {
        WasCalled = true;
        CapturedDateTimeOffset = timestamp;
    }

    public static void Reset()
    {
        WasCalled = false;
        CapturedUri = null;
        CapturedDateTime = null;
        CapturedTimeSpan = null;
        CapturedGuid = null;
        CapturedFileInfo = null;
        CapturedDirectoryInfo = null;
        CapturedDateTimeOffset = null;
    }
}
