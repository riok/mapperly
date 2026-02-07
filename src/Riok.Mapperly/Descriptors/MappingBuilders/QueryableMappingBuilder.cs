using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
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

        var mapping = InlineExpressionMappingBuilder.TryBuildInlineMappingForExpression(ctx, sourceType, targetType);
        if (mapping == null)
            return null;

        return new QueryableProjectionMapping(ctx.Source, ctx.Target, mapping, ctx.Configuration.SupportedFeatures.NullableAttributes);
    }
}
