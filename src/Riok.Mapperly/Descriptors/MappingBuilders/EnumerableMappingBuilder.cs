using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class EnumerableMappingBuilder
{
    private const string SelectMethodName = "global::System.Linq.Enumerable.Select";
    private const string ToArrayMethodName = "global::System.Linq.Enumerable.ToArray";
    private const string ToListMethodName = "global::System.Linq.Enumerable.ToList";
    private const string ToHashSetMethodName = "ToHashSet";

    private const string ToImmutableArrayMethodName = "global::System.Collections.Immutable.ImmutableArray.ToImmutableArray";
    private const string ToImmutableListMethodName = "global::System.Collections.Immutable.ImmutableList.ToImmutableList";
    private const string ToImmutableHashSetMethodName = "global::System.Collections.Immutable.ImmutableHashSet.ToImmutableHashSet";
    private const string CreateRangeQueueMethodName = "global::System.Collections.Immutable.ImmutableQueue.CreateRange";
    private const string CreateRangeStackMethodName = "global::System.Collections.Immutable.ImmutableStack.CreateRange";
    private const string ToImmutableSortedSetMethodName = "global::System.Collections.Immutable.ImmutableSortedSet.ToImmutableSortedSet";

    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Enumerable))
            return null;

        if (ctx.CollectionInfos == null)
            return null;

        if (!ctx.CollectionInfos.Source.ImplementsIEnumerable || !ctx.CollectionInfos.Target.ImplementsIEnumerable)
            return null;

        // When mapping collection elements, check if there's a user mapping for the non-nullable element types.
        // If a user mapping exists, use FindOrBuildLooseNullableMapping to preserve user configurations
        // (like MapperIgnoreSource attributes) and prevent false positive RMG020 diagnostics.
        var sourceType = ctx.CollectionInfos.Source.EnumeratedType;
        var targetType = ctx.CollectionInfos.Target.EnumeratedType;
        var nonNullableSourceType = sourceType.NonNullable();
        var nonNullableTargetType = targetType.NonNullable();

        var existingMapping = ctx.FindMapping(nonNullableSourceType, nonNullableTargetType);
        var hasUserMappingForNonNullable = existingMapping is IUserMapping;

        var elementMapping = hasUserMappingForNonNullable
            ? ctx.FindOrBuildLooseNullableMapping(new TypeMappingKey(sourceType, targetType))
            : ctx.FindOrBuildMapping(sourceType, targetType);
        if (elementMapping == null)
            return null;

        if (TryBuildCastMapping(ctx, elementMapping) is { } castMapping)
            return castMapping;

        if (TryBuildFastConversion(ctx, elementMapping) is { } fastLoopMapping)
            return fastLoopMapping;

        // try linq mapping: x.Select(Map).ToArray/ToList
        // if that doesn't work do a foreach with add calls
        var (canMapWithLinq, collectMethodName) = ResolveCollectMethodName(ctx);
        if (canMapWithLinq)
            return BuildLinqMapping(ctx, elementMapping, collectMethodName);

        // try linq mapping: x.Select(Map).ToImmutableArray/ToImmutableList
        // if that doesn't work do a foreach with add calls
        var immutableLinqMapping = TryBuildImmutableLinqMapping(ctx, elementMapping);
        if (immutableLinqMapping is not null)
            return immutableLinqMapping;

        // if target is a type that takes IEnumerable in its constructor
        if (GetTypeConstructableFromEnumerable(ctx, elementMapping.TargetType) is { } constructableType)
            return BuildLinqConstructorMapping(ctx, constructableType, elementMapping);

        return ctx.IsExpression ? null : BuildCustomTypeMapping(ctx, elementMapping);
    }

    public static IExistingTargetMapping? TryBuildExistingTargetMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Enumerable))
            return null;

        if (ctx.CollectionInfos == null)
            return null;

        if (!ctx.CollectionInfos.Source.ImplementsIEnumerable || !ctx.CollectionInfos.Target.ImplementsIEnumerable)
            return null;

        if (ctx.CollectionInfos.Target.IsImmutableCollectionType || ctx.CollectionInfos.Target.IsArray)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyType, ctx.Target);
            return new NoOpMapping(ctx.Source, ctx.Target);
        }

        var elementMapping = ctx.FindOrBuildMapping(ctx.CollectionInfos.Source.EnumeratedType, ctx.CollectionInfos.Target.EnumeratedType);
        if (elementMapping == null)
            return null;

        var addMethodName = ctx.CollectionInfos.Target.AddMethodName;
        if (addMethodName != null)
        {
            return new ForEachAddEnumerableExistingTargetMapping(ctx.CollectionInfos, elementMapping, addMethodName);
        }

        return null;
    }

    private static NewInstanceMapping? TryBuildCastMapping(MappingBuilderContext ctx, ITypeMapping elementMapping)
    {
        // cannot cast if the method mapping is synthetic, deep clone is enabled or target is an unknown collection
        if (
            !elementMapping.IsSynthetic
            || ctx.Configuration.UseDeepCloning
            || ctx.CollectionInfos!.Target.CollectionType == CollectionType.None
        )
        {
            return null;
        }

        // manually check if source is an Array as it implements IList and ICollection at runtime, see https://stackoverflow.com/q/47361775/3302887
        if (
            ctx.CollectionInfos.Source.IsArray
            && ctx.CollectionInfos.Target.CollectionType is CollectionType.ICollection or CollectionType.IList
        )
        {
            return null;
        }

        // if not an array check if source implements the target type
        if (ctx.CollectionInfos.Source.ImplementedTypes.HasFlag(ctx.CollectionInfos.Target.CollectionType))
        {
            return new CastMapping(ctx.Source, ctx.Target);
        }

        return null;
    }

    /// <summary>
    /// Tries to build a faster conversion when the source count is known and not in an expression.
    /// </summary>
    private static INewInstanceMapping? TryBuildFastConversion(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        if (
            ctx.IsExpression
            || !ctx.CollectionInfos!.Source.CountIsKnown
            || ctx.CollectionInfos.Target.CollectionType == CollectionType.None
        )
        {
            return null;
        }

        // if target is a list or type implemented by list
        if (ctx.CollectionInfos.Target.CollectionType is CollectionType.List or CollectionType.ICollection or CollectionType.IList)
        {
            return BuildEnumerableToListMapping(ctx, elementMapping);
        }

        // if target is not an array or a type implemented by array return early
        if (
            ctx.CollectionInfos.Target.CollectionType
            is not (
                CollectionType.Array
                or CollectionType.IReadOnlyCollection
                or CollectionType.IReadOnlyList
                or CollectionType.IEnumerable
            )
        )
        {
            return null;
        }

        // if source is not an array use a foreach mapping
        // if source is an array and target is an array, IEnumerable, IReadOnlyCollection faster mappings can be applied
        return ctx.CollectionInfos.Source.CollectionType != CollectionType.Array
            ? BuildEnumerableToArrayMapping(ctx, elementMapping)
            : BuildArrayToArrayMapping(ctx, elementMapping);
    }

    /// <summary>
    /// Tries to build a mapping for a source for which the count is known and
    /// a target type assignable form a list.
    /// </summary>
    private static INewInstanceMapping? BuildEnumerableToListMapping(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        // if mapping is synthetic then ToList is probably faster
        if (elementMapping.IsSynthetic)
            return null;

        // try to reuse a IEnumerable<S> => List<T> mapping
        var collectionInfos = new CollectionInfos(
            BuildCollectionTypeForICollection(ctx, ctx.CollectionInfos!.Source),
            CollectionInfoBuilder.BuildGenericCollectionInfo(ctx, CollectionType.List, ctx.CollectionInfos.Target)
        );
        var existingMapping = ctx.BuildDelegatedMapping(collectionInfos.Source.Type, collectionInfos.Target.Type);
        if (existingMapping != null)
            return new DelegateMapping(ctx.Source, ctx.Target, existingMapping);

        return new ForEachAddEnumerableMapping(
            null,
            collectionInfos,
            elementMapping,
            ctx.Configuration.Mapper.UseReferenceHandling,
            collectionInfos.Target.AddMethodName!
        );
    }

    /// <summary>
    /// Builds a mapping from an array to a target which is assignable from an array (e.g. array, <see cref="IReadOnlyCollection{T}"/>, ...).
    /// </summary>
    private static INewInstanceMapping BuildArrayToArrayMapping(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        // if element mapping is synthetic
        // a single Array.Clone / cast mapping call should be sufficient and fast,
        // use a for loop mapping otherwise.
        if (elementMapping.IsSynthetic)
        {
            return ctx.Configuration.UseDeepCloning
                ? new ArrayCloneMapping(ctx.Source, ctx.Target)
                : new CastMapping(ctx.Source, ctx.Target);
        }

        // ensure the target is an array and not an interface
        // => mapping can be reused by a delegate mapping for different implementations
        var targetType = ctx.Target.IsArrayType() ? ctx.Target : ctx.Types.GetArrayType(elementMapping.TargetType);
        var delegatedMapping = ctx.BuildDelegatedMapping(ctx.Source, targetType);
        if (delegatedMapping != null)
            return delegatedMapping;

        return new ArrayForMapping(ctx.Source, targetType, elementMapping, elementMapping.TargetType);
    }

    /// <summary>
    /// Builds a mapping from an <see cref="IEnumerable{T}"/> with a known count to a target which is assignable from an array (e.g. array, <see cref="IReadOnlyCollection{T}"/>, ...).
    /// </summary>
    private static INewInstanceMapping? BuildEnumerableToArrayMapping(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        // if mapping is synthetic then ToArray is probably faster
        if (elementMapping.IsSynthetic)
            return null;

        // ensure the source is IEnumerable<S>
        // and ensure the target is an array and not an interface
        // => mapping can be reused by a delegate mapping for different implementations
        var sourceCollectionInfo = BuildCollectionTypeForICollection(ctx, ctx.CollectionInfos!.Source);
        var targetType = ctx.Target.IsArrayType() ? ctx.Target : ctx.Types.GetArrayType(elementMapping.TargetType);
        var delegatedMapping = ctx.BuildDelegatedMapping(sourceCollectionInfo.Type, targetType);
        if (delegatedMapping != null)
            return delegatedMapping;

        return new ArrayForEachMapping(
            sourceCollectionInfo.Type,
            targetType,
            elementMapping,
            elementMapping.TargetType,
            sourceCollectionInfo.CountMember!.BuildGetter(ctx.UnsafeAccessorContext)
        );
    }

    private static LinqEnumerableMapping BuildLinqMapping(
        MappingBuilderContext ctx,
        INewInstanceMapping elementMapping,
        string? collectMethod
    )
    {
        var selectMethod = elementMapping.IsSynthetic ? null : SelectMethodName;
        return new LinqEnumerableMapping(ctx.Source, ctx.Target, elementMapping, selectMethod, collectMethod);
    }

    private static INamedTypeSymbol? GetTypeConstructableFromEnumerable(MappingBuilderContext ctx, ITypeSymbol typeSymbol)
    {
        if (ctx.Target is not INamedTypeSymbol namedType)
            return null;

        var typedEnumerable = ctx.Types.Get(typeof(IEnumerable<>)).Construct(typeSymbol);
        var hasCtor = namedType.Constructors.Any(m =>
            m.Parameters.Length == 1 && SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, typedEnumerable)
        );
        if (hasCtor)
            return namedType;

        if (ctx.CollectionInfos!.Target.CollectionType is CollectionType.ISet or CollectionType.IReadOnlySet)
            return CollectionInfoBuilder.BuildGenericCollectionType(ctx, CollectionType.HashSet, typeSymbol);

        return null;
    }

    private static LinqConstructorMapping BuildLinqConstructorMapping(
        MappingBuilderContext ctx,
        INamedTypeSymbol targetTypeToConstruct,
        INewInstanceMapping elementMapping
    )
    {
        var selectMethod = elementMapping.IsSynthetic ? null : SelectMethodName;
        return new LinqConstructorMapping(ctx.Source, targetTypeToConstruct, elementMapping, selectMethod);
    }

    private static INewInstanceMapping? BuildCustomTypeMapping(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        // create a foreach loop with add calls if source is not an array
        // and has an implicit .Add() method
        // the implicit check is an easy way to exclude for example immutable types.
        if (ctx.CollectionInfos?.Target.AddMethodName == null)
            return null;

        // try to reuse an existing mapping
        var collectionInfos = ctx.CollectionInfos;
        if (!ctx.InstanceConstructors.TryBuildObjectFactory(ctx.Source, ctx.Target, out var constructor))
        {
            collectionInfos = collectionInfos with { Source = BuildCollectionTypeForICollection(ctx, collectionInfos.Source) };
            var existingMapping = ctx.BuildDelegatedMapping(collectionInfos.Source.Type, ctx.Target);
            if (existingMapping != null)
                return existingMapping;

            ctx.InstanceConstructors.TryBuildObjectFactory(collectionInfos.Source.Type, ctx.Target, out constructor);
        }

        return new ForEachAddEnumerableMapping(
            constructor,
            collectionInfos,
            elementMapping,
            ctx.Configuration.Mapper.UseReferenceHandling,
            collectionInfos.Target.AddMethodName
        );
    }

    private static (bool CanMapWithLinq, string? CollectMethod) ResolveCollectMethodName(MappingBuilderContext ctx)
    {
        // if the target is an array we need to collect to array
        if (ctx.Target.IsArrayType())
            return (true, ToArrayMethodName);

        // if the target is an IEnumerable<T> don't collect at all
        // except deep cloning is enabled.
        var targetIsIEnumerable = ctx.CollectionInfos!.Target.CollectionType == CollectionType.IEnumerable;
        if (targetIsIEnumerable && !ctx.Configuration.UseDeepCloning)
            return (true, null);

        // if the target is IReadOnlyCollection<T> or IEnumerable<T>
        // and the count of the source is known (array, IReadOnlyCollection<T>, ICollection<T>) we collect to array
        // for performance/space reasons
        var targetIsReadOnlyCollection = ctx.CollectionInfos.Target.CollectionType == CollectionType.IReadOnlyCollection;
        if ((targetIsReadOnlyCollection || targetIsIEnumerable) && ctx.CollectionInfos.Source.CountIsKnown)
            return (true, ToArrayMethodName);

        // if target is Set
        // and ToHashSet is supported (only supported for .NET5+)
        // use ToHashSet
        if (
            ctx.CollectionInfos.Target.CollectionType is CollectionType.ISet or CollectionType.IReadOnlySet or CollectionType.HashSet
            && GetToHashSetLinqCollectMethod(ctx.Types) is { } toHashSetMethod
        )
        {
            return (true, SyntaxFactoryHelper.StaticMethodString(toHashSetMethod));
        }

        // if target is a IReadOnlyCollection<T>, IEnumerable<T>, IList<T>, List<T> or ICollection<T> with ToList()
        return
            targetIsReadOnlyCollection
            || targetIsIEnumerable
            || ctx.CollectionInfos.Target.CollectionType
                is CollectionType.IReadOnlyList
                    or CollectionType.IList
                    or CollectionType.List
                    or CollectionType.ICollection
            ? (true, ToListMethodName)
            : (false, null);
    }

    private static LinqEnumerableMapping? TryBuildImmutableLinqMapping(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        var collectMethod = ResolveImmutableCollectMethod(ctx);
        if (collectMethod is null)
            return null;

        var selectMethod = elementMapping.IsSynthetic ? null : SelectMethodName;
        return new LinqEnumerableMapping(ctx.Source, ctx.Target, elementMapping, selectMethod, collectMethod);
    }

    private static string? ResolveImmutableCollectMethod(MappingBuilderContext ctx)
    {
        return ctx.CollectionInfos!.Target.CollectionType switch
        {
            CollectionType.ImmutableArray => ToImmutableArrayMethodName,
            CollectionType.ImmutableList or CollectionType.IImmutableList => ToImmutableListMethodName,
            CollectionType.ImmutableHashSet or CollectionType.IImmutableSet => ToImmutableHashSetMethodName,
            CollectionType.ImmutableQueue or CollectionType.IImmutableQueue => CreateRangeQueueMethodName,
            CollectionType.ImmutableStack or CollectionType.IImmutableStack => CreateRangeStackMethodName,
            CollectionType.ImmutableSortedSet => ToImmutableSortedSetMethodName,
            _ => null,
        };
    }

    private static IMethodSymbol? GetToHashSetLinqCollectMethod(WellKnownTypes wellKnownTypes) =>
        wellKnownTypes.Get(typeof(Enumerable)).GetStaticGenericMethod(ToHashSetMethodName);

    private static CollectionInfo BuildCollectionTypeForICollection(MappingBuilderContext ctx, CollectionInfo info)
    {
        // the types cannot be changed for mappings with a user symbol
        // as the types are defined by the user
        if (ctx.HasUserSymbol)
            return info;

        CollectionType? collectionType =
            info.ImplementedTypes.HasFlag(CollectionType.IReadOnlyCollection) ? CollectionType.IReadOnlyCollection
            : info.ImplementedTypes.HasFlag(CollectionType.ICollection) ? CollectionType.ICollection
            : null;

        return collectionType == null ? info : CollectionInfoBuilder.BuildGenericCollectionInfo(ctx, collectionType.Value, info);
    }
}
