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
            SpecialType.System_String when ctx.Source.SpecialType == SpecialType.System_String => new DirectAssignmentMapping(ctx.Source),
            _ => null,
        };
    }
}
