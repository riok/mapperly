using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class SpecialTypeMappingBuilder
{
    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        return ctx.Target.SpecialType switch
        {
            SpecialType.System_Object when ctx.MapperConfiguration.UseDeepCloning
                => new CastMapping(ctx.Source, ctx.Target, ctx.FindOrBuildMapping(ctx.Source, ctx.Source)),
            SpecialType.System_Object => new CastMapping(ctx.Source, ctx.Target),
            _ => null,
        };
    }
}
