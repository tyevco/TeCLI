using TeCLI.Tests.TestCommands;
using Xunit;

namespace TeCLI.Tests;

/// <summary>
/// Tests for exit code management feature
/// </summary>
public class ExitCodeTests
{
    private readonly CommandDispatcher _dispatcher = new();

    [Fact]
    public async Task ExitCodeAction_ReturnsSuccess_ExitCodeIsZero()
    {
        // Arrange
        ExitCodeCommand.Reset();
        var args = new[] { "exitcode", "success" };

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ExitCodeCommand.WasCalled);
        Assert.Equal("success", ExitCodeCommand.LastAction);
        Assert.Equal(0, exitCode);
        Assert.Equal(0, _dispatcher.LastExitCode);
    }

    [Fact]
    public async Task ExitCodeAction_ReturnsError_ExitCodeIsOne()
    {
        // Arrange
        ExitCodeCommand.Reset();
        var args = new[] { "exitcode", "error" };

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ExitCodeCommand.WasCalled);
        Assert.Equal("error", ExitCodeCommand.LastAction);
        Assert.Equal(1, exitCode);
        Assert.Equal(1, _dispatcher.LastExitCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(64)]
    [InlineData(78)]
    public async Task ExitCodeAction_ReturnsSpecificCode_CorrectExitCode(int expectedCode)
    {
        // Arrange
        ExitCodeCommand.Reset();
        var args = new[] { "exitcode", "code", expectedCode.ToString() };

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ExitCodeCommand.WasCalled);
        Assert.Equal("code", ExitCodeCommand.LastAction);
        Assert.Equal(expectedCode, exitCode);
        Assert.Equal(expectedCode, _dispatcher.LastExitCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(100)]
    public async Task IntExitCodeAction_ReturnsCorrectCode(int expectedCode)
    {
        // Arrange
        ExitCodeCommand.Reset();
        var args = new[] { "exitcode", "intcode", expectedCode.ToString() };

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ExitCodeCommand.WasCalled);
        Assert.Equal("intcode", ExitCodeCommand.LastAction);
        Assert.Equal(expectedCode, exitCode);
    }

    [Fact]
    public async Task ExitCodeAction_ReturnsFileNotFound_ExitCodeIsThree()
    {
        // Arrange
        ExitCodeCommand.Reset();
        var args = new[] { "exitcode", "filenotfound" };

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ExitCodeCommand.WasCalled);
        Assert.Equal("filenotfound", ExitCodeCommand.LastAction);
        Assert.Equal((int)ExitCode.FileNotFound, exitCode);
    }

    [Fact]
    public async Task VoidAction_ReturnsZero()
    {
        // Arrange
        ExitCodeCommand.Reset();
        var args = new[] { "exitcode", "void" };

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ExitCodeCommand.WasCalled);
        Assert.Equal("void", ExitCodeCommand.LastAction);
        Assert.Equal(0, exitCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public async Task AsyncEnumExitCode_ReturnsCorrectCode(int expectedCode)
    {
        // Arrange
        ExitCodeCommand.Reset();
        var args = new[] { "exitcode", "asyncenum", expectedCode.ToString() };

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ExitCodeCommand.WasCalled);
        Assert.Equal("asyncenum", ExitCodeCommand.LastAction);
        Assert.Equal(expectedCode, exitCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(42)]
    public async Task AsyncIntExitCode_ReturnsCorrectCode(int expectedCode)
    {
        // Arrange
        ExitCodeCommand.Reset();
        var args = new[] { "exitcode", "asyncint", expectedCode.ToString() };

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ExitCodeCommand.WasCalled);
        Assert.Equal("asyncint", ExitCodeCommand.LastAction);
        Assert.Equal(expectedCode, exitCode);
    }

    [Fact]
    public async Task AsyncVoidAction_ReturnsZero()
    {
        // Arrange
        ExitCodeCommand.Reset();
        var args = new[] { "exitcode", "asyncvoid" };

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(ExitCodeCommand.WasCalled);
        Assert.Equal("asyncvoid", ExitCodeCommand.LastAction);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task LastExitCode_UpdatesAfterEachDispatch()
    {
        // Arrange
        ExitCodeCommand.Reset();

        // Act - First call returns success
        await _dispatcher.DispatchAsync(new[] { "exitcode", "success" }, TestContext.Current.CancellationToken);
        Assert.Equal(0, _dispatcher.LastExitCode);

        // Act - Second call returns error
        await _dispatcher.DispatchAsync(new[] { "exitcode", "error" }, TestContext.Current.CancellationToken);
        Assert.Equal(1, _dispatcher.LastExitCode);

        // Act - Third call returns specific code
        await _dispatcher.DispatchAsync(new[] { "exitcode", "intcode", "42" }, TestContext.Current.CancellationToken);
        Assert.Equal(42, _dispatcher.LastExitCode);
    }

    [Fact]
    public async Task CustomEnumExitCode_ReturnsCorrectCode()
    {
        // Arrange
        CustomExitCodeCommand.Reset();
        var args = new[] { "customexit", "custom", "20" }; // CustomExitCode.Failure = 20

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomExitCodeCommand.WasCalled);
        Assert.Equal("custom", CustomExitCodeCommand.LastAction);
        Assert.Equal(20, exitCode);
    }

    [Fact]
    public async Task AsyncCustomEnumExitCode_ReturnsCorrectCode()
    {
        // Arrange
        CustomExitCodeCommand.Reset();
        var args = new[] { "customexit", "asynccustom", "30" }; // CustomExitCode.Critical = 30

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(CustomExitCodeCommand.WasCalled);
        Assert.Equal("asynccustom", CustomExitCodeCommand.LastAction);
        Assert.Equal(30, exitCode);
    }

    [Fact]
    public async Task HelpFlag_ReturnsZero()
    {
        // Arrange
        var args = new[] { "--help" };

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task VersionFlag_ReturnsZero()
    {
        // Arrange
        var args = new[] { "--version" };

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task UnknownCommand_ReturnsOne()
    {
        // Arrange
        var args = new[] { "unknowncommand" };

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task EmptyArgs_ReturnsZero()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var exitCode = await _dispatcher.DispatchAsync(args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExitCodeEnum_HasExpectedValues()
    {
        // Verify the ExitCode enum has the expected values
        Assert.Equal(0, (int)ExitCode.Success);
        Assert.Equal(1, (int)ExitCode.Error);
        Assert.Equal(2, (int)ExitCode.InvalidArguments);
        Assert.Equal(3, (int)ExitCode.FileNotFound);
        Assert.Equal(4, (int)ExitCode.PermissionDenied);
        Assert.Equal(5, (int)ExitCode.NetworkError);
        Assert.Equal(6, (int)ExitCode.Cancelled);
        Assert.Equal(7, (int)ExitCode.ConfigurationError);
        Assert.Equal(8, (int)ExitCode.ResourceUnavailable);

        // BSD sysexits.h codes
        Assert.Equal(64, (int)ExitCode.Usage);
        Assert.Equal(65, (int)ExitCode.DataError);
        Assert.Equal(66, (int)ExitCode.NoInput);
        Assert.Equal(74, (int)ExitCode.IoError);
        Assert.Equal(78, (int)ExitCode.Config);
    }
}
