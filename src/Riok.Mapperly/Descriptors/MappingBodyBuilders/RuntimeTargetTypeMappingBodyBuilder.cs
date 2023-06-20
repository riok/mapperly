using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

public static class RuntimeTargetTypeMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedNewInstanceGenericTypeMapping mapping)
    {
        // source nulls are filtered out by the type switch arms,
        // therefore set source type always to nun-nullable
        // as non-nullables are also assignable to nullables.
        var mappings = GetUserMappingCandidates(ctx)
            .Where(x => mapping.TypeParameters.CanConsumeTypes(ctx.Compilation, x.SourceType.NonNullable(), x.TargetType));

        BuildMappingBody(ctx, mapping, mappings);
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedNewInstanceRuntimeTargetTypeMapping mapping)
    {
        // source nulls are filtered out by the type switch arms,
        // therefore set source type always to nun-nullable
        // as non-nullables are also assignable to nullables.
        var mappings = GetUserMappingCandidates(ctx)
            .Where(
                x =>
                    x.SourceType.NonNullable().IsAssignableTo(ctx.Compilation, mapping.SourceType)
                    && x.TargetType.IsAssignableTo(ctx.Compilation, mapping.TargetType)
            );

        BuildMappingBody(ctx, mapping, mappings);
    }

    private static IEnumerable<ITypeMapping> GetUserMappingCandidates(MappingBuilderContext ctx)
    {
        foreach (var userMapping in ctx.UserMappings)
        {
            // exclude runtime target type mappings
            if (userMapping is UserDefinedNewInstanceRuntimeTargetTypeMapping)
                continue;

            if (userMapping.CallableByOtherMappings)
                yield return userMapping;

            if (userMapping is IDelegateUserMapping { DelegateMapping.CallableByOtherMappings: true } delegateUserMapping)
                yield return delegateUserMapping.DelegateMapping;
        }
    }

    private static void BuildMappingBody(
        MappingBuilderContext ctx,
        UserDefinedNewInstanceRuntimeTargetTypeMapping mapping,
        IEnumerable<ITypeMapping> childMappings
    )
    {
        // include derived type mappings declared on this user defined method
        var derivedTypeMappings = DerivedTypeMappingBuilder.TryBuildContainedMappings(ctx, true);
        if (derivedTypeMappings != null)
        {
            childMappings = derivedTypeMappings.Concat(childMappings);
        }

        // prefer non-nullable return types
        // and prefer types with a higher inheritance level
        // over types with a lower inheritance level
        // in the type switch
        // to use the most specific mapping
        var runtimeTargetTypeMappings = childMappings
            .OrderByDescending(x => x.SourceType.GetInheritanceLevel())
            .ThenByDescending(x => x.TargetType.GetInheritanceLevel())
            .ThenBy(x => x.TargetType.IsNullable())
            .GroupBy(x => new TypeMappingKey(x, false))
            .Select(x => x.First())
            .Select(x => new RuntimeTargetTypeMapping(x, x.TargetType.IsAssignableTo(ctx.Compilation, ctx.Target)));
        mapping.AddMappings(runtimeTargetTypeMappings);
    }
}
