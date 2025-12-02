; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
TCLI18N001 | Usage | Error | EmptyResourceKeyAnalyzer - Empty resource key in localization attribute
TCLI18N002 | Usage | Warning | DuplicateLocalizationAnalyzer - Both Description and LocalizedDescription set
TCLI18N003 | Usage | Info | LocalizationConsistencyAnalyzer - Inconsistent localization usage
