using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.Enums;
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
        var explicitMappings = BuildExplicitValueMapping(ctx);
        return ctx.Configuration.Enum.Strategy switch
        {
            EnumMappingStrategy.ByName when ctx.IsExpression => BuildCastMappingAndDiagnostic(ctx),
            EnumMappingStrategy.ByValue when ctx.IsExpression && explicitMappings.Count > 0 => BuildCastMappingAndDiagnostic(ctx),
            EnumMappingStrategy.ByValueCheckDefined when ctx.IsExpression => BuildCastMappingAndDiagnostic(ctx),
            EnumMappingStrategy.ByName => BuildNameMapping(ctx, explicitMappings),
            EnumMappingStrategy.ByValueCheckDefined => BuildEnumToEnumCastMapping(ctx, explicitMappings, true),
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
        IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> explicitMappings,
        bool checkTargetDefined = false
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
        var targetMemberNames = ctx.Target.GetMembers().OfType<IFieldSymbol>().Select(x => x.Name).ToHashSet();

        var missingTargetValues = targetValues.Where(
            field =>
                !sourceValues.ContainsValue(field.Value) && ctx.Configuration.Enum.FallbackValue?.ConstantValue?.Equals(field.Value) != true
        );
        foreach (var member in missingTargetValues)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.TargetEnumValueNotMapped, member.Key, member.Value!, ctx.Target, ctx.Source);
        }

        var missingSourceValues = sourceValues.Where(field => !targetValues.ContainsValue(field.Value));
        foreach (var member in missingSourceValues)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.SourceEnumValueNotMapped, member.Key, member.Value!, ctx.Source, ctx.Target);
        }

        var fallbackMapping = BuildFallbackMapping(ctx);
        if (fallbackMapping.FallbackMember != null && !checkTargetDefined)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.EnumFallbackValueRequiresByValueCheckDefinedStrategy);
            checkTargetDefined = true;
        }

        var checkDefinedMode = checkTargetDefined switch
        {
            false => EnumCastMapping.CheckDefinedMode.NoCheck,
            _ when ctx.Target.HasAttribute(ctx.Types.Get<FlagsAttribute>()) => EnumCastMapping.CheckDefinedMode.Flags,
            _ => EnumCastMapping.CheckDefinedMode.Value,
        };

        var castFallbackMapping = new EnumCastMapping(ctx.Source, ctx.Target, checkDefinedMode, targetMemberNames, fallbackMapping);
        if (explicitMappings.Count == 0)
            return castFallbackMapping;

        var explicitNameMappings = explicitMappings
            .Where(x => !x.Value.ConstantValue?.Equals(x.Key.ConstantValue) == true)
            .ToDictionary(x => x.Key.Name, x => x.Value.Name);
        return new EnumNameMapping(
            ctx.Source,
            ctx.Target,
            explicitNameMappings,
            new EnumFallbackValueMapping(ctx.Source, ctx.Target, castFallbackMapping)
        );
    }

    private static EnumNameMapping BuildNameMapping(
        MappingBuilderContext ctx,
        IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> explicitMappings
    )
    {
        var fallbackMapping = BuildFallbackMapping(ctx);
        var targetFieldsByName = ctx.Target.GetMembers().OfType<IFieldSymbol>().ToDictionary(x => x.Name);
        var sourceFieldsByName = ctx.Source.GetMembers().OfType<IFieldSymbol>().ToDictionary(x => x.Name);

        Func<IFieldSymbol, IFieldSymbol?> getTargetField;
        if (ctx.Configuration.Enum.IgnoreCase)
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

        var missingTargetMembers = targetFieldsByName.Where(
            field =>
                !enumMemberMappings.ContainsValue(field.Key)
                && ctx.Configuration.Enum.FallbackValue?.ConstantValue?.Equals(field.Value.ConstantValue) != true
        );
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

        return new EnumNameMapping(ctx.Source, ctx.Target, enumMemberMappings, fallbackMapping);
    }

    private static EnumFallbackValueMapping BuildFallbackMapping(MappingBuilderContext ctx)
    {
        var fallbackValue = ctx.Configuration.Enum.FallbackValue;
        if (fallbackValue == null)
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target);

        if (SymbolEqualityComparer.Default.Equals(ctx.Target, fallbackValue.Type))
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target, fallbackMember: fallbackValue);

        ctx.ReportDiagnostic(
            DiagnosticDescriptors.EnumFallbackValueTypeDoesNotMatchTargetEnumType,
            fallbackValue,
            fallbackValue.ConstantValue ?? 0,
            fallbackValue.Type,
            ctx.Target
        );
        return new EnumFallbackValueMapping(ctx.Source, ctx.Target);
    }

    private static IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> BuildExplicitValueMapping(MappingBuilderContext ctx)
    {
        var targetFieldsByExplicitValue = new Dictionary<IFieldSymbol, IFieldSymbol>(SymbolEqualityComparer.Default);
        foreach (var (source, target) in ctx.Configuration.Enum.ExplicitMappings)
        {
            if (!SymbolEqualityComparer.Default.Equals(source.Type, ctx.Source))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.SourceEnumValueDoesNotMatchSourceEnumType,
                    source,
                    source.ConstantValue ?? 0,
                    source.Type,
                    ctx.Source
                );
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(target.Type, ctx.Target))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.TargetEnumValueDoesNotMatchTargetEnumType,
                    target,
                    target.ConstantValue ?? 0,
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
