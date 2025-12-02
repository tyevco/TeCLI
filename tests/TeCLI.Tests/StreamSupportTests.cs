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
}
