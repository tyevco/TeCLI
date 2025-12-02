# TeCLI Advanced Example

This example demonstrates advanced features of TeCLI for building sophisticated command-line applications.

## Features Demonstrated

### Enums
- Regular enums (LogLevel, Environment, OutputFormat)
- Flags enums (FilePermissions)
- Case-insensitive parsing

### Validation
- `[Range]` - Numeric range validation
- `[RegularExpression]` - Pattern validation
- `[FileExists]` - File existence validation
- `[DirectoryExists]` - Directory existence validation

### Collections
- String arrays (`string[]`) for multi-value options
- Comma-separated input parsing

### Nested Commands (Subcommands)
- Git-style command hierarchy
- `config database setup` pattern
- `config cache clear` pattern

### Command & Action Aliases
- Multiple names for commands: `deploy` or `dpl`
- Multiple names for actions: `rollback`, `rb`, or `undo`

### Environment Variables
- Fallback values from environment: `--host` or `DB_HOST`

### Required Options
- Mandatory options with `Required = true`

### Pipeline/Stream Support
- Unix-style stdin/stdout piping
- `Stream`, `TextReader`, `TextWriter` parameters
- Special `-` handling for stdin/stdout
- File path to stream conversion

## Usage Examples

### Deploy Command

```bash
# Deploy to production with required version
dotnet run -- deploy start Production --version 1.2.3

# Deploy with all options
dotnet run -- deploy start Staging -v 1.2.3 -r 5 --regions us-west,eu-central --tags release,stable --force

# Using alias
dotnet run -- dpl start Development -v 1.0.0

# Check deployment status
dotnet run -- deploy status dep-123 --format json --watch

# Rollback (multiple aliases)
dotnet run -- deploy rollback Production --to-version 1.2.2
dotnet run -- deploy rb Production -s 2
dotnet run -- deploy undo Staging

# View deployment history
dotnet run -- deploy history Production -n 5 -o json
```

### File Command

```bash
# File info (requires existing file)
dotnet run -- file info ./README.md
dotnet run -- file info ./README.md --checksums

# List directory (with aliases)
dotnet run -- file list . --recursive --hidden
dotnet run -- file ls . -ra
dotnet run -- file dir ./src -p "*.cs"

# Copy with permissions
dotnet run -- file copy source.txt dest.txt --overwrite --permissions ReadWrite

# Search files
dotnet run -- file search "Test" --path ./src --type cs,txt --max-depth 3
dotnet run -- file find "*.json" -p ./config
```

### Config Command (Nested)

```bash
# Show all configuration
dotnet run -- config show
dotnet run -- config show --section database --format json

# Set/get configuration values
dotnet run -- config set app.name "MyApp"
dotnet run -- config set app.debug true --global
dotnet run -- config get app.name

# Database subcommand
dotnet run -- config database setup --name mydb --host localhost --port 5432 --ssl
dotnet run -- config db setup -n mydb -h localhost -p 5432
dotnet run -- config database test --timeout 10
dotnet run -- config database migrate --direction Up --dry-run
dotnet run -- config db migrate -d Down -s 2

# Using environment variables for database
export DB_HOST=production-db.example.com
export DB_USER=admin
dotnet run -- config database setup --name proddb

# Cache subcommand
dotnet run -- config cache setup --provider Redis --ttl 7200 --max-size 512
dotnet run -- config cache clear --pattern "user:*" --force
```

### Stream Command (Pipeline Support)

```bash
# Transform text (stdin to stdout)
cat input.txt | dotnet run -- stream transform --uppercase
echo "hello world" | dotnet run -- stream transform -u

# Transform with file input/output
dotnet run -- stream transform -i input.txt -o output.txt --uppercase

# Count lines, words, characters (like wc)
cat document.txt | dotnet run -- stream count
dotnet run -- stream count -i file.txt --lines --words

# Filter lines (like grep)
cat log.txt | dotnet run -- stream filter "ERROR"
dotnet run -- stream filter "TODO" -i source.cs --ignore-case

# Uppercase/lowercase
echo "hello" | dotnet run -- stream uppercase
dotnet run -- stream lowercase -i file.txt

# Binary data analysis
dotnet run -- stream binary -i data.bin --hex
dotnet run -- stream binary -i image.png --analyze

# Reverse lines
cat file.txt | dotnet run -- stream reverse
dotnet run -- stream reverse -i file.txt --chars  # Reverse characters
```

## Validation Examples

```bash
# Range validation - port must be 1-65535
dotnet run -- config database setup --name db --port 99999  # Error!

# Regex validation - version must be semver
dotnet run -- deploy start Production --version invalid  # Error!
dotnet run -- deploy start Production --version 1.2.3  # OK
dotnet run -- deploy start Production --version 1.2.3-beta  # OK

# File exists validation
dotnet run -- file info /nonexistent/file.txt  # Error!

# Directory exists validation
dotnet run -- file list /nonexistent/dir  # Error!
```

## Code Structure

- `Program.cs` - Entry point
- `Enums.cs` - Shared enum definitions
- `DeployCommand.cs` - Deployment with enums, validation, arrays, aliases
- `FileCommand.cs` - File operations with path validation
- `ConfigCommand.cs` - Nested subcommands, environment variables
- `StreamCommand.cs` - Pipeline/stream operations with stdin/stdout
