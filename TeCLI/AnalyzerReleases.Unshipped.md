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
CLI013 | Usage | Warning | DefaultBeforeRequiredArgumentAnalyzer - Optional argument before required argument
CLI014 | Usage | Info | ContainerParameterSuggestionAnalyzer - Consider using container parameter for 4+ options
CLI015 | Usage | Warning | UnusedActionMethodAnalyzer - Action method in non-command class or inaccessible
CLI016 | Usage | Error/Warning | ValidationAttributeCombinationAnalyzer - Invalid validation attribute combinations
CLI017 | Usage | Warning | ReservedSwitchNameAnalyzer - Option name conflicts with reserved switch
CLI018 | Usage | Error | DuplicateCommandNameAnalyzer - Duplicate command names across classes
CLI019 | Usage | Info | MissingDescriptionAnalyzer - Missing description on Command/Option/Argument
CLI020 | Usage | Warning | RequiredBooleanOptionAnalyzer - Boolean option marked as required
CLI021 | Usage | Error | CollectionArgumentPositionAnalyzer - Collection argument not in last position
CLI022 | Usage | Error | PropertyWithoutSetterAnalyzer - Option/Argument property without setter
CLI023 | Usage | Info | AsyncWithoutCancellationTokenAnalyzer - Async action without CancellationToken
CLI024 | Usage | Warning | CommandWithoutActionAnalyzer - Command class without action methods
CLI025 | Usage | Info | InconsistentNamingConventionAnalyzer - Inconsistent naming convention
CLI026 | Usage | Info | ShortOptionNameAnalyzer - Single-character option name should use ShortName
CLI027 | Usage | Info | RedundantNameAnalyzer - Redundant name specification
CLI028 | Usage | Warning | HiddenRequiredOptionAnalyzer - Hidden option marked as required
CLI029 | Usage | Info | NullableWithoutDefaultAnalyzer - Nullable option without explicit default
CLI030 | Usage | Error | EmptyEnumTypeAnalyzer - Option uses empty enum type
CLI031 | Usage | Warning | MultipleGlobalOptionsAnalyzer - Multiple GlobalOptions classes
CLI032 | Security | Info | SensitiveOptionAnalyzer - Sensitive option detected
CLI033 | Usage | Error | AliasConflictAnalyzer - Conflicting command/action alias
CLI034 | Usage | Warning | InvalidEnvironmentVariableAnalyzer - Invalid or duplicate EnvVar mapping
CLI035 | Usage | Error | TypeConverterValidationAnalyzer - Invalid TypeConverter reference
CLI036 | Usage | Error | HookTypeValidationAnalyzer - Invalid hook type reference
CLI037 | Usage | Error | ExitCodeMappingAnalyzer - Invalid or duplicate MapExitCode mapping