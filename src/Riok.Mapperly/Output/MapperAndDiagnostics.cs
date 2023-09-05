using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Templates;

namespace Riok.Mapperly.Output;

public readonly record struct MapperAndDiagnostics(
    MapperNode Mapper,
    ImmutableEquatableArray<Diagnostic> Diagnostics,
    IReadOnlyCollection<TemplateReference> Templates
)
{
    public bool Equals(MapperAndDiagnostics other)
    {
        return Mapper.Equals(other.Mapper) && Diagnostics.Equals(other.Diagnostics) && Templates.SequenceEqual(other.Templates);
    }

    public override int GetHashCode() => HashCode.Combine(Mapper.GetHashCode(), Diagnostics.GetHashCode(), Templates.Count);
}
