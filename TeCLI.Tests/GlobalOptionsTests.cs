using TeCLI.Tests.TestCommands;
using Xunit;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for global options support
/// </summary>
public class GlobalOptionsTests
{
    #region Basic Global Options Tests

    [Fact]
    public void GlobalOptions_VerboseFlag_ShouldBeAvailableInAction()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "--verbose", "globaltest", "process", "test.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.Equal("process", GlobalOptionsCommand.LastAction);
        Assert.True(GlobalOptionsCommand.CapturedVerbose);
        Assert.Equal("test.txt", GlobalOptionsCommand.CapturedFileName);
    }

    [Fact]
    public void GlobalOptions_ShortVerboseFlag_ShouldWork()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "-v", "globaltest", "process", "data.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.True(GlobalOptionsCommand.CapturedVerbose);
    }

    [Fact]
    public void GlobalOptions_ConfigFile_ShouldBeParsed()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "--config", "/path/to/config.json", "globaltest", "process", "file.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.Equal("/path/to/config.json", GlobalOptionsCommand.CapturedConfig);
    }

    [Fact]
    public void GlobalOptions_LogLevel_ShouldHaveDefaultValue()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "globaltest", "simple" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.Equal("info", GlobalOptionsCommand.CapturedLogLevel); // default value
    }

    [Fact]
    public void GlobalOptions_LogLevel_CanBeOverridden()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "--log-level", "debug", "globaltest", "simple" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.Equal("debug", GlobalOptionsCommand.CapturedLogLevel);
    }

    [Fact]
    public void GlobalOptions_Timeout_ShouldParseInt()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "--timeout", "60", "globaltest", "simple" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.Equal(60, GlobalOptionsCommand.CapturedTimeout);
    }

    #endregion

    #region Multiple Global Options Tests

    [Fact]
    public void GlobalOptions_MultipleOptions_ShouldAllBeParsed()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "--verbose", "--config", "app.conf", "--timeout", "45", "globaltest", "process", "input.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.True(GlobalOptionsCommand.CapturedVerbose);
        Assert.Equal("app.conf", GlobalOptionsCommand.CapturedConfig);
        Assert.Equal(45, GlobalOptionsCommand.CapturedTimeout);
    }

    [Fact]
    public void GlobalOptions_MixedLongAndShortNames_ShouldWork()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "-v", "-d", "--config", "cfg.json", "globaltest", "simple" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.True(GlobalOptionsCommand.CapturedVerbose);
        Assert.True(GlobalOptionsCommand.CapturedDebug);
        Assert.Equal("cfg.json", GlobalOptionsCommand.CapturedConfig);
    }

    #endregion

    #region Global Options Positioning Tests

    [Fact]
    public void GlobalOptions_BeforeCommand_ShouldWork()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "--verbose", "globaltest", "process", "file.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.True(GlobalOptionsCommand.CapturedVerbose);
    }

    [Fact]
    public void GlobalOptions_InterspersedWithCommand_ShouldWork()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "--verbose", "globaltest", "--config", "app.json", "process", "file.txt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.True(GlobalOptionsCommand.CapturedVerbose);
        Assert.Equal("app.json", GlobalOptionsCommand.CapturedConfig);
    }

    #endregion

    #region Mixed Global and Local Options Tests

    [Fact]
    public void GlobalOptions_WithLocalOptions_ShouldBothWork()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "--verbose", "globaltest", "deploy", "--count", "5", "production" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.Equal("deploy", GlobalOptionsCommand.LastAction);
        Assert.True(GlobalOptionsCommand.CapturedVerbose);
        Assert.Equal("production", GlobalOptionsCommand.CapturedFileName);
        Assert.Equal(5, GlobalOptionsCommand.CapturedCount);
    }

    [Fact]
    public void GlobalOptions_Mixed_WithRegularOptions()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "-v", "globaltest", "mixed", "--name", "John", "--age", "30" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.Equal("mixed", GlobalOptionsCommand.LastAction);
        Assert.True(GlobalOptionsCommand.CapturedVerbose);
        Assert.Equal("John", GlobalOptionsCommand.CapturedConfig);
        Assert.Equal(30, GlobalOptionsCommand.CapturedTimeout);
    }

    #endregion

    #region Actions Without Global Options Tests

    [Fact]
    public void GlobalOptions_ActionWithoutGlobalOptionsParam_ShouldStillWork()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "--verbose", "globaltest", "noopt" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.Equal("noopt", GlobalOptionsCommand.LastAction);
        // Global options were parsed but not passed to this action
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public void GlobalOptions_NotProvided_ShouldUseDefaults()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "globaltest", "simple" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(GlobalOptionsCommand.WasCalled);
        Assert.False(GlobalOptionsCommand.CapturedVerbose.GetValueOrDefault());
        Assert.False(GlobalOptionsCommand.CapturedDebug.GetValueOrDefault());
        Assert.Null(GlobalOptionsCommand.CapturedConfig);
        Assert.Equal("info", GlobalOptionsCommand.CapturedLogLevel);
        Assert.Equal(30, GlobalOptionsCommand.CapturedTimeout);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GlobalOptions_OnlyGlobalOptionsProvided_ShouldNotAffectEmptyArgs()
    {
        // Global options are parsed and removed, leaving empty args
        // This should trigger help display behavior
        GlobalOptionsCommand.Reset();
        var args = new[] { "--verbose" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert - should display help, not call command
        Assert.False(GlobalOptionsCommand.WasCalled);
    }

    [Fact]
    public void GlobalOptions_GlobalAndHelpFlag_ShouldShowHelp()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "--verbose", "--help" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert - help should be shown, command not called
        Assert.False(GlobalOptionsCommand.WasCalled);
    }

    [Fact]
    public void GlobalOptions_GlobalAndVersionFlag_ShouldShowVersion()
    {
        // Arrange
        GlobalOptionsCommand.Reset();
        var args = new[] { "--verbose", "--version" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert - version should be shown, command not called
        Assert.False(GlobalOptionsCommand.WasCalled);
    }

    #endregion

    #region Multiple Actions Tests

    [Fact]
    public void GlobalOptions_DifferentActions_ShouldAllReceiveGlobalOptions()
    {
        // Test that multiple actions in the same command can all receive global options

        // Test simple action
        GlobalOptionsCommand.Reset();
        var args1 = new[] { "--verbose", "globaltest", "simple" };
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args1).Wait();
        Assert.True(GlobalOptionsCommand.CapturedVerbose.GetValueOrDefault());

        // Test process action
        GlobalOptionsCommand.Reset();
        var args2 = new[] { "--debug", "globaltest", "process", "file.txt" };
        dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args2).Wait();
        Assert.True(GlobalOptionsCommand.CapturedDebug.GetValueOrDefault());

        // Test deploy action
        GlobalOptionsCommand.Reset();
        var args3 = new[] { "--config", "cfg.json", "globaltest", "deploy", "prod" };
        dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args3).Wait();
        Assert.Equal("cfg.json", GlobalOptionsCommand.CapturedConfig);
    }

    #endregion
}
