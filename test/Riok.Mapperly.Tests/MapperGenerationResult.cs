using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Tests;

public record MapperGenerationResult(IReadOnlyCollection<Diagnostic> Diagnostics, IReadOnlyDictionary<string, GeneratedMethod> Methods)
{
    public MapperGenerationResultAssertions Should() => new(this);
}
