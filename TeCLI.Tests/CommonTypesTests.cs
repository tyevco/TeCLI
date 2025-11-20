using TeCLI.Tests.TestCommands;
using Xunit;
using System;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for common type support (Uri, DateTime, TimeSpan, Guid, FileInfo, DirectoryInfo)
/// </summary>
public class CommonTypesTests
{
    [Fact]
    public void UriOption_WithValidUrl_ShouldParse()
    {
        // Arrange
        CommonTypesCommand.Reset();
        var args = new[] { "types", "uri", "--url", "https://example.com/path?query=value" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CommonTypesCommand.WasCalled);
        Assert.NotNull(CommonTypesCommand.CapturedUri);
        Assert.Equal("https://example.com/path?query=value", CommonTypesCommand.CapturedUri.ToString());
        Assert.Equal("example.com", CommonTypesCommand.CapturedUri.Host);
    }

    [Fact]
    public void DateTimeOption_WithValidDate_ShouldParse()
    {
        // Arrange
        CommonTypesCommand.Reset();
        var args = new[] { "types", "datetime", "--date", "2025-11-20" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CommonTypesCommand.WasCalled);
        Assert.NotNull(CommonTypesCommand.CapturedDateTime);
        Assert.Equal(new DateTime(2025, 11, 20), CommonTypesCommand.CapturedDateTime.Value.Date);
    }

    [Fact]
    public void DateTimeOption_WithFullTimestamp_ShouldParse()
    {
        // Arrange
        CommonTypesCommand.Reset();
        var args = new[] { "types", "datetime", "--date", "2025-11-20T14:30:00" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CommonTypesCommand.WasCalled);
        Assert.NotNull(CommonTypesCommand.CapturedDateTime);
        Assert.Equal(new DateTime(2025, 11, 20, 14, 30, 0), CommonTypesCommand.CapturedDateTime.Value);
    }

    [Fact]
    public void TimeSpanOption_WithDaysFormat_ShouldParse()
    {
        // Arrange
        CommonTypesCommand.Reset();
        var args = new[] { "types", "timespan", "--duration", "2.14:30:00" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CommonTypesCommand.WasCalled);
        Assert.NotNull(CommonTypesCommand.CapturedTimeSpan);
        Assert.Equal(TimeSpan.FromDays(2) + TimeSpan.FromHours(14) + TimeSpan.FromMinutes(30), CommonTypesCommand.CapturedTimeSpan.Value);
    }

    [Fact]
    public void TimeSpanOption_WithSimpleFormat_ShouldParse()
    {
        // Arrange
        CommonTypesCommand.Reset();
        var args = new[] { "types", "timespan", "--duration", "01:30:00" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CommonTypesCommand.WasCalled);
        Assert.NotNull(CommonTypesCommand.CapturedTimeSpan);
        Assert.Equal(TimeSpan.FromHours(1) + TimeSpan.FromMinutes(30), CommonTypesCommand.CapturedTimeSpan.Value);
    }

    [Fact]
    public void GuidOption_WithValidGuid_ShouldParse()
    {
        // Arrange
        CommonTypesCommand.Reset();
        var testGuid = Guid.NewGuid();
        var args = new[] { "types", "guid", "--id", testGuid.ToString() };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CommonTypesCommand.WasCalled);
        Assert.NotNull(CommonTypesCommand.CapturedGuid);
        Assert.Equal(testGuid, CommonTypesCommand.CapturedGuid.Value);
    }

    [Fact]
    public void GuidOption_WithDifferentFormat_ShouldParse()
    {
        // Arrange
        CommonTypesCommand.Reset();
        var args = new[] { "types", "guid", "--id", "a1b2c3d4-e5f6-7890-abcd-ef1234567890" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CommonTypesCommand.WasCalled);
        Assert.NotNull(CommonTypesCommand.CapturedGuid);
        Assert.Equal(new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), CommonTypesCommand.CapturedGuid.Value);
    }

    [Fact]
    public void FileInfoOption_WithValidPath_ShouldParse()
    {
        // Arrange
        CommonTypesCommand.Reset();
        var args = new[] { "types", "file", "--path", "/path/to/file.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CommonTypesCommand.WasCalled);
        Assert.NotNull(CommonTypesCommand.CapturedFileInfo);
        Assert.Equal("/path/to/file.txt", CommonTypesCommand.CapturedFileInfo.FullName);
    }

    [Fact]
    public void DirectoryInfoOption_WithValidPath_ShouldParse()
    {
        // Arrange
        CommonTypesCommand.Reset();
        var args = new[] { "types", "directory", "--path", "/path/to/directory" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CommonTypesCommand.WasCalled);
        Assert.NotNull(CommonTypesCommand.CapturedDirectoryInfo);
        Assert.Equal("/path/to/directory", CommonTypesCommand.CapturedDirectoryInfo.FullName);
    }

    [Fact]
    public void DateTimeOffsetOption_WithValidValue_ShouldParse()
    {
        // Arrange
        CommonTypesCommand.Reset();
        var args = new[] { "types", "datetimeoffset", "--timestamp", "2025-11-20T14:30:00+00:00" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CommonTypesCommand.WasCalled);
        Assert.NotNull(CommonTypesCommand.CapturedDateTimeOffset);
        Assert.Equal(new DateTimeOffset(2025, 11, 20, 14, 30, 0, TimeSpan.Zero), CommonTypesCommand.CapturedDateTimeOffset.Value);
    }

    [Fact]
    public void UriOption_WithInvalidUrl_ShouldThrow()
    {
        // Arrange
        CommonTypesCommand.Reset();
        var args = new[] { "types", "uri", "--url", "not a valid url" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = Assert.ThrowsAsync<ArgumentException>(() => dispatcher.DispatchAsync(args)).Result;

        Assert.Contains("url", exception.Message);
    }

    [Fact]
    public void DateTimeOption_WithInvalidDate_ShouldThrow()
    {
        // Arrange
        CommonTypesCommand.Reset();
        var args = new[] { "types", "datetime", "--date", "not-a-date" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = Assert.ThrowsAsync<ArgumentException>(() => dispatcher.DispatchAsync(args)).Result;

        Assert.Contains("date", exception.Message);
    }

    [Fact]
    public void GuidOption_WithInvalidGuid_ShouldThrow()
    {
        // Arrange
        CommonTypesCommand.Reset();
        var args = new[] { "types", "guid", "--id", "not-a-guid" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = Assert.ThrowsAsync<ArgumentException>(() => dispatcher.DispatchAsync(args)).Result;

        Assert.Contains("id", exception.Message);
    }
}
