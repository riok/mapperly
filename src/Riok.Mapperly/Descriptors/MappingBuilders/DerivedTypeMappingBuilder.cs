using Microsoft.CodeAnalysis;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class DerivedTypeMappingBuilder
{
    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        var derivedTypeMappings = TryBuildContainedMappings(ctx);
        if (derivedTypeMappings == null)
            return null;

        return ctx.IsExpression
            ? new DerivedTypeIfExpressionMapping(ctx.Source, ctx.Target, derivedTypeMappings)
            : new DerivedTypeSwitchMapping(ctx.Source, ctx.Target, derivedTypeMappings);
    }

    public static IExistingTargetMapping? TryBuildExistingTargetMapping(MappingBuilderContext ctx)
    {
        var derivedTypeMappings = TryBuildExistingTargetContainedMappings(ctx);
        return derivedTypeMappings == null ? null : new DerivedExistingTargetTypeSwitchMapping(ctx.Source, ctx.Target, derivedTypeMappings);
    }

    public static IReadOnlyCollection<INewInstanceMapping>? TryBuildContainedMappings(
        MappingBuilderContext ctx,
        bool duplicatedSourceTypesAllowed = false
    )
    {
        return ctx.Configuration.DerivedTypes.Count == 0
            ? null
            : BuildContainedMappings(ctx, ctx.Configuration.DerivedTypes, ctx.FindOrBuildMapping, duplicatedSourceTypesAllowed);
    }

    private static IReadOnlyCollection<IExistingTargetMapping>? TryBuildExistingTargetContainedMappings(
        MappingBuilderContext ctx,
        bool duplicatedSourceTypesAllowed = false
    )
    {
        return ctx.Configuration.DerivedTypes.Count == 0
            ? null
            : BuildContainedMappings(
                ctx,
                ctx.Configuration.DerivedTypes,
                (source, target, options, _) => ctx.FindOrBuildExistingTargetMapping(source, target, options),
                duplicatedSourceTypesAllowed
            );
    }

    private static IReadOnlyCollection<TMapping> BuildContainedMappings<TMapping>(
        MappingBuilderContext ctx,
        IReadOnlyCollection<DerivedTypeMappingConfiguration> configs,
        Func<ITypeSymbol, ITypeSymbol, MappingBuildingOptions, Location?, TMapping?> findOrBuildMapping,
        bool duplicatedSourceTypesAllowed
    )
        where TMapping : ITypeMapping
    {
        var derivedTypeMappingSourceTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var derivedTypeMappings = new List<TMapping>(configs.Count);

        foreach (var config in configs)
        {
            // set types non-nullable as they can never be null when type-switching.
            var sourceType = config.SourceType.NonNullable();
            var targetType = config.TargetType.NonNullable();
            if (!duplicatedSourceTypesAllowed && !derivedTypeMappingSourceTypes.Add(sourceType))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.DerivedSourceTypeDuplicated, sourceType);
                continue;
            }

            var typeCheckerResult = ctx.GenericTypeChecker.InferAndCheckTypes(
                ctx.UserSymbol!.TypeParameters,
                (ctx.Source, sourceType),
                (ctx.Target, targetType)
            );
            if (!typeCheckerResult.Success)
            {
                if (ReferenceEquals(sourceType, typeCheckerResult.FailedArgument))
                {
                    ctx.ReportDiagnostic(DiagnosticDescriptors.DerivedSourceTypeIsNotAssignableToParameterType, sourceType, ctx.Source);
                }
                else
                {
                    ctx.ReportDiagnostic(DiagnosticDescriptors.DerivedTargetTypeIsNotAssignableToReturnType, targetType, ctx.Target);
                }

                continue;
            }

            var mapping = findOrBuildMapping(
                sourceType,
                targetType,
                MappingBuildingOptions.KeepUserSymbol | MappingBuildingOptions.MarkAsReusable | MappingBuildingOptions.IgnoreDerivedTypes,
                config.Location
            );
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
