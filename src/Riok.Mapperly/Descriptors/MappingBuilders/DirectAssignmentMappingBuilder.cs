using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class DirectAssignmentMappingBuilder
{
    public static NewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (ctx.UseCloning && !ctx.Source.IsImmutable())
        {
            return null;
        }

        if (!SymbolEqualityComparer.IncludeNullability.Equals(ctx.Source, ctx.Target))
        {
            return null;
        }

        return new DirectAssignmentMapping(ctx.Source);
    }
}
