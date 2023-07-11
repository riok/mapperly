using Microsoft.CodeAnalysis;

namespace Riok.Mapperly;

public readonly record struct MapperResults(ImmutableEquatableArray<MapperNode> Mappers, ImmutableEquatableArray<Diagnostic> Diagnostics)
{
    public static readonly MapperResults Empty = new(ImmutableEquatableArray<MapperNode>.Empty, ImmutableEquatableArray<Diagnostic>.Empty);
}
