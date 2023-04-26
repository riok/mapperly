using System.Linq;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class EnumMappingBuilder
{
    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        var sourceIsEnum = ctx.Source.TryGetEnumUnderlyingType(out var sourceEnumType);
        var targetIsEnum = ctx.Target.TryGetEnumUnderlyingType(out var targetEnumType);

        // none is an enum
        if (!sourceIsEnum && !targetIsEnum)
            return null;

        // one is an enum, other may be an underlying type (eg. int)
        if (!sourceIsEnum || !targetIsEnum)
        {
            return
                ctx.IsConversionEnabled(MappingConversionType.ExplicitCast)
                && ctx.FindOrBuildMapping(sourceEnumType ?? ctx.Source, targetEnumType ?? ctx.Target) is { } delegateMapping
                ? new CastMapping(ctx.Source, ctx.Target, delegateMapping)
                : null;
        }

        // since enums are immutable they can be directly assigned if they are of the same type
        if (SymbolEqualityComparer.IncludeNullability.Equals(ctx.Source, ctx.Target))
            return new DirectAssignmentMapping(ctx.Source);

        if (!ctx.IsConversionEnabled(MappingConversionType.EnumToEnum))
            return null;

        // map enums by strategy
        var config = ctx.GetConfigurationOrDefault<MapEnumAttribute>();
        return config.Strategy switch
        {
            EnumMappingStrategy.ByName when ctx.IsExpression => BuildCastMappingAndDiagnostic(ctx),
            EnumMappingStrategy.ByName => BuildNameMapping(ctx, config.IgnoreCase),
            _ => BuildEnumToEnumCastMapping(ctx),
        };
    }

    private static TypeMapping BuildCastMappingAndDiagnostic(MappingBuilderContext ctx)
    {
        ctx.ReportDiagnostic(
            DiagnosticDescriptors.EnumMappingStrategyByNameNotSupportedInProjectionMappings,
            ctx.Source.ToDisplayString(),
            ctx.Target.ToDisplayString()
        );
        return BuildEnumToEnumCastMapping(ctx);
    }

    private static TypeMapping BuildEnumToEnumCastMapping(MappingBuilderContext ctx)
    {
        var sourceValues = ctx.Source.GetMembers().OfType<IFieldSymbol>().Select(symbol => symbol.ConstantValue).ToList();

        var targetValues = ctx.Target.GetMembers().OfType<IFieldSymbol>().Select(symbol => symbol.ConstantValue).ToList();

        var missingTargetValues = targetValues.Except(sourceValues);
        if (missingTargetValues.Any())
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NotAllTargetEnumValuesMapped, missingTargetValues.First()!, ctx.Target, ctx.Source);
        }

        var missingSourceValues = sourceValues.Except(targetValues);
        if (missingSourceValues.Any())
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NotAllSourceEnumValuesMapped, missingSourceValues.First()!, ctx.Source, ctx.Target);
        }

        return new CastMapping(ctx.Source, ctx.Target);
    }

    private static TypeMapping BuildNameMapping(MappingBuilderContext ctx, bool ignoreCase)
    {
        var targetFieldsByName = ctx.Target.GetMembers().OfType<IFieldSymbol>().ToDictionary(x => x.Name);
        var sourceFieldsByName = ctx.Source.GetMembers().OfType<IFieldSymbol>().ToDictionary(x => x.Name);

        Func<IFieldSymbol, IFieldSymbol?> getTargetField;
        if (ignoreCase)
        {
            var targetFieldsByNameIgnoreCase = targetFieldsByName
                .DistinctBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            getTargetField = source =>
                targetFieldsByName.GetValueOrDefault(source.Name) ?? targetFieldsByNameIgnoreCase.GetValueOrDefault(source.Name);
        }
        else
        {
            getTargetField = source => targetFieldsByName.GetValueOrDefault(source.Name);
        }

        var enumMemberMappings = ctx.Source
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Select(x => (Source: x, Target: getTargetField(x)))
            .Where(x => x.Target != null)
            .ToDictionary(x => x.Source.Name, x => x.Target!.Name);

        if (enumMemberMappings.Count == 0)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.EnumNameMappingNoOverlappingValuesFound, ctx.Source, ctx.Target);
        }

        var missingSourceMembers = sourceFieldsByName.Select(item => item.Key).Except(enumMemberMappings.Select(mapping => mapping.Key));

        if (missingSourceMembers.Any())
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NotAllSourceEnumValuesMapped, missingSourceMembers.First(), ctx.Source, ctx.Target);
        }

        var missingTargetMembers = targetFieldsByName.Select(item => item.Key).Except(enumMemberMappings.Select(mapping => mapping.Value));

        if (missingTargetMembers.Any())
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NotAllTargetEnumValuesMapped, missingTargetMembers.First(), ctx.Target, ctx.Source);
        }

        return new EnumNameMapping(ctx.Source, ctx.Target, enumMemberMappings);
    }
}
