using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class EnumerableMappingBuilder
{
    private const string SelectMethodName = nameof(Enumerable.Select);
    private const string ToArrayMethodName = nameof(Enumerable.ToArray);
    private const string ToListMethodName = nameof(Enumerable.ToList);

    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        var enumeratedSourceType = GetEnumeratedType(ctx, ctx.Source);
        if (enumeratedSourceType == null)
            return null;

        var enumeratedTargetType = GetEnumeratedType(ctx, ctx.Target);
        if (enumeratedTargetType == null)
            return null;

        var elementMapping = ctx.FindOrBuildMapping(enumeratedSourceType, enumeratedTargetType);
        if (elementMapping == null)
            return null;

        // if element mapping is synthetic
        // and target is an IEnumerable, there is no mapping needed at all.
        if (elementMapping.IsSynthetic && ctx.IsType(ctx.Target.OriginalDefinition, typeof(IEnumerable<>)))
            return new CastMapping(ctx.Source, ctx.Target);

        // if source is an array and target is an array or IReadOnlyCollection faster mappings can be applied
        if (ctx.Source.IsArrayType()
            && (ctx.Target.IsArrayType() || ctx.IsType(ctx.Target.OriginalDefinition, typeof(IReadOnlyCollection<>))))
        {
            // if element mapping is synthetic
            // a single Array.Clone / cast mapping call should be sufficient and fast,
            // use a for loop mapping otherwise.
            if (!elementMapping.IsSynthetic)
                return new ArrayForMapping(ctx.Source, ctx.Target, elementMapping, enumeratedTargetType);

            return ctx.MapperConfiguration.UseDeepCloning
                ? new ArrayCloneMapping(ctx.Source, ctx.Target)
                : new CastMapping(ctx.Source, ctx.Target);
        }

        // try linq mapping: x.Select(Map).ToArray/ToList
        // if that doesn't work do a foreach with add calls
        var (canMapWithLinq, collectMethodName) = ResolveCollectMethodName(ctx);
        if (!canMapWithLinq)
            return BuildCustomTypeMapping(ctx, elementMapping);

        return BuildLinqMapping(ctx, elementMapping, collectMethodName);
    }

    private static LinqEnumerableMapping BuildLinqMapping(
        MappingBuilderContext ctx,
        TypeMapping elementMapping,
        string? collectMethodName)
    {
        var collectMethod = collectMethodName == null
            ? null
            : ResolveLinqMethod(ctx, collectMethodName);

        var selectMethod = elementMapping.IsSynthetic
            ? null
            : ResolveLinqMethod(ctx, SelectMethodName);

        return new LinqEnumerableMapping(ctx.Source, ctx.Target, elementMapping, selectMethod, collectMethod);
    }

    private static ForEachAddEnumerableMapping? BuildCustomTypeMapping(
        MappingBuilderContext ctx,
        TypeMapping elementMapping)
    {
        if (!ctx.ObjectFactories.TryFindObjectFactory(ctx.Source, ctx.Target, out var objectFactory) && !ctx.Target.HasAccessibleParameterlessConstructor())
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, ctx.Target);
            return null;
        }

        return ctx.Target.ImplementsGeneric(ctx.GetTypeSymbol(typeof(ICollection<>)), out _)
            ? new ForEachAddEnumerableMapping(ctx.Source, ctx.Target, elementMapping, objectFactory)
            : null;
    }

    private static (bool CanMapWithLinq, string? CollectMethod) ResolveCollectMethodName(MappingBuilderContext ctx)
    {
        // if the target is an array we need to collect to array
        if (ctx.Target.IsArrayType())
            return (true, ToArrayMethodName);

        // if the target is an IEnumerable<T> don't collect at all.
        if (ctx.IsType(ctx.Target.OriginalDefinition, typeof(IEnumerable<>)))
            return (true, null);

        // if the target is IReadOnlyCollection<T>
        // and the count of the source is known (array, IReadOnlyCollection<T>, ICollection<T>) we collect to array
        // for performance/space reasons
        var targetIsReadOnlyCollection = ctx.IsType(ctx.Target.OriginalDefinition, typeof(IReadOnlyCollection<>));
        var sourceCountIsKnown =
            ctx.Source.IsArrayType()
            || ctx.Source.ImplementsGeneric(ctx.GetTypeSymbol(typeof(IReadOnlyCollection<>)), out _)
            || ctx.Source.ImplementsGeneric(ctx.GetTypeSymbol(typeof(ICollection<>)), out _);
        if (targetIsReadOnlyCollection && sourceCountIsKnown)
            return (true, ToArrayMethodName);

        // if target is a list, ICollection<T> or IReadOnlyCollection<T> collect with ToList()
        return targetIsReadOnlyCollection
            || ctx.IsType(ctx.Target.OriginalDefinition, typeof(ICollection<>))
            || ctx.IsType(ctx.Target.OriginalDefinition, typeof(List<>))
            ? (true, ToListMethodName)
            : (false, null);
    }

    private static IMethodSymbol? ResolveLinqMethod(MappingBuilderContext ctx, string methodName)
    {
        var method = ctx.GetTypeSymbol(typeof(Enumerable))
            .GetMembers(methodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m =>
                m.IsStatic
                && m.IsGenericMethod);

        return method;
    }

    private static ITypeSymbol? GetEnumeratedType(MappingBuilderContext ctx, ITypeSymbol type)
    {
        return type.ImplementsGeneric(ctx.GetTypeSymbol(typeof(IEnumerable<>)), out var enumerableIntf)
            ? enumerableIntf.TypeArguments[0]
            : null;
    }
}
