using TeCLI.Tests.TestCommands;
using TeCLI.Tests.TestTypes;
using Xunit;
using System;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for custom type converter support
/// </summary>
public class CustomConverterTests
{
    public CustomConverterTests()
    {
        // Clean up environment variables
        Environment.SetEnvironmentVariable("USER_EMAIL", null);
    }

    #region Basic Custom Converter Tests

    [Fact]
    public async Task CustomConverter_WhenValidValue_ShouldConvert()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "send", "--email", "user@example.com" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomConverterCommand.WasCalled);
        Assert.NotNull(CustomConverterCommand.CapturedEmail);
        Assert.Equal("user@example.com", CustomConverterCommand.CapturedEmail.Value);
    }

    [Fact]
    public async Task CustomConverter_WhenInvalidValue_ShouldThrowException()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "send", "--email", "invalid-email" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("Invalid email address", exception.Message);
        Assert.Contains("@", exception.Message);
    }

    [Fact]
    public async Task CustomConverter_WhenEmptyValue_ShouldThrowException()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "send", "--email", "" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("cannot be empty", exception.Message);
    }

    #endregion

    #region Optional Parameters with Custom Converters

    [Fact]
    public async Task CustomConverter_WithOptionalParameter_WhenProvided_ShouldConvert()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "send", "--email", "user@example.com", "--phone", "+1-555-1234" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomConverterCommand.WasCalled);
        Assert.NotNull(CustomConverterCommand.CapturedEmail);
        Assert.Equal("user@example.com", CustomConverterCommand.CapturedEmail.Value);
        Assert.NotNull(CustomConverterCommand.CapturedPhone);
        Assert.Equal("+15551234", CustomConverterCommand.CapturedPhone.Value);
    }

    [Fact]
    public async Task CustomConverter_WithOptionalParameter_WhenNotProvided_ShouldBeNull()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "send", "--email", "user@example.com" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomConverterCommand.WasCalled);
        Assert.NotNull(CustomConverterCommand.CapturedEmail);
        Assert.Null(CustomConverterCommand.CapturedPhone);
    }

    [Fact]
    public async Task CustomConverter_WithOptionalParameter_WhenInvalid_ShouldThrowException()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "send", "--email", "user@example.com", "--phone", "123" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("Invalid phone number", exception.Message);
        Assert.Contains("at least 10 digits", exception.Message);
    }

    #endregion

    #region Arguments with Custom Converters

    [Fact]
    public async Task CustomConverter_WithArgument_WhenValid_ShouldConvert()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "notify", "admin@example.com" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomConverterCommand.WasCalled);
        Assert.NotNull(CustomConverterCommand.CapturedRecipient);
        Assert.Equal("admin@example.com", CustomConverterCommand.CapturedRecipient.Value);
    }

    [Fact]
    public async Task CustomConverter_WithArgument_WhenInvalid_ShouldThrowException()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "notify", "not-an-email" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("Invalid email address", exception.Message);
    }

    #endregion

    #region Collections with Custom Converters

    [Fact]
    public async Task CustomConverter_WithCollection_WhenProvided_ShouldConvertAll()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "notify", "admin@example.com", "--contacts", "+1-555-1111", "--contacts", "+1-555-2222" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomConverterCommand.WasCalled);
        Assert.NotNull(CustomConverterCommand.CapturedContacts);
        Assert.Equal(2, CustomConverterCommand.CapturedContacts.Length);
        Assert.Equal("+15551111", CustomConverterCommand.CapturedContacts[0].Value);
        Assert.Equal("+15552222", CustomConverterCommand.CapturedContacts[1].Value);
    }

    [Fact]
    public async Task CustomConverter_WithCollection_CommaSeparated_ShouldConvertAll()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "notify", "admin@example.com", "--contacts", "+1-555-1111,+1-555-2222,+1-555-3333" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomConverterCommand.WasCalled);
        Assert.NotNull(CustomConverterCommand.CapturedContacts);
        Assert.Equal(3, CustomConverterCommand.CapturedContacts.Length);
        Assert.Equal("+15551111", CustomConverterCommand.CapturedContacts[0].Value);
        Assert.Equal("+15552222", CustomConverterCommand.CapturedContacts[1].Value);
        Assert.Equal("+15553333", CustomConverterCommand.CapturedContacts[2].Value);
    }

    [Fact]
    public async Task CustomConverter_WithCollection_ShortName_ShouldWork()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "notify", "admin@example.com", "-c", "+1-555-1111,+1-555-2222" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomConverterCommand.WasCalled);
        Assert.NotNull(CustomConverterCommand.CapturedContacts);
        Assert.Equal(2, CustomConverterCommand.CapturedContacts.Length);
    }

    [Fact]
    public async Task CustomConverter_WithCollection_WhenOneInvalid_ShouldThrowException()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "notify", "admin@example.com", "--contacts", "+1-555-1111,invalid,+1-555-3333" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("Invalid phone number", exception.Message);
    }

    [Fact]
    public async Task CustomConverter_WithCollection_WhenNotProvided_ShouldBeNull()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "notify", "admin@example.com" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomConverterCommand.WasCalled);
        Assert.Null(CustomConverterCommand.CapturedContacts);
    }

    #endregion

    #region Environment Variables with Custom Converters

    [Fact]
    public async Task CustomConverter_WithEnvVar_ShouldConvert()
    {
        // Arrange
        CustomConverterCommand.Reset();
        Environment.SetEnvironmentVariable("USER_EMAIL", "env@example.com");
        var args = new[] { "custom", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomConverterCommand.WasCalled);
        Assert.NotNull(CustomConverterCommand.CapturedFromEnv);
        Assert.Equal("env@example.com", CustomConverterCommand.CapturedFromEnv.Value);
    }

    [Fact]
    public async Task CustomConverter_WithEnvVar_CLITakesPrecedence()
    {
        // Arrange
        CustomConverterCommand.Reset();
        Environment.SetEnvironmentVariable("USER_EMAIL", "env@example.com");
        var args = new[] { "custom", "connect", "--email", "cli@example.com" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomConverterCommand.WasCalled);
        Assert.NotNull(CustomConverterCommand.CapturedFromEnv);
        Assert.Equal("cli@example.com", CustomConverterCommand.CapturedFromEnv.Value);
    }

    [Fact]
    public async Task CustomConverter_WithEnvVar_WhenInvalid_ShouldThrowException()
    {
        // Arrange
        CustomConverterCommand.Reset();
        Environment.SetEnvironmentVariable("USER_EMAIL", "invalid-email");
        var args = new[] { "custom", "connect" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("Invalid email address", exception.Message);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task CustomConverter_WithSpecialCharacters_ShouldHandle()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "send", "--email", "user+tag@example.co.uk" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomConverterCommand.WasCalled);
        Assert.NotNull(CustomConverterCommand.CapturedEmail);
        Assert.Equal("user+tag@example.co.uk", CustomConverterCommand.CapturedEmail.Value);
    }

    [Fact]
    public async Task CustomConverter_PhoneNumber_WithFormatting_ShouldClean()
    {
        // Arrange
        CustomConverterCommand.Reset();
        var args = new[] { "custom", "send", "--email", "user@example.com", "--phone", "(555) 123-4567" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomConverterCommand.WasCalled);
        Assert.NotNull(CustomConverterCommand.CapturedPhone);
        Assert.Equal("5551234567", CustomConverterCommand.CapturedPhone.Value);
    }

    #endregion
}
