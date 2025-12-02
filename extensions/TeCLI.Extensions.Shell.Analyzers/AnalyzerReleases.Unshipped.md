; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
TCLSHELL001 | Usage | Warning | ShellWithoutActionsAnalyzer - Shell attribute on command with no actions
TCLSHELL002 | Usage | Warning | ShellHistoryPathAnalyzer - Suspicious history file path
TCLSHELL003 | Usage | Info | ShellPromptValidationAnalyzer - Empty or invalid shell prompt
