using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class StringToEnumMappingBuilder
{
    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.StringToEnum))
            return null;

        if (ctx.Source.SpecialType != SpecialType.System_String || !ctx.Target.IsEnum())
            return null;

        var genericEnumParseMethodSupported = ctx.Types.Enum.GetMembers(nameof(Enum.Parse))
            .OfType<IMethodSymbol>()
            .Any(x => x.IsGenericMethod);

        var config = ctx.GetConfigurationOrDefault<MapEnumAttribute>();
        if (ctx.IsExpression)
            return new EnumFromStringParseMapping(ctx.Source, ctx.Target, genericEnumParseMethodSupported, config.IgnoreCase);

        // from string => use an optimized method of Enum.Parse which would use slow reflection
        // however we currently don't support all features of Enum.Parse yet (ex. flags)
        // therefore we use Enum.Parse as fallback.
        var members = ctx.Target.GetMembers().OfType<IFieldSymbol>();
        return new EnumFromStringSwitchMapping(ctx.Source, ctx.Target, members, genericEnumParseMethodSupported, config.IgnoreCase);
    }
}
