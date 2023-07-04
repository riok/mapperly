using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.ObjectFactories;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class SpanMappingBuilder
{
    private const string ToArrayMethodName = nameof(Enumerable.ToArray);
    private const string AddMethodName = nameof(ICollection<object>.Add);

    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Span))
            return null;

        if (ctx.CollectionInfos == null)
            return null;

        var source = ctx.CollectionInfos.Source;
        var target = ctx.CollectionInfos.Target;

        // if neither the source or target are Span then end
        // also check for memory type
        if (!source.IsSpan && !target.IsSpan || source.IsMemory || target.IsMemory)
            return null;

        if (ctx.FindOrBuildMapping(source.EnumeratedType, target.EnumeratedType) is not { } elementMapping)
            return null;

        return (source.Type, target.Type) switch
        {
            // if target is Enumerable then source is Span/ReadOnlySpan
            (_, not CollectionType.Array) when target.ImplementsIEnumerable => BuildSpanToEnumerable(ctx, elementMapping),

            // if source is Enumerable then target is Span/ReadOnlySpan
            (not CollectionType.Array, _) when source.ImplementsIEnumerable => BuildEnumerableToSpan(ctx, elementMapping),

            // if source is ReadOnlySpan and target is a Span
            (CollectionType.ReadOnlySpan, CollectionType.Span) => BuildToArrayOrMap(ctx, elementMapping),

            // if the source is Span/ReadOnlySpan or Array and target is Span/ReadOnlySpan
            // and element type is the same, then direct cast
            (CollectionType.Span or CollectionType.ReadOnlySpan or CollectionType.Array, CollectionType.Span or CollectionType.ReadOnlySpan)
                when elementMapping.IsSynthetic && !ctx.MapperConfiguration.UseDeepCloning
                => new CastMapping(ctx.Source, ctx.Target),

            // otherwise map each value into an Array
            _ => BuildToArrayOrMap(ctx, elementMapping),
        };
    }

    public static IExistingTargetMapping? TryBuildExistingTargetMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Span) || !ctx.IsConversionEnabled(MappingConversionType.Enumerable))
            return null;

        if (ctx.CollectionInfos == null)
            return null;

        var source = ctx.CollectionInfos.Source;
        var target = ctx.CollectionInfos.Target;

        // if neither the source or target are Span then end
        if (!source.IsSpan && !target.IsSpan || source.IsMemory || target.IsMemory)
            return null;

        if (target.IsSpan)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember, ctx.Target);
            return null;
        }

        if (source.IsMemory || target.IsMemory)
            return null;

        if (ctx.FindOrBuildMapping(source.EnumeratedType, target.EnumeratedType) is not { } elementMapping)
            return null;

        if (target.Type is CollectionType.Stack)
            return CreateForEach(nameof(Stack<object>.Push));

        if (target.Type is CollectionType.Queue)
            return CreateForEach(nameof(Queue<object>.Enqueue));

        // create a foreach loop with add calls if source is not an array
        // and ICollection.Add(T): void is implemented and not explicit
        // ensures add is not called and immutable types
        if (
            target.Type is not CollectionType.Array
            && ctx.Target.HasImplicitGenericImplementation(ctx.Types.Get(typeof(ICollection<>)), AddMethodName)
        )
            return CreateForEach(AddMethodName);

        // if a mapping could be created for an immutable collection
        // we diagnostic when it is an existing target mapping
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

    private static TypeMapping? BuildSpanToEnumerable(MappingBuilderContext ctx, ITypeMapping elementMapping)
    {
        var target = ctx.CollectionInfos!.Target;

        // if IEnumerable or IReadOnlyCollection map to array
        if (target.Type is CollectionType.IEnumerable or CollectionType.IReadOnlyCollection)
            return BuildToArrayOrMap(ctx, elementMapping);

        // if target is IList, IReadOnlyList or ICollection map to List
        if (target.Type is CollectionType.ICollection or CollectionType.IReadOnlyList or CollectionType.IList)
            return BuildSpanToList(ctx, elementMapping);

        if (
            !ctx.ObjectFactories.TryFindObjectFactory(ctx.Source, ctx.Target, out var objectFactory)
            && !ctx.Target.HasAccessibleParameterlessConstructor()
        )
        {
            return MapSpanArrayToEnumerableMethod(ctx);
        }

        if (target.Type is CollectionType.Stack)
            return CreateForEach(nameof(Stack<object>.Push), objectFactory);

        if (target.Type is CollectionType.Queue)
            return CreateForEach(nameof(Queue<object>.Enqueue), objectFactory);

        // create a foreach loop with add calls if source is not an array
        // and ICollection.Add(T): void is implemented and not explicit
        // ensures add is not called and immutable types
        if (
            target.Type is not CollectionType.Array
            && ctx.Target.HasImplicitGenericImplementation(ctx.Types.Get(typeof(ICollection<>)), AddMethodName)
        )
            return CreateForEach(AddMethodName, objectFactory);

        return MapSpanArrayToEnumerableMethod(ctx);

        ExistingTargetMappingMethodWrapper CreateForEach(string methodName, ObjectFactory? factory)
        {
            var ensureCapacityStatement = EnsureCapacityBuilder.TryBuildEnsureCapacity(ctx);
            return new ForEachAddEnumerableMapping(ctx.Source, ctx.Target, elementMapping, factory, methodName, ensureCapacityStatement);
        }
    }

    private static TypeMapping BuildToArrayOrMap(MappingBuilderContext ctx, ITypeMapping elementMapping)
    {
        if (!elementMapping.IsSynthetic)
            return new ArrayForMapping(ctx.Source, ctx.Target, elementMapping, elementMapping.TargetType);

        return new SourceObjectMethodMapping(ctx.Source, ctx.Target, ToArrayMethodName);
    }

    private static TypeMapping? BuildEnumerableToSpan(MappingBuilderContext ctx, ITypeMapping elementMapping)
    {
        var typedArray = ctx.Types.GetArrayType(elementMapping.TargetType);
        if (ctx.FindOrBuildMapping(ctx.Source, typedArray) is not { } arrayMapping)
            return null;

        return new CastMapping(ctx.Source, ctx.Target, arrayMapping);
    }

    private static TypeMapping? BuildSpanToList(MappingBuilderContext ctx, ITypeMapping elementMapping)
    {
        var typedList = ctx.Types.Get(typeof(List<>)).Construct(elementMapping.TargetType);
        if (ctx.FindOrBuildMapping(ctx.Source, typedList) is not { } listMapping)
            return null;

        return new DelegateMapping(ctx.Source, ctx.Target, listMapping);
    }

    private static TypeMapping? MapSpanArrayToEnumerableMethod(MappingBuilderContext ctx)
    {
        var enumeratedType = ctx.CollectionInfos!.Source.EnumeratedType;
        var typedArray = ctx.Types.GetArrayType(enumeratedType);
        if (ctx.FindOrBuildMapping(typedArray, ctx.Target) is not { } arrayMapping)
            return null;

        return new SourceObjectMethodMapping(ctx.Source, ctx.Target, ToArrayMethodName, arrayMapping);
    }
}
