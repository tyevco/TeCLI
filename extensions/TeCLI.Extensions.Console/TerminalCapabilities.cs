using System;
using System.Runtime.InteropServices;

namespace TeCLI.Console;

/// <summary>
/// Detects terminal capabilities for color and ANSI support.
/// </summary>
public static class TerminalCapabilities
{
    private static bool? _supportsColor;
    private static bool? _supportsAnsi;

    /// <summary>
    /// Gets a value indicating whether the terminal supports colors.
    /// This checks for NO_COLOR environment variable and terminal type.
    /// </summary>
    public static bool SupportsColor
    {
        get
        {
            if (_supportsColor.HasValue)
                return _supportsColor.Value;

            _supportsColor = DetectColorSupport();
            return _supportsColor.Value;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the terminal supports ANSI escape codes.
    /// </summary>
    public static bool SupportsAnsi
    {
        get
        {
            if (_supportsAnsi.HasValue)
                return _supportsAnsi.Value;

            _supportsAnsi = DetectAnsiSupport();
            return _supportsAnsi.Value;
        }
    }

    /// <summary>
    /// Forces re-detection of terminal capabilities.
    /// Useful after environment changes.
    /// </summary>
    public static void Refresh()
    {
        _supportsColor = null;
        _supportsAnsi = null;
    }

    /// <summary>
    /// Allows overriding color support detection.
    /// </summary>
    /// <param name="supportsColor">Whether to force color support on or off.</param>
    public static void SetColorSupport(bool supportsColor)
    {
        _supportsColor = supportsColor;
    }

    /// <summary>
    /// Allows overriding ANSI support detection.
    /// </summary>
    /// <param name="supportsAnsi">Whether to force ANSI support on or off.</param>
    public static void SetAnsiSupport(bool supportsAnsi)
    {
        _supportsAnsi = supportsAnsi;
    }

    private static bool DetectColorSupport()
    {
        // Check NO_COLOR environment variable (https://no-color.org/)
        var noColor = Environment.GetEnvironmentVariable("NO_COLOR");
        if (!string.IsNullOrEmpty(noColor))
            return false;

        // Check FORCE_COLOR for explicit enable
        var forceColor = Environment.GetEnvironmentVariable("FORCE_COLOR");
        if (!string.IsNullOrEmpty(forceColor) && forceColor != "0")
            return true;

        // Check if we're in a CI environment that might not support colors
        var ciEnv = Environment.GetEnvironmentVariable("CI");
        var githubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS");
        if (!string.IsNullOrEmpty(githubActions))
            return true; // GitHub Actions supports colors

        // Check TERM environment variable
        var term = Environment.GetEnvironmentVariable("TERM");
        if (term != null)
        {
            // Common terminals that support colors
            if (term.Contains("color") ||
                term.Contains("xterm") ||
                term.Contains("screen") ||
                term.Contains("vt100") ||
                term.Contains("linux") ||
                term.Contains("ansi") ||
                term.Contains("cygwin") ||
                term.Contains("256") ||
                term == "dumb")
            {
                return term != "dumb";
            }
        }

        // Check if stdout is a terminal
        try
        {
            // On Windows, check if we're in a console
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return !System.Console.IsOutputRedirected;
            }

            // On Unix-like systems, check TERM and if stdout is a TTY
            return !System.Console.IsOutputRedirected && !string.IsNullOrEmpty(term);
        }
        catch
        {
            // If we can't determine, assume color is supported for better UX
            return true;
        }
    }

    private static bool DetectAnsiSupport()
    {
        // If colors aren't supported, ANSI isn't either
        if (!SupportsColor)
            return false;

        // On Windows 10+ (build 10586+), ANSI is natively supported
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return TryEnableWindowsAnsi();
        }

        // On Unix-like systems, ANSI is generally supported
        return true;
    }

    private static bool TryEnableWindowsAnsi()
    {
        try
        {
            // Check Windows version - ANSI support was added in Windows 10 build 10586
            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Environment.OSVersion.Version.Major >= 10)
            {
                // Try to enable virtual terminal processing
                // This is a best-effort approach; if it fails, we fall back to non-ANSI
                var handle = GetStdHandle(-11); // STD_OUTPUT_HANDLE
                if (handle != IntPtr.Zero && GetConsoleMode(handle, out var mode))
                {
                    // ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004
                    if (SetConsoleMode(handle, mode | 0x0004))
                    {
                        return true;
                    }
                }

                // Even if we can't enable it, Windows Terminal and VS Code terminal
                // support ANSI by default
                var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
                var wtSession = Environment.GetEnvironmentVariable("WT_SESSION");
                var vsCodeTerminal = Environment.GetEnvironmentVariable("VSCODE_INJECTION");

                if (!string.IsNullOrEmpty(wtSession) ||
                    !string.IsNullOrEmpty(vsCodeTerminal) ||
                    termProgram == "vscode")
                {
                    return true;
                }
            }
        }
        catch
        {
            // Silently fail - we'll just not use ANSI
        }

        return false;
    }

    // P/Invoke declarations for Windows console API
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
}
