using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.TypeMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class EnumerableMappingBuilder
{
    private const string SelectMethodName = nameof(Enumerable.Select);
    private const string ToArrayMethodName = nameof(Enumerable.ToArray);
    private const string ToListMethodName = nameof(Enumerable.ToList);

    private static readonly string _enumerableIntfName = typeof(IEnumerable<>).FullName;
    private static readonly string _readOnlyCollectionIntfName = typeof(IReadOnlyCollection<>).FullName;
    private static readonly string _collectionIntfName = typeof(ICollection<>).FullName;

    private static readonly string _listClassName = typeof(List<>).FullName;
    private static readonly string _linqEnumerableClassName = typeof(Enumerable).FullName;

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

        // true if there is no need to convert elements (eg. the source instances can directly be reused)
        // this is the case if the types are equal and the mapping is a direct assignment.
        var isDirectMapping = elementMapping is DirectAssignmentMapping;

        // if element mapping is a direct assignment
        // and target is an IEnumerable, there is no mapping needed at all.
        if (isDirectMapping && IsType(ctx, _enumerableIntfName, ctx.Target.OriginalDefinition))
            return new CastMapping(ctx.Source, ctx.Target);

        // try linq mapping: x.Select(Map).ToArray/ToList
        // if that doesn't work do a foreach with add calls
        var (canMapWithLinq, collectMethodName) = ResolveCollectMethodName(ctx);
        if (!canMapWithLinq)
            return BuildCustomTypeMapping(ctx, elementMapping);

        return BuildLinqMapping(ctx, isDirectMapping, elementMapping, collectMethodName);
    }

    private static LinqEnumerableMapping BuildLinqMapping(
        MappingBuilderContext ctx,
        bool isDirectMapping,
        TypeMapping elementMapping,
        string? collectMethodName)
    {
        var collectMethod = collectMethodName == null
            ? null
            : ResolveLinqMethod(ctx, collectMethodName);

        var selectMethod = isDirectMapping
            ? null
            : ResolveLinqMethod(ctx, SelectMethodName);

        return new LinqEnumerableMapping(ctx.Source, ctx.Target, elementMapping, selectMethod, collectMethod);
    }

    private static ForEachAddEnumerableMapping? BuildCustomTypeMapping(
        MappingBuilderContext ctx,
        TypeMapping elementMapping)
    {
        if (!ctx.Target.HasAccessibleParameterlessConstructor())
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, ctx.Target);
            return null;
        }

        if (ctx.Compilation.GetTypeByMetadataName(_collectionIntfName) is not { } collectionSymbol)
            return null;

        return ctx.Target.ImplementsGeneric(collectionSymbol, out _)
            ? new ForEachAddEnumerableMapping(ctx.Source, ctx.Target, elementMapping)
            : null;
    }

    private static (bool CanMapWithLinq, string? CollectMethod) ResolveCollectMethodName(MappingBuilderContext ctx)
    {
        // if the target is an array we need to collect to array
        if (ctx.Target.IsArrayType())
            return (true, ToArrayMethodName);

        // if the target is an IEnumerable<T> don't collect at all.
        if (IsType(ctx, _enumerableIntfName, ctx.Target.OriginalDefinition))
            return (true, null);

        // if the target is IReadOnlyCollection<T>
        // and the count of the source is known (array, IReadOnlyCollection<T>, ICollection<T>) we collect to array
        // for performance/space reasons
        var targetIsReadOnlyCollection = IsType(ctx, _readOnlyCollectionIntfName, ctx.Target.OriginalDefinition);
        if (ctx.Compilation.GetTypeByMetadataName(_readOnlyCollectionIntfName) is not { } readOnlyCollectionSymbol
            || ctx.Compilation.GetTypeByMetadataName(_collectionIntfName) is not { } collectionSymbol)
        {
            return (false, null);
        }

        var sourceCountIsKnown =
            ctx.Source.IsArrayType()
            || ctx.Source.ImplementsGeneric(readOnlyCollectionSymbol, out _)
            || ctx.Source.ImplementsGeneric(collectionSymbol, out _);
        if (targetIsReadOnlyCollection && sourceCountIsKnown)
            return (true, ToArrayMethodName);

        // if target is a list, ICollection<T> or IReadOnlyCollection<T> collect with ToList()
        return targetIsReadOnlyCollection
            || IsType(ctx, _collectionIntfName, ctx.Target.OriginalDefinition)
            || IsType(ctx, _listClassName, ctx.Target.OriginalDefinition)
            ? (true, ToListMethodName)
            : (false, null);
    }

    private static bool IsType(MappingBuilderContext ctx, string typeFqn, ITypeSymbol? t)
    {
        var typeSymbol = ctx.Compilation.GetTypeByMetadataName(typeFqn);
        return SymbolEqualityComparer.Default.Equals(t, typeSymbol);
    }

    private static IMethodSymbol? ResolveLinqMethod(MappingBuilderContext ctx, string methodName)
    {
        if (ctx.Compilation.GetTypeByMetadataName(_linqEnumerableClassName) is not ITypeSymbol arrayType)
            return null;

        var method = arrayType.GetMembers(methodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m =>
                m.IsStatic
                && m.IsGenericMethod);

        return method;
    }

    private static ITypeSymbol? GetEnumeratedType(MappingBuilderContext ctx, ITypeSymbol type)
    {
        if (ctx.Compilation.GetTypeByMetadataName(_enumerableIntfName) is not { } enumerableSymbol)
            return null;

        return type.ImplementsGeneric(enumerableSymbol, out var enumerableIntf)
            ? enumerableIntf.TypeArguments[0]
            : null;
    }
}
