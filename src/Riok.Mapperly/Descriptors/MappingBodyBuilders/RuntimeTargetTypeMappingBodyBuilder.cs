using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
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
            .Where(x =>
                ctx.GenericTypeChecker.InferAndCheckTypes(
                    mapping.Method.TypeParameters,
                    (mapping.SourceType, x.SourceType.NonNullable()),
                    (mapping.TargetType, x.TargetType)
                ).Success
            );

        BuildMappingBody(ctx, mapping, mappings);
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedNewInstanceRuntimeTargetTypeMapping mapping)
    {
        // source nulls are filtered out by the type switch arms,
        // therefore set source type always to nun-nullable
        // as non-nullables are also assignable to nullables.
        var mappings = GetUserMappingCandidates(ctx)
            .Where(x =>
                ctx.SymbolAccessor.CanAssign(x.SourceType.NonNullable(), mapping.SourceType)
                && ctx.SymbolAccessor.CanAssign(x.TargetType, mapping.TargetType)
            );

        BuildMappingBody(ctx, mapping, mappings);
    }

    private static IEnumerable<INewInstanceUserMapping> GetUserMappingCandidates(MappingBuilderContext ctx) =>
        ctx
            .NewInstanceMappings.Select(x => x.Value)
            .Where(x => x is not UserDefinedNewInstanceRuntimeTargetTypeMapping)
            .OfType<INewInstanceUserMapping>();

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
            .OfType<INewInstanceMapping>()
            .OrderByDescending(x => x.SourceType.GetInheritanceLevel())
            .ThenByDescending(x => x.TargetType.GetInheritanceLevel())
            .ThenBy(x => x.TargetType.IsNullable())
            .GroupBy(x => new TypeMappingKey(x, includeNullability: false))
            .Select(x => x.First())
            .Select(x => new RuntimeTargetTypeMapping(x, ctx.Compilation.HasImplicitConversion(x.TargetType, ctx.Target)))
            .ToList();

        if (runtimeTargetTypeMappings.Count == 0)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.RuntimeTargetTypeMappingNoContentMappings);
        }

        mapping.AddMappings(runtimeTargetTypeMappings);
    }
}
