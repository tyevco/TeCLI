using TeCLI.Shell;
using Xunit;

namespace TeCLI.Extensions.Shell.Tests;

public class ShellSessionTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var session = new ShellSession();

        Assert.True(session.IsActive);
        Assert.Equal(0, session.CommandCount);
        Assert.NotNull(session.History);
        Assert.Null(session.LastResult);
        Assert.Equal(0, session.LastExitCode);
    }

    [Fact]
    public void Set_And_Get_ReturnsValue()
    {
        var session = new ShellSession();

        session.Set("key", "value");
        var result = session.Get<string>("key");

        Assert.Equal("value", result);
    }

    [Fact]
    public void Get_NonExistent_ReturnsDefault()
    {
        var session = new ShellSession();

        var result = session.Get<string>("nonexistent", "default");

        Assert.Equal("default", result);
    }

    [Fact]
    public void Get_WrongType_ReturnsDefault()
    {
        var session = new ShellSession();
        session.Set("key", "string value");

        var result = session.Get<int>("key", 42);

        Assert.Equal(42, result);
    }

    [Fact]
    public void Has_ExistingKey_ReturnsTrue()
    {
        var session = new ShellSession();
        session.Set("key", "value");

        Assert.True(session.Has("key"));
    }

    [Fact]
    public void Has_NonExistentKey_ReturnsFalse()
    {
        var session = new ShellSession();

        Assert.False(session.Has("nonexistent"));
    }

    [Fact]
    public void Has_IsCaseInsensitive()
    {
        var session = new ShellSession();
        session.Set("Key", "value");

        Assert.True(session.Has("key"));
        Assert.True(session.Has("KEY"));
    }

    [Fact]
    public void Remove_ExistingKey_ReturnsTrue()
    {
        var session = new ShellSession();
        session.Set("key", "value");

        var result = session.Remove("key");

        Assert.True(result);
        Assert.False(session.Has("key"));
    }

    [Fact]
    public void Remove_NonExistentKey_ReturnsFalse()
    {
        var session = new ShellSession();

        var result = session.Remove("nonexistent");

        Assert.False(result);
    }

    [Fact]
    public void RegisterProvider_ComputesValueOnAccess()
    {
        var session = new ShellSession();
        var counter = 0;
        session.RegisterProvider("counter", () => ++counter);

        var first = session.Get<int>("counter");
        var second = session.Get<int>("counter");

        Assert.Equal(1, first);
        Assert.Equal(2, second);
    }

    [Fact]
    public void GetVariableNames_ReturnsAllNames()
    {
        var session = new ShellSession();
        session.Set("var1", "value1");
        session.Set("var2", "value2");
        session.RegisterProvider("provider1", () => "computed");

        var names = session.GetVariableNames().ToList();

        Assert.Contains("var1", names);
        Assert.Contains("var2", names);
        Assert.Contains("provider1", names);
    }

    [Fact]
    public void ClearVariables_RemovesAll()
    {
        var session = new ShellSession();
        session.Set("var1", "value1");
        session.RegisterProvider("provider1", () => "computed");

        session.ClearVariables();

        Assert.False(session.Has("var1"));
        Assert.False(session.Has("provider1"));
    }

    [Fact]
    public void OnCommandExecuted_IncrementsCommandCount()
    {
        var session = new ShellSession();

        session.OnCommandExecuted("test", 0);
        session.OnCommandExecuted("test2", 0);

        Assert.Equal(2, session.CommandCount);
    }

    [Fact]
    public void OnCommandExecuted_SetsLastExitCode()
    {
        var session = new ShellSession();

        session.OnCommandExecuted("test", 42);

        Assert.Equal(42, session.LastExitCode);
    }

    [Fact]
    public void OnCommandExecuted_SetsLastResult()
    {
        var session = new ShellSession();
        var result = new { Data = "test" };

        session.OnCommandExecuted("test", 0, result);

        Assert.Same(result, session.LastResult);
    }

    [Fact]
    public void CommandExecuting_RaisesEvent()
    {
        var session = new ShellSession();
        ShellCommandEventArgs? eventArgs = null;
        session.CommandExecuting += (_, args) => eventArgs = args;

        session.OnCommandExecuting("test command");

        Assert.NotNull(eventArgs);
        Assert.Equal("test command", eventArgs.CommandLine);
    }

    [Fact]
    public void CommandExecuted_RaisesEvent()
    {
        var session = new ShellSession();
        ShellCommandEventArgs? eventArgs = null;
        session.CommandExecuted += (_, args) => eventArgs = args;

        session.OnCommandExecuted("test command", 5, "result");

        Assert.NotNull(eventArgs);
        Assert.Equal("test command", eventArgs.CommandLine);
        Assert.Equal(5, eventArgs.ExitCode);
        Assert.Equal("result", eventArgs.Result);
    }

    [Fact]
    public void OnSessionEnding_SetsInactive()
    {
        var session = new ShellSession();

        session.OnSessionEnding();

        Assert.False(session.IsActive);
    }

    [Fact]
    public void SessionEnding_RaisesEvent()
    {
        var session = new ShellSession();
        var eventRaised = false;
        session.SessionEnding += (_, _) => eventRaised = true;

        session.OnSessionEnding();

        Assert.True(eventRaised);
    }

    [Fact]
    public void StartTime_IsSet()
    {
        var before = DateTime.UtcNow;
        var session = new ShellSession();
        var after = DateTime.UtcNow;

        Assert.True(session.StartTime >= before);
        Assert.True(session.StartTime <= after);
    }
}
