using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class QueryableMappingBuilder
{
    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Queryable))
            return null;

        if (!ctx.Source.ImplementsGeneric(ctx.Types.Get(typeof(IQueryable<>)), out var sourceQueryable))
            return null;

        if (!ctx.Target.ImplementsGeneric(ctx.Types.Get(typeof(IQueryable<>)), out var targetQueryable))
            return null;

        var sourceType = sourceQueryable.TypeArguments[0];
        var targetType = targetQueryable.TypeArguments[0];

        var mappingKey = TryBuildMappingKey(ctx, sourceType, targetType);
        var userMapping = ctx.FindMapping(sourceType, targetType) as IUserMapping;
        var inlineCtx = new InlineExpressionMappingBuilderContext(ctx, userMapping, mappingKey);
        var mapping = inlineCtx.BuildMapping(mappingKey, MappingBuildingOptions.KeepUserSymbol);
        if (mapping == null)
            return null;

        if (ctx.Configuration.Mapper.UseReferenceHandling)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingsDoNotSupportReferenceHandling);
        }

        return new QueryableProjectionMapping(ctx.Source, ctx.Target, mapping, ctx.Configuration.SupportedFeatures.NullableAttributes);
    }

    private static TypeMappingKey TryBuildMappingKey(MappingBuilderContext ctx, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        // if nullable reference types are disabled
        // and there was no explicit nullable annotation,
        // the non-nullable variant is used here.
        // Otherwise, this would lead to a select like source.Select(x => x == null ? throw ... : new ...)
        // which is not expected in this case.
        // see also https://github.com/riok/mapperly/issues/1196
        sourceType = ctx.SymbolAccessor.NonNullableIfNullableReferenceTypesDisabled(sourceType, ctx.UserMapping?.SourceType);
        targetType = ctx.SymbolAccessor.NonNullableIfNullableReferenceTypesDisabled(targetType, ctx.UserMapping?.TargetType);

        return new TypeMappingKey(sourceType, targetType);
    }
}
