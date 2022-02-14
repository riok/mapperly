using Microsoft.CodeAnalysis.CSharp;
using Riok.Mapperly.Descriptors.TypeMappings;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class ImplicitCastMappingBuilder
{
    public static CastMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        var conversion = ctx.Compilation.ClassifyConversion(ctx.Source, ctx.Target);
        return conversion.IsImplicit
            ? new CastMapping(ctx.Source, ctx.Target)
            : null;
    }
}
