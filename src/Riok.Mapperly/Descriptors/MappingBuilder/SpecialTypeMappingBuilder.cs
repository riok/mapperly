using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.TypeMappings;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class SpecialTypeMappingBuilder
{
    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        return ctx.Target.SpecialType switch
        {
            SpecialType.System_Object => new CastMapping(ctx.Source, ctx.Target),
            _ => null,
        };
    }
}
