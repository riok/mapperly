using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class DirectAssignmentMappingBuilder
{
    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        return SymbolEqualityComparer.IncludeNullability.Equals(ctx.Source, ctx.Target)
            && (ctx.TargetRefKind == RefKind.None || ctx.TargetRefKind == RefKind.Out)
            && (!ctx.MapperConfiguration.UseDeepCloning || ctx.Source.IsImmutable())
            ? new DirectAssignmentMapping(ctx.Source)
            : null;
    }
}
