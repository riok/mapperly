using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Tests;

public record MapperGenerationResult(IReadOnlyCollection<Diagnostic> Diagnostics, IReadOnlyDictionary<string, string> MethodBodies)
{
    public MapperGenerationResultAssertions Should()
        => new(this);
}
