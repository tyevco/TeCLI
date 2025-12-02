using System;
using System.IO;
using TeCLI.Attributes;

namespace TeCLI.Tests.TestCommands;

/// <summary>
/// Test command for stream/pipeline support
/// </summary>
[Command("stream", Description = "Test command with stream types")]
public class StreamCommand
{
    public static bool WasCalled { get; private set; }
    public static Stream? CapturedStream { get; private set; }
    public static TextReader? CapturedTextReader { get; private set; }
    public static TextWriter? CapturedTextWriter { get; private set; }
    public static StreamReader? CapturedStreamReader { get; private set; }
    public static StreamWriter? CapturedStreamWriter { get; private set; }
    public static string? CapturedContent { get; private set; }

    [Action("input")]
    public void TestInputStream([Option("input", ShortName = 'i')] Stream input)
    {
        WasCalled = true;
        CapturedStream = input;

        // Try to read from the stream
        using var reader = new StreamReader(input);
        CapturedContent = reader.ReadToEnd();
    }

    [Action("output")]
    public void TestOutputStream([Option("output", ShortName = 'o')] StreamWriter output)
    {
        WasCalled = true;
        CapturedStreamWriter = output;
        output.WriteLine("Test output");
    }

    [Action("textreader")]
    public void TestTextReader([Option("input")] TextReader input)
    {
        WasCalled = true;
        CapturedTextReader = input;
        CapturedContent = input.ReadToEnd();
    }

    [Action("textwriter")]
    public void TestTextWriter([Option("output")] TextWriter output)
    {
        WasCalled = true;
        CapturedTextWriter = output;
        output.WriteLine("Test output from TextWriter");
    }

    [Action("streamreader")]
    public void TestStreamReader([Option("input")] StreamReader input)
    {
        WasCalled = true;
        CapturedStreamReader = input;
        CapturedContent = input.ReadToEnd();
    }

    [Action("transform")]
    public void Transform(
        [Option("input", ShortName = 'i')] TextReader input,
        [Option("output", ShortName = 'o')] TextWriter output)
    {
        WasCalled = true;
        CapturedTextReader = input;
        CapturedTextWriter = output;

        // Simple transform: uppercase the input
        var content = input.ReadToEnd();
        CapturedContent = content;
        output.Write(content.ToUpperInvariant());
    }

    [Action("argstream")]
    public void TestArgumentStream(TextReader input)
    {
        WasCalled = true;
        CapturedTextReader = input;
        CapturedContent = input.ReadToEnd();
    }

    [Action("optional")]
    public void TestOptionalStream([Option("input")] TextReader? input = null)
    {
        WasCalled = true;
        CapturedTextReader = input;
        if (input != null)
        {
            CapturedContent = input.ReadToEnd();
        }
    }

    public static void Reset()
    {
        WasCalled = false;
        CapturedStream = null;
        CapturedTextReader = null;
        CapturedTextWriter = null;
        CapturedStreamReader = null;
        CapturedStreamWriter = null;
        CapturedContent = null;
    }
}
