using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.TypeMappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class ImmutableTypeMappingBuilder
{
    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        return SymbolEqualityComparer.IncludeNullability.Equals(ctx.Source, ctx.Target) && ctx.Source.IsImmutable()
            ? new DirectAssignmentMapping(ctx.Source)
            : null;
    }
}
