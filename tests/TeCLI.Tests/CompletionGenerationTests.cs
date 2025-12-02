using System;
using System.IO;
using TeCLI.Tests.TestCommands;
using Xunit;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for shell completion generation
/// </summary>
public class CompletionGenerationTests
{
    [Fact]
    public async Task GenerateCompletion_Bash_ShouldOutputScript()
    {
        // Arrange
        var args = new[] { "--generate-completion", "bash" };
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        var result = output.ToString();
        Assert.Contains("# Bash completion script", result);
        Assert.Contains("_completions()", result);
        Assert.Contains("complete -F", result);
        Assert.Contains("COMPREPLY", result);
    }

    [Fact]
    public async Task GenerateCompletion_Zsh_ShouldOutputScript()
    {
        // Arrange
        var args = new[] { "--generate-completion", "zsh" };
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        var result = output.ToString();
        Assert.Contains("#compdef", result);
        Assert.Contains("# Zsh completion script", result);
        Assert.Contains("_arguments", result);
    }

    [Fact]
    public async Task GenerateCompletion_PowerShell_ShouldOutputScript()
    {
        // Arrange
        var args = new[] { "--generate-completion", "powershell" };
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        var result = output.ToString();
        Assert.Contains("# PowerShell completion script", result);
        Assert.Contains("Register-ArgumentCompleter", result);
        Assert.Contains("-Native", result);
        Assert.Contains("CompletionResult", result);
    }

    [Fact]
    public async Task GenerateCompletion_Fish_ShouldOutputScript()
    {
        // Arrange
        var args = new[] { "--generate-completion", "fish" };
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        var result = output.ToString();
        Assert.Contains("# Fish completion script", result);
        Assert.Contains("complete -c", result);
    }

    [Fact]
    public async Task GenerateCompletion_UnknownShell_ShouldShowError()
    {
        // Arrange
        var args = new[] { "--generate-completion", "unknown-shell" };
        var errorOutput = new StringWriter();
        Console.SetError(errorOutput);

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        var result = errorOutput.ToString();
        Assert.Contains("Unknown shell: unknown-shell", result);
        Assert.Contains("Supported shells: bash, zsh, powershell, fish", result);
    }

    [Fact]
    public async Task GenerateCompletion_Bash_ShouldIncludeCommands()
    {
        // Arrange
        var args = new[] { "--generate-completion", "bash" };
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        var result = output.ToString();
        // Should include global options
        Assert.Contains("--help", result);
        Assert.Contains("--version", result);
    }

    [Fact]
    public async Task GenerateCompletion_Zsh_ShouldIncludeGlobalOptions()
    {
        // Arrange
        var args = new[] { "--generate-completion", "zsh" };
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        var result = output.ToString();
        Assert.Contains("--help", result);
        Assert.Contains("--version", result);
    }

    [Fact]
    public async Task GenerateCompletion_PowerShell_ShouldIncludeGlobalOptions()
    {
        // Arrange
        var args = new[] { "--generate-completion", "powershell" };
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        var result = output.ToString();
        Assert.Contains("--help", result);
        Assert.Contains("--version", result);
    }

    [Fact]
    public async Task GenerateCompletion_Fish_ShouldIncludeGlobalOptions()
    {
        // Arrange
        var args = new[] { "--generate-completion", "fish" };
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        var result = output.ToString();
        Assert.Contains("--help", result);
        Assert.Contains("--version", result);
    }

    [Fact]
    public async Task GenerateCompletion_CaseInsensitive_Bash_ShouldWork()
    {
        // Arrange
        var args = new[] { "--generate-completion", "BASH" };
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        var result = output.ToString();
        Assert.Contains("# Bash completion script", result);
    }

    [Fact]
    public async Task GenerateCompletion_CaseInsensitive_Zsh_ShouldWork()
    {
        // Arrange
        var args = new[] { "--generate-completion", "ZSH" };
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        var result = output.ToString();
        Assert.Contains("# Zsh completion script", result);
    }

    [Fact]
    public async Task GenerateCompletion_CaseInsensitive_PowerShell_ShouldWork()
    {
        // Arrange
        var args = new[] { "--generate-completion", "PowerShell" };
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        var result = output.ToString();
        Assert.Contains("# PowerShell completion script", result);
    }

    [Fact]
    public async Task GenerateCompletion_CaseInsensitive_Fish_ShouldWork()
    {
        // Arrange
        var args = new[] { "--generate-completion", "FISH" };
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        var result = output.ToString();
        Assert.Contains("# Fish completion script", result);
    }
}
