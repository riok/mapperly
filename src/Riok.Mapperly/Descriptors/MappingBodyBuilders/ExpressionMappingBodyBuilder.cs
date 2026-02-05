using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

/// <summary>
/// Mapping body builder for user defined methods returnning Expression<Func<TSource, TTarget>>.
/// </summary>
public static class ExpressionMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedExpressionMethodMapping mapping)
    {
        // For Expression mappings, the source and target types come from Expression<Func<TSource, TTarget>>
        // We need to build an ExpressionMapping using the InlineExpressionMappingBuilderContext
        var sourceType = mapping.ExpressionSourceType;
        var targetType = mapping.ExpressionTargetType;

        // if nullable reference types are disabled
        // and there was no explicit nullable annotation,
        // the non-nullable variant is used here.
        // Otherwise, this would lead to a lambda like x => x == null ? throw ... : new ...
        // which is not expected in this case.
        // see also https://github.com/riok/mapperly/issues/1196
        sourceType = ctx.SymbolAccessor.NonNullableIfNullableReferenceTypesDisabled(sourceType, ctx.UserMapping?.SourceType);
        targetType = ctx.SymbolAccessor.NonNullableIfNullableReferenceTypesDisabled(targetType, ctx.UserMapping?.TargetType);

        var mappingKey = new TypeMappingKey(sourceType, targetType);
        var userMapping = ctx.FindMapping(sourceType, targetType) as IUserMapping;
        var inlineCtx = new InlineExpressionMappingBuilderContext(ctx, userMapping, mappingKey);

        // Check if there's a user-implemented method that can be inlined (same as IQueryable)
        if (userMapping is UserImplementedMethodMapping && inlineCtx.FindMapping(sourceType, targetType) is { } inlinedUserMapping)
        {
            var expressionMapping = new ExpressionMapping(
                sourceType,
                mapping.TargetType, // The return type (Expression<Func<TSource, TTarget>>)
                inlinedUserMapping,
                ctx.Configuration.SupportedFeatures.NullableAttributes
            );
            mapping.SetDelegateMapping(expressionMapping);
            return;
        }

        var delegateMapping = inlineCtx.BuildMapping(mappingKey, MappingBuildingOptions.KeepUserSymbol);
        if (delegateMapping != null)
        {
            if (ctx.Configuration.Mapper.UseReferenceHandling)
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingsDoNotSupportReferenceHandling);
            }

            var expressionMapping = new ExpressionMapping(
                sourceType,
                mapping.TargetType, // The return type (Expression<Func<TSource, TTarget>>)
                delegateMapping,
                ctx.Configuration.SupportedFeatures.NullableAttributes
            );
            mapping.SetDelegateMapping(expressionMapping);
            return;
        }

        ctx.ReportDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping, sourceType, targetType);
    }
}
