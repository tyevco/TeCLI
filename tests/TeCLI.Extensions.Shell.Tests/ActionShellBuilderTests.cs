using TeCLI.Shell;
using Xunit;

namespace TeCLI.Extensions.Shell.Tests;

public class ActionShellBuilderTests
{
    [Fact]
    public void Build_ReturnsShellHost()
    {
        var builder = ShellExtensions.CreateActionShell();

        var host = builder.Build();

        Assert.NotNull(host);
    }

    [Fact]
    public void WithAction_RegistersAction()
    {
        var executed = false;
        var builder = ShellExtensions.CreateActionShell()
            .WithAction("test", "Test action", _ =>
            {
                executed = true;
                return 0;
            });

        var host = builder.Build();
        host.ExecuteCommandAsync("test").Wait();

        Assert.True(executed);
    }

    [Fact]
    public async Task WithAction_PassesArguments()
    {
        var receivedArgs = Array.Empty<string>();
        var builder = ShellExtensions.CreateActionShell()
            .WithAction("test", "Test action", args =>
            {
                receivedArgs = args;
                return 0;
            });

        var host = builder.Build();
        await host.ExecuteCommandAsync("test arg1 arg2");

        Assert.Equal(new[] { "arg1", "arg2" }, receivedArgs);
    }

    [Fact]
    public async Task WithActionAsync_RegistersAsyncAction()
    {
        var executed = false;
        var builder = ShellExtensions.CreateActionShell()
            .WithActionAsync("test", "Test action", async args =>
            {
                await Task.Delay(1);
                executed = true;
                return 0;
            });

        var host = builder.Build();
        await host.ExecuteCommandAsync("test");

        Assert.True(executed);
    }

    [Fact]
    public async Task WithDefaultAction_HandlesUnknownCommands()
    {
        var receivedArgs = Array.Empty<string>();
        var builder = ShellExtensions.CreateActionShell()
            .WithDefaultAction(args =>
            {
                receivedArgs = args;
                return 99;
            });

        var host = builder.Build();
        var result = await host.ExecuteCommandAsync("unknown arg1");

        Assert.Equal(new[] { "unknown", "arg1" }, receivedArgs);
        Assert.Equal(99, result);
    }

    [Fact]
    public async Task Build_WithOptions_UsesOptions()
    {
        var options = new ShellOptions { Prompt = "custom> " };
        var builder = ShellExtensions.CreateActionShell(options);

        var host = builder.Build();

        Assert.Equal("custom> ", host.Options.Prompt);
    }

    [Fact]
    public async Task MultipleActions_CanBeRegistered()
    {
        var action1Called = false;
        var action2Called = false;

        var builder = ShellExtensions.CreateActionShell()
            .WithAction("action1", "First action", _ =>
            {
                action1Called = true;
                return 0;
            })
            .WithAction("action2", "Second action", _ =>
            {
                action2Called = true;
                return 0;
            });

        var host = builder.Build();
        await host.ExecuteCommandAsync("action1");
        await host.ExecuteCommandAsync("action2");

        Assert.True(action1Called);
        Assert.True(action2Called);
    }

    [Fact]
    public async Task Actions_BuiltInCommand_TakesPrecedence()
    {
        // Built-in commands like 'exit' should still work
        var builder = ShellExtensions.CreateActionShell()
            .WithAction("test", "Test action", _ => 0);

        var host = builder.Build();
        await host.ExecuteCommandAsync("exit");

        Assert.False(host.Session.IsActive);
    }

    [Fact]
    public async Task ActionsCommand_ListsRegisteredActions()
    {
        var builder = ShellExtensions.CreateActionShell()
            .WithAction("query", "Execute query", _ => 0)
            .WithAction("insert", "Insert data", _ => 0);

        var host = builder.Build();
        var result = await host.ExecuteCommandAsync("actions");

        Assert.Equal(0, result);
        // The actions should be printed (we can't easily verify console output in this test)
    }
}

public class ShellExtensionsTests
{
    [Fact]
    public void CreateShell_WithOptions_ReturnsConfiguredHost()
    {
        var options = new ShellOptions { Prompt = "test> " };
        var executed = false;

        var host = ShellExtensions.CreateShell(options, (h, args) =>
        {
            executed = true;
            return Task.FromResult(0);
        });

        host.ExecuteCommandAsync("test").Wait();

        Assert.True(executed);
        Assert.Equal("test> ", host.Options.Prompt);
    }

    [Fact]
    public void CreateShell_Sync_Works()
    {
        var options = new ShellOptions();
        var executed = false;

        var host = ShellExtensions.CreateShell(options, (h, args) =>
        {
            executed = true;
            return 0;
        });

        host.ExecuteCommandAsync("test").Wait();

        Assert.True(executed);
    }

    [Fact]
    public void CreateShell_FromAttribute_Works()
    {
        var attr = new ShellAttribute
        {
            Prompt = "attr> ",
            WelcomeMessage = "Hello"
        };
        var executed = false;

        var host = ShellExtensions.CreateShell(attr, (h, args) =>
        {
            executed = true;
            return Task.FromResult(0);
        });

        host.ExecuteCommandAsync("test").Wait();

        Assert.True(executed);
        Assert.Equal("attr> ", host.Options.Prompt);
        Assert.Equal("Hello", host.Options.WelcomeMessage);
    }
}
