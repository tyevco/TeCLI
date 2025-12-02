using TeCLI.Tests.TestCommands;
using Xunit;
using System;
using System.IO;
using System.Text;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for stream/pipeline support (Stream, TextReader, TextWriter, StreamReader, StreamWriter)
/// Tests: cat input.txt | myapp transform | tee output.txt
/// </summary>
public class StreamSupportTests
{
    [Fact]
    public async Task StreamOption_WithFilePath_ShouldOpenFileStream()
    {
        // Arrange
        StreamCommand.Reset();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "Hello from file");

        try
        {
            var args = new[] { "stream", "input", "--input", tempFile };

            // Act
            var dispatcher = new TeCLI.CommandDispatcher();
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(StreamCommand.WasCalled);
            Assert.NotNull(StreamCommand.CapturedContent);
            Assert.Equal("Hello from file", StreamCommand.CapturedContent);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task TextReaderOption_WithFilePath_ShouldOpenStreamReader()
    {
        // Arrange
        StreamCommand.Reset();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "Hello from TextReader");

        try
        {
            var args = new[] { "stream", "textreader", "--input", tempFile };

            // Act
            var dispatcher = new TeCLI.CommandDispatcher();
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(StreamCommand.WasCalled);
            Assert.NotNull(StreamCommand.CapturedTextReader);
            Assert.Equal("Hello from TextReader", StreamCommand.CapturedContent);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task TextWriterOption_WithFilePath_ShouldOpenStreamWriter()
    {
        // Arrange
        StreamCommand.Reset();
        var tempFile = Path.GetTempFileName();

        try
        {
            var args = new[] { "stream", "textwriter", "--output", tempFile };

            // Act
            var dispatcher = new TeCLI.CommandDispatcher();
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(StreamCommand.WasCalled);
            Assert.NotNull(StreamCommand.CapturedTextWriter);

            // Flush and close the writer
            StreamCommand.CapturedTextWriter.Dispose();

            // Verify the file was written
            var content = File.ReadAllText(tempFile);
            Assert.Contains("Test output from TextWriter", content);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StreamReaderOption_WithFilePath_ShouldOpenStreamReader()
    {
        // Arrange
        StreamCommand.Reset();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "StreamReader content");

        try
        {
            var args = new[] { "stream", "streamreader", "--input", tempFile };

            // Act
            var dispatcher = new TeCLI.CommandDispatcher();
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(StreamCommand.WasCalled);
            Assert.NotNull(StreamCommand.CapturedStreamReader);
            Assert.Equal("StreamReader content", StreamCommand.CapturedContent);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StreamWriterOption_WithFilePath_ShouldOpenStreamWriter()
    {
        // Arrange
        StreamCommand.Reset();
        var tempFile = Path.GetTempFileName();

        try
        {
            var args = new[] { "stream", "output", "--output", tempFile };

            // Act
            var dispatcher = new TeCLI.CommandDispatcher();
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(StreamCommand.WasCalled);
            Assert.NotNull(StreamCommand.CapturedStreamWriter);

            // Flush and close the writer
            StreamCommand.CapturedStreamWriter.Dispose();

            // Verify the file was written
            var content = File.ReadAllText(tempFile);
            Assert.Contains("Test output", content);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StreamOption_WithShortFlag_ShouldParseCorrectly()
    {
        // Arrange
        StreamCommand.Reset();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "Short flag test");

        try
        {
            var args = new[] { "stream", "input", "-i", tempFile };

            // Act
            var dispatcher = new TeCLI.CommandDispatcher();
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(StreamCommand.WasCalled);
            Assert.Equal("Short flag test", StreamCommand.CapturedContent);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task TransformAction_WithInputAndOutput_ShouldProcessBothStreams()
    {
        // Arrange
        StreamCommand.Reset();
        var inputFile = Path.GetTempFileName();
        var outputFile = Path.GetTempFileName();
        File.WriteAllText(inputFile, "hello world");

        try
        {
            var args = new[] { "stream", "transform", "-i", inputFile, "-o", outputFile };

            // Act
            var dispatcher = new TeCLI.CommandDispatcher();
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(StreamCommand.WasCalled);
            Assert.NotNull(StreamCommand.CapturedTextReader);
            Assert.NotNull(StreamCommand.CapturedTextWriter);
            Assert.Equal("hello world", StreamCommand.CapturedContent);

            // Close the writer to flush
            StreamCommand.CapturedTextWriter.Dispose();

            // Verify the output file has uppercase content
            var outputContent = File.ReadAllText(outputFile);
            Assert.Equal("HELLO WORLD", outputContent);
        }
        finally
        {
            File.Delete(inputFile);
            File.Delete(outputFile);
        }
    }

    [Fact]
    public async Task StreamOption_WithNonExistentFile_ShouldThrow()
    {
        // Arrange
        StreamCommand.Reset();
        var args = new[] { "stream", "input", "--input", "/nonexistent/file/path.txt" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        await Assert.ThrowsAnyAsync<Exception>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task StreamOption_WithMissingRequiredOption_ShouldThrow()
    {
        // Arrange
        StreamCommand.Reset();
        var args = new[] { "stream", "textreader" };

        // Act & Assert
        // When no input is provided and stdin is not redirected, it should throw
        var dispatcher = new TeCLI.CommandDispatcher();
        await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task OptionalStreamOption_WithNoInput_ShouldNotThrow()
    {
        // Arrange
        StreamCommand.Reset();
        var args = new[] { "stream", "optional" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(StreamCommand.WasCalled);
        Assert.Null(StreamCommand.CapturedTextReader);
        Assert.Null(StreamCommand.CapturedContent);
    }

    [Fact]
    public async Task OptionalStreamOption_WithInput_ShouldReadContent()
    {
        // Arrange
        StreamCommand.Reset();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "Optional content");

        try
        {
            var args = new[] { "stream", "optional", "--input", tempFile };

            // Act
            var dispatcher = new TeCLI.CommandDispatcher();
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(StreamCommand.WasCalled);
            Assert.NotNull(StreamCommand.CapturedTextReader);
            Assert.Equal("Optional content", StreamCommand.CapturedContent);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StreamOption_WithMultilineContent_ShouldReadAllLines()
    {
        // Arrange
        StreamCommand.Reset();
        var tempFile = Path.GetTempFileName();
        var multilineContent = "Line 1\nLine 2\nLine 3";
        File.WriteAllText(tempFile, multilineContent);

        try
        {
            var args = new[] { "stream", "textreader", "--input", tempFile };

            // Act
            var dispatcher = new TeCLI.CommandDispatcher();
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(StreamCommand.WasCalled);
            Assert.Equal(multilineContent, StreamCommand.CapturedContent);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StreamOption_WithEmptyFile_ShouldReadEmptyString()
    {
        // Arrange
        StreamCommand.Reset();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "");

        try
        {
            var args = new[] { "stream", "textreader", "--input", tempFile };

            // Act
            var dispatcher = new TeCLI.CommandDispatcher();
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(StreamCommand.WasCalled);
            Assert.NotNull(StreamCommand.CapturedContent);
            Assert.Equal("", StreamCommand.CapturedContent);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StreamOption_WithUnicodeContent_ShouldHandleCorrectly()
    {
        // Arrange
        StreamCommand.Reset();
        var tempFile = Path.GetTempFileName();
        var unicodeContent = "Hello 世界 🌍 émoji";
        File.WriteAllText(tempFile, unicodeContent, Encoding.UTF8);

        try
        {
            var args = new[] { "stream", "textreader", "--input", tempFile };

            // Act
            var dispatcher = new TeCLI.CommandDispatcher();
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(StreamCommand.WasCalled);
            Assert.Equal(unicodeContent, StreamCommand.CapturedContent);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StreamOption_WithLargeFile_ShouldReadAllContent()
    {
        // Arrange
        StreamCommand.Reset();
        var tempFile = Path.GetTempFileName();
        var largeContent = new string('x', 100000); // 100KB of content
        File.WriteAllText(tempFile, largeContent);

        try
        {
            var args = new[] { "stream", "textreader", "--input", tempFile };

            // Act
            var dispatcher = new TeCLI.CommandDispatcher();
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(StreamCommand.WasCalled);
            Assert.NotNull(StreamCommand.CapturedContent);
            Assert.Equal(100000, StreamCommand.CapturedContent.Length);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StreamArgument_WithFilePath_ShouldOpenStream()
    {
        // Arrange
        StreamCommand.Reset();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "Argument stream content");

        try
        {
            var args = new[] { "stream", "argstream", tempFile };

            // Act
            var dispatcher = new TeCLI.CommandDispatcher();
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(StreamCommand.WasCalled);
            Assert.NotNull(StreamCommand.CapturedTextReader);
            Assert.Equal("Argument stream content", StreamCommand.CapturedContent);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
