using TeCLI.Tests.TestCommands;
using Xunit;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for command and action aliases support
/// </summary>
public class AliasesTests
{
    [Fact]
    public async Task CommandPrimaryName_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "remove", "file", "--path", "/test/file.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("RemoveFile", AliasesCommand.CapturedMethod);
        Assert.Equal("/test/file.txt", AliasesCommand.CapturedPath);
    }

    [Fact]
    public async Task CommandAlias_Rm_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "rm", "file", "--path", "/test/file.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("RemoveFile", AliasesCommand.CapturedMethod);
        Assert.Equal("/test/file.txt", AliasesCommand.CapturedPath);
    }

    [Fact]
    public async Task CommandAlias_Delete_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "delete", "file", "--path", "/test/file.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("RemoveFile", AliasesCommand.CapturedMethod);
        Assert.Equal("/test/file.txt", AliasesCommand.CapturedPath);
    }

    [Fact]
    public async Task ActionPrimaryName_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "remove", "file", "-p", "/test/file.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("RemoveFile", AliasesCommand.CapturedMethod);
    }

    [Fact]
    public async Task ActionAlias_SingleLetter_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "remove", "f", "--path", "/test/file.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("RemoveFile", AliasesCommand.CapturedMethod);
        Assert.Equal("/test/file.txt", AliasesCommand.CapturedPath);
    }

    [Fact]
    public async Task ActionAlias_Dir_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "remove", "dir", "--path", "/test/directory" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("RemoveDirectory", AliasesCommand.CapturedMethod);
        Assert.Equal("/test/directory", AliasesCommand.CapturedPath);
    }

    [Fact]
    public async Task ActionAlias_D_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "remove", "d", "--path", "/test/directory" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("RemoveDirectory", AliasesCommand.CapturedMethod);
    }

    [Fact]
    public async Task ActionAlias_Folder_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "remove", "folder", "--path", "/test/directory" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("RemoveDirectory", AliasesCommand.CapturedMethod);
    }

    [Fact]
    public async Task CommandAliasAndActionAlias_Combined_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "rm", "f", "-p", "/test/file.txt", "--force" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("RemoveFile", AliasesCommand.CapturedMethod);
        Assert.Equal("/test/file.txt", AliasesCommand.CapturedPath);
        Assert.True(AliasesCommand.CapturedForce);
    }

    [Fact]
    public async Task CommandAlias_DeleteAndActionAlias_Dir_Combined_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "delete", "dir", "--path", "/test/directory", "-r" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("RemoveDirectory", AliasesCommand.CapturedMethod);
        Assert.Equal("/test/directory", AliasesCommand.CapturedPath);
        Assert.True(AliasesCommand.CapturedForce);
    }

    [Fact]
    public async Task ActionAlias_Ls_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "remove", "ls" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("List", AliasesCommand.CapturedMethod);
    }

    [Fact]
    public async Task ActionAlias_Show_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "remove", "show" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("List", AliasesCommand.CapturedMethod);
    }

    [Fact]
    public async Task MultipleAliases_AllWorkIndependently()
    {
        // Test each command alias
        var commandAliases = new[] { "remove", "rm", "delete" };
        var actionAliases = new[] { "directory", "dir", "d", "folder" };

        foreach (var cmdAlias in commandAliases)
        {
            foreach (var actAlias in actionAliases)
            {
                // Arrange
                AliasesCommand.Reset();
                var args = new[] { cmdAlias, actAlias, "--path", "/test" };

                // Act
                var dispatcher = new TeCLI.CommandDispatcher();
                await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

                // Assert
                Assert.True(AliasesCommand.WasCalled, $"Failed for command '{cmdAlias}' and action '{actAlias}'");
                Assert.Equal("RemoveDirectory", AliasesCommand.CapturedMethod);
            }
        }
    }

    [Fact]
    public async Task CaseInsensitive_CommandAliases_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "RM", "file", "--path", "/test/file.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("RemoveFile", AliasesCommand.CapturedMethod);
    }

    [Fact]
    public async Task CaseInsensitive_ActionAliases_ShouldWork()
    {
        // Arrange
        AliasesCommand.Reset();
        var args = new[] { "remove", "LS" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(AliasesCommand.WasCalled);
        Assert.Equal("List", AliasesCommand.CapturedMethod);
    }
}
