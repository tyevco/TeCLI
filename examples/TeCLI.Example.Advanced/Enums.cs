namespace TeCLI.Example.Advanced;

/// <summary>
/// Log level enum demonstrating enum parameter support
/// </summary>
public enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

/// <summary>
/// Output format enum
/// </summary>
public enum OutputFormat
{
    Text,
    Json,
    Xml,
    Yaml
}

/// <summary>
/// Environment type enum
/// </summary>
public enum Environment
{
    Development,
    Staging,
    Production
}

/// <summary>
/// File permissions using flags enum
/// </summary>
[Flags]
public enum FilePermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4,
    ReadWrite = Read | Write,
    All = Read | Write | Execute
}
