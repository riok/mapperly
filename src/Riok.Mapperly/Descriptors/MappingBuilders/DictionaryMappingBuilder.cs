using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class DictionaryMappingBuilder
{
    private const string SetterIndexerPropertyName = "set_Item";

    private const string ToImmutableDictionaryMethodName = "global::System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary";
    private const string ToImmutableSortedDictionaryMethodName =
        "global::System.Collections.Immutable.ImmutableSortedDictionary.ToImmutableSortedDictionary";

    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Dictionary))
            return null;

        if (ctx.CollectionInfos == null)
            return null;

        if (BuildKeyValueMapping(ctx) is not var (keyMapping, valueMapping))
            return null;

        // target is of type IDictionary<,>, IReadOnlyDictionary<,> or Dictionary<,>.
        // The constructed type should be Dictionary<,>
        if (
            ctx.CollectionInfos?.Target.CollectionType
            is CollectionType.Dictionary
                or CollectionType.IDictionary
                or CollectionType.IReadOnlyDictionary
        )
        {
            var targetDictionarySymbol = ctx.Types.Get(typeof(Dictionary<,>)).Construct(keyMapping.TargetType, valueMapping.TargetType);
            ctx.ObjectFactories.TryFindObjectFactory(ctx.Source, ctx.Target, out var dictionaryObjectFactory);
            return new ForEachSetDictionaryMapping(
                ctx.Source,
                ctx.Target,
                keyMapping,
                valueMapping,
                ctx.CollectionInfos.Source.CountIsKnown,
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
            && !ctx.SymbolAccessor.HasAccessibleParameterlessConstructor(ctx.Target)
        )
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, ctx.Target);
            return null;
        }

        if (!ctx.CollectionInfos!.Target.ImplementedTypes.HasFlag(CollectionType.IDictionary))
            return null;

        var ensureCapacityStatement = EnsureCapacityBuilder.TryBuildEnsureCapacity(ctx);
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

        if (ctx.CollectionInfos == null)
            return null;

        if (!ctx.CollectionInfos.Target.ImplementedTypes.HasFlag(CollectionType.IDictionary))
            return null;

        if (BuildKeyValueMapping(ctx) is not var (keyMapping, valueMapping))
            return null;

        // if target is an immutable dictionary then don't create a foreach loop
        if (ctx.CollectionInfos.Target.ImplementedTypes.HasFlag(CollectionType.IImmutableDictionary))
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember);
            return null;
        }

        // add values to dictionary by setting key values in a foreach loop
        var ensureCapacityStatement = EnsureCapacityBuilder.TryBuildEnsureCapacity(ctx);

        return new ForEachSetDictionaryExistingTargetMapping(
            ctx.Source,
            ctx.Target,
            keyMapping,
            valueMapping,
            explicitCast: GetExplicitIndexer(ctx),
            ensureCapacity: ensureCapacityStatement
        );
    }

    private static (INewInstanceMapping, INewInstanceMapping)? BuildKeyValueMapping(MappingBuilderContext ctx)
    {
        if (ctx.CollectionInfos!.Target.GetDictionaryKeyValueTypes(ctx) is not var (targetKeyType, targetValueType))
            return null;

        if (ctx.CollectionInfos.Source.GetEnumeratedKeyValueTypes(ctx.Types) is not var (sourceKeyType, sourceValueType))
            return null;

        var keyMapping = ctx.FindOrBuildMapping(sourceKeyType, targetKeyType);
        if (keyMapping == null)
            return null;

        var valueMapping = ctx.FindOrBuildMapping(sourceValueType, targetValueType);
        if (valueMapping == null)
            return null;

        return (keyMapping, valueMapping);
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

    private static LinqDictionaryMapping? ResolveImmutableCollectMethod(
        MappingBuilderContext ctx,
        INewInstanceMapping keyMapping,
        INewInstanceMapping valueMapping
    )
    {
        return ctx.CollectionInfos!.Target.CollectionType switch
        {
            CollectionType.ImmutableSortedDictionary
                => new LinqDictionaryMapping(ctx.Source, ctx.Target, ToImmutableSortedDictionaryMethodName, keyMapping, valueMapping),
            CollectionType.ImmutableDictionary
            or CollectionType.IImmutableDictionary
                => new LinqDictionaryMapping(ctx.Source, ctx.Target, ToImmutableDictionaryMethodName, keyMapping, valueMapping),

            _ => null,
        };
    }
}
