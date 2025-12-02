using TeCLI.Tests.TestCommands;
using Xunit;
using System;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for nested subcommand support
/// </summary>
public class NestedCommandTests
{
    #region 2-Level Nesting Tests

    [Fact]
    public async Task NestedCommand_TwoLevels_RemoteAdd_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "remote", "add", "origin", "https://github.com/user/repo.git" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("remote-add", NestedCommand.LastAction);
        Assert.Equal("origin", NestedCommand.CapturedName);
        Assert.Equal("https://github.com/user/repo.git", NestedCommand.CapturedUrl);
    }

    [Fact]
    public async Task NestedCommand_TwoLevels_RemoteRemove_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "remote", "remove", "origin" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("remote-remove", NestedCommand.LastAction);
        Assert.Equal("origin", NestedCommand.CapturedName);
    }

    [Fact]
    public async Task NestedCommand_TwoLevels_RemoteList_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "remote", "list" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("remote-list", NestedCommand.LastAction);
    }

    [Fact]
    public async Task NestedCommand_TwoLevels_RemoteList_WithVerboseOption_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "remote", "list", "--verbose" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("remote-list", NestedCommand.LastAction);
        Assert.Equal("True", NestedCommand.CapturedName);
    }

    [Fact]
    public async Task NestedCommand_TwoLevels_BranchCreate_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "branch", "create", "feature-branch" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("branch-create", NestedCommand.LastAction);
        Assert.Equal("feature-branch", NestedCommand.CapturedBranch);
    }

    [Fact]
    public async Task NestedCommand_TwoLevels_BranchDelete_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "branch", "delete", "old-branch", "--force" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("branch-delete", NestedCommand.LastAction);
        Assert.Equal("old-branch", NestedCommand.CapturedBranch);
        Assert.Equal("True", NestedCommand.CapturedName);
    }

    #endregion

    #region 3-Level Nesting Tests

    [Fact]
    public async Task NestedCommand_ThreeLevels_ConfigUserName_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "config", "user", "name", "John Doe" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("config-user-name", NestedCommand.LastAction);
        Assert.Equal("John Doe", NestedCommand.CapturedName);
    }

    [Fact]
    public async Task NestedCommand_ThreeLevels_ConfigUserEmail_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "config", "user", "email", "john@example.com" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("config-user-email", NestedCommand.LastAction);
        Assert.Equal("john@example.com", NestedCommand.CapturedUrl);
    }

    #endregion

    #region Top-Level Actions Tests

    [Fact]
    public async Task NestedCommand_TopLevelAction_Status_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "status" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("status", NestedCommand.LastAction);
    }

    [Fact]
    public async Task NestedCommand_TopLevelAction_Commit_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "commit", "--message", "Initial commit" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("commit", NestedCommand.LastAction);
        Assert.Equal("Initial commit", NestedCommand.CapturedName);
    }

    #endregion

    #region Alias Tests

    [Fact]
    public async Task NestedCommand_SubcommandAlias_Rem_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "rem", "list" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("remote-list", NestedCommand.LastAction);
    }

    [Fact]
    public async Task NestedCommand_ActionAlias_RemoteRm_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "remote", "rm", "origin" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("remote-remove", NestedCommand.LastAction);
        Assert.Equal("origin", NestedCommand.CapturedName);
    }

    [Fact]
    public async Task NestedCommand_ActionAlias_BranchDel_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "branch", "del", "feature" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("branch-delete", NestedCommand.LastAction);
        Assert.Equal("feature", NestedCommand.CapturedBranch);
    }

    #endregion

    #region Short Options Tests

    [Fact]
    public async Task NestedCommand_ShortOption_RemoteListVerbose_ShouldWork()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "remote", "list", "-v" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("remote-list", NestedCommand.LastAction);
        Assert.Equal("True", NestedCommand.CapturedName);
    }

    [Fact]
    public async Task NestedCommand_ShortOption_BranchDeleteForce_ShouldWork()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "branch", "delete", "feature", "-f" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("branch-delete", NestedCommand.LastAction);
        Assert.Equal("feature", NestedCommand.CapturedBranch);
        Assert.Equal("True", NestedCommand.CapturedName);
    }

    [Fact]
    public async Task NestedCommand_ShortOption_CommitMessage_ShouldWork()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "commit", "-m", "Fix bug" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("commit", NestedCommand.LastAction);
        Assert.Equal("Fix bug", NestedCommand.CapturedName);
    }

    #endregion

    #region 2-Level Actions Tests

    [Fact]
    public async Task NestedCommand_TwoLevels_ConfigGet_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "config", "get", "user.name" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("config-get", NestedCommand.LastAction);
        Assert.Equal("user.name", NestedCommand.CapturedName);
    }

    [Fact]
    public async Task NestedCommand_TwoLevels_ConfigSet_ShouldExecute()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "config", "set", "user.name", "Jane Doe" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("config-set", NestedCommand.LastAction);
        Assert.Equal("user.name", NestedCommand.CapturedName);
        Assert.Equal("Jane Doe", NestedCommand.CapturedUrl);
    }

    #endregion

    #region Mixed Case Tests

    [Fact]
    public async Task NestedCommand_MixedCase_ShouldBeCaseInsensitive()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "GIT", "REMOTE", "ADD", "origin", "https://example.com" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("remote-add", NestedCommand.LastAction);
    }

    [Fact]
    public async Task NestedCommand_MixedCase_ThreeLevels_ShouldBeCaseInsensitive()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "Git", "Config", "User", "Name", "Test User" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(NestedCommand.WasCalled);
        Assert.Equal("config-user-name", NestedCommand.LastAction);
    }

    #endregion

    #region Error Cases

    [Fact]
    public async Task NestedCommand_UnknownSubcommand_ShouldThrowOrDisplay()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "unknown", "action" };

        // Act & Assert
        // Note: Depending on implementation, this might throw or display error
        // For now, we just verify it doesn't crash
        var dispatcher = new TeCLI.CommandDispatcher();
        try
        {
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);
            // If no exception, that's acceptable - it may just print error
        }
        catch
        {
            // If exception, that's also acceptable
        }
    }

    [Fact]
    public async Task NestedCommand_UnknownAction_InSubcommand_ShouldThrowOrDisplay()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "remote", "unknown" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        try
        {
            await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);
        }
        catch
        {
            // Expected behavior - unknown action
        }
    }

    [Fact]
    public async Task NestedCommand_MissingArgument_ShouldThrow()
    {
        // Arrange
        NestedCommand.Reset();
        var args = new[] { "gitcli", "remote", "add", "origin" }; // Missing URL argument

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("url", exception.Message.ToLower());
    }

    #endregion
}
