# TeCLI.Example.Shell

Example demonstrating TeCLI.Extensions.Shell features with a database-like interactive shell.

## Features Demonstrated

- Interactive REPL loop with custom prompt
- Action-based command registration
- Command history with up/down navigation
- Built-in commands (exit, help, history, clear)
- Welcome and exit messages

## Running the Example

```bash
cd examples/TeCLI.Example.Shell
dotnet run
```

## Available Actions

Once in the shell, the following actions are available:

| Action | Description | Example |
|--------|-------------|---------|
| `query` | Execute a SQL query | `query SELECT * FROM users` |
| `tables` | List all tables | `tables` |
| `describe` | Describe a table schema | `describe users` |
| `insert` | Insert a record | `insert users name=John` |
| `update` | Update records | `update users SET active=true` |
| `delete` | Delete records | `delete FROM users WHERE id=1` |
| `count` | Count records in a table | `count users` |

## Built-in Commands

| Command | Description |
|---------|-------------|
| `exit` / `quit` | Exit the shell |
| `help` | Show shell commands and keyboard shortcuts |
| `actions` | List available database actions |
| `history` | Show command history |
| `clear` / `cls` | Clear the screen |

## Example Session

```
Welcome to the Database Shell!
Type 'help' for shell commands, 'actions' for database operations.

db> tables
Tables:
  - users
  - orders
  - products

db> describe users
Table: users
| column | type    | nullable |
|--------|---------|----------|
| id     | int     | no       |
| name   | varchar | no       |
| email  | varchar | yes      |

db> query SELECT * FROM users
Executing: SELECT * FROM users
| id | name     | email              |
|----|----------|--------------------|
| 1  | Alice    | alice@example.com  |
| 2  | Bob      | bob@example.com    |
(2 rows)

db> history
     1  tables
     2  describe users
     3  query SELECT * FROM users

db> exit
Goodbye!
```

## Code Structure

- `Program.cs` - Shell configuration and action registration

## More Information

See the [TeCLI.Extensions.Shell](../../extensions/TeCLI.Extensions.Shell/README.md) package documentation.
