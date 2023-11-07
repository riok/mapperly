using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class DateTimeToTimeOnlyMappingBuilder
{
    private const string FromDateTimeMethodName = "FromDateTime";

    public static StaticMethodMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.DateTimeToTimeOnly) || ctx.Types.TimeOnly == null)
            return null;

        if (ctx.Source.SpecialType != SpecialType.System_DateTime)
            return null;

        if (ctx.Target is not INamedTypeSymbol namedSymbol || !SymbolEqualityComparer.Default.Equals(namedSymbol, ctx.Types.TimeOnly))
            return null;

        var fromDateTimeMethod = ResolveFromDateTimeMethod(ctx);
        if (fromDateTimeMethod is null)
            return null;

        return new StaticMethodMapping(fromDateTimeMethod);
    }

    private static IMethodSymbol? ResolveFromDateTimeMethod(MappingBuilderContext ctx)
    {
        return ctx.Types.TimeOnly?.GetMembers(FromDateTimeMethodName).OfType<IMethodSymbol>().FirstOrDefault(m => m.IsStatic);
    }
}
