using TeCLI.Shell;
using Xunit;

namespace TeCLI.Extensions.Shell.Tests;

public class ShellHostTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var host = new ShellHost();

        Assert.NotNull(host.Session);
        Assert.NotNull(host.Options);
        Assert.Equal("> ", host.Options.Prompt);
    }

    [Fact]
    public void Constructor_WithOptions_UsesOptions()
    {
        var options = new ShellOptions { Prompt = "test> " };

        var host = new ShellHost(options);

        Assert.Equal("test> ", host.Options.Prompt);
    }

    [Fact]
    public void RegisterCommand_AddsBuiltInCommand()
    {
        var host = new ShellHost();
        var executed = false;

        host.RegisterCommand("test", "Test command", (_, _) =>
        {
            executed = true;
            return 0;
        });

        // Execute the command
        host.ExecuteCommandAsync("test").Wait();

        Assert.True(executed);
    }

    [Fact]
    public async Task ExecuteCommandAsync_ExitCommand_SetsInactive()
    {
        var host = new ShellHost();

        await host.ExecuteCommandAsync("exit");

        Assert.False(host.Session.IsActive);
    }

    [Fact]
    public async Task ExecuteCommandAsync_QuitCommand_SetsInactive()
    {
        var host = new ShellHost();

        await host.ExecuteCommandAsync("quit");

        Assert.False(host.Session.IsActive);
    }

    [Fact]
    public async Task ExecuteCommandAsync_UnknownCommand_Returns1()
    {
        var host = new ShellHost();

        var result = await host.ExecuteCommandAsync("unknowncommand");

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_CustomHandler_IsInvoked()
    {
        var host = new ShellHost();
        var receivedArgs = Array.Empty<string>();
        host.CommandHandler += (h, args) =>
        {
            receivedArgs = args;
            return Task.FromResult(42);
        };

        var result = await host.ExecuteCommandAsync("custom arg1 arg2");

        Assert.Equal(42, result);
        Assert.Equal(new[] { "custom", "arg1", "arg2" }, receivedArgs);
    }

    [Fact]
    public async Task ExecuteCommandAsync_EmptyLine_Returns0()
    {
        var host = new ShellHost();

        var result = await host.ExecuteCommandAsync("");

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_QuotedArguments_ParsedCorrectly()
    {
        var host = new ShellHost();
        var receivedArgs = Array.Empty<string>();
        host.CommandHandler += (h, args) =>
        {
            receivedArgs = args;
            return Task.FromResult(0);
        };

        await host.ExecuteCommandAsync("cmd \"arg with spaces\" 'single quoted'");

        Assert.Equal(new[] { "cmd", "arg with spaces", "single quoted" }, receivedArgs);
    }

    [Fact]
    public async Task ExecuteCommandAsync_EscapedCharacters_ParsedCorrectly()
    {
        var host = new ShellHost();
        var receivedArgs = Array.Empty<string>();
        host.CommandHandler += (h, args) =>
        {
            receivedArgs = args;
            return Task.FromResult(0);
        };

        await host.ExecuteCommandAsync("cmd arg\\ with\\ space");

        Assert.Equal(new[] { "cmd", "arg with space" }, receivedArgs);
    }

    [Fact]
    public async Task ExecuteCommandAsync_RecordsCommandCount()
    {
        var host = new ShellHost();
        host.CommandHandler += (_, _) => Task.FromResult(0);

        await host.ExecuteCommandAsync("cmd1");
        await host.ExecuteCommandAsync("cmd2");

        Assert.Equal(2, host.Session.CommandCount);
    }

    [Fact]
    public async Task ExecuteCommandAsync_RecordsLastExitCode()
    {
        var host = new ShellHost();
        host.CommandHandler += (_, _) => Task.FromResult(123);

        await host.ExecuteCommandAsync("cmd");

        Assert.Equal(123, host.Session.LastExitCode);
    }

    [Fact]
    public void RegisterCommand_Sync_Works()
    {
        var host = new ShellHost();
        var executed = false;

        host.RegisterCommand("sync", "Sync command", (_, _) =>
        {
            executed = true;
            return 0;
        });

        host.ExecuteCommandAsync("sync").Wait();

        Assert.True(executed);
    }

    [Fact]
    public async Task ExecuteCommandAsync_BuiltInHelp_Returns0()
    {
        var host = new ShellHost();

        var result = await host.ExecuteCommandAsync("help");

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_BuiltInHistory_Returns0()
    {
        var host = new ShellHost();
        host.Session.History.Add("previous command");

        var result = await host.ExecuteCommandAsync("history");

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_Clear_Returns0()
    {
        var host = new ShellHost();

        var result = await host.ExecuteCommandAsync("clear");

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_Cls_Returns0()
    {
        var host = new ShellHost();

        var result = await host.ExecuteCommandAsync("cls");

        Assert.Equal(0, result);
    }
}
