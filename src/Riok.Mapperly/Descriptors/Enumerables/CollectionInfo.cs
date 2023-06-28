using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.Enumerables;

public record CollectionInfo(
    ITypeSymbol Type,
    CollectionType CollectionType,
    CollectionType ImplementedTypes,
    ITypeSymbol EnumeratedType,
    string? CountPropertyName,
    bool HasImplicitCollectionAddMethod,
    bool IsImmutableCollectionType
)
{
    public bool ImplementsIEnumerable => ImplementedTypes.HasFlag(CollectionType.IEnumerable);

    public bool IsArray => CollectionType is CollectionType.Array;
    public bool IsMemory => CollectionType is CollectionType.Memory or CollectionType.ReadOnlyMemory;
    public bool IsSpan => CollectionType is CollectionType.Span or CollectionType.ReadOnlySpan;

    [MemberNotNullWhen(true, nameof(CountPropertyName))]
    public bool CountIsKnown => CountPropertyName != null;

    public (ITypeSymbol, ITypeSymbol)? GetDictionaryKeyValueTypes(MappingBuilderContext ctx)
    {
        if (Type.ImplementsGeneric(ctx.Types.Get(typeof(IDictionary<,>)), out var dictionaryImpl))
        {
            return (dictionaryImpl.TypeArguments[0], dictionaryImpl.TypeArguments[1]);
        }

        if (Type.ImplementsGeneric(ctx.Types.Get(typeof(IReadOnlyDictionary<,>)), out var readOnlyDictionaryImpl))
        {
            return (readOnlyDictionaryImpl.TypeArguments[0], readOnlyDictionaryImpl.TypeArguments[1]);
        }

        return null;
    }

    public (ITypeSymbol, ITypeSymbol)? GetEnumeratedKeyValueTypes(WellKnownTypes types)
    {
        if (
            EnumeratedType is not INamedTypeSymbol namedEnumeratedType
            || !SymbolEqualityComparer.Default.Equals(namedEnumeratedType.ConstructedFrom, types.Get(typeof(KeyValuePair<,>)))
        )
            return null;

        return (namedEnumeratedType.TypeArguments[0], namedEnumeratedType.TypeArguments[1]);
    }
}
