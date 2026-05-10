using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class NullableMappingBuilder
{
    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!TryBuildNonNullableMappingKey(ctx, out var mappingKey))
            return null;

        var delegateMapping = ctx.BuildMapping(mappingKey, MappingBuildingOptions.KeepUserSymbol | MappingBuildingOptions.EmbeddedMapping);
        return delegateMapping == null ? null : BuildNullDelegateMapping(ctx, delegateMapping);
    }

    public static IExistingTargetMapping? TryBuildExistingTargetMapping(MappingBuilderContext ctx)
    {
        if (!TryBuildNonNullableMappingKey(ctx, out var mappingKey))
            return null;

        var delegateMapping = ctx.FindOrBuildExistingTargetMapping(mappingKey);
        return delegateMapping == null ? null : new NullDelegateExistingTargetMapping(ctx.Source, ctx.Target, delegateMapping);
    }

    private static bool TryBuildNonNullableMappingKey(MappingBuilderContext ctx, out TypeMappingKey mappingKey)
    {
        var sourceIsNullable = ctx.Source.TryGetNonNullable(out var sourceNonNullable);
        var targetIsNullable = ctx.Target.TryGetNonNullable(out var targetNonNullable);
        if (!sourceIsNullable && !targetIsNullable)
        {
            mappingKey = default;
            return false;
        }

        mappingKey = new TypeMappingKey(sourceNonNullable ?? ctx.Source, targetNonNullable ?? ctx.Target);

        if (sourceIsNullable && !targetIsNullable)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NullableSourceTypeToNonNullableTargetType, ctx.Source, ctx.Target);
        }

        return true;
    }

    private static INewInstanceMapping BuildNullDelegateMapping(MappingBuilderContext ctx, INewInstanceMapping mapping)
    {
        var nullFallback = ctx.GetNullFallbackValue();

        if (mapping is not NewInstanceMethodMapping methodMapping)
            return new NullDelegateMapping(ctx.Source, ctx.Target, mapping, nullFallback);

        var additionalSourceParams = GetAdditionalSourceParameters(ctx);
        return new NullDelegateMethodMapping(
            ctx.Source,
            ctx.Target,
            methodMapping,
            nullFallback,
            ctx.Configuration.SupportedFeatures.NullableAttributes
        )
        {
            AdditionalSourceParameters = additionalSourceParams,
            AdditionalSourceMergeParameters = additionalSourceParams,
        };
    }

    private static IReadOnlyCollection<MethodParameter> GetAdditionalSourceParameters(MappingBuilderContext ctx)
    {
        if (ctx.UserSymbol == null)
            return [];

        var refHandlerOrdinal =
            ctx.UserSymbol.Parameters.FirstOrDefault(p => ctx.SymbolAccessor.HasAttribute<ReferenceHandlerAttribute>(p))?.Ordinal ?? -1;

        var sourceOrdinal =
            ctx.UserSymbol.Parameters.FirstOrDefault(p =>
                p.Ordinal != refHandlerOrdinal
                && !ctx.SymbolAccessor.HasAttribute<ReferenceHandlerAttribute>(p)
                && !ctx.SymbolAccessor.HasAttribute<MappingTargetAttribute>(p)
            )?.Ordinal
            ?? -1;

        return ctx
            .UserSymbol.Parameters.Where(p =>
                p.Ordinal != sourceOrdinal
                && p.Ordinal != refHandlerOrdinal
                && ctx.SymbolAccessor.HasAttribute<MapAdditionalSourceAttribute>(p)
            )
            .Select(p => ctx.SymbolAccessor.WrapMethodParameter(p))
            .ToList();
    }
}
