using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.Enums;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class EnumToStringMappingBuilder
{
    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.EnumToString))
            return null;

        if (ctx.Target.SpecialType != SpecialType.System_String || !ctx.Source.IsEnum())
            return null;

        // to string => use an optimized method of Enum.ToString which would use slow reflection
        // use Enum.ToString as fallback (for ex. for flags)
        return new EnumToStringMapping(
            ctx.Source,
            ctx.Target,
            ctx.SymbolAccessor.GetAllFields(ctx.Source),
            ctx.Configuration.Mapper.EnableAggressiveInlining
        );
    }
}
