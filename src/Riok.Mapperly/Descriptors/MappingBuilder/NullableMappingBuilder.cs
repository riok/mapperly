using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class NullableMappingBuilder
{
    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        var sourceIsNullable = ctx.Source.TryGetNonNullable(out var sourceNonNullable);
        var targetIsNullable = ctx.Target.TryGetNonNullable(out var targetNonNullable);
        if (!sourceIsNullable && !targetIsNullable)
            return null;

        var delegateMapping = ctx.BuildDelegateMapping(sourceNonNullable ?? ctx.Source, targetNonNullable ?? ctx.Target);
        return delegateMapping == null
            ? null
            : BuildNullDelegateMapping(ctx, delegateMapping);
    }

    private static TypeMapping BuildNullDelegateMapping(MappingBuilderContext ctx, ITypeMapping mapping)
    {
        var nullFallback = ctx.GetNullFallbackValue();
        return mapping switch
        {
            MethodMapping methodMapping => new NullDelegateMethodMapping(
                ctx.Source,
                ctx.Target,
                methodMapping,
                nullFallback),
            _ => new NullDelegateMapping(ctx.Source, ctx.Target, mapping, nullFallback),
        };
    }
}
