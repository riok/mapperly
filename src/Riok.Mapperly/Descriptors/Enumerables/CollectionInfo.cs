using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.Enumerables;

public record CollectionInfo(
    CollectionType Type,
    CollectionType ImplementedTypes,
    ITypeSymbol EnumeratedType,
    bool CountIsKnown,
    bool HasImplicitCollectionAddMethod,
    bool IsImmutableCollectionType
)
{
    public (ITypeSymbol, ITypeSymbol)? GetDictionaryKeyValueTypes(MappingBuilderContext ctx, ITypeSymbol t)
    {
        if (t.ImplementsGeneric(ctx.Types.Get(typeof(IDictionary<,>)), out var dictionaryImpl))
        {
            return (dictionaryImpl.TypeArguments[0], dictionaryImpl.TypeArguments[1]);
        }

        if (t.ImplementsGeneric(ctx.Types.Get(typeof(IReadOnlyDictionary<,>)), out var readOnlyDictionaryImpl))
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
