using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Output;

public readonly record struct MapperAndDiagnostics(MapperNode Mapper, ImmutableEquatableArray<Diagnostic> Diagnostics)
{
    public bool Equals(MapperAndDiagnostics other)
    {
        return Mapper.Equals(other.Mapper) && Diagnostics.Equals(other.Diagnostics);
    }

    public override int GetHashCode() => HashCode.Combine(Mapper.GetHashCode(), Diagnostics.GetHashCode());
}
