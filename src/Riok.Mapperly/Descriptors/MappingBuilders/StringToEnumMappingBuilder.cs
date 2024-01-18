using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.Enums;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class StringToEnumMappingBuilder
{
    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.StringToEnum))
            return null;

        if (ctx.Source.SpecialType != SpecialType.System_String || !ctx.Target.IsEnum())
            return null;

        var genericEnumParseMethodSupported = ctx
            .Types.Get<Enum>()
            .GetMembers(nameof(Enum.Parse))
            .OfType<IMethodSymbol>()
            .Any(x => x.IsGenericMethod);

        if (ctx.IsExpression)
        {
            return new EnumFromStringParseMapping(
                ctx.Source,
                ctx.Target,
                genericEnumParseMethodSupported,
                ctx.Configuration.Enum.IgnoreCase
            );
        }

        // from string => use an optimized method of Enum.Parse which would use slow reflection
        // however we currently don't support all features of Enum.Parse yet (ex. flags)
        // therefore we use Enum.Parse as fallback.
        var fallbackMapping = BuildFallbackParseMapping(ctx, genericEnumParseMethodSupported);
        var members = ctx.SymbolAccessor.GetAllFields(ctx.Target);
        if (fallbackMapping.FallbackMember != null)
        {
            // no need to explicitly map fallback value
            members = members.Where(x => fallbackMapping.FallbackMember.ConstantValue?.Equals(x.ConstantValue) != true);
        }

        return new EnumFromStringSwitchMapping(ctx.Source, ctx.Target, members, ctx.Configuration.Enum.IgnoreCase, fallbackMapping);
    }

    private static EnumFallbackValueMapping BuildFallbackParseMapping(MappingBuilderContext ctx, bool genericEnumParseMethodSupported)
    {
        var fallbackValue = ctx.Configuration.Enum.FallbackValue;
        if (fallbackValue == null)
        {
            return new EnumFallbackValueMapping(
                ctx.Source,
                ctx.Target,
                new EnumFromStringParseMapping(ctx.Source, ctx.Target, genericEnumParseMethodSupported, ctx.Configuration.Enum.IgnoreCase)
            );
        }

        if (SymbolEqualityComparer.Default.Equals(ctx.Target, fallbackValue.Type))
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target, fallbackMember: fallbackValue);

        ctx.ReportDiagnostic(
            DiagnosticDescriptors.EnumFallbackValueTypeDoesNotMatchTargetEnumType,
            fallbackValue,
            fallbackValue.ConstantValue ?? 0,
            fallbackValue.Type,
            ctx.Target
        );
        return new EnumFallbackValueMapping(
            ctx.Source,
            ctx.Target,
            new EnumFromStringParseMapping(ctx.Source, ctx.Target, genericEnumParseMethodSupported, ctx.Configuration.Enum.IgnoreCase)
        );
    }
}
