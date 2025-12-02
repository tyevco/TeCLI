using TeCLI.Shell;
using Xunit;

namespace TeCLI.Extensions.Shell.Tests;

public class ShellAttributeTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var attr = new ShellAttribute();

        Assert.Equal("> ", attr.Prompt);
        Assert.Null(attr.WelcomeMessage);
        Assert.Null(attr.ExitMessage);
        Assert.True(attr.EnableHistory);
        Assert.Equal(100, attr.MaxHistorySize);
        Assert.Null(attr.HistoryFile);
        Assert.False(attr.ShowHelpOnStart);
    }

    [Fact]
    public void PromptConstructor_SetsPrompt()
    {
        var attr = new ShellAttribute("db> ");

        Assert.Equal("db> ", attr.Prompt);
    }

    [Fact]
    public void PromptConstructor_NullPrompt_UsesDefault()
    {
        var attr = new ShellAttribute(null!);

        Assert.Equal("> ", attr.Prompt);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var attr = new ShellAttribute
        {
            Prompt = "test> ",
            WelcomeMessage = "Welcome!",
            ExitMessage = "Goodbye!",
            EnableHistory = false,
            MaxHistorySize = 50,
            HistoryFile = "/tmp/history",
            ShowHelpOnStart = true
        };

        Assert.Equal("test> ", attr.Prompt);
        Assert.Equal("Welcome!", attr.WelcomeMessage);
        Assert.Equal("Goodbye!", attr.ExitMessage);
        Assert.False(attr.EnableHistory);
        Assert.Equal(50, attr.MaxHistorySize);
        Assert.Equal("/tmp/history", attr.HistoryFile);
        Assert.True(attr.ShowHelpOnStart);
    }

    [Fact]
    public void Attribute_CanBeAppliedToClass()
    {
        var type = typeof(TestShellCommand);
        var attrs = type.GetCustomAttributes(typeof(ShellAttribute), true);

        Assert.Single(attrs);
        var attr = (ShellAttribute)attrs[0];
        Assert.Equal("custom> ", attr.Prompt);
    }

    [Shell("custom> ")]
    private class TestShellCommand { }
}

public class ShellOptionsTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var options = new ShellOptions();

        Assert.Equal("> ", options.Prompt);
        Assert.Null(options.WelcomeMessage);
        Assert.Null(options.ExitMessage);
        Assert.Equal(100, options.MaxHistorySize);
        Assert.Null(options.HistoryFile);
        Assert.False(options.ShowHelpOnStart);
    }

    [Fact]
    public void FromAttribute_CopiesAllProperties()
    {
        var attr = new ShellAttribute
        {
            Prompt = "test> ",
            WelcomeMessage = "Welcome!",
            ExitMessage = "Goodbye!",
            MaxHistorySize = 50,
            HistoryFile = "/tmp/history",
            ShowHelpOnStart = true
        };

        var options = ShellOptions.FromAttribute(attr);

        Assert.Equal("test> ", options.Prompt);
        Assert.Equal("Welcome!", options.WelcomeMessage);
        Assert.Equal("Goodbye!", options.ExitMessage);
        Assert.Equal(50, options.MaxHistorySize);
        Assert.Equal("/tmp/history", options.HistoryFile);
        Assert.True(options.ShowHelpOnStart);
    }
}
