using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Tests;

public class MapperGenerationResultAssertions
{
    private readonly MapperGenerationResult _result;

    public MapperGenerationResultAssertions(MapperGenerationResult result)
    {
        _result = result;
    }

    public MapperGenerationResultAssertions NotHaveDiagnostics(IReadOnlySet<DiagnosticSeverity> allowedDiagnosticSeverities)
    {
        _result.Diagnostics
            .FirstOrDefault(d => !allowedDiagnosticSeverities.Contains(d.Severity))
            .Should()
            .BeNull();
        return this;
    }

    public MapperGenerationResultAssertions HaveDiagnostic(DiagnosticMatcher diagnosticMatcher)
    {
        _result.Diagnostics.FirstOrDefault(diagnosticMatcher.Matches)
            .Should()
            .NotBeNull();
        return this;
    }
}
