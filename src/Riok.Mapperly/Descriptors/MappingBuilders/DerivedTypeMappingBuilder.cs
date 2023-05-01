using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class DerivedTypeMappingBuilder
{
    public static ITypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        var derivedTypeMappings = TryBuildContainedMappings(ctx);
        if (derivedTypeMappings == null)
            return null;

        return ctx.IsExpression
            ? new DerivedTypeIfExpressionMapping(ctx.Source, ctx.Target, derivedTypeMappings)
            : new DerivedTypeSwitchMapping(ctx.Source, ctx.Target, derivedTypeMappings);
    }

    public static IReadOnlyCollection<ITypeMapping>? TryBuildContainedMappings(
        MappingBuilderContext ctx,
        bool duplicatedSourceTypesAllowed = false
    )
    {
        var configs = ctx.ListConfiguration<MapDerivedTypeAttribute, MapDerivedType>()
            .Concat(ctx.ListConfiguration<MapDerivedTypeAttribute<object, object>, MapDerivedType>())
            .ToList();
        return configs.Count == 0 ? null : BuildContainedMappings(ctx, configs, duplicatedSourceTypesAllowed);
    }

    private static IReadOnlyCollection<ITypeMapping> BuildContainedMappings(
        MappingBuilderContext ctx,
        IReadOnlyCollection<MapDerivedType> configs,
        bool duplicatedSourceTypesAllowed
    )
    {
        var derivedTypeMappingSourceTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var derivedTypeMappings = new List<ITypeMapping>(configs.Count);

        foreach (var config in configs)
        {
            // set types non-nullable as they can never be null when type-switching.
            var sourceType = config.SourceType.NonNullable();
            if (!duplicatedSourceTypesAllowed && !derivedTypeMappingSourceTypes.Add(sourceType))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.DerivedSourceTypeDuplicated, sourceType);
                continue;
            }

            if (!sourceType.IsAssignableTo(ctx.Compilation, ctx.Source))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.DerivedSourceTypeIsNotAssignableToParameterType, sourceType, ctx.Source);
                continue;
            }

            var targetType = config.TargetType.NonNullable();
            if (!targetType.IsAssignableTo(ctx.Compilation, ctx.Target))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.DerivedTargetTypeIsNotAssignableToReturnType, targetType, ctx.Target);
                continue;
            }

            var mapping = ctx.FindOrBuildMapping(sourceType, targetType);
            if (mapping == null)
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping, sourceType, targetType);
                continue;
            }

            derivedTypeMappings.Add(mapping);
        }

        return derivedTypeMappings;
    }
}
