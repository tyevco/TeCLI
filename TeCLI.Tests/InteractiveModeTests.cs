using TeCLI.Tests.TestCommands;
using Xunit;
using System;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for interactive mode prompt support.
/// Note: These tests verify that the prompt attributes don't interfere with normal CLI argument parsing.
/// Manual testing is required to verify actual interactive prompting behavior.
/// </summary>
public class InteractiveModeTests
{
    [Fact]
    public void ArgumentWithPrompt_WhenProvidedViaArgs_ShouldUseProvidedValue()
    {
        // Arrange
        InteractiveModeCommand.Reset();
        var args = new[] { "interactive", "deploy", "production" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(InteractiveModeCommand.WasCalled);
        Assert.Equal("production", InteractiveModeCommand.LastEnvironment);
        Assert.Equal("us-west", InteractiveModeCommand.LastRegion); // default value
    }

    [Fact]
    public void OptionWithPrompt_WhenProvidedViaArgs_ShouldUseProvidedValue()
    {
        // Arrange
        InteractiveModeCommand.Reset();
        var args = new[] { "interactive", "deploy", "staging", "--region", "us-east" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(InteractiveModeCommand.WasCalled);
        Assert.Equal("staging", InteractiveModeCommand.LastEnvironment);
        Assert.Equal("us-east", InteractiveModeCommand.LastRegion);
    }

    [Fact]
    public void MultipleArgumentsWithPrompts_WhenProvidedViaArgs_ShouldUseProvidedValues()
    {
        // Arrange
        InteractiveModeCommand.Reset();
        var args = new[] { "interactive", "login", "alice", "secret123" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(InteractiveModeCommand.WasCalled);
        Assert.Equal("alice", InteractiveModeCommand.LastUsername);
        Assert.Equal("secret123", InteractiveModeCommand.LastPassword);
    }

    [Fact]
    public void OptionWithPromptAndDefaultValue_WhenNotProvided_ShouldUseDefaultValue()
    {
        // Arrange
        InteractiveModeCommand.Reset();
        var args = new[] { "interactive", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(InteractiveModeCommand.WasCalled);
        Assert.Equal(8080, InteractiveModeCommand.LastPort); // default value
    }

    [Fact]
    public void OptionWithPromptAndDefaultValue_WhenProvidedViaArgs_ShouldUseProvidedValue()
    {
        // Arrange
        InteractiveModeCommand.Reset();
        var args = new[] { "interactive", "connect", "--port", "3000" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(InteractiveModeCommand.WasCalled);
        Assert.Equal(3000, InteractiveModeCommand.LastPort);
    }

    [Fact]
    public void SecureOptionWithPrompt_WhenProvidedViaArgs_ShouldUseProvidedValue()
    {
        // Arrange
        InteractiveModeCommand.Reset();
        var args = new[] { "interactive", "secure-option", "--api-key", "my-secret-key" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert
        Assert.True(InteractiveModeCommand.WasCalled);
        Assert.Equal("my-secret-key", InteractiveModeCommand.LastPassword);
    }

    [Fact]
    public void PromptAttribute_DoesNotMakeParameterRequired()
    {
        // Arrange
        InteractiveModeCommand.Reset();
        var args = new[] { "interactive", "deploy", "production" };

        // Act - region has a prompt but also has a default value, so it's optional
        var dispatcher = new TeCLI.CommandDispatcher();
        dispatcher.DispatchAsync(args).Wait();

        // Assert - should not throw, uses default
        Assert.True(InteractiveModeCommand.WasCalled);
        Assert.Equal("production", InteractiveModeCommand.LastEnvironment);
        Assert.Equal("us-west", InteractiveModeCommand.LastRegion);
    }

    // Note: Testing actual interactive prompting requires mocking Console.ReadLine and Console.ReadKey
    // which is beyond the scope of these unit tests. Manual testing should verify:
    // 1. Arguments without CLI values trigger prompts with the correct message
    // 2. Options without CLI values (and no env vars) trigger prompts
    // 3. SecurePrompt = true properly masks input with asterisks
    // 4. Prompted values are correctly parsed and validated
    // 5. Empty input for required parameters throws appropriate errors
}
