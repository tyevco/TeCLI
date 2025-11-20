using TeCLI.Tests.TestCommands;
using Xunit;
using System.Linq;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for array and collection parameter support
/// </summary>
public class CollectionSupportTests
{
    [Fact]
    public void CollectionOption_WithRepeatedValues_ShouldCollectAllValues()
    {
        // Arrange
        CollectionCommand.Reset();
        var args = new[] { "collection", "process", "--files", "file1.txt", "--files", "file2.txt", "--files", "file3.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CollectionCommand.WasCalled);
        Assert.NotNull(CollectionCommand.CapturedFiles);
        Assert.Equal(3, CollectionCommand.CapturedFiles.Length);
        Assert.Equal("file1.txt", CollectionCommand.CapturedFiles[0]);
        Assert.Equal("file2.txt", CollectionCommand.CapturedFiles[1]);
        Assert.Equal("file3.txt", CollectionCommand.CapturedFiles[2]);
    }

    [Fact]
    public void CollectionOption_WithCommaSeparatedValues_ShouldSplitValues()
    {
        // Arrange
        CollectionCommand.Reset();
        var args = new[] { "collection", "process", "--files", "file1.txt,file2.txt,file3.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CollectionCommand.WasCalled);
        Assert.NotNull(CollectionCommand.CapturedFiles);
        Assert.Equal(3, CollectionCommand.CapturedFiles.Length);
        Assert.Equal("file1.txt", CollectionCommand.CapturedFiles[0]);
        Assert.Equal("file2.txt", CollectionCommand.CapturedFiles[1]);
        Assert.Equal("file3.txt", CollectionCommand.CapturedFiles[2]);
    }

    [Fact]
    public void CollectionOption_WithMixedRepeatedAndCommaSeparated_ShouldCombineAll()
    {
        // Arrange
        CollectionCommand.Reset();
        var args = new[] { "collection", "process", "--files", "file1.txt,file2.txt", "--files", "file3.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CollectionCommand.WasCalled);
        Assert.NotNull(CollectionCommand.CapturedFiles);
        Assert.Equal(3, CollectionCommand.CapturedFiles.Length);
        Assert.Equal("file1.txt", CollectionCommand.CapturedFiles[0]);
        Assert.Equal("file2.txt", CollectionCommand.CapturedFiles[1]);
        Assert.Equal("file3.txt", CollectionCommand.CapturedFiles[2]);
    }

    [Fact]
    public void CollectionOption_WithShortName_ShouldWork()
    {
        // Arrange
        CollectionCommand.Reset();
        var args = new[] { "collection", "process", "-f", "file1.txt", "-f", "file2.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CollectionCommand.WasCalled);
        Assert.NotNull(CollectionCommand.CapturedFiles);
        Assert.Equal(2, CollectionCommand.CapturedFiles.Length);
        Assert.Equal("file1.txt", CollectionCommand.CapturedFiles[0]);
        Assert.Equal("file2.txt", CollectionCommand.CapturedFiles[1]);
    }

    [Fact]
    public void CollectionOption_WithNumericType_ShouldParseCorrectly()
    {
        // Arrange
        CollectionCommand.Reset();
        var args = new[] { "collection", "process", "--files", "dummy.txt", "--ports", "8080", "--ports", "9090", "--ports", "3000" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CollectionCommand.WasCalled);
        Assert.NotNull(CollectionCommand.CapturedPorts);
        Assert.Equal(3, CollectionCommand.CapturedPorts.Count);
        Assert.Equal(8080, CollectionCommand.CapturedPorts[0]);
        Assert.Equal(9090, CollectionCommand.CapturedPorts[1]);
        Assert.Equal(3000, CollectionCommand.CapturedPorts[2]);
    }

    [Fact]
    public void CollectionOption_WithCommaSeparatedNumbers_ShouldParseCorrectly()
    {
        // Arrange
        CollectionCommand.Reset();
        var args = new[] { "collection", "process", "--files", "dummy.txt", "--ports", "8080,9090,3000" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CollectionCommand.WasCalled);
        Assert.NotNull(CollectionCommand.CapturedPorts);
        Assert.Equal(3, CollectionCommand.CapturedPorts.Count);
        Assert.Equal(8080, CollectionCommand.CapturedPorts[0]);
        Assert.Equal(9090, CollectionCommand.CapturedPorts[1]);
        Assert.Equal(3000, CollectionCommand.CapturedPorts[2]);
    }

    [Fact]
    public void CollectionOption_WithIEnumerableType_ShouldWork()
    {
        // Arrange
        CollectionCommand.Reset();
        var args = new[] { "collection", "process", "--files", "dummy.txt", "--tags", "tag1", "--tags", "tag2", "--tags", "tag3" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CollectionCommand.WasCalled);
        Assert.NotNull(CollectionCommand.CapturedTags);
        var tagsList = CollectionCommand.CapturedTags.ToList();
        Assert.Equal(3, tagsList.Count);
        Assert.Equal("tag1", tagsList[0]);
        Assert.Equal("tag2", tagsList[1]);
        Assert.Equal("tag3", tagsList[2]);
    }

    [Fact]
    public void CollectionArgument_WithMultipleValues_ShouldCollectAll()
    {
        // Arrange
        CollectionCommand.Reset();
        var args = new[] { "collection", "build", "source1.cs", "source2.cs", "source3.cs" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CollectionCommand.WasCalled);
        Assert.NotNull(CollectionCommand.CapturedArgs);
        Assert.Equal(3, CollectionCommand.CapturedArgs.Length);
        Assert.Equal("source1.cs", CollectionCommand.CapturedArgs[0]);
        Assert.Equal("source2.cs", CollectionCommand.CapturedArgs[1]);
        Assert.Equal("source3.cs", CollectionCommand.CapturedArgs[2]);
    }

    [Fact]
    public void CollectionArgument_WithCommaSeparated_ShouldSplit()
    {
        // Arrange
        CollectionCommand.Reset();
        var args = new[] { "collection", "build", "source1.cs,source2.cs,source3.cs" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CollectionCommand.WasCalled);
        Assert.NotNull(CollectionCommand.CapturedArgs);
        Assert.Equal(3, CollectionCommand.CapturedArgs.Length);
        Assert.Equal("source1.cs", CollectionCommand.CapturedArgs[0]);
        Assert.Equal("source2.cs", CollectionCommand.CapturedArgs[1]);
        Assert.Equal("source3.cs", CollectionCommand.CapturedArgs[2]);
    }

    [Fact]
    public void MixedArguments_WithSingleAndCollection_ShouldParseCorrectly()
    {
        // Arrange
        CollectionCommand.Reset();
        var args = new[] { "collection", "copy", "source.txt", "dest1.txt", "dest2.txt", "dest3.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CollectionCommand.WasCalled);
        Assert.NotNull(CollectionCommand.CapturedFiles);
        Assert.NotNull(CollectionCommand.CapturedArgs);
        Assert.Equal("source.txt", CollectionCommand.CapturedFiles[0]);
        Assert.Equal(3, CollectionCommand.CapturedArgs.Length);
        Assert.Equal("dest1.txt", CollectionCommand.CapturedArgs[0]);
        Assert.Equal("dest2.txt", CollectionCommand.CapturedArgs[1]);
        Assert.Equal("dest3.txt", CollectionCommand.CapturedArgs[2]);
    }

    [Fact]
    public void CollectionOption_WithTrailingSpaces_ShouldTrimValues()
    {
        // Arrange
        CollectionCommand.Reset();
        var args = new[] { "collection", "process", "--files", " file1.txt , file2.txt , file3.txt " };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CollectionCommand.WasCalled);
        Assert.NotNull(CollectionCommand.CapturedFiles);
        Assert.Equal(3, CollectionCommand.CapturedFiles.Length);
        Assert.Equal("file1.txt", CollectionCommand.CapturedFiles[0]);
        Assert.Equal("file2.txt", CollectionCommand.CapturedFiles[1]);
        Assert.Equal("file3.txt", CollectionCommand.CapturedFiles[2]);
    }

    [Fact]
    public void CollectionOption_MixedWithOtherOptions_ShouldParseCorrectly()
    {
        // Arrange
        CollectionCommand.Reset();
        var args = new[] { "collection", "process", "--files", "file1.txt", "--tags", "tag1", "--files", "file2.txt", "--tags", "tag2" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(CollectionCommand.WasCalled);
        Assert.NotNull(CollectionCommand.CapturedFiles);
        Assert.NotNull(CollectionCommand.CapturedTags);
        Assert.Equal(2, CollectionCommand.CapturedFiles.Length);
        Assert.Equal("file1.txt", CollectionCommand.CapturedFiles[0]);
        Assert.Equal("file2.txt", CollectionCommand.CapturedFiles[1]);
        var tagsList = CollectionCommand.CapturedTags.ToList();
        Assert.Equal(2, tagsList.Count);
        Assert.Equal("tag1", tagsList[0]);
        Assert.Equal("tag2", tagsList[1]);
    }
}
