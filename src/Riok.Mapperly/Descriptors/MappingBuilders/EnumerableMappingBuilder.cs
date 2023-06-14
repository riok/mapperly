using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
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

        if (BuildElementMapping(ctx) is not { } elementMapping)
            return null;

        // if source is an array and target is an array, IEnumerable, IReadOnlyCollection faster mappings can be applied
        if (
            !ctx.IsExpression
            && ctx.Source.IsArrayType()
            && (
                ctx.Target.IsArrayType()
                || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(IReadOnlyCollection<>)))
                || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(IEnumerable<>)))
            )
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
        var (canMapWithLinq, collectMethodName) = ResolveCollectMethodName(ctx, elementMapping.IsSynthetic);
        if (canMapWithLinq)
            return BuildLinqMapping(ctx, elementMapping, collectMethodName);

        // try linq mapping: x.Select(Map).ToImmutableArray/ToImmutableList
        // if that doesn't work do a foreach with add calls
        var immutableLinqMapping = TryBuildImmutableLinqMapping(ctx, elementMapping);
        if (immutableLinqMapping is not null)
            return immutableLinqMapping;

        // if target is a type that takes IEnumerable in its constructor
        if (HasEnumerableConstructor(ctx, elementMapping.TargetType))
            return BuildLinqConstructorMapping(ctx, elementMapping);

        return ctx.IsExpression ? null : BuildCustomTypeMapping(ctx, elementMapping);
    }

    public static IExistingTargetMapping? TryBuildExistingTargetMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Enumerable))
            return null;

        if (BuildElementMapping(ctx) is not { } elementMapping)
            return null;

        if (ctx.Target.ImplementsGeneric(ctx.Types.Get(typeof(Stack<>)), out _))
            return CreateForEach(nameof(Stack<object>.Push));

        if (ctx.Target.ImplementsGeneric(ctx.Types.Get(typeof(Queue<>)), out _))
            return CreateForEach(nameof(Queue<object>.Enqueue));

        // create a foreach loop with add calls if source is not an array
        // and void ICollection.Add(T) or bool ISet.Add(T) is implemented and not explicit
        // ensures add is not called and immutable types
        // ISet.Add(T) is explicitly needed as sets implement the ICollection.Add(T) explicit,
        // and override the add method with new
        var hasImplicitCollectionAdd = ctx.Target.HasImplicitGenericImplementation(ctx.Types.Get(typeof(ICollection<>)), AddMethodName);
        var hasImplicitSetAdd = ctx.Target.HasImplicitGenericImplementation(ctx.Types.Get(typeof(ISet<>)), AddMethodName);
        if (!ctx.Target.IsArrayType() && (hasImplicitCollectionAdd || hasImplicitSetAdd))
        {
            return CreateForEach(AddMethodName);
        }

        // if a mapping could be created for an immutable collection
        // we diagnostic when it is an existing target mapping
        if (ResolveImmutableCollectMethod(ctx) != null)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember);
        }

        return null;

        ForEachAddEnumerableExistingTargetMapping CreateForEach(string methodName)
        {
            var ensureCapacityStatement = EnsureCapacityBuilder.TryBuildEnsureCapacity(ctx.Source, ctx.Target, ctx.Types);
            return new ForEachAddEnumerableExistingTargetMapping(
                ctx.Source,
                ctx.Target,
                elementMapping,
                methodName,
                ensureCapacityStatement
            );
        }
    }

    private static ITypeMapping? BuildElementMapping(MappingBuilderContext ctx)
    {
        var enumeratedSourceType = GetEnumeratedType(ctx, ctx.Source);
        if (enumeratedSourceType == null)
            return null;

        var enumeratedTargetType = GetEnumeratedType(ctx, ctx.Target);
        if (enumeratedTargetType == null)
            return null;

        return ctx.FindOrBuildMapping(enumeratedSourceType, enumeratedTargetType);
    }

    private static LinqEnumerableMapping BuildLinqMapping(MappingBuilderContext ctx, ITypeMapping elementMapping, string? collectMethodName)
    {
        var collectMethod = collectMethodName == null ? null : ctx.Types.Get(typeof(Enumerable)).GetStaticGenericMethod(collectMethodName);

        var selectMethod = elementMapping.IsSynthetic ? null : ctx.Types.Get(typeof(Enumerable)).GetStaticGenericMethod(SelectMethodName);

        return new LinqEnumerableMapping(ctx.Source, ctx.Target, elementMapping, selectMethod, collectMethod);
    }

    private static bool HasEnumerableConstructor(MappingBuilderContext ctx, ITypeSymbol typeSymbol)
    {
        if (ctx.Target is not INamedTypeSymbol namedType)
            return false;

        var typedEnumerable = ctx.Types.Get(typeof(IEnumerable<>)).Construct(typeSymbol);

        return namedType.Constructors.Any(
            m => m.Parameters.Length == 1 && SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, typedEnumerable)
        );
    }

    private static LinqConstructorMapping BuildLinqConstructorMapping(MappingBuilderContext ctx, ITypeMapping elementMapping)
    {
        var selectMethod = elementMapping.IsSynthetic ? null : ctx.Types.Get(typeof(Enumerable)).GetStaticGenericMethod(SelectMethodName);

        return new LinqConstructorMapping(ctx.Source, ctx.Target, elementMapping, selectMethod);
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
        // and void ICollection.Add(T) or bool ISet.Add(T) is implemented and not explicit
        // ensures .Add() is not called on immutable types
        var hasImplicitCollectionAdd = ctx.Target.HasImplicitGenericImplementation(ctx.Types.Get(typeof(ICollection<>)), AddMethodName);
        var hasImplicitSetAdd = ctx.Target.HasImplicitGenericImplementation(ctx.Types.Get(typeof(ISet<>)), AddMethodName);
        if (ctx.Target.IsArrayType() || (!hasImplicitCollectionAdd && !hasImplicitSetAdd))
            return null;

        var ensureCapacityStatement = EnsureCapacityBuilder.TryBuildEnsureCapacity(ctx.Source, ctx.Target, ctx.Types);
        return new ForEachAddEnumerableMapping(
            ctx.Source,
            ctx.Target,
            elementMapping,
            objectFactory,
            AddMethodName,
            ensureCapacityStatement
        );
    }

    private static (bool CanMapWithLinq, string? CollectMethod) ResolveCollectMethodName(
        MappingBuilderContext ctx,
        bool elementMappingIsSynthetic
    )
    {
        // if the target is an array we need to collect to array
        if (ctx.Target.IsArrayType())
            return (true, ToArrayMethodName);

        // if the target is an IEnumerable<T> don't collect at all
        // except deep cloning is enabled.
        var targetIsIEnumerable = SymbolEqualityComparer.Default.Equals(
            ctx.Target.OriginalDefinition,
            ctx.Types.Get(typeof(IEnumerable<>))
        );
        if (targetIsIEnumerable && !ctx.MapperConfiguration.UseDeepCloning)
            return (true, null);

        // if the target is IReadOnlyCollection<T> or IEnumerable<T>
        // and the count of the source is known (array, IReadOnlyCollection<T>, ICollection<T>) we collect to array
        // for performance/space reasons
        var targetIsReadOnlyCollection = SymbolEqualityComparer.Default.Equals(
            ctx.Target.OriginalDefinition,
            ctx.Types.Get(typeof(IReadOnlyCollection<>))
        );
        var sourceCountIsKnown =
            ctx.Source.IsArrayType()
            || ctx.Source.ImplementsGeneric(ctx.Types.Get(typeof(IReadOnlyCollection<>)), out _)
            || ctx.Source.ImplementsGeneric(ctx.Types.Get(typeof(ICollection<>)), out _);
        if ((targetIsReadOnlyCollection || targetIsIEnumerable) && sourceCountIsKnown)
            return (true, ToArrayMethodName);

        // if target is a IReadOnlyCollection<T>, IEnumerable<T>, IList<T>, List<T> or ICollection<T> with ToList()
        return
            targetIsReadOnlyCollection
            || targetIsIEnumerable
            || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(IReadOnlyList<>)))
            || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(IList<>)))
            || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(List<>)))
            || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(ICollection<>)))
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
        if (SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(ImmutableArray<>))))
            return ctx.Types.Get(typeof(ImmutableArray)).GetStaticGenericMethod(ToImmutableArrayMethodName);

        if (
            SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(ImmutableList<>)))
            || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(IImmutableList<>)))
        )
            return ctx.Types.Get(typeof(ImmutableList)).GetStaticGenericMethod(ToImmutableListMethodName);

        if (
            SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(ImmutableHashSet<>)))
            || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(IImmutableSet<>)))
        )
            return ctx.Types.Get(typeof(ImmutableHashSet)).GetStaticGenericMethod(ToImmutableHashSetMethodName);

        if (
            SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(ImmutableQueue<>)))
            || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(IImmutableQueue<>)))
        )
            return ctx.Types.Get(typeof(ImmutableQueue)).GetStaticGenericMethod(CreateRangeQueueMethodName);

        if (
            SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(ImmutableStack<>)))
            || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(IImmutableStack<>)))
        )
            return ctx.Types.Get(typeof(ImmutableStack)).GetStaticGenericMethod(CreateRangeStackMethodName);

        if (SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(ImmutableSortedSet<>))))
            return ctx.Types.Get(typeof(ImmutableSortedSet)).GetStaticGenericMethod(ToImmutableSortedSetMethodName);

        return null;
    }

    private static ITypeSymbol? GetEnumeratedType(MappingBuilderContext ctx, ITypeSymbol type)
    {
        return type.ImplementsGeneric(ctx.Types.Get(typeof(IEnumerable<>)), out var enumerableIntf)
            ? enumerableIntf.TypeArguments[0]
            : null;
    }
}
