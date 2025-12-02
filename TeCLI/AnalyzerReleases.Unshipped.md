; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CLI001 | Usage | Error | PrimitiveTypeArgumentAnalyzer
CLI002 | Usage | Error | SetOptionAccessibilityAnalyzer
CLI003 | Usage | Error | MultiplePrimaryAttributeAnalyzer
CLI004 | Usage | Warning | CommandOptionNameValidationAnalyzer - Invalid command name
CLI005 | Usage | Warning | CommandOptionNameValidationAnalyzer - Invalid option name
CLI006 | Usage | Error | CommandOptionNameValidationAnalyzer - Empty name not allowed
CLI007 | Usage | Error | DuplicateNameAnalyzer - Duplicate action name
CLI008 | Usage | Error | DuplicateNameAnalyzer - Duplicate option name
CLI009 | Usage | Error | DuplicateNameAnalyzer - Conflicting argument positions
CLI010 | Usage | Error | ConflictingShortNameAnalyzer - Conflicting option short names
CLI011 | Usage | Warning | AsyncMethodReturnTypeAnalyzer - Async method must return Task
CLI012 | Usage | Warning | AsyncVoidActionAnalyzer - Avoid async void in action methods