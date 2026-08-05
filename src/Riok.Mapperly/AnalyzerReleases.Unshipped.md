; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/master/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|------
RMG032  | Mapper       | Error        | Mapper       | Warning      | The enum mapping strategy ByName, ByValueCheckDefined, explicit enum mappings and ignored enum values cannot be used in projection mappings
