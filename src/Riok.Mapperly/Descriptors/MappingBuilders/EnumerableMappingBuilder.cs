using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
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
    private const string AddMethodName = nameof(ICollection<object>.Add);

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

        var elementMapping = ctx.FindOrBuildMapping(ctx.CollectionInfos.Source.EnumeratedType, ctx.CollectionInfos.Target.EnumeratedType);
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

        var elementMapping = ctx.FindOrBuildMapping(ctx.CollectionInfos.Source.EnumeratedType, ctx.CollectionInfos.Target.EnumeratedType);
        if (elementMapping == null)
            return null;

        if (ctx.CollectionInfos.Target.CollectionType == CollectionType.Stack)
            return CreateForEach(nameof(Stack<object>.Push));

        if (ctx.CollectionInfos.Target.CollectionType == CollectionType.Queue)
            return CreateForEach(nameof(Queue<object>.Enqueue));

        // create a foreach loop with add calls if source is not an array
        // and has an implicit .Add() method
        // the implicit check is an easy way to exclude for example immutable types.
        if (ctx.CollectionInfos.Target.CollectionType != CollectionType.Array && ctx.CollectionInfos.Target.HasImplicitCollectionAddMethod)
            return CreateForEach(AddMethodName);

        if (ctx.CollectionInfos.Target.IsImmutableCollectionType)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember);
        }

        return null;

        ForEachAddEnumerableExistingTargetMapping CreateForEach(string methodName)
        {
            var ensureCapacityStatement = EnsureCapacityBuilder.TryBuildEnsureCapacity(ctx);
            return new ForEachAddEnumerableExistingTargetMapping(
                ctx.Source,
                ctx.Target,
                elementMapping,
                methodName,
                ensureCapacityStatement
            );
        }
    }

    private static NewInstanceMapping? TryBuildCastMapping(MappingBuilderContext ctx, ITypeMapping elementMapping)
    {
        // cannot cast if the method mapping is synthetic, deep clone is enabled or target is an unknown collection
        if (
            !elementMapping.IsSynthetic
            || ctx.MapperConfiguration.UseDeepCloning
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
        var sourceType = BuildCollectionTypeForICollection(ctx, ctx.CollectionInfos!.Source);
        var targetType = BuildCollectionType(ctx, CollectionType.List, ctx.CollectionInfos.Target.EnumeratedType);
        var existingMapping = ctx.BuildDelegatedMapping(sourceType, targetType);
        if (existingMapping != null)
            return new DelegateMapping(ctx.Source, ctx.Target, existingMapping);

        return new ForEachAddEnumerableMapping(
            sourceType,
            targetType,
            elementMapping,
            AddMethodName,
            ctx.CollectionInfos!.Source.CountPropertyName
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
            return ctx.MapperConfiguration.UseDeepCloning
                ? new ArrayCloneMapping(ctx.Source, ctx.Target)
                : new CastMapping(ctx.Source, ctx.Target);
        }

        // ensure the target is an array and not an interface
        // => mapping can be reused by a delegate mapping for different implementations
        var targetType = ctx.Target.IsArrayType() ? ctx.Target : ctx.Types.GetArrayType(ctx.CollectionInfos!.Target.EnumeratedType);
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
        var sourceType = BuildCollectionTypeForICollection(ctx, ctx.CollectionInfos!.Source);
        var targetType = ctx.Target.IsArrayType() ? ctx.Target : ctx.Types.GetArrayType(ctx.CollectionInfos!.Target.EnumeratedType);
        var delegatedMapping = ctx.BuildDelegatedMapping(sourceType, targetType);
        if (delegatedMapping != null)
            return delegatedMapping;

        return new ArrayForEachMapping(
            sourceType,
            targetType,
            elementMapping,
            elementMapping.TargetType,
            ctx.CollectionInfos!.Source.CountPropertyName!
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
            return BuildCollectionType(ctx, CollectionType.HashSet, typeSymbol);

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
        var hasObjectFactory = ctx.ObjectFactories.TryFindObjectFactory(ctx.Source, ctx.Target, out var objectFactory);
        if (!hasObjectFactory && !ctx.SymbolAccessor.HasDirectlyAccessibleParameterlessConstructor(ctx.Target))
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, ctx.Target);
            return null;
        }

        // create a foreach loop with add calls if source is not an array
        // and has an implicit .Add() method
        // the implicit check is an easy way to exclude for example immutable types.
        if (
            ctx.CollectionInfos!.Target.CollectionType == CollectionType.Array
            || !ctx.CollectionInfos.Target.HasImplicitCollectionAddMethod
        )
        {
            return null;
        }

        // try to reuse an existing mapping
        var sourceType = ctx.Source;
        if (!hasObjectFactory)
        {
            sourceType = BuildCollectionTypeForICollection(ctx, ctx.CollectionInfos.Source);
            ctx.ObjectFactories.TryFindObjectFactory(sourceType, ctx.Target, out objectFactory);
            var existingMapping = ctx.BuildDelegatedMapping(sourceType, ctx.Target);
            if (existingMapping != null)
                return existingMapping;
        }

        var ensureCapacityStatement = EnsureCapacityBuilder.TryBuildEnsureCapacity(ctx);
        return new ForEachAddEnumerableMapping(
            sourceType,
            ctx.Target,
            elementMapping,
            objectFactory,
            AddMethodName,
            ensureCapacityStatement
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
        if (targetIsIEnumerable && !ctx.MapperConfiguration.UseDeepCloning)
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
            _ => null
        };
    }

    private static IMethodSymbol? GetToHashSetLinqCollectMethod(WellKnownTypes wellKnownTypes) =>
        wellKnownTypes.Get(typeof(Enumerable)).GetStaticGenericMethod(ToHashSetMethodName);

    private static ITypeSymbol BuildCollectionTypeForICollection(MappingBuilderContext ctx, CollectionInfo info)
    {
        return info.ImplementedTypes.HasFlag(CollectionType.IReadOnlyCollection)
            ? BuildCollectionType(ctx, CollectionType.IReadOnlyCollection, info.EnumeratedType)
            : info.ImplementedTypes.HasFlag(CollectionType.ICollection)
                ? BuildCollectionType(ctx, CollectionType.ICollection, info.EnumeratedType)
                : info.Type;
    }

    private static INamedTypeSymbol BuildCollectionType(MappingBuilderContext ctx, CollectionType type, ITypeSymbol enumeratedType)
    {
        var genericType = CollectionInfoBuilder.GetGenericClrCollectionType(type);
        return (INamedTypeSymbol)
            ctx.Types.Get(genericType).Construct(enumeratedType).WithNullableAnnotation(NullableAnnotation.NotAnnotated);
    }
}
