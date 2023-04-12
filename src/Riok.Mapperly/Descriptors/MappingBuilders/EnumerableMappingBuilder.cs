using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
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
    private const string AddValueMethodName = nameof(ICollection<object>.Add);

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

        // if element mapping is synthetic
        // and target is an IEnumerable, there is no mapping needed at all.
        if (elementMapping.IsSynthetic && SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.IEnumerableT))
            return new CastMapping(ctx.Source, ctx.Target);

        // if source is an array and target is an array or IReadOnlyCollection faster mappings can be applied
        if (!ctx.IsExpression
            && ctx.Source.IsArrayType()
            && (ctx.Target.IsArrayType() || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.IReadOnlyCollectionT)))
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

        return ctx.IsExpression
            ? null
            : BuildCustomTypeMapping(ctx, elementMapping);
    }

    public static IExistingTargetMapping? TryBuildExistingTargetMapping(MappingBuilderContext ctx)
    {
        if (BuildElementMapping(ctx) is not { } elementMapping)
            return null;

        if (ctx.Target.ImplementsGeneric(ctx.Types.StackT, out _))
            return new ForEachAddEnumerableExistingTargetMapping(ctx.Source, ctx.Target, elementMapping, nameof(Stack<object>.Push));

        if (ctx.Target.ImplementsGeneric(ctx.Types.QueueT, out _))
            return new ForEachAddEnumerableExistingTargetMapping(ctx.Source, ctx.Target, elementMapping, nameof(Queue<object>.Enqueue));

        // create a foreach loop with add calls if source is not an array
        // and  ICollection.Add(T): void is implemented and not explicit
        // ensures add is not called and immutable types
        if (!ctx.Target.IsArrayType() && ctx.Target.HasImplicitInterfaceMethod(ctx.Types.ICollectionT, AddValueMethodName))
            return new ForEachAddEnumerableExistingTargetMapping(ctx.Source, ctx.Target, elementMapping, AddValueMethodName);

        return null;
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

    private static LinqEnumerableMapping BuildLinqMapping(
        MappingBuilderContext ctx,
        ITypeMapping elementMapping,
        string? collectMethodName)
    {
        var collectMethod = collectMethodName == null
            ? null
            : ResolveStaticMethod(ctx.Types.Enumerable, collectMethodName);

        var selectMethod = elementMapping.IsSynthetic
            ? null
            : ResolveStaticMethod(ctx.Types.Enumerable, SelectMethodName);

        return new LinqEnumerableMapping(ctx.Source, ctx.Target, elementMapping, selectMethod, collectMethod);
    }

    private static ExistingTargetMappingMethodWrapper? BuildCustomTypeMapping(
        MappingBuilderContext ctx,
        ITypeMapping elementMapping)
    {
        if (!ctx.ObjectFactories.TryFindObjectFactory(ctx.Source, ctx.Target, out var objectFactory) && !ctx.Target.HasAccessibleParameterlessConstructor())
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, ctx.Target);
            return null;
        }

        if (ctx.Target.ImplementsGeneric(ctx.Types.StackT, out _))
            return new ForEachAddEnumerableMapping(ctx.Source, ctx.Target, elementMapping, objectFactory, nameof(Stack<object>.Push));

        if (ctx.Target.ImplementsGeneric(ctx.Types.QueueT, out _))
            return new ForEachAddEnumerableMapping(ctx.Source, ctx.Target, elementMapping, objectFactory, nameof(Queue<object>.Enqueue));

        // create a foreach loop with add calls if source is not an array
        // and  ICollection.Add(T): void is implemented and not explicit
        // ensures add is not called and immutable types
        if (!ctx.Target.IsArrayType() && ctx.Target.HasImplicitInterfaceMethod(ctx.Types.ICollectionT, AddValueMethodName))
            return new ForEachAddEnumerableMapping(ctx.Source, ctx.Target, elementMapping, objectFactory, AddValueMethodName);

        return null;
    }

    private static (bool CanMapWithLinq, string? CollectMethod) ResolveCollectMethodName(MappingBuilderContext ctx)
    {
        // if the target is an array we need to collect to array
        if (ctx.Target.IsArrayType())
            return (true, ToArrayMethodName);

        // if the target is an IEnumerable<T> don't collect at all.
        if (SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.IEnumerableT))
            return (true, null);

        // if the target is IReadOnlyCollection<T>
        // and the count of the source is known (array, IReadOnlyCollection<T>, ICollection<T>) we collect to array
        // for performance/space reasons
        var targetIsReadOnlyCollection = SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.IReadOnlyCollectionT);
        var sourceCountIsKnown =
            ctx.Source.IsArrayType()
            || ctx.Source.ImplementsGeneric(ctx.Types.IReadOnlyCollectionT, out _)
            || ctx.Source.ImplementsGeneric(ctx.Types.ICollectionT, out _);
        if (targetIsReadOnlyCollection && sourceCountIsKnown)
            return (true, ToArrayMethodName);

        // if target is a IReadOnlyCollection<T>, IList<T>, List<T> or ICollection<T> with ToList()
        return targetIsReadOnlyCollection
            || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.IReadOnlyListT)
            || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.IListT)
            || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.ListT)
            || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.ICollectionT)
            ? (true, ToListMethodName)
            : (false, null);
    }

    private static LinqEnumerableMapping? TryBuildImmutableLinqMapping(
        MappingBuilderContext ctx,
        ITypeMapping elementMapping)
    {
        var collectMethod = ResolveImmutableCollectMethod(ctx);
        if (collectMethod is null)
            return null;

        var selectMethod = elementMapping.IsSynthetic
            ? null
            : ResolveStaticMethod(ctx.Types.Enumerable, SelectMethodName);

        return new LinqEnumerableMapping(ctx.Source, ctx.Target, elementMapping, selectMethod, collectMethod);
    }

    private static IMethodSymbol? ResolveImmutableCollectMethod(MappingBuilderContext ctx)
    {
        if (SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.ImmutableArrayT))
            return ResolveStaticMethod(ctx.Types.ImmutableArray, ToImmutableArrayMethodName);

        if (SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.ImmutableListT))
            return ResolveStaticMethod(ctx.Types.ImmutableList, ToImmutableListMethodName);

        if (SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.ImmutableHashSetT))
            return ResolveStaticMethod(ctx.Types.ImmutableHashSet, ToImmutableHashSetMethodName);

        if (SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.ImmutableQueueT))
            return ResolveStaticMethod(ctx.Types.ImmutableQueue, CreateRangeQueueMethodName);

        if (SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.ImmutableStackT))
            return ResolveStaticMethod(ctx.Types.ImmutableStack, CreateRangeStackMethodName);

        if (SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.ImmutableSortedSetT))
            return ResolveStaticMethod(ctx.Types.ImmutableSortedSet, ToImmutableSortedSetMethodName);

        return null;
    }

    private static IMethodSymbol? ResolveStaticMethod(INamedTypeSymbol namedType, string methodName)
    {
        var method = namedType.GetMembers(methodName)
                                .OfType<IMethodSymbol>()
                                .FirstOrDefault(m => m.IsStatic && m.IsGenericMethod);

        return method;
    }

    private static ITypeSymbol? GetEnumeratedType(MappingBuilderContext ctx, ITypeSymbol type)
    {
        return type.ImplementsGeneric(ctx.Types.IEnumerableT, out var enumerableIntf)
            ? enumerableIntf.TypeArguments[0]
            : null;
    }
}
