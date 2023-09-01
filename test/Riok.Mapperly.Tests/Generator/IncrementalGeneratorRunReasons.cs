using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Tests.Generator;

internal record IncrementalGeneratorRunReasons(
    IncrementalStepRunReason CompilationStep,
    IncrementalStepRunReason BuildMapperDefaultsStep,
    IncrementalStepRunReason BuildMappersStep,
    IncrementalStepRunReason ReportDiagnosticsStep
)
{
    public static readonly IncrementalGeneratorRunReasons New =
        new(IncrementalStepRunReason.New, IncrementalStepRunReason.New, IncrementalStepRunReason.New, IncrementalStepRunReason.New);

    public static readonly IncrementalGeneratorRunReasons Cached =
        new(
            // compilation step should always be modified as each time a new compilation is passed
            IncrementalStepRunReason.Modified,
            IncrementalStepRunReason.Unchanged,
            IncrementalStepRunReason.Cached,
            IncrementalStepRunReason.Cached
        );

    public static readonly IncrementalGeneratorRunReasons Modified = Cached with
    {
        BuildMapperDefaultsStep = IncrementalStepRunReason.Modified,
        ReportDiagnosticsStep = IncrementalStepRunReason.Modified,
        BuildMappersStep = IncrementalStepRunReason.Modified,
    };

    public static readonly IncrementalGeneratorRunReasons ModifiedDiagnostics = Cached with
    {
        BuildMappersStep = IncrementalStepRunReason.Unchanged,
        ReportDiagnosticsStep = IncrementalStepRunReason.Modified,
    };

    public static readonly IncrementalGeneratorRunReasons ModifiedSource = Cached with
    {
        ReportDiagnosticsStep = IncrementalStepRunReason.Unchanged,
        BuildMappersStep = IncrementalStepRunReason.Modified,
    };

    public static readonly IncrementalGeneratorRunReasons ModifiedSourceAndDiagnostics = Cached with
    {
        ReportDiagnosticsStep = IncrementalStepRunReason.Modified,
        BuildMappersStep = IncrementalStepRunReason.Modified,
    };

    public static readonly IncrementalGeneratorRunReasons ModifiedDefaults = Cached with
    {
        BuildMapperDefaultsStep = IncrementalStepRunReason.Modified,
    };
}
