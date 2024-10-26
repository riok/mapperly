using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Enumerables;

public static class CollectionInfoBuilder
{
    private readonly record struct CollectionTypeInfo(
        CollectionType CollectionType,
        Type? ReflectionType = null,
        string? AddMethodName = null,
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

    // types which may not be available on the target compiler platform
    // are referenced via their full name instead of the reflection type
    private static readonly IReadOnlyCollection<CollectionTypeInfo> _collectionTypeInfos =
    [
        new CollectionTypeInfo(CollectionType.IEnumerable, typeof(IEnumerable<>)),
        new CollectionTypeInfo(CollectionType.List, typeof(List<>), nameof(List<object>.Add)),
        new CollectionTypeInfo(CollectionType.Stack, typeof(Stack<>), nameof(Stack<object>.Push)),
        new CollectionTypeInfo(CollectionType.Queue, typeof(Queue<>), nameof(Queue<object>.Enqueue)),
        new CollectionTypeInfo(CollectionType.IReadOnlyCollection, typeof(IReadOnlyCollection<>)),
        new CollectionTypeInfo(CollectionType.IList, typeof(IList<>), nameof(IList<object>.Add)),
        new CollectionTypeInfo(CollectionType.IReadOnlyList, typeof(IReadOnlyList<>)),
        new CollectionTypeInfo(CollectionType.ICollection, typeof(ICollection<>), nameof(ICollection<object>.Add)),
        new CollectionTypeInfo(CollectionType.HashSet, typeof(HashSet<>), nameof(HashSet<object>.Add)),
        new CollectionTypeInfo(CollectionType.SortedSet, typeof(SortedSet<>), nameof(SortedSet<object>.Add)),
        new CollectionTypeInfo(CollectionType.ISet, typeof(ISet<>), nameof(ISet<object>.Add)),
        new CollectionTypeInfo(CollectionType.IReadOnlySet, TypeFullName: "System.Collections.Generic.IReadOnlySet`1"),
        new CollectionTypeInfo(CollectionType.IDictionary, typeof(IDictionary<,>)),
        new CollectionTypeInfo(CollectionType.IReadOnlyDictionary, typeof(IReadOnlyDictionary<,>)),
        new CollectionTypeInfo(CollectionType.Dictionary, typeof(Dictionary<,>)),
        new CollectionTypeInfo(
            CollectionType.ImmutableArray,
            TypeFullName: "System.Collections.Immutable.ImmutableArray`1",
            Immutable: true
        ),
        new CollectionTypeInfo(CollectionType.ImmutableList, TypeFullName: "System.Collections.Immutable.ImmutableList`1", Immutable: true),
        new CollectionTypeInfo(
            CollectionType.IImmutableList,
            TypeFullName: "System.Collections.Immutable.IImmutableList`1",
            Immutable: true
        ),
        new CollectionTypeInfo(
            CollectionType.ImmutableHashSet,
            TypeFullName: "System.Collections.Immutable.ImmutableHashSet`1",
            Immutable: true
        ),
        new CollectionTypeInfo(CollectionType.IImmutableSet, TypeFullName: "System.Collections.Immutable.IImmutableSet`1", Immutable: true),
        new CollectionTypeInfo(
            CollectionType.ImmutableSortedSet,
            TypeFullName: "System.Collections.Immutable.ImmutableSortedSet`1",
            Immutable: true
        ),
        new CollectionTypeInfo(
            CollectionType.ImmutableQueue,
            TypeFullName: "System.Collections.Immutable.ImmutableQueue`1",
            Immutable: true
        ),
        new CollectionTypeInfo(
            CollectionType.IImmutableQueue,
            TypeFullName: "System.Collections.Immutable.IImmutableQueue`1",
            Immutable: true
        ),
        new CollectionTypeInfo(
            CollectionType.ImmutableStack,
            TypeFullName: "System.Collections.Immutable.ImmutableStack`1",
            Immutable: true
        ),
        new CollectionTypeInfo(
            CollectionType.IImmutableStack,
            TypeFullName: "System.Collections.Immutable.IImmutableStack`1",
            Immutable: true
        ),
        new CollectionTypeInfo(
            CollectionType.IImmutableDictionary,
            TypeFullName: "System.Collections.Immutable.IImmutableDictionary`2",
            Immutable: true
        ),
        new CollectionTypeInfo(
            CollectionType.ImmutableDictionary,
            TypeFullName: "System.Collections.Immutable.ImmutableDictionary`2",
            Immutable: true
        ),
        new CollectionTypeInfo(
            CollectionType.ImmutableSortedDictionary,
            TypeFullName: "System.Collections.Immutable.ImmutableSortedDictionary`2",
            Immutable: true
        ),
        new CollectionTypeInfo(CollectionType.Span, TypeFullName: "System.Span`1"),
        new CollectionTypeInfo(CollectionType.ReadOnlySpan, TypeFullName: "System.ReadOnlySpan`1"),
        new CollectionTypeInfo(CollectionType.Memory, TypeFullName: "System.Memory`1"),
        new CollectionTypeInfo(CollectionType.ReadOnlyMemory, TypeFullName: "System.ReadOnlyMemory`1"),
    ];

    private static readonly IReadOnlyDictionary<CollectionType, Type> _collectionClrTypeByType = _collectionTypeInfos
        .Where(x => x.ReflectionType != null)
        .ToDictionary(x => x.CollectionType, x => x.ReflectionType!);

    public static CollectionInfos? Build(
        WellKnownTypes wellKnownTypes,
        SymbolAccessor symbolAccessor,
        ITypeSymbol source,
        ITypeSymbol target
    )
    {
        // check for enumerated type to quickly check that both are collection types
        var enumeratedSourceType = GetEnumeratedType(wellKnownTypes, source);
        if (enumeratedSourceType == null)
            return null;

        var enumeratedTargetType = GetEnumeratedType(wellKnownTypes, target);
        if (enumeratedTargetType == null)
            return null;

        var sourceInfo = BuildCollectionInfo(wellKnownTypes, symbolAccessor, source, enumeratedSourceType);
        var targetInfo = BuildCollectionInfo(wellKnownTypes, symbolAccessor, target, enumeratedTargetType);

        return new CollectionInfos(sourceInfo, targetInfo);
    }

    public static CollectionInfo BuildGenericCollectionInfo(
        SimpleMappingBuilderContext ctx,
        CollectionType collectionType,
        CollectionInfo info
    )
    {
        var type = BuildGenericCollectionType(ctx, collectionType, info.EnumeratedType);
        return BuildCollectionInfo(ctx.Types, ctx.SymbolAccessor, type, info.EnumeratedType);
    }

    public static CollectionInfo BuildGenericCollectionInfo(
        SimpleMappingBuilderContext ctx,
        CollectionType collectionType,
        DictionaryInfo info
    )
    {
        var type = BuildGenericCollectionType(ctx, collectionType, info.Key, info.Value);
        return BuildCollectionInfo(ctx.Types, ctx.SymbolAccessor, type, info.Collection.EnumeratedType);
    }

    public static INamedTypeSymbol BuildGenericCollectionType(
        SimpleMappingBuilderContext ctx,
        CollectionType collectionType,
        params ITypeSymbol[] typeArguments
    )
    {
        var genericType = GetGenericClrCollectionType(collectionType);
        return (INamedTypeSymbol)
            ctx.Types.Get(genericType).Construct(typeArguments).WithNullableAnnotation(NullableAnnotation.NotAnnotated);
    }

    private static CollectionInfo BuildCollectionInfo(
        WellKnownTypes wellKnownTypes,
        SymbolAccessor symbolAccessor,
        ITypeSymbol type,
        ITypeSymbol enumeratedType
    )
    {
        var collectionTypeInfo = GetCollectionTypeInfo(wellKnownTypes, type);
        var typeInfo = collectionTypeInfo?.CollectionType ?? CollectionType.None;
        var implementedTypes = GetImplementedCollectionTypes(wellKnownTypes, type, typeInfo);

        return new CollectionInfo(
            type,
            typeInfo,
            implementedTypes,
            symbolAccessor.UpgradeNullable(enumeratedType),
            FindCountMember(symbolAccessor, type, typeInfo),
            GetAddMethodName(wellKnownTypes, type, implementedTypes, collectionTypeInfo),
            collectionTypeInfo?.Immutable == true
        );
    }

    private static ITypeSymbol? GetEnumeratedType(WellKnownTypes types, ITypeSymbol type)
    {
        // if type is array return element type
        // otherwise using the IEnumerable element type can erase the null annotation for external types
        if (type.IsArrayType(out var arraySymbol))
            return arraySymbol.ElementType;

        if (type.ImplementsGeneric(types.Get(typeof(IEnumerable<>)), out var enumerableIntf))
            return enumerableIntf.TypeArguments[0];

        // if type is not readonly struct with one type argument then return null
        if (type is not ({ IsValueType: true, IsReadOnly: true } and INamedTypeSymbol { TypeArguments.Length: 1 } namedType))
            return null;

        // if the collection is a ref type the check for Span<> or ReadOnlySpanSpan<>
        if (
            namedType.IsRefLikeType
            && (
                SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, types.Get(typeof(Span<>)))
                || SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, types.Get(typeof(ReadOnlySpan<>)))
            )
        )
        {
            return namedType.TypeArguments[0];
        }

        // Memory<> or ReadOnlyMemory<> etc., get the type symbol
        if (
            SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, types.Get(typeof(Memory<>)))
            || SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, types.Get(typeof(ReadOnlyMemory<>)))
        )
        {
            return namedType.TypeArguments[0];
        }

        return null;
    }

    private static string? GetAddMethodName(
        WellKnownTypes types,
        ITypeSymbol t,
        CollectionType implementedTypes,
        CollectionTypeInfo? collectionTypeInfo
    )
    {
        if (collectionTypeInfo != null)
            return collectionTypeInfo.Value.AddMethodName;

        // has valid add if type implements ICollection and has implicit Add method
        if (
            implementedTypes.HasFlag(CollectionType.ICollection)
            && t.HasImplicitGenericImplementation(types.Get(typeof(ICollection<>)), nameof(ICollection<object>.Add))
        )
        {
            return nameof(ICollection<object>.Add);
        }

        // has valid add if type implements ISet and has implicit Add method
        if (
            implementedTypes.HasFlag(CollectionType.ISet)
            && t.HasImplicitGenericImplementation(types.Get(typeof(ISet<>)), nameof(ISet<object>.Add))
        )
        {
            return nameof(ISet<object>.Add);
        }

        return null;
    }

    private static IMappableMember? FindCountMember(SymbolAccessor symbolAccessor, ITypeSymbol t, CollectionType typeInfo)
    {
        if (typeInfo is CollectionType.IEnumerable)
            return null;

        if (
            typeInfo
            is CollectionType.Array
                or CollectionType.Span
                or CollectionType.ReadOnlySpan
                or CollectionType.Memory
                or CollectionType.ReadOnlyMemory
        )
        {
            return symbolAccessor.GetMappableMember(t, "Length");
        }

        if (typeInfo is not CollectionType.None)
            return symbolAccessor.GetMappableMember(t, "Count");

        var member = symbolAccessor.GetMappableMember(t, "Count") ?? symbolAccessor.GetMappableMember(t, "Length");
        return member?.Type.SpecialType == SpecialType.System_Int32 ? member : null;
    }

    private static CollectionTypeInfo? GetCollectionTypeInfo(WellKnownTypes types, ITypeSymbol type)
    {
        if (type.IsArrayType())
            return _collectionTypeInfoArray;

        // string is a collection but does implement IEnumerable, return early
        if (type.SpecialType == SpecialType.System_String)
            return null;

        foreach (var typeInfo in _collectionTypeInfos)
        {
            if (typeInfo.GetTypeSymbol(types) is not { } typeSymbol)
                continue;

            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, typeSymbol))
                return typeInfo;
        }

        return null;
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "Just one large switch, static data, no logic")]
    private static CollectionType GetImplementedCollectionTypes(WellKnownTypes types, ITypeSymbol type, CollectionType collectionType)
    {
        // if the collectionType is not CollectionType.None, return the known implemented types
        // this is done for performance reasons
        // when collectionType is None then manually check for implemented types with IterateImplementedTypes
        return collectionType switch
        {
            CollectionType.Array => CollectionType.Array
                | CollectionType.IList
                | CollectionType.IReadOnlyList
                | CollectionType.ICollection
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,
            CollectionType.IEnumerable => CollectionType.IEnumerable,
            CollectionType.List => CollectionType.List
                | CollectionType.IList
                | CollectionType.IReadOnlyList
                | CollectionType.ICollection
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,
            CollectionType.Stack => CollectionType.Stack | CollectionType.IReadOnlyCollection | CollectionType.IEnumerable,
            CollectionType.Queue => CollectionType.Queue | CollectionType.IReadOnlyCollection | CollectionType.IEnumerable,
            CollectionType.IReadOnlyCollection => CollectionType.IReadOnlyCollection | CollectionType.IEnumerable,
            CollectionType.IList => CollectionType.IList | CollectionType.ICollection | CollectionType.IEnumerable,
            CollectionType.IReadOnlyList => CollectionType.IReadOnlyList | CollectionType.IReadOnlyCollection | CollectionType.IEnumerable,
            CollectionType.ICollection => CollectionType.ICollection | CollectionType.IEnumerable,
            CollectionType.HashSet => CollectionType.HashSet
                | CollectionType.ISet
                | CollectionType.IReadOnlySet
                | CollectionType.ICollection
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,
            CollectionType.SortedSet => CollectionType.SortedSet
                | CollectionType.ISet
                | CollectionType.IReadOnlySet
                | CollectionType.ICollection
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,
            CollectionType.ISet => CollectionType.ISet | CollectionType.ICollection | CollectionType.IEnumerable,
            CollectionType.IReadOnlySet => CollectionType.IReadOnlySet | CollectionType.IReadOnlyCollection | CollectionType.IEnumerable,
            CollectionType.IDictionary => CollectionType.IDictionary | CollectionType.ICollection | CollectionType.IEnumerable,
            CollectionType.IReadOnlyDictionary => CollectionType.IReadOnlyDictionary
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,
            CollectionType.Dictionary => CollectionType.Dictionary
                | CollectionType.IDictionary
                | CollectionType.IReadOnlyDictionary
                | CollectionType.ICollection
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,

            CollectionType.ImmutableArray => CollectionType.ImmutableArray
                | CollectionType.IImmutableList
                | CollectionType.IList
                | CollectionType.IReadOnlyList
                | CollectionType.ICollection
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,
            CollectionType.ImmutableList => CollectionType.ImmutableList
                | CollectionType.IImmutableList
                | CollectionType.IList
                | CollectionType.IReadOnlyList
                | CollectionType.ICollection
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,
            CollectionType.IImmutableList => CollectionType.IImmutableList
                | CollectionType.IReadOnlyList
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,
            CollectionType.ImmutableHashSet => CollectionType.ImmutableHashSet
                | CollectionType.IImmutableSet
                | CollectionType.IReadOnlySet
                | CollectionType.ISet
                | CollectionType.ICollection
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,
            CollectionType.IImmutableSet => CollectionType.IImmutableSet | CollectionType.IReadOnlyCollection | CollectionType.IEnumerable,
            CollectionType.ImmutableSortedSet => CollectionType.ImmutableSortedSet
                | CollectionType.IImmutableSet
                | CollectionType.IList
                | CollectionType.IReadOnlyList
                | CollectionType.ISet
                | CollectionType.IReadOnlySet
                | CollectionType.ICollection
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,
            CollectionType.ImmutableQueue => CollectionType.ImmutableQueue | CollectionType.IImmutableQueue | CollectionType.IEnumerable,
            CollectionType.IImmutableQueue => CollectionType.IImmutableQueue | CollectionType.IEnumerable,
            CollectionType.ImmutableStack => CollectionType.ImmutableStack | CollectionType.IImmutableStack | CollectionType.IEnumerable,
            CollectionType.IImmutableStack => CollectionType.IImmutableStack | CollectionType.IEnumerable,
            CollectionType.ImmutableDictionary => CollectionType.ImmutableDictionary
                | CollectionType.IImmutableDictionary
                | CollectionType.IDictionary
                | CollectionType.IReadOnlyDictionary
                | CollectionType.ICollection
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,
            CollectionType.IImmutableDictionary => CollectionType.IImmutableDictionary
                | CollectionType.IReadOnlyDictionary
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,
            CollectionType.ImmutableSortedDictionary => CollectionType.ImmutableSortedDictionary
                | CollectionType.IImmutableDictionary
                | CollectionType.IReadOnlyDictionary
                | CollectionType.IReadOnlyCollection
                | CollectionType.IEnumerable,
            CollectionType.Span => CollectionType.Span,
            CollectionType.ReadOnlySpan => CollectionType.ReadOnlySpan,
            CollectionType.Memory => CollectionType.Memory,
            CollectionType.ReadOnlyMemory => CollectionType.ReadOnlyMemory,

            // check for if the type is a string, returning IEnumerable
            CollectionType.None when type.SpecialType == SpecialType.System_String => CollectionType.IEnumerable,

            // fallback for CollectionType.None, manually checking for known implemented types
            _ => IterateImplementedTypes(type, types),
        };

        static CollectionType IterateImplementedTypes(ITypeSymbol type, WellKnownTypes types)
        {
            var implementedCollectionTypes = type.IsArrayType() ? CollectionType.Array : CollectionType.None;

            foreach (var typeInfo in _collectionTypeInfos)
            {
                if (typeInfo.GetTypeSymbol(types) is not { } typeSymbol)
                    continue;

                if (type.ExtendsOrImplementsGeneric(typeSymbol, out _))
                {
                    implementedCollectionTypes |= typeInfo.CollectionType;
                }
            }

            return implementedCollectionTypes;
        }
    }

    private static Type GetGenericClrCollectionType(CollectionType type) =>
        _collectionClrTypeByType.GetValueOrDefault(type)
        ?? throw new InvalidOperationException("Could not get clr collection type for " + type);
}
