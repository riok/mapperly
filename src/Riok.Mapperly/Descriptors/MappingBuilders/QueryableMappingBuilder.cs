using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class QueryableMappingBuilder
{
    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Queryable))
            return null;

        if (!TryBuildMappingKey(ctx, out var mappingKey))
            return null;

        var inlineCtx = new InlineExpressionMappingBuilderContext(ctx, mappingKey);
        var mapping = inlineCtx.BuildMapping(mappingKey, MappingBuildingOptions.KeepUserSymbol);
        if (mapping == null)
            return null;

        if (ctx.Configuration.Mapper.UseReferenceHandling)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingsDoNotSupportReferenceHandling);
        }

        return new QueryableProjectionMapping(ctx.Source, ctx.Target, mapping, ctx.Configuration.Mapper.EnableAggressiveInlining);
    }

    private static bool TryBuildMappingKey(MappingBuilderContext ctx, out TypeMappingKey mappingKey)
    {
        mappingKey = default;
        if (!ctx.Source.ImplementsGeneric(ctx.Types.Get(typeof(IQueryable<>)), out var sourceQueryable))
            return false;

        if (!ctx.Target.ImplementsGeneric(ctx.Types.Get(typeof(IQueryable<>)), out var targetQueryable))
            return false;

        // if nullable reference types are disabled
        // and there was no explicit nullable annotation,
        // the non-nullable variant is used here.
        // Otherwise, this would lead to a select like source.Select(x => x == null ? throw ... : new ...)
        // which is not expected in this case.
        // see also https://github.com/riok/mapperly/issues/1196
        var sourceType = ctx.SymbolAccessor.NonNullableIfNullableReferenceTypesDisabled(
            sourceQueryable.TypeArguments[0],
            ctx.UserMapping?.SourceType
        );
        var targetType = ctx.SymbolAccessor.NonNullableIfNullableReferenceTypesDisabled(
            targetQueryable.TypeArguments[0],
            ctx.UserMapping?.TargetType
        );

        mappingKey = new TypeMappingKey(sourceType, targetType);
        return true;
    }
}
