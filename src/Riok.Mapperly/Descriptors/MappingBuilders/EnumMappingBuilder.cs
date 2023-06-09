using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
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

        if (!ctx.IsConversionEnabled(MappingConversionType.EnumToEnum))
            return null;

        // map enums by strategy
        var config = ctx.GetConfigurationOrDefault<MapEnumAttribute>();
        var explicitMappings = BuildExplicitValueMapping(ctx);
        return config.Strategy switch
        {
            EnumMappingStrategy.ByName when ctx.IsExpression => BuildCastMappingAndDiagnostic(ctx),
            EnumMappingStrategy.ByValue when ctx.IsExpression && explicitMappings.Count > 0 => BuildCastMappingAndDiagnostic(ctx),
            EnumMappingStrategy.ByName => BuildNameMapping(ctx, explicitMappings, config.IgnoreCase),
            _ => BuildEnumToEnumCastMapping(ctx, explicitMappings),
        };
    }

    private static TypeMapping BuildCastMappingAndDiagnostic(MappingBuilderContext ctx)
    {
        ctx.ReportDiagnostic(
            DiagnosticDescriptors.EnumMappingStrategyByNameNotSupportedInProjectionMappings,
            ctx.Source.ToDisplayString(),
            ctx.Target.ToDisplayString()
        );
        return BuildEnumToEnumCastMapping(ctx, new Dictionary<IFieldSymbol, IFieldSymbol>(SymbolEqualityComparer.Default));
    }

    private static TypeMapping BuildEnumToEnumCastMapping(
        MappingBuilderContext ctx,
        IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> explicitMappings
    )
    {
        var explicitMappingSourceNames = explicitMappings.Keys.Select(x => x.Name).ToHashSet();
        var explicitMappingTargetNames = explicitMappings.Values.Select(x => x.Name).ToHashSet();
        var sourceValues = ctx.Source
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(x => !explicitMappingSourceNames.Contains(x.Name))
            .ToDictionary(field => field.Name, field => field.ConstantValue);
        var targetValues = ctx.Target
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(x => !explicitMappingTargetNames.Contains(x.Name))
            .ToDictionary(field => field.Name, field => field.ConstantValue);

        var missingTargetValues = targetValues.Where(field => !sourceValues.ContainsValue(field.Value));
        foreach (var member in missingTargetValues)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.TargetEnumValueNotMapped, member.Key, member.Value!, ctx.Target, ctx.Source);
        }

        var missingSourceValues = sourceValues.Where(field => !targetValues.ContainsValue(field.Value));
        foreach (var member in missingSourceValues)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.SourceEnumValueNotMapped, member.Key, member.Value!, ctx.Source, ctx.Target);
        }

        var fallbackMapping = new CastMapping(ctx.Source, ctx.Target);
        if (explicitMappings.Count == 0)
            return fallbackMapping;

        var explicitNameMappings = explicitMappings
            .Where(x => !x.Value.ConstantValue!.Equals(x.Key.ConstantValue))
            .ToDictionary(x => x.Key.Name, x => x.Value.Name);
        return new EnumNameMapping(ctx.Source, ctx.Target, explicitNameMappings, fallbackMapping);
    }

    private static EnumNameMapping BuildNameMapping(
        MappingBuilderContext ctx,
        IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> explicitMappings,
        bool ignoreCase
    )
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
                explicitMappings.GetValueOrDefault(source)
                ?? targetFieldsByName.GetValueOrDefault(source.Name)
                ?? targetFieldsByNameIgnoreCase.GetValueOrDefault(source.Name);
        }
        else
        {
            getTargetField = source => explicitMappings.GetValueOrDefault(source) ?? targetFieldsByName.GetValueOrDefault(source.Name);
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

        var missingSourceMembers = sourceFieldsByName.Where(field => !enumMemberMappings.ContainsKey(field.Key));
        foreach (var member in missingSourceMembers)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.SourceEnumValueNotMapped,
                member.Key,
                member.Value.ConstantValue!,
                ctx.Source,
                ctx.Target
            );
        }

        var missingTargetMembers = targetFieldsByName.Where(field => !enumMemberMappings.ContainsValue(field.Key));
        foreach (var member in missingTargetMembers)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.TargetEnumValueNotMapped,
                member.Key,
                member.Value.ConstantValue!,
                ctx.Target,
                ctx.Source
            );
        }

        return new EnumNameMapping(ctx.Source, ctx.Target, enumMemberMappings);
    }

    private static Dictionary<IFieldSymbol, IFieldSymbol> BuildExplicitValueMapping(MappingBuilderContext ctx)
    {
        var values = ctx.ListConfiguration<MapEnumValueAttribute, MapEnumValue>();
        var targetFieldsByExplicitValue = new Dictionary<IFieldSymbol, IFieldSymbol>(SymbolEqualityComparer.Default);
        foreach (var (sourceConstant, targetConstant) in values)
        {
            var source = sourceConstant.Type!.GetMembers().OfType<IFieldSymbol>().First(e => sourceConstant.Value!.Equals(e.ConstantValue));
            var target = targetConstant.Type!.GetMembers().OfType<IFieldSymbol>().First(e => targetConstant.Value!.Equals(e.ConstantValue));
            if (!SymbolEqualityComparer.Default.Equals(sourceConstant.Type, ctx.Source))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.SourceEnumValueDoesNotMatchSourceEnumType,
                    source,
                    sourceConstant.Value ?? 0,
                    source.Type,
                    ctx.Source
                );
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(targetConstant.Type, ctx.Target))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.TargetEnumValueDoesNotMatchTargetEnumType,
                    target,
                    targetConstant.Value ?? 0,
                    target.Type,
                    ctx.Target
                );
                continue;
            }

            if (targetFieldsByExplicitValue.ContainsKey(source))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.EnumSourceValueDuplicated, source, ctx.Source, ctx.Target);
            }
            else
            {
                targetFieldsByExplicitValue.Add(source, target);
            }
        }

        return targetFieldsByExplicitValue;
    }
}
