// TeCLI Shell Example
// Demonstrates the interactive shell/REPL functionality

using TeCLI.Shell;

// Example 1: Simple action-based shell
var shell = ShellExtensions.CreateActionShell(new ShellOptions
{
    Prompt = "db> ",
    WelcomeMessage = "Welcome to the Database Shell!\nType 'help' for shell commands, 'actions' for database operations.",
    ExitMessage = "Goodbye!"
})
.WithAction("query", "Execute a SQL query", args =>
{
    if (args.Length == 0)
    {
        Console.Error.WriteLine("Usage: query <sql>");
        return 1;
    }
    var sql = string.Join(" ", args);
    Console.WriteLine($"Executing: {sql}");
    Console.WriteLine("| id | name     | email              |");
    Console.WriteLine("|----|----------|--------------------|");
    Console.WriteLine("| 1  | Alice    | alice@example.com  |");
    Console.WriteLine("| 2  | Bob      | bob@example.com    |");
    Console.WriteLine("(2 rows)");
    return 0;
})
.WithAction("tables", "List all tables", _ =>
{
    Console.WriteLine("Tables:");
    Console.WriteLine("  - users");
    Console.WriteLine("  - orders");
    Console.WriteLine("  - products");
    return 0;
})
.WithAction("describe", "Describe a table schema", args =>
{
    if (args.Length == 0)
    {
        Console.Error.WriteLine("Usage: describe <table>");
        return 1;
    }
    Console.WriteLine($"Table: {args[0]}");
    Console.WriteLine("| column | type    | nullable |");
    Console.WriteLine("|--------|---------|----------|");
    Console.WriteLine("| id     | int     | no       |");
    Console.WriteLine("| name   | varchar | no       |");
    Console.WriteLine("| email  | varchar | yes      |");
    return 0;
})
.WithAction("insert", "Insert a record", args =>
{
    Console.WriteLine("INSERT 1");
    return 0;
})
.WithAction("update", "Update records", args =>
{
    Console.WriteLine("UPDATE 1");
    return 0;
})
.WithAction("delete", "Delete records", args =>
{
    Console.WriteLine("DELETE 1");
    return 0;
})
.WithAction("count", "Count records in a table", args =>
{
    if (args.Length == 0)
    {
        Console.Error.WriteLine("Usage: count <table>");
        return 1;
    }
    Console.WriteLine($"Count: 42");
    return 0;
})
.Build();

// Run the shell
return shell.Run();
