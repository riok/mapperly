using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class ToStringMappingBuilder
{

    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.ToStringMethod))
            return null;

        return ctx.Target.SpecialType == SpecialType.System_String
            ? new SourceObjectMethodMapping(ctx.Source, ctx.Target, nameof(ToString))
            : null;
    }
}
