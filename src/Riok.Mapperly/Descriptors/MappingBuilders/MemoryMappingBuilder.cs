using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class MemoryMappingBuilder
{
    private const string SpanMemberName = nameof(Memory<int>.Span);
    private const string ToArrayMethodName = nameof(Enumerable.ToArray);

    public static NewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Memory))
            return null;

        if (ctx.CollectionInfos == null)
            return null;

        var source = ctx.CollectionInfos.Source;
        var target = ctx.CollectionInfos.Target;

        // if neither the source or target are Memory then end
        if (!source.IsMemory && !target.IsMemory)
            return null;

        if (ctx.FindOrBuildMapping(source.EnumeratedType, target.EnumeratedType) is not { } elementMapping)
            return null;

        return (source.CollectionType, target.CollectionType) switch
        {
            // if source is Enumerable then target must be Memory
            // use MapToArray(enumerable)
            (not CollectionType.Array, _) when source.ImplementsIEnumerable => BuildEnumerableToMemoryMapping(ctx, elementMapping),

            // if target is Enumerable
            // use MapToEnumerable(memory.Span)
            (_, not CollectionType.Array) when target.ImplementsIEnumerable => BuildMemoryToEnumerableMapping(ctx, elementMapping),

            // if source is Span/ReadOnlySpan then target must be Memory/ReadOnlyMemory
            // either use ToArray or MapToArray(span)
            (CollectionType.Span or CollectionType.ReadOnlySpan, _) => BuildSpanToMemoryMapping(ctx, elementMapping),

            // if source is Array then target must be memory
            // either use ToArray or MapToArray(array)
            (CollectionType.Array, _) => BuildArrayToMemoryMapping(ctx, elementMapping),

            // all source types are Memory/ReadOnlyMemory use target specific mapping strategy

            // if source is ReadOnlyMemory and target is Span
            // either use ToArray or MapToArray(memory.Span)
            (CollectionType.ReadOnlyMemory, CollectionType.Span) => BuildMemoryToArrayMapping(ctx, elementMapping),

            // if target is Span
            // either use memory.Span or MapToSpan(memory.Span)
            (_, CollectionType.Span or CollectionType.ReadOnlySpan) => BuildMemoryToSpanMapping(ctx, elementMapping),

            // if source is ReadOnlyMemory and target is Memory
            // either use ToArray or MapToSpan(memory.Span)
            (CollectionType.ReadOnlyMemory, CollectionType.Memory) => BuildMemoryToArrayMapping(ctx, elementMapping),

            // if target is Memory
            // either use Cast or MapToArray(memory.Span)
            (_, CollectionType.ReadOnlyMemory or CollectionType.Memory) => BuildMemoryToMemoryMapping(ctx, elementMapping),

            // if target is Array
            // either use ToArray or MapToArray(memory.Span)
            _ => BuildMemoryToArrayMapping(ctx, elementMapping),
        };
    }

    public static IExistingTargetMapping? TryBuildExistingTargetMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Memory))
            return null;

        if (ctx.CollectionInfos == null)
            return null;

        var source = ctx.CollectionInfos.Source;
        var target = ctx.CollectionInfos.Target;

        // if neither the source or target are Memory then end
        if (!source.IsMemory && !target.IsMemory)
            return null;

        // can only map to Enumerable. Existing types Span, Memory and array are all immutable
        if (!target.ImplementsIEnumerable)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember, ctx.Target);
            return null;
        }

        if (ctx.FindOrBuildMapping(source.EnumeratedType, target.EnumeratedType) is not { } elementMapping)
            return null;

        var sourceSpan = ctx.Types.Get(typeof(ReadOnlySpan<>)).Construct(elementMapping.SourceType);
        if (ctx.FindOrBuildExistingTargetMapping(sourceSpan, ctx.Target) is not ExistingTargetMapping enumerableMapping)
            return null;

        return new SourceObjectMemberDelegateExistingTargetMapping(ctx.Source, ctx.Target, SpanMemberName, enumerableMapping);
    }

    private static NewInstanceMapping? BuildSpanToMemoryMapping(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        if (elementMapping.IsSynthetic && !ctx.MapperConfiguration.UseDeepCloning)
            return new SourceObjectMethodMapping(ctx.Source, ctx.Target, ToArrayMethodName);

        var targetArray = ctx.Types.GetArrayType(elementMapping.TargetType);
        return ctx.FindOrBuildMapping(ctx.Source, targetArray.NonNullable()) is not { } arrayMapping
            ? null
            : new CastMapping(ctx.Source, ctx.Target, arrayMapping);
    }

    private static NewInstanceMapping? BuildMemoryToArrayMapping(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        if (!elementMapping.IsSynthetic || ctx.MapperConfiguration.UseDeepCloning)
            return BuildSpanToArrayMethodMapping(ctx, elementMapping);

        return new SourceObjectMethodMapping(ctx.Source, ctx.Target, ToArrayMethodName);
    }

    private static NewInstanceMapping? BuildMemoryToSpanMapping(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        if (!elementMapping.IsSynthetic || ctx.MapperConfiguration.UseDeepCloning)
            return BuildMemoryToSpanMethod(ctx, elementMapping);

        return new SourceObjectMemberMapping(ctx.Source, ctx.Target, SpanMemberName);
    }

    private static NewInstanceMapping BuildArrayToMemoryMapping(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        if (!elementMapping.IsSynthetic || ctx.MapperConfiguration.UseDeepCloning)
            return new ArrayForMapping(
                ctx.Source,
                ctx.Types.GetArrayType(elementMapping.TargetType),
                elementMapping,
                elementMapping.TargetType
            );

        return new CastMapping(ctx.Source, ctx.Target);
    }

    private static NewInstanceMapping? BuildMemoryToSpanMethod(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        var sourceSpan = ctx.Types.Get(typeof(ReadOnlySpan<>)).Construct(elementMapping.SourceType);
        if (ctx.FindOrBuildMapping(sourceSpan, ctx.Target) is not { } spanMapping)
            return null;

        return new SourceObjectMemberMapping(ctx.Source, ctx.Target, SpanMemberName, spanMapping);
    }

    private static NewInstanceMapping? BuildMemoryToMemoryMapping(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        if (!elementMapping.IsSynthetic || ctx.MapperConfiguration.UseDeepCloning)
            return BuildSpanToArrayMethodMapping(ctx, elementMapping);

        return new CastMapping(ctx.Source, ctx.Target);
    }

    private static NewInstanceMapping? BuildMemoryToEnumerableMapping(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        var sourceSpan = ctx.Types.Get(typeof(ReadOnlySpan<>)).Construct(elementMapping.SourceType);
        if (ctx.FindOrBuildMapping(sourceSpan, ctx.Target) is not { } enumerableMapping)
            return null;

        return new SourceObjectMemberMapping(ctx.Source, ctx.Target, SpanMemberName, enumerableMapping);
    }

    private static NewInstanceMapping? BuildEnumerableToMemoryMapping(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        var targetArray = ctx.Types.GetArrayType(elementMapping.TargetType);
        if (ctx.FindOrBuildMapping(ctx.Source, targetArray.NonNullable()) is not { } arrayMapping)
            return null;

        return new DelegateMapping(ctx.Source, ctx.Target, arrayMapping);
    }

    private static NewInstanceMapping? BuildSpanToArrayMethodMapping(MappingBuilderContext ctx, INewInstanceMapping elementMapping)
    {
        var sourceSpan = ctx.Types.Get(typeof(ReadOnlySpan<>)).Construct(elementMapping.SourceType);
        var targetArray = ctx.Types.GetArrayType(elementMapping.TargetType);
        if (ctx.FindOrBuildMapping(sourceSpan, targetArray.NonNullable()) is not { } arrayMapping)
            return null;

        return new SourceObjectMemberMapping(ctx.Source, ctx.Target, SpanMemberName, arrayMapping);
    }
}
