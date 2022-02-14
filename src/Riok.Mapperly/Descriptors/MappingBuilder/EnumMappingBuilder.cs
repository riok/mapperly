using System.Diagnostics.CodeAnalysis;
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
        var sourceIsEnum = TryGetEnumType(ctx.Source, out var sourceEnumType);
        var targetIsEnum = TryGetEnumType(ctx.Target, out var targetEnumType);

        if (!sourceIsEnum && !targetIsEnum)
            return null;

        if (sourceIsEnum && targetIsEnum)
        {
            if (SymbolEqualityComparer.Default.Equals(ctx.Source, ctx.Target))
                return new DirectAssignmentMapping(ctx.Source);

            var config = ctx.GetConfigurationOrDefault<MapEnumAttribute>();
            return config.Strategy switch
            {
                EnumMappingStrategy.ByName => BuildNameMapping(ctx),
                _ => new NullDelegateMapping(ctx.Source, ctx.Target, new CastMapping(ctx.Source.NonNullable(), ctx.Target.NonNullable())),
            };
        }

        // to string => use an optimized method of Enum.ToString which would use slow reflection
        // use Enum.ToString as fallback (for ex. for flags)
        if (sourceIsEnum && ctx.Target.SpecialType == SpecialType.System_String)
            return new EnumToStringMapping(ctx.Source, ctx.Target, ctx.Source.GetMembers().OfType<IFieldSymbol>());

        // from string => use an optimized method of Enum.Parse which would use slow reflection
        // however we currently don't support all features of Enum.Parse yet (ex. flags)
        // therefore we use Enum.Parse as fallback.
        if (targetIsEnum && ctx.Source.SpecialType == SpecialType.System_String)
            return new EnumFromStringMapping(ctx.Source, ctx.Target, ctx.Target.GetMembers().OfType<IFieldSymbol>());

        return ctx.FindOrBuildMapping(sourceEnumType ?? ctx.Source, targetEnumType ?? ctx.Target) is { } delegateMapping
            ? new EnumDelegateMapping(ctx.Source, ctx.Target, delegateMapping)
            : null;
    }

    private static TypeMapping BuildNameMapping(MappingBuilderContext ctx)
    {
        var memberNames = ctx.Source.GetMembers().OfType<IFieldSymbol>().Select(x => x.Name)
            .Intersect(ctx.Target.GetMembers().OfType<IFieldSymbol>().Select(x => x.Name))
            .ToList();

        if (memberNames.Count == 0)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.EnumNameMappingNoOverlappingValuesFound,
                ctx.Source,
                ctx.Target);
        }

        return new EnumNameMapping(ctx.Source, ctx.Target, memberNames);
    }

    private static bool TryGetEnumType(ITypeSymbol t, [NotNullWhen(true)] out INamedTypeSymbol? enumType)
    {
        enumType = (t.NonNullable() as INamedTypeSymbol)?.EnumUnderlyingType;
        return enumType != null;
    }
}
