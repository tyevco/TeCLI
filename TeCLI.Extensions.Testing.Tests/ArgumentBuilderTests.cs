using TeCLI.Testing;
using Xunit;

namespace TeCLI.Extensions.Testing.Tests;

public class ArgumentBuilderTests
{
    [Fact]
    public void Command_CreatesBuilderWithCommand()
    {
        // Act
        var args = ArgumentBuilder.Command("mycommand").Build();

        // Assert
        Assert.Single(args);
        Assert.Equal("mycommand", args[0]);
    }

    [Fact]
    public void Action_AddsAction()
    {
        // Act
        var args = ArgumentBuilder.Command("mycommand")
            .Action("myaction")
            .Build();

        // Assert
        Assert.Equal(2, args.Length);
        Assert.Equal("mycommand", args[0]);
        Assert.Equal("myaction", args[1]);
    }

    [Fact]
    public void Argument_AddsPositionalArgument()
    {
        // Act
        var args = ArgumentBuilder.Command("greet")
            .Argument("World")
            .Build();

        // Assert
        Assert.Equal(2, args.Length);
        Assert.Equal("World", args[1]);
    }

    [Fact]
    public void Arguments_AddsMultiplePositionalArguments()
    {
        // Act
        var args = ArgumentBuilder.Command("copy")
            .Arguments("source.txt", "dest.txt")
            .Build();

        // Assert
        Assert.Equal(3, args.Length);
        Assert.Equal("source.txt", args[1]);
        Assert.Equal("dest.txt", args[2]);
    }

    [Fact]
    public void Option_AddsLongOption()
    {
        // Act
        var args = ArgumentBuilder.Command("deploy")
            .Option("environment", "prod")
            .Build();

        // Assert
        Assert.Equal(3, args.Length);
        Assert.Equal("--environment", args[1]);
        Assert.Equal("prod", args[2]);
    }

    [Fact]
    public void Option_Generic_AddsTypedOption()
    {
        // Act
        var args = ArgumentBuilder.Command("config")
            .Option("timeout", 30)
            .Option("enabled", true)
            .Build();

        // Assert
        Assert.Equal(5, args.Length);
        Assert.Equal("--timeout", args[1]);
        Assert.Equal("30", args[2]);
        Assert.Equal("--enabled", args[3]);
        Assert.Equal("True", args[4]);
    }

    [Fact]
    public void ShortOption_AddsShortOption()
    {
        // Act
        var args = ArgumentBuilder.Command("deploy")
            .ShortOption('e', "prod")
            .Build();

        // Assert
        Assert.Equal(3, args.Length);
        Assert.Equal("-e", args[1]);
        Assert.Equal("prod", args[2]);
    }

    [Fact]
    public void Flag_AddsLongFlag()
    {
        // Act
        var args = ArgumentBuilder.Command("deploy")
            .Flag("force")
            .Build();

        // Assert
        Assert.Equal(2, args.Length);
        Assert.Equal("--force", args[1]);
    }

    [Fact]
    public void ShortFlag_AddsShortFlag()
    {
        // Act
        var args = ArgumentBuilder.Command("deploy")
            .ShortFlag('f')
            .Build();

        // Assert
        Assert.Equal(2, args.Length);
        Assert.Equal("-f", args[1]);
    }

    [Fact]
    public void OptionIf_AddsOption_WhenConditionTrue()
    {
        // Act
        var args = ArgumentBuilder.Command("test")
            .OptionIf(true, "config", "debug")
            .Build();

        // Assert
        Assert.Equal(3, args.Length);
        Assert.Contains("--config", args);
    }

    [Fact]
    public void OptionIf_DoesNotAddOption_WhenConditionFalse()
    {
        // Act
        var args = ArgumentBuilder.Command("test")
            .OptionIf(false, "config", "debug")
            .Build();

        // Assert
        Assert.Single(args);
        Assert.DoesNotContain("--config", args);
    }

    [Fact]
    public void FlagIf_AddsFlag_WhenConditionTrue()
    {
        // Act
        var args = ArgumentBuilder.Command("test")
            .FlagIf(true, "verbose")
            .Build();

        // Assert
        Assert.Equal(2, args.Length);
        Assert.Contains("--verbose", args);
    }

    [Fact]
    public void FlagIf_DoesNotAddFlag_WhenConditionFalse()
    {
        // Act
        var args = ArgumentBuilder.Command("test")
            .FlagIf(false, "verbose")
            .Build();

        // Assert
        Assert.Single(args);
    }

    [Fact]
    public void Help_AddsHelpFlag()
    {
        // Act
        var args = ArgumentBuilder.Command("mycommand")
            .Help()
            .Build();

        // Assert
        Assert.Contains("--help", args);
    }

    [Fact]
    public void Version_AddsVersionFlag()
    {
        // Act
        var args = ArgumentBuilder.Command("mycommand")
            .Version()
            .Build();

        // Assert
        Assert.Contains("--version", args);
    }

    [Fact]
    public void Raw_AddsRawArgument()
    {
        // Act
        var args = ArgumentBuilder.Create()
            .Raw("--custom=value")
            .Build();

        // Assert
        Assert.Single(args);
        Assert.Equal("--custom=value", args[0]);
    }

    [Fact]
    public void Raw_AddsMultipleRawArguments()
    {
        // Act
        var args = ArgumentBuilder.Create()
            .Raw("arg1", "arg2", "arg3")
            .Build();

        // Assert
        Assert.Equal(3, args.Length);
    }

    [Fact]
    public void ImplicitConversion_ReturnsArray()
    {
        // Act
        string[] args = ArgumentBuilder.Command("test").Action("run");

        // Assert
        Assert.Equal(2, args.Length);
    }

    [Fact]
    public void ToString_ReturnsCommandLine()
    {
        // Arrange
        var builder = ArgumentBuilder.Command("deploy")
            .Option("environment", "prod")
            .Flag("force");

        // Act
        var result = builder.ToString();

        // Assert
        Assert.Equal("deploy --environment prod --force", result);
    }

    [Fact]
    public void ToString_QuotesArgumentsWithSpaces()
    {
        // Arrange
        var builder = ArgumentBuilder.Command("copy")
            .Argument("file with spaces.txt");

        // Act
        var result = builder.ToString();

        // Assert
        Assert.Contains("\"file with spaces.txt\"", result);
    }

    [Fact]
    public void Parse_SplitsSimpleCommandLine()
    {
        // Act
        var args = ArgumentBuilder.Parse("deploy --environment prod --force");

        // Assert
        Assert.Equal(4, args.Length);
        Assert.Equal("deploy", args[0]);
        Assert.Equal("--environment", args[1]);
        Assert.Equal("prod", args[2]);
        Assert.Equal("--force", args[3]);
    }

    [Fact]
    public void Parse_HandlesQuotedStrings()
    {
        // Act
        var args = ArgumentBuilder.Parse("copy \"source file.txt\" \"dest file.txt\"");

        // Assert
        Assert.Equal(3, args.Length);
        Assert.Equal("copy", args[0]);
        Assert.Equal("source file.txt", args[1]);
        Assert.Equal("dest file.txt", args[2]);
    }

    [Fact]
    public void Parse_HandlesEscapedCharacters()
    {
        // Act
        var args = ArgumentBuilder.Parse("echo hello\\ world");

        // Assert
        Assert.Equal(2, args.Length);
        Assert.Equal("hello world", args[1]);
    }

    [Fact]
    public void Parse_ReturnsEmptyArray_ForEmptyString()
    {
        // Act
        var args = ArgumentBuilder.Parse("");

        // Assert
        Assert.Empty(args);
    }

    [Fact]
    public void Parse_ReturnsEmptyArray_ForWhitespaceString()
    {
        // Act
        var args = ArgumentBuilder.Parse("   ");

        // Assert
        Assert.Empty(args);
    }

    [Fact]
    public void FluentChaining_BuildsComplexCommandLine()
    {
        // Act
        var args = ArgumentBuilder.Command("deploy")
            .Action("production")
            .Option("region", "us-west-2")
            .Option("instances", 3)
            .Flag("force")
            .Flag("no-cache")
            .ShortFlag('v')
            .Build();

        // Assert
        Assert.Equal(10, args.Length);
        Assert.Equal("deploy", args[0]);
        Assert.Equal("production", args[1]);
        Assert.Equal("--region", args[2]);
        Assert.Equal("us-west-2", args[3]);
        Assert.Equal("--instances", args[4]);
        Assert.Equal("3", args[5]);
        Assert.Equal("--force", args[6]);
        Assert.Equal("--no-cache", args[7]);
        Assert.Equal("-v", args[8]);
    }
}
