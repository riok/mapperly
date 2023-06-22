using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class EnumerableMappingBuilder
{
    private const string SelectMethodName = nameof(Enumerable.Select);
    private const string ToArrayMethodName = nameof(Enumerable.ToArray);
    private const string ToListMethodName = nameof(Enumerable.ToList);
    private const string ToHashSetMethodName = "ToHashSet";
    private const string AddMethodName = nameof(ICollection<object>.Add);

    private const string ToImmutableArrayMethodName = nameof(ImmutableArray.ToImmutableArray);
    private const string ToImmutableListMethodName = nameof(ImmutableList.ToImmutableList);
    private const string ToImmutableHashSetMethodName = nameof(ImmutableHashSet.ToImmutableHashSet);
    private const string CreateRangeQueueMethodName = nameof(ImmutableQueue.CreateRange);
    private const string CreateRangeStackMethodName = nameof(ImmutableStack.CreateRange);
    private const string ToImmutableSortedSetMethodName = nameof(ImmutableSortedSet.ToImmutableSortedSet);

    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Enumerable))
            return null;

        if (ctx.CollectionInfos == null)
            return null;

        var elementMapping = ctx.FindOrBuildMapping(
            ctx.CollectionInfos.Source.EnumeratedType,
            ctx.CollectionInfos.Target.EnumeratedType,
            ctx.Parameters
        );
        if (elementMapping == null)
            return null;

        // if source is an array and target is an array, IEnumerable, IReadOnlyCollection faster mappings can be applied
        if (
            !ctx.IsExpression
            && ctx.CollectionInfos.Source.Type == CollectionType.Array
            && ctx.CollectionInfos.Target.Type is CollectionType.Array or CollectionType.IReadOnlyCollection or CollectionType.IEnumerable
        )
        {
            // if element mapping is synthetic
            // a single Array.Clone / cast mapping call should be sufficient and fast,
            // use a for loop mapping otherwise.
            if (!elementMapping.IsSynthetic)
                return new ArrayForMapping(ctx.Source, ctx.Target, elementMapping, elementMapping.TargetType);

            return ctx.MapperConfiguration.UseDeepCloning
                ? new ArrayCloneMapping(ctx.Source, ctx.Target)
                : new CastMapping(ctx.Source, ctx.Target);
        }

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

        var elementMapping = ctx.FindOrBuildMapping(
            ctx.CollectionInfos.Source.EnumeratedType,
            ctx.CollectionInfos.Target.EnumeratedType,
            ctx.Parameters
        );
        if (elementMapping == null)
            return null;

        if (ctx.CollectionInfos.Target.Type == CollectionType.Stack)
            return CreateForEach(nameof(Stack<object>.Push));

        if (ctx.CollectionInfos.Target.Type == CollectionType.Queue)
            return CreateForEach(nameof(Queue<object>.Enqueue));

        // create a foreach loop with add calls if source is not an array
        // and has an implicit .Add() method
        // the implicit check is an easy way to exclude for example immutable types.
        if (ctx.CollectionInfos.Target.Type != CollectionType.Array && ctx.CollectionInfos.Target.HasImplicitCollectionAddMethod)
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

    private static LinqEnumerableMapping BuildLinqMapping(MappingBuilderContext ctx, ITypeMapping elementMapping, string? collectMethodName)
    {
        var collectMethod = collectMethodName == null ? null : ctx.Types.Get(typeof(Enumerable)).GetStaticGenericMethod(collectMethodName);
        var selectMethod = elementMapping.IsSynthetic ? null : ctx.Types.Get(typeof(Enumerable)).GetStaticGenericMethod(SelectMethodName);
        return new LinqEnumerableMapping(ctx.Source, ctx.Target, elementMapping, selectMethod, collectMethod);
    }

    private static INamedTypeSymbol? GetTypeConstructableFromEnumerable(MappingBuilderContext ctx, ITypeSymbol typeSymbol)
    {
        if (ctx.Target is not INamedTypeSymbol namedType)
            return null;

        var typedEnumerable = ctx.Types.Get(typeof(IEnumerable<>)).Construct(typeSymbol);
        var hasCtor = namedType.Constructors.Any(
            m => m.Parameters.Length == 1 && SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, typedEnumerable)
        );
        if (hasCtor)
            return namedType;

        if (ctx.CollectionInfos!.Target.Type is CollectionType.ISet or CollectionType.IReadOnlySet)
            return ctx.Types.Get(typeof(HashSet<>)).Construct(typeSymbol);

        return null;
    }

    private static LinqConstructorMapping BuildLinqConstructorMapping(
        MappingBuilderContext ctx,
        INamedTypeSymbol targetTypeToConstruct,
        ITypeMapping elementMapping
    )
    {
        var selectMethod = elementMapping.IsSynthetic ? null : ctx.Types.Get(typeof(Enumerable)).GetStaticGenericMethod(SelectMethodName);
        return new LinqConstructorMapping(ctx.Source, ctx.Target, targetTypeToConstruct, elementMapping, selectMethod);
    }

    private static ExistingTargetMappingMethodWrapper? BuildCustomTypeMapping(MappingBuilderContext ctx, ITypeMapping elementMapping)
    {
        if (
            !ctx.ObjectFactories.TryFindObjectFactory(ctx.Source, ctx.Target, out var objectFactory)
            && !ctx.Target.HasAccessibleParameterlessConstructor()
        )
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, ctx.Target);
            return null;
        }

        // create a foreach loop with add calls if source is not an array
        // and has an implicit .Add() method
        // the implicit check is an easy way to exclude for example immutable types.
        if (ctx.CollectionInfos!.Target.Type == CollectionType.Array || !ctx.CollectionInfos.Target.HasImplicitCollectionAddMethod)
            return null;

        var ensureCapacityStatement = EnsureCapacityBuilder.TryBuildEnsureCapacity(ctx);
        return new ForEachAddEnumerableMapping(
            ctx.Source,
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
        var targetIsIEnumerable = ctx.CollectionInfos!.Target.Type == CollectionType.IEnumerable;
        if (targetIsIEnumerable && !ctx.MapperConfiguration.UseDeepCloning)
            return (true, null);

        // if the target is IReadOnlyCollection<T> or IEnumerable<T>
        // and the count of the source is known (array, IReadOnlyCollection<T>, ICollection<T>) we collect to array
        // for performance/space reasons
        var targetIsReadOnlyCollection = ctx.CollectionInfos.Target.Type == CollectionType.IReadOnlyCollection;
        if ((targetIsReadOnlyCollection || targetIsIEnumerable) && ctx.CollectionInfos.Source.CountIsKnown)
            return (true, ToArrayMethodName);

        // if target is Set
        // and ToHashSet is supported (only supported for .NET5+)
        // use ToHashSet
        if (
            ctx.CollectionInfos.Target.Type is CollectionType.ISet or CollectionType.IReadOnlySet or CollectionType.HashSet
            && GetToHashSetLinqCollectMethod(ctx.Types) is { } toHashSetMethod
        )
        {
            return (true, toHashSetMethod.Name);
        }

        // if target is a IReadOnlyCollection<T>, IEnumerable<T>, IList<T>, List<T> or ICollection<T> with ToList()
        return
            targetIsReadOnlyCollection
            || targetIsIEnumerable
            || ctx.CollectionInfos.Target.Type
                is CollectionType.IReadOnlyList
                    or CollectionType.IList
                    or CollectionType.List
                    or CollectionType.ICollection
            ? (true, ToListMethodName)
            : (false, null);
    }

    private static LinqEnumerableMapping? TryBuildImmutableLinqMapping(MappingBuilderContext ctx, ITypeMapping elementMapping)
    {
        var collectMethod = ResolveImmutableCollectMethod(ctx);
        if (collectMethod is null)
            return null;

        var selectMethod = elementMapping.IsSynthetic ? null : ctx.Types.Get(typeof(Enumerable)).GetStaticGenericMethod(SelectMethodName);
        return new LinqEnumerableMapping(ctx.Source, ctx.Target, elementMapping, selectMethod, collectMethod);
    }

    private static IMethodSymbol? ResolveImmutableCollectMethod(MappingBuilderContext ctx)
    {
        if (ctx.CollectionInfos!.Target.Type == CollectionType.ImmutableArray)
            return ctx.Types.Get(typeof(ImmutableArray)).GetStaticGenericMethod(ToImmutableArrayMethodName);

        if (ctx.CollectionInfos.Target.Type is CollectionType.ImmutableList or CollectionType.IImmutableList)
            return ctx.Types.Get(typeof(ImmutableList)).GetStaticGenericMethod(ToImmutableListMethodName);

        if (ctx.CollectionInfos.Target.Type is CollectionType.ImmutableHashSet or CollectionType.IImmutableSet)
            return ctx.Types.Get(typeof(ImmutableHashSet)).GetStaticGenericMethod(ToImmutableHashSetMethodName);

        if (ctx.CollectionInfos.Target.Type is CollectionType.ImmutableQueue or CollectionType.IImmutableQueue)
            return ctx.Types.Get(typeof(ImmutableQueue)).GetStaticGenericMethod(CreateRangeQueueMethodName);

        if (ctx.CollectionInfos.Target.Type is CollectionType.ImmutableStack or CollectionType.IImmutableStack)
            return ctx.Types.Get(typeof(ImmutableStack)).GetStaticGenericMethod(CreateRangeStackMethodName);

        if (ctx.CollectionInfos.Target.Type is CollectionType.ImmutableSortedSet)
            return ctx.Types.Get(typeof(ImmutableSortedSet)).GetStaticGenericMethod(ToImmutableSortedSetMethodName);

        return null;
    }

    private static IMethodSymbol? GetToHashSetLinqCollectMethod(WellKnownTypes wellKnownTypes) =>
        wellKnownTypes.Get(typeof(Enumerable)).GetStaticGenericMethod(ToHashSetMethodName);
}
