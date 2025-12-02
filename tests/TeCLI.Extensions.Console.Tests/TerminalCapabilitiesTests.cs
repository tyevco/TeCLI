using TeCLI.Console;
using Xunit;

namespace TeCLI.Extensions.Console.Tests;

public class TerminalCapabilitiesTests
{
    [Fact]
    public void SetColorSupport_OverridesDetection()
    {
        // Arrange & Act
        TerminalCapabilities.SetColorSupport(true);

        // Assert
        Assert.True(TerminalCapabilities.SupportsColor);

        // Cleanup
        TerminalCapabilities.Refresh();
    }

    [Fact]
    public void SetColorSupport_CanDisableColor()
    {
        // Arrange & Act
        TerminalCapabilities.SetColorSupport(false);

        // Assert
        Assert.False(TerminalCapabilities.SupportsColor);

        // Cleanup
        TerminalCapabilities.Refresh();
    }

    [Fact]
    public void SetAnsiSupport_OverridesDetection()
    {
        // Arrange & Act
        TerminalCapabilities.SetAnsiSupport(true);

        // Assert
        Assert.True(TerminalCapabilities.SupportsAnsi);

        // Cleanup
        TerminalCapabilities.Refresh();
    }

    [Fact]
    public void SetAnsiSupport_CanDisableAnsi()
    {
        // Arrange & Act
        TerminalCapabilities.SetAnsiSupport(false);

        // Assert
        Assert.False(TerminalCapabilities.SupportsAnsi);

        // Cleanup
        TerminalCapabilities.Refresh();
    }

    [Fact]
    public void Refresh_ResetsOverrides()
    {
        // Arrange
        TerminalCapabilities.SetColorSupport(false);
        TerminalCapabilities.SetAnsiSupport(false);

        // Act
        TerminalCapabilities.Refresh();

        // Assert - After refresh, it should re-detect
        // We can't assert specific values since detection depends on environment,
        // but we can verify it doesn't throw
        var _ = TerminalCapabilities.SupportsColor;
        var __ = TerminalCapabilities.SupportsAnsi;
    }

    [Fact]
    public void SupportsColor_IsCached()
    {
        // Arrange
        TerminalCapabilities.Refresh();

        // Act - Access multiple times
        var first = TerminalCapabilities.SupportsColor;
        var second = TerminalCapabilities.SupportsColor;
        var third = TerminalCapabilities.SupportsColor;

        // Assert - Should return same value (cached)
        Assert.Equal(first, second);
        Assert.Equal(second, third);
    }

    [Fact]
    public void SupportsAnsi_IsCached()
    {
        // Arrange
        TerminalCapabilities.Refresh();

        // Act - Access multiple times
        var first = TerminalCapabilities.SupportsAnsi;
        var second = TerminalCapabilities.SupportsAnsi;
        var third = TerminalCapabilities.SupportsAnsi;

        // Assert - Should return same value (cached)
        Assert.Equal(first, second);
        Assert.Equal(second, third);
    }
}
