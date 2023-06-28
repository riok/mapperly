using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.Enumerables;

public static class CollectionInfoBuilder
{
    private record CollectionTypeInfo(
        CollectionType CollectionType,
        Type? ReflectionType = null,
        string? TypeFullName = null,
        bool Immutable = false
    )
    {
        public INamedTypeSymbol? GetTypeSymbol(WellKnownTypes types)
        {
            if (ReflectionType != null)
                return types.Get(ReflectionType);

            if (TypeFullName != null)
                return types.TryGet(TypeFullName);

            throw new InvalidOperationException("One type needs to be set for each collection type");
        }
    }

    private static readonly CollectionTypeInfo _collectionTypeInfoArray = new(CollectionType.Array);

    private static readonly IReadOnlyCollection<CollectionTypeInfo> _collectionTypeInfos = new[]
    {
        new CollectionTypeInfo(CollectionType.IEnumerable, typeof(IEnumerable<>)),
        new CollectionTypeInfo(CollectionType.List, typeof(List<>)),
        new CollectionTypeInfo(CollectionType.Stack, typeof(Stack<>)),
        new CollectionTypeInfo(CollectionType.Queue, typeof(Queue<>)),
        new CollectionTypeInfo(CollectionType.IReadOnlyCollection, typeof(IReadOnlyCollection<>)),
        new CollectionTypeInfo(CollectionType.IList, typeof(IList<>)),
        new CollectionTypeInfo(CollectionType.IReadOnlyList, typeof(IReadOnlyList<>)),
        new CollectionTypeInfo(CollectionType.ICollection, typeof(ICollection<>)),
        new CollectionTypeInfo(CollectionType.HashSet, typeof(HashSet<>)),
        new CollectionTypeInfo(CollectionType.SortedSet, typeof(SortedSet<>)),
        new CollectionTypeInfo(CollectionType.ISet, typeof(ISet<>)),
        new CollectionTypeInfo(CollectionType.IReadOnlySet, TypeFullName: "System.Collections.Generic.IReadOnlySet`1"),
        new CollectionTypeInfo(CollectionType.IDictionary, typeof(IDictionary<,>)),
        new CollectionTypeInfo(CollectionType.IReadOnlyDictionary, typeof(IReadOnlyDictionary<,>)),
        new CollectionTypeInfo(CollectionType.Dictionary, typeof(Dictionary<,>)),
        new CollectionTypeInfo(CollectionType.ImmutableArray, typeof(ImmutableArray<>), Immutable: true),
        new CollectionTypeInfo(CollectionType.ImmutableList, typeof(ImmutableList<>), Immutable: true),
        new CollectionTypeInfo(CollectionType.IImmutableList, typeof(IImmutableList<>), Immutable: true),
        new CollectionTypeInfo(CollectionType.ImmutableHashSet, typeof(ImmutableHashSet<>), Immutable: true),
        new CollectionTypeInfo(CollectionType.IImmutableSet, typeof(IImmutableSet<>), Immutable: true),
        new CollectionTypeInfo(CollectionType.ImmutableSortedSet, typeof(ImmutableSortedSet<>), Immutable: true),
        new CollectionTypeInfo(CollectionType.ImmutableQueue, typeof(ImmutableQueue<>), Immutable: true),
        new CollectionTypeInfo(CollectionType.IImmutableQueue, typeof(IImmutableQueue<>), Immutable: true),
        new CollectionTypeInfo(CollectionType.IImmutableQueue, typeof(IImmutableQueue<>), Immutable: true),
        new CollectionTypeInfo(CollectionType.ImmutableStack, typeof(ImmutableStack<>), Immutable: true),
        new CollectionTypeInfo(CollectionType.IImmutableStack, typeof(IImmutableStack<>), Immutable: true),
        new CollectionTypeInfo(CollectionType.IImmutableDictionary, typeof(IImmutableDictionary<,>), Immutable: true),
        new CollectionTypeInfo(CollectionType.ImmutableDictionary, typeof(ImmutableDictionary<,>), Immutable: true),
        new CollectionTypeInfo(CollectionType.Span, typeof(Span<>)),
        new CollectionTypeInfo(CollectionType.ReadOnlySpan, typeof(ReadOnlySpan<>)),
        new CollectionTypeInfo(CollectionType.Memory, typeof(Memory<>)),
        new CollectionTypeInfo(CollectionType.ReadOnlyMemory, typeof(ReadOnlyMemory<>)),
    };

    public static CollectionInfos? Build(WellKnownTypes wellKnownTypes, ITypeSymbol source, ITypeSymbol target)
    {
        if (Build(wellKnownTypes, source) is not { } sourceInfo)
            return null;

        if (Build(wellKnownTypes, target) is not { } targetInfo)
            return null;

        return new CollectionInfos(sourceInfo, targetInfo);
    }

    private static CollectionInfo? Build(WellKnownTypes wellKnownTypes, ITypeSymbol type)
    {
        var enumeratedType = GetEnumeratedType(wellKnownTypes, type);
        if (enumeratedType == null)
            return null;

        var collectionTypeInfo = GetCollectionTypeInfo(wellKnownTypes, type);
        return new CollectionInfo(
            type,
            collectionTypeInfo?.CollectionType ?? CollectionType.None,
            GetImplementedCollectionTypes(wellKnownTypes, type),
            enumeratedType,
            FindCountProperty(wellKnownTypes, type),
            HasValidAddMethod(wellKnownTypes, type),
            collectionTypeInfo?.Immutable == true
        );
    }

    private static ITypeSymbol? GetEnumeratedType(WellKnownTypes types, ITypeSymbol type)
    {
        if (type.ImplementsGeneric(types.Get(typeof(IEnumerable<>)), out var enumerableIntf))
            return enumerableIntf.TypeArguments[0];

        // if type is not readonly struct with one type argument then return null
        if (type is not ({ IsValueType: true, IsReadOnly: true } and INamedTypeSymbol { TypeArguments.Length: 1 } namedType))
            return null;

        // if the collection is Span<> or Memory<> etc, get the type symbol
        if (
            SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, types.Get(typeof(Span<>)))
            || SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, types.Get(typeof(ReadOnlySpan<>)))
            || SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, types.Get(typeof(Memory<>)))
            || SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, types.Get(typeof(ReadOnlyMemory<>)))
        )
        {
            return namedType.TypeArguments[0];
        }

        return null;
    }

    private static bool HasValidAddMethod(WellKnownTypes types, ITypeSymbol t)
    {
        return t.HasImplicitGenericImplementation(types.Get(typeof(ICollection<>)), nameof(ICollection<object>.Add))
            || t.HasImplicitGenericImplementation(types.Get(typeof(ISet<>)), nameof(ISet<object>.Add));
    }

    private static string? FindCountProperty(WellKnownTypes types, ITypeSymbol t)
    {
        var intType = types.Get<int>();
        var member = t.GetAccessibleMappableMembers()
            .FirstOrDefault(
                x =>
                    x.Name is nameof(ICollection<object>.Count) or nameof(Array.Length)
                    && SymbolEqualityComparer.IncludeNullability.Equals(intType, x.Type)
            );
        return member?.Name;
    }

    private static CollectionTypeInfo? GetCollectionTypeInfo(WellKnownTypes types, ITypeSymbol type)
    {
        if (type.IsArrayType())
            return _collectionTypeInfoArray;

        foreach (var typeInfo in _collectionTypeInfos)
        {
            if (typeInfo.GetTypeSymbol(types) is not { } typeSymbol)
                continue;

            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, typeSymbol))
                return typeInfo;
        }

        return null;
    }

    private static CollectionType GetImplementedCollectionTypes(WellKnownTypes types, ITypeSymbol type)
    {
        var implementedCollectionTypes = type.IsArrayType() ? CollectionType.Array : CollectionType.None;

        foreach (var typeInfo in _collectionTypeInfos)
        {
            if (typeInfo.GetTypeSymbol(types) is not { } typeSymbol)
                continue;

            if (type.ImplementsGeneric(typeSymbol, out _))
            {
                implementedCollectionTypes |= typeInfo.CollectionType;
            }
        }

        return implementedCollectionTypes;
    }
}
