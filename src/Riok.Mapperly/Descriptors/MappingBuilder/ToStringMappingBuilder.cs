using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.TypeMappings;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class ToStringMappingBuilder
{

    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        return ctx.Target.SpecialType == SpecialType.System_String
            ? new SourceObjectMethodMapping(ctx.Source, ctx.Target, nameof(ToString))
            : null;
    }
}
