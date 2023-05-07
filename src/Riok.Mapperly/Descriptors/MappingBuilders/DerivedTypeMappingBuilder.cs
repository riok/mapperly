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
        var configs = ctx.ListConfiguration<MapDerivedTypeAttribute, MapDerivedType>()
            .Concat(ctx.ListConfiguration<MapDerivedTypeAttribute<object, object>, MapDerivedType>())
            .ToList();
        if (configs.Count == 0)
            return null;

        var derivedTypeMappings = BuildDerivedTypeMappings(ctx, configs);
        return ctx.IsExpression
            ? new DerivedTypeIfExpressionMapping(ctx.Source, ctx.Target, derivedTypeMappings)
            : new DerivedTypeSwitchMapping(ctx.Source, ctx.Target, derivedTypeMappings);
    }

    private static IReadOnlyCollection<ITypeMapping> BuildDerivedTypeMappings(
        MappingBuilderContext ctx,
        IReadOnlyCollection<MapDerivedType> configs
    )
    {
        var derivedTypeMappingSourceTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var derivedTypeMappings = new List<ITypeMapping>(configs.Count);

        foreach (var config in configs)
        {
            // set reference types non-nullable as they can never be null when type-switching.
            var sourceType = config.SourceType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            if (!derivedTypeMappingSourceTypes.Add(sourceType))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.DerivedSourceTypeDuplicated, sourceType);
                continue;
            }

            if (!sourceType.IsAssignableTo(ctx.Compilation, ctx.Source))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.DerivedSourceTypeIsNotAssignableToParameterType, sourceType, ctx.Source);
                continue;
            }

            var targetType = config.TargetType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
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
