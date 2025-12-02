using TeCLI.Attributes;
using TeCLI.Attributes.Validation;

namespace TeCLI.Example.Advanced;

/// <summary>
/// File operations command demonstrating:
/// - FileExists validation
/// - DirectoryExists validation
/// - FileInfo/DirectoryInfo parameters
/// - Exit code returns (ExitCode enum and int)
/// - Exception-to-exit-code mapping
/// </summary>
[Command("file", Description = "File and directory operations")]
[MapExitCode(typeof(UnauthorizedAccessException), ExitCode.PermissionDenied)]
[MapExitCode(typeof(IOException), ExitCode.IoError)]
public class FileCommand
{
    /// <summary>
    /// Display file information with exit code support
    /// </summary>
    [Primary]
    [Action("info", Description = "Display information about a file")]
    [MapExitCode(typeof(FileNotFoundException), ExitCode.FileNotFound)]
    public ExitCode Info(
        [Argument(Description = "Path to the file")]
        string filePath,

        [Option("checksums", ShortName = 'c', Description = "Calculate file checksums")]
        bool checksums = false)
    {
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"Error: File '{filePath}' not found.");
            return ExitCode.FileNotFound;
        }

        var fileInfo = new FileInfo(filePath);

        Console.WriteLine($"File Information:");
        Console.WriteLine($"  Name: {fileInfo.Name}");
        Console.WriteLine($"  Full Path: {fileInfo.FullName}");
        Console.WriteLine($"  Size: {FormatFileSize(fileInfo.Length)}");
        Console.WriteLine($"  Created: {fileInfo.CreationTime}");
        Console.WriteLine($"  Modified: {fileInfo.LastWriteTime}");
        Console.WriteLine($"  Read-Only: {fileInfo.IsReadOnly}");

        if (checksums)
        {
            Console.WriteLine($"  MD5: (calculating...)");
            Console.WriteLine($"  SHA256: (calculating...)");
        }

        return ExitCode.Success;
    }

    /// <summary>
    /// List directory contents
    /// </summary>
    [Action("list", Description = "List directory contents", Aliases = new[] { "ls", "dir" })]
    public void List(
        [Argument(Description = "Directory path")]
        [DirectoryExists(ErrorMessage = "The specified directory does not exist")]
        string directoryPath = ".",

        [Option("recursive", ShortName = 'r', Description = "List recursively")]
        bool recursive = false,

        [Option("hidden", ShortName = 'a', Description = "Include hidden files")]
        bool includeHidden = false,

        [Option("pattern", ShortName = 'p', Description = "File pattern filter")]
        string pattern = "*")
    {
        var dirInfo = new DirectoryInfo(directoryPath);
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        Console.WriteLine($"Contents of: {dirInfo.FullName}");
        Console.WriteLine();

        foreach (var file in dirInfo.GetFiles(pattern, searchOption))
        {
            if (!includeHidden && file.Attributes.HasFlag(FileAttributes.Hidden))
                continue;

            var prefix = file.Attributes.HasFlag(FileAttributes.Directory) ? "[D]" : "[F]";
            Console.WriteLine($"  {prefix} {file.Name,-40} {FormatFileSize(file.Length),10}");
        }
    }

    /// <summary>
    /// Copy file(s) to a destination with exit code
    /// </summary>
    [Action("copy", Description = "Copy files to a destination", Aliases = new[] { "cp" })]
    public int Copy(
        [Argument(Description = "Source file path")]
        string source,

        [Argument(Description = "Destination path")]
        string destination,

        [Option("overwrite", ShortName = 'f', Description = "Overwrite existing files")]
        bool overwrite = false,

        [Option("permissions", Description = "File permissions to set")]
        FilePermissions permissions = FilePermissions.ReadWrite)
    {
        if (!File.Exists(source))
        {
            Console.Error.WriteLine($"Error: Source file '{source}' not found.");
            return (int)ExitCode.FileNotFound;
        }

        if (File.Exists(destination) && !overwrite)
        {
            Console.Error.WriteLine($"Error: Destination file '{destination}' already exists. Use -f to overwrite.");
            return (int)ExitCode.Error;
        }

        Console.WriteLine($"Copying: {source}");
        Console.WriteLine($"     To: {destination}");
        Console.WriteLine($"  Overwrite: {overwrite}");
        Console.WriteLine($"  Permissions: {permissions}");

        // Simulate copy
        Console.WriteLine("Copy completed successfully!");
        return 0; // Success
    }

    /// <summary>
    /// Search for files matching a pattern
    /// </summary>
    [Action("search", Description = "Search for files", Aliases = new[] { "find" })]
    public void Search(
        [Argument(Description = "Search pattern (regex)")]
        [RegularExpression(@"^[\w.*?]+$", ErrorMessage = "Invalid search pattern")]
        string pattern,

        [Option("path", ShortName = 'p', Description = "Starting path")]
        [DirectoryExists]
        string path = ".",

        [Option("type", ShortName = 't', Description = "File type filter (e.g., cs, txt, json)")]
        string[]? types = null,

        [Option("max-depth", ShortName = 'd', Description = "Maximum directory depth")]
        [Range(1, 20)]
        int maxDepth = 5)
    {
        Console.WriteLine($"Searching for: {pattern}");
        Console.WriteLine($"Starting from: {path}");
        Console.WriteLine($"Max depth: {maxDepth}");

        if (types?.Length > 0)
        {
            Console.WriteLine($"File types: {string.Join(", ", types)}");
        }

        Console.WriteLine();
        Console.WriteLine("Search results:");
        Console.WriteLine("  ./src/Program.cs");
        Console.WriteLine("  ./src/Utils/Helper.cs");
        Console.WriteLine($"Found 2 files matching '{pattern}'");
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}
