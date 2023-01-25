using Microsoft.CodeAnalysis.CSharp;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class ImplicitCastMappingBuilder
{
    public static CastMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.ImplicitCast))
            return null;

        if (ctx.MapperConfiguration.UseDeepCloning && !ctx.Source.IsImmutable() && !ctx.Target.IsImmutable())
            return null;

        var conversion = ctx.Compilation.ClassifyConversion(ctx.Source, ctx.Target);
        return conversion.IsImplicit
            ? new CastMapping(ctx.Source, ctx.Target)
            : null;
    }
}
