# Security Policy

## Supported Versions

We actively support the following versions of TeCLI with security updates:

| Version | Supported          | .NET Version |
| ------- | ------------------ | ------------ |
| 1.x.x   | :white_check_mark: | .NET 8.0+    |
| < 1.0   | :x:                | N/A          |

## Reporting a Vulnerability

We take the security of TeCLI seriously. If you discover a security vulnerability, please follow these steps:

### Private Disclosure Process

1. **DO NOT** open a public GitHub issue for security vulnerabilities
2. Report the vulnerability privately using one of these methods:

   **GitHub Security Advisories (Preferred):**
   - Navigate to the [Security Advisories](https://github.com/tyevco/TeCLI/security/advisories) page
   - Click "Report a vulnerability"
   - Provide detailed information about the vulnerability

   **Email:**
   - Send details to: [security@tylercode.dev](mailto:security@tylercode.dev)
   - Include "TeCLI Security" in the subject line

### What to Include

Please provide as much information as possible:

- **Description**: Clear description of the vulnerability
- **Impact**: What could an attacker accomplish?
- **Steps to Reproduce**: Detailed steps to reproduce the issue
- **Affected Versions**: Which versions are impacted?
- **Proof of Concept**: Code sample demonstrating the vulnerability (if possible)
- **Suggested Fix**: If you have ideas for mitigation

### Example Report

```
Subject: TeCLI Security - Code Injection in Command Parser

Description:
The command parser in TeCLI v1.0.0 is vulnerable to code injection when
processing user input containing special characters.

Impact:
An attacker could execute arbitrary code by crafting malicious command-line
arguments.

Steps to Reproduce:
1. Create a CLI application with TeCLI
2. Pass the following argument: --input="$(malicious_command)"
3. The command gets executed

Affected Versions: 1.0.0 - 1.2.3

Proof of Concept:
[Attach code sample]

Suggested Fix:
Sanitize user input before processing in CommandLineArgsParser.cs:123
```

## Response Timeline

We are committed to responding quickly to security issues:

- **Initial Response**: Within 48 hours of report
- **Status Update**: Within 7 days with assessment
- **Fix Timeline**:
  - Critical: Within 7 days
  - High: Within 14 days
  - Medium: Within 30 days
  - Low: Next minor/patch release

## Security Update Process

1. **Acknowledgment**: We confirm receipt of your report
2. **Investigation**: We investigate and validate the vulnerability
3. **Fix Development**: We develop and test a fix
4. **Disclosure**: We coordinate disclosure timing with you
5. **Release**: We release a security update
6. **Advisory**: We publish a security advisory

## Public Disclosure

- We follow coordinated disclosure practices
- Security advisories are published after a fix is available
- We credit reporters (unless they prefer to remain anonymous)
- We provide CVE identifiers for significant vulnerabilities

## Security Best Practices

When using TeCLI in your applications:

### Input Validation

```csharp
[Command("secure")]
public class SecureCommand
{
    [Primary]
    public void Execute(
        [Option("input", Required = true)] string input)
    {
        // Always validate and sanitize user input
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty");

        // Use parameterized queries for database operations
        // Escape special characters for shell commands
        // Validate file paths to prevent directory traversal
    }
}
```

### Secure File Operations

```csharp
[Command("files")]
public class FileCommand
{
    [Primary]
    public void Execute([Argument] string filePath)
    {
        // Validate file paths
        var fullPath = Path.GetFullPath(filePath);
        if (!fullPath.StartsWith(allowedDirectory))
            throw new SecurityException("Access denied");
    }
}

### Sensitive Data Handling

```csharp
[Command("auth")]
public class AuthCommand
{
    [Primary]
    public void Login(
        [Option("password", Prompt = "Enter password", SecurePrompt = true)] string password)
    {
        // Never log sensitive data
        // Use SecureString for passwords when possible
        // Clear sensitive data from memory after use
    }
}

## Security Features

TeCLI includes built-in security features:

- **Input Validation**: Attribute-based validation prevents invalid input
- **Type Safety**: Strong typing reduces injection vulnerabilities
- **Source Generation**: Compile-time code generation eliminates runtime reflection risks
- **No Dynamic Evaluation**: No eval() or dynamic code execution

## Known Security Considerations

### Command Injection

Always validate user input before passing to system commands:

```csharp
// BAD - vulnerable to command injection
Process.Start("cmd.exe", $"/c {userInput}");

// GOOD - use parameterized approach
var processInfo = new ProcessStartInfo
{
    FileName = "program.exe",
    Arguments = userInput // Properly escaped by ProcessStartInfo
};
Process.Start(processInfo);
```

### Path Traversal

Validate file paths to prevent directory traversal:

```csharp
// BAD - vulnerable to path traversal
File.ReadAllText(userProvidedPath);

// GOOD - validate and restrict paths
var fullPath = Path.GetFullPath(userProvidedPath);
if (!fullPath.StartsWith(allowedDirectory))
    throw new SecurityException("Invalid path");
File.ReadAllText(fullPath);
```

## Dependencies

We use Dependabot to monitor and update dependencies automatically:

- Automated weekly scans for vulnerable dependencies
- Automated PRs for security updates
- Regular audits of transitive dependencies

## Security Tools

We use the following tools to maintain security:

- **CodeQL**: Advanced security analysis on every PR
- **Dependabot**: Automated dependency updates
- **.NET Security Analyzers**: Compile-time security checks
- **NuGet Package Scanning**: Vulnerability detection

## Contact

For security-related questions or concerns:
- **Security Issues**: Use GitHub Security Advisories
- **General Security Questions**: security@tylercode.dev
- **General Support**: Open a regular GitHub issue

## Hall of Fame

We recognize security researchers who help keep TeCLI secure:

<!-- Security researchers will be listed here after responsible disclosure -->

---

Thank you for helping keep TeCLI and its users safe!
