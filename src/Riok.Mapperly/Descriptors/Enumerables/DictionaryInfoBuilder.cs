using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.Enumerables;

public static class DictionaryInfoBuilder
{
    public static DictionaryInfos? Build(WellKnownTypes types, CollectionInfos? collectionInfos)
    {
        if (collectionInfos == null)
            return null;

        var source = BuildSource(types, collectionInfos);
        if (source == null)
            return null;

        var target = BuildTarget(types, collectionInfos);
        if (target == null)
            return null;

        return new DictionaryInfos(source, target);
    }

    private static DictionaryInfo? BuildSource(WellKnownTypes types, CollectionInfos infos)
    {
        if (GetEnumeratedKeyValueTypes(types, infos.Source) is not var (key, value))
            return null;

        return new DictionaryInfo(infos.Source, key, value);
    }

    private static DictionaryInfo? BuildTarget(WellKnownTypes types, CollectionInfos infos)
    {
        if (GetDictionaryKeyValueTypes(types, infos.Target) is not var (key, value))
            return null;

        return new DictionaryInfo(infos.Target, key, value);
    }

    private static (ITypeSymbol, ITypeSymbol)? GetDictionaryKeyValueTypes(WellKnownTypes types, CollectionInfo info)
    {
        if (info.Type.ImplementsGeneric(types.Get(typeof(IDictionary<,>)), out var dictionaryImpl))
        {
            return (dictionaryImpl.TypeArguments[0], dictionaryImpl.TypeArguments[1]);
        }

        if (info.Type.ImplementsGeneric(types.Get(typeof(IReadOnlyDictionary<,>)), out var readOnlyDictionaryImpl))
        {
            return (readOnlyDictionaryImpl.TypeArguments[0], readOnlyDictionaryImpl.TypeArguments[1]);
        }

        return null;
    }

    private static (ITypeSymbol, ITypeSymbol)? GetEnumeratedKeyValueTypes(WellKnownTypes types, CollectionInfo info)
    {
        if (
            info.EnumeratedType is not INamedTypeSymbol namedEnumeratedType
            || !SymbolEqualityComparer.Default.Equals(namedEnumeratedType.ConstructedFrom, types.Get(typeof(KeyValuePair<,>)))
        )
            return null;

        return (namedEnumeratedType.TypeArguments[0], namedEnumeratedType.TypeArguments[1]);
    }
}
