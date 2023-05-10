using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

public static class RuntimeTargetTypeMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedNewInstanceRuntimeTargetTypeMapping mapping)
    {
        // source nulls are filtered out by the type switch arms,
        // therefore set source type always to nun-nullable
        // as non-nullables are also assignable to nullables.
        IEnumerable<ITypeMapping> mappings = ctx.CallableUserMappings.Where(
            x =>
                x.SourceType.NonNullable().IsAssignableTo(ctx.Compilation, mapping.SourceType)
                && x.TargetType.IsAssignableTo(ctx.Compilation, mapping.TargetType)
        );

        // include derived type mappings declared on this user defined method
        var derivedTypeMappings = DerivedTypeMappingBuilder.TryBuildContainedMappings(ctx, true);
        if (derivedTypeMappings != null)
        {
            mappings = derivedTypeMappings.Concat(mappings);
        }

        // prefer non-nullable return types
        // and prefer types with a higher inheritance level
        // over types with a lower inheritance level
        // in the type switch
        // to use the most specific mapping
        mappings = mappings
            .OrderByDescending(x => x.SourceType.GetInheritanceLevel())
            .ThenByDescending(x => x.TargetType.GetInheritanceLevel())
            .ThenBy(x => x.TargetType.IsNullable())
            .GroupBy(x => new TypeMappingKey(x, false))
            .Select(x => x.First());
        mapping.AddMappings(mappings);
    }
}
