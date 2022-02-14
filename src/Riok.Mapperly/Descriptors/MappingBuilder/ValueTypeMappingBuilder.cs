using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.TypeMappings;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class ValueTypeMappingBuilder
{
    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        return SymbolEqualityComparer.Default.Equals(ctx.Source, ctx.Target) && ctx.Source.IsValueType
            ? new DirectAssignmentMapping(ctx.Source)
            : null;
    }
}
