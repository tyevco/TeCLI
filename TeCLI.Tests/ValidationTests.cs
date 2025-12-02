using TeCLI.Tests.TestCommands;
using Xunit;
using System;
using System.IO;

namespace TeCLI.Tests;

/// <summary>
/// Integration tests for validation attribute support
/// </summary>
public class ValidationTests
{
    private readonly string _tempTestDir;
    private readonly string _tempTestFile;

    public ValidationTests()
    {
        // Setup temp files and directories for testing
        _tempTestDir = Path.Combine(Path.GetTempPath(), "TeCLI_ValidationTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempTestDir);

        _tempTestFile = Path.Combine(_tempTestDir, "test.txt");
        File.WriteAllText(_tempTestFile, "test content");
    }

    ~ValidationTests()
    {
        // Cleanup
        try
        {
            if (Directory.Exists(_tempTestDir))
            {
                Directory.Delete(_tempTestDir, true);
            }
        }
        catch { }
    }

    #region Range Validation Tests

    [Fact]
    public async Task RangeValidation_WhenValueWithinRange_ShouldParse()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "connect", "--port", "8080" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Equal(8080, ValidationCommand.CapturedPort);
    }

    [Fact]
    public async Task RangeValidation_WhenValueAtMinimum_ShouldParse()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "connect", "--port", "1" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Equal(1, ValidationCommand.CapturedPort);
    }

    [Fact]
    public async Task RangeValidation_WhenValueAtMaximum_ShouldParse()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "connect", "--port", "65535" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Equal(65535, ValidationCommand.CapturedPort);
    }

    [Fact]
    public async Task RangeValidation_WhenValueBelowMinimum_ShouldThrowException()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "connect", "--port", "0" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("outside the allowed range", exception.Message);
        Assert.Contains("[1, 65535]", exception.Message);
    }

    [Fact]
    public async Task RangeValidation_WhenValueAboveMaximum_ShouldThrowException()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "connect", "--port", "65536" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("outside the allowed range", exception.Message);
        Assert.Contains("[1, 65535]", exception.Message);
    }

    [Fact]
    public async Task RangeValidation_WithDouble_WhenValueWithinRange_ShouldParse()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "analyze", "--percentage", "75.5" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Equal(75.5, ValidationCommand.CapturedPercentage);
    }

    [Fact]
    public async Task RangeValidation_WithDouble_WhenValueOutsideRange_ShouldThrowException()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "analyze", "--percentage", "150.0" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("outside the allowed range", exception.Message);
        Assert.Contains("[0, 100]", exception.Message);
    }

    [Fact]
    public async Task RangeValidation_WithOptionalParameter_WhenNotProvided_ShouldUseDefault()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Equal(8080, ValidationCommand.CapturedPort); // default value
    }

    [Fact]
    public async Task RangeValidation_WithTimeout_WhenValueValid_ShouldParse()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "connect", "--timeout", "60" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Equal(60, ValidationCommand.CapturedTimeout);
    }

    [Fact]
    public async Task RangeValidation_WithTimeout_WhenValueInvalid_ShouldThrowException()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "connect", "--timeout", "5000" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("outside the allowed range", exception.Message);
        Assert.Contains("[0, 3600]", exception.Message);
    }

    #endregion

    #region RegularExpression Validation Tests

    [Fact]
    public async Task RegexValidation_WhenValueMatchesPattern_ShouldParse()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "connect", "--username", "john_doe123" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Equal("john_doe123", ValidationCommand.CapturedUsername);
    }

    [Fact]
    public async Task RegexValidation_WhenValueTooShort_ShouldThrowException()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "connect", "--username", "ab" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("does not match the required pattern", exception.Message);
    }

    [Fact]
    public async Task RegexValidation_WhenValueTooLong_ShouldThrowException()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "connect", "--username", "abcdefghijklmnopqrstuvwxyz" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("does not match the required pattern", exception.Message);
    }

    [Fact]
    public async Task RegexValidation_WhenValueContainsInvalidCharacters_ShouldThrowException()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "connect", "--username", "john-doe" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("does not match the required pattern", exception.Message);
    }

    [Fact]
    public async Task RegexValidation_WithEmail_WhenValid_ShouldParse()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "analyze", "--email", "test@example.com" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Equal("test@example.com", ValidationCommand.CapturedEmail);
    }

    [Fact]
    public async Task RegexValidation_WithEmail_WhenInvalid_ShouldThrowException()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "analyze", "--email", "invalid-email" };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("Invalid email format", exception.Message); // Custom error message
    }

    [Fact]
    public async Task RegexValidation_WithOptionalParameter_WhenNotProvided_ShouldNotValidate()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "connect" };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Null(ValidationCommand.CapturedUsername);
    }

    #endregion

    #region FileExists Validation Tests

    [Fact]
    public async Task FileExistsValidation_WhenFileExists_ShouldParse()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "process", _tempTestFile };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Equal(_tempTestFile, ValidationCommand.CapturedInputFile);
    }

    [Fact]
    public async Task FileExistsValidation_WhenFileDoesNotExist_ShouldThrowException()
    {
        // Arrange
        ValidationCommand.Reset();
        var nonExistentFile = Path.Combine(_tempTestDir, "nonexistent.txt");
        var args = new[] { "validate", "process", nonExistentFile };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("does not exist", exception.Message);
        Assert.Contains(nonExistentFile, exception.Message);
    }

    [Fact]
    public async Task FileExistsValidation_WithRunAction_WhenFileExists_ShouldParse()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "run", _tempTestFile };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Equal(_tempTestFile, ValidationCommand.CapturedConfigFile);
    }

    #endregion

    #region DirectoryExists Validation Tests

    [Fact]
    public async Task DirectoryExistsValidation_WhenDirectoryExists_ShouldParse()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "process", _tempTestFile, "--output-dir", _tempTestDir };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Equal(_tempTestDir, ValidationCommand.CapturedOutputDir);
    }

    [Fact]
    public async Task DirectoryExistsValidation_WhenDirectoryDoesNotExist_ShouldThrowException()
    {
        // Arrange
        ValidationCommand.Reset();
        var nonExistentDir = Path.Combine(_tempTestDir, "nonexistent");
        var args = new[] { "validate", "process", _tempTestFile, "--output-dir", nonExistentDir };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("does not exist", exception.Message);
        Assert.Contains(nonExistentDir, exception.Message);
    }

    [Fact]
    public async Task DirectoryExistsValidation_WithOptionalParameter_WhenNotProvided_ShouldNotValidate()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[] { "validate", "process", _tempTestFile };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Null(ValidationCommand.CapturedOutputDir);
    }

    #endregion

    #region Combined Validation Tests

    [Fact]
    public async Task CombinedValidations_AllValid_ShouldParse()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[]
        {
            "validate", "connect",
            "--port", "443",
            "--username", "admin123",
            "--timeout", "120"
        };

        // Act
        var dispatcher = new TeCLI.CommandDispatcher();
        await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ValidationCommand.WasCalled);
        Assert.Equal(443, ValidationCommand.CapturedPort);
        Assert.Equal("admin123", ValidationCommand.CapturedUsername);
        Assert.Equal(120, ValidationCommand.CapturedTimeout);
    }

    [Fact]
    public async Task CombinedValidations_OneInvalid_ShouldThrowException()
    {
        // Arrange
        ValidationCommand.Reset();
        var args = new[]
        {
            "validate", "connect",
            "--port", "443",
            "--username", "a", // Too short
            "--timeout", "120"
        };

        // Act & Assert
        var dispatcher = new TeCLI.CommandDispatcher();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken));

        Assert.Contains("does not match the required pattern", exception.Message);
    }

    #endregion
}
