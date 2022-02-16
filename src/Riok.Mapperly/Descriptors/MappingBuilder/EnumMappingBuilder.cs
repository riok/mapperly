using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.TypeMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

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
            return ctx.FindOrBuildMapping(sourceEnumType ?? ctx.Source, targetEnumType ?? ctx.Target) is { } delegateMapping
                ? new EnumDelegateMapping(ctx.Source, ctx.Target, delegateMapping)
                : null;
        }

        // since enums are immutable they can be directly assigned if they are of the same type
        if (SymbolEqualityComparer.Default.Equals(ctx.Source, ctx.Target))
            return new NullDelegateMapping(ctx.Source, ctx.Target, new DirectAssignmentMapping(ctx.Source.NonNullable()));

        // map enums by strategy
        var config = ctx.GetConfigurationOrDefault<MapEnumAttribute>();
        return config.Strategy switch
        {
            EnumMappingStrategy.ByName => BuildNameMapping(ctx, config.IgnoreCase),
            _ => new NullDelegateMapping(ctx.Source, ctx.Target, new CastMapping(ctx.Source.NonNullable(), ctx.Target.NonNullable())),
        };
    }

    private static TypeMapping BuildNameMapping(MappingBuilderContext ctx, bool ignoreCase)
    {
        var targetFieldsByName = ctx.Target.GetMembers().OfType<IFieldSymbol>().ToDictionary(x => x.Name);
        Func<IFieldSymbol, IFieldSymbol?> getTargetField;
        if (ignoreCase)
        {
            var targetFieldsByNameIgnoreCase = targetFieldsByName
                .DistinctBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            getTargetField = source => targetFieldsByName.GetValueOrDefault(source.Name) ?? targetFieldsByNameIgnoreCase.GetValueOrDefault(source.Name);
        }
        else
        {
            getTargetField = source => targetFieldsByName.GetValueOrDefault(source.Name);
        }

        var enumMemberMappings = ctx.Source.GetMembers().OfType<IFieldSymbol>()
            .Select(x => (Source: x, Target: getTargetField(x)))
            .Where(x => x.Target != null)
            .ToDictionary(x => x.Source.Name, x => x.Target!.Name);

        if (enumMemberMappings.Count == 0)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.EnumNameMappingNoOverlappingValuesFound,
                ctx.Source,
                ctx.Target);
        }

        return new EnumNameMapping(ctx.Source, ctx.Target, enumMemberMappings);
    }
}
