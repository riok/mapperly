using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class DictionaryMappingBuilder
{
    private const string CountPropertyName = nameof(IDictionary<object, object>.Count);
    private const string SetterIndexerPropertyName = "set_Item";

    private const string ToImmutableDictionaryMethodName = nameof(ImmutableDictionary.ToImmutableDictionary);
    private const string ToImmutableSortedDictionaryMethodName = nameof(ImmutableSortedDictionary.ToImmutableSortedDictionary);

    public static ITypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Dictionary))
            return null;

        if (BuildKeyValueMapping(ctx) is not var (keyMapping, valueMapping))
            return null;

        // target is of type IDictionary<,>, IReadOnlyDictionary<,> or Dictionary<,>.
        // The constructed type should be Dictionary<,>
        if (IsDictionaryType(ctx, ctx.Target))
        {
            var sourceHasCount = ctx.Source
                .GetAllProperties(CountPropertyName)
                .Any(x => !x.IsStatic && !x.IsIndexer && !x.IsWriteOnly && x.Type.SpecialType == SpecialType.System_Int32);

            var targetDictionarySymbol = ctx.Types.Get(typeof(Dictionary<,>)).Construct(keyMapping.TargetType, valueMapping.TargetType);
            ctx.ObjectFactories.TryFindObjectFactory(ctx.Source, ctx.Target, out var dictionaryObjectFactory);
            return new ForEachSetDictionaryMapping(
                ctx.Source,
                ctx.Target,
                keyMapping,
                valueMapping,
                sourceHasCount,
                targetDictionarySymbol,
                dictionaryObjectFactory
            );
        }

        // if target is an immutable dictionary then use LinqDictionaryMapper
        var immutableLinqMapping = ResolveImmutableCollectMethod(ctx, keyMapping, valueMapping);
        if (immutableLinqMapping != null)
            return immutableLinqMapping;

        // the target is not a well known dictionary type
        // it should have a an object factory or a parameterless public ctor
        if (
            !ctx.ObjectFactories.TryFindObjectFactory(ctx.Source, ctx.Target, out var objectFactory)
            && !ctx.Target.HasAccessibleParameterlessConstructor()
        )
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, ctx.Target);
            return null;
        }

        if (!ctx.Target.ImplementsGeneric(ctx.Types.Get(typeof(IDictionary<,>)), out _))
            return null;

        var ensureCapacityStatement = EnsureCapacityBuilder.TryBuildEnsureCapacity(ctx.Source, ctx.Target, ctx.Types);

        return new ForEachSetDictionaryMapping(
            ctx.Source,
            ctx.Target,
            keyMapping,
            valueMapping,
            false,
            objectFactory: objectFactory,
            explicitCast: GetExplicitIndexer(ctx),
            ensureCapacity: ensureCapacityStatement
        );
    }

    public static IExistingTargetMapping? TryBuildExistingTargetMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Dictionary))
            return null;

        if (!ctx.Target.ImplementsGeneric(ctx.Types.Get(typeof(IDictionary<,>)), out _))
            return null;

        if (BuildKeyValueMapping(ctx) is not var (keyMapping, valueMapping))
            return null;

        // if target is an immutable dictionary then don't create a foreach loop
        if (ctx.Target.OriginalDefinition.ImplementsGeneric(ctx.Types.Get(typeof(IImmutableDictionary<,>)), out _))
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember);
            return null;
        }

        // add values to dictionary by setting key values in a foreach loop
        var ensureCapacityStatement = EnsureCapacityBuilder.TryBuildEnsureCapacity(ctx.Source, ctx.Target, ctx.Types);

        return new ForEachSetDictionaryExistingTargetMapping(
            ctx.Source,
            ctx.Target,
            keyMapping,
            valueMapping,
            explicitCast: GetExplicitIndexer(ctx),
            ensureCapacity: ensureCapacityStatement
        );
    }

    private static (ITypeMapping, ITypeMapping)? BuildKeyValueMapping(MappingBuilderContext ctx)
    {
        if (GetDictionaryKeyValueTypes(ctx, ctx.Target) is not var (targetKeyType, targetValueType))
            return null;

        if (GetEnumerableKeyValueTypes(ctx, ctx.Source) is not var (sourceKeyType, sourceValueType))
            return null;

        var keyMapping = ctx.FindOrBuildMapping(sourceKeyType, targetKeyType);
        if (keyMapping == null)
            return null;

        var valueMapping = ctx.FindOrBuildMapping(sourceValueType, targetValueType);
        if (valueMapping == null)
            return null;

        return (keyMapping, valueMapping);
    }

    private static bool IsDictionaryType(MappingBuilderContext ctx, ITypeSymbol symbol)
    {
        if (symbol is not INamedTypeSymbol namedSymbol)
            return false;

        return SymbolEqualityComparer.Default.Equals(namedSymbol.ConstructedFrom, ctx.Types.Get(typeof(Dictionary<,>)))
            || SymbolEqualityComparer.Default.Equals(namedSymbol.ConstructedFrom, ctx.Types.Get(typeof(IDictionary<,>)))
            || SymbolEqualityComparer.Default.Equals(namedSymbol.ConstructedFrom, ctx.Types.Get(typeof(IReadOnlyDictionary<,>)));
    }

    private static (ITypeSymbol, ITypeSymbol)? GetDictionaryKeyValueTypes(MappingBuilderContext ctx, ITypeSymbol t)
    {
        if (t.ImplementsGeneric(ctx.Types.Get(typeof(IDictionary<,>)), out var dictionaryImpl))
        {
            return (dictionaryImpl.TypeArguments[0], dictionaryImpl.TypeArguments[1]);
        }

        if (t.ImplementsGeneric(ctx.Types.Get(typeof(IReadOnlyDictionary<,>)), out var readOnlyDictionaryImpl))
        {
            return (readOnlyDictionaryImpl.TypeArguments[0], readOnlyDictionaryImpl.TypeArguments[1]);
        }

        return null;
    }

    private static (ITypeSymbol, ITypeSymbol)? GetEnumerableKeyValueTypes(MappingBuilderContext ctx, ITypeSymbol t)
    {
        if (!t.ImplementsGeneric(ctx.Types.Get(typeof(IEnumerable<>)), out var enumerableImpl))
            return null;

        if (enumerableImpl.TypeArguments[0] is not INamedTypeSymbol enumeratedType)
            return null;

        if (!SymbolEqualityComparer.Default.Equals(enumeratedType.ConstructedFrom, ctx.Types.Get(typeof(KeyValuePair<,>))))
            return null;

        return (enumeratedType.TypeArguments[0], enumeratedType.TypeArguments[1]);
    }

    private static INamedTypeSymbol? GetExplicitIndexer(MappingBuilderContext ctx)
    {
        if (
            ctx.Target.ImplementsGeneric(
                ctx.Types.Get(typeof(IDictionary<,>)),
                SetterIndexerPropertyName,
                out var typedInter,
                out var isExplicit
            ) && !isExplicit
        )
            return null;

        return typedInter;
    }

    private static LinqDicitonaryMapping? ResolveImmutableCollectMethod(
        MappingBuilderContext ctx,
        ITypeMapping keyMapping,
        ITypeMapping valueMapping
    )
    {
        if (SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(ImmutableSortedDictionary<,>))))
            return new LinqDicitonaryMapping(
                ctx.Source,
                ctx.Target,
                ctx.Types.Get(typeof(ImmutableSortedDictionary)).GetStaticGenericMethod(ToImmutableSortedDictionaryMethodName)!,
                keyMapping,
                valueMapping
            );

        // if target is an ImmutableDictionary or IImmutableDictionary
        if (
            SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(IImmutableDictionary<,>)))
            || SymbolEqualityComparer.Default.Equals(ctx.Target.OriginalDefinition, ctx.Types.Get(typeof(ImmutableDictionary<,>)))
        )
            return new LinqDicitonaryMapping(
                ctx.Source,
                ctx.Target,
                ctx.Types.Get(typeof(ImmutableDictionary)).GetStaticGenericMethod(ToImmutableDictionaryMethodName)!,
                keyMapping,
                valueMapping
            );

        return null;
    }
}
