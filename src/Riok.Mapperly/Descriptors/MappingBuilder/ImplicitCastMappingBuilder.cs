using Microsoft.CodeAnalysis.CSharp;
using Riok.Mapperly.Descriptors.TypeMappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class ImplicitCastMappingBuilder
{
    public static CastMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (ctx.MapperConfiguration.UseDeepCloning && !ctx.Source.IsImmutable() && !ctx.Target.IsImmutable())
            return null;

        var conversion = ctx.Compilation.ClassifyConversion(ctx.Source, ctx.Target);
        return conversion.IsImplicit
            ? new CastMapping(ctx.Source, ctx.Target)
            : null;
    }
}
