using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

/// <summary>
/// Mapping body builder for user defined methods returning Expression<Func<TSource, TTarget>>.
/// </summary>
public static class ExpressionMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedExpressionMethodMapping mapping)
    {
        // For Expression mappings, the source and target types come from Expression<Func<TSource, TTarget>>
        var sourceType = mapping.ExpressionSourceType;
        var targetType = mapping.ExpressionTargetType;

        var delegateMapping = InlineExpressionMappingBuilder.TryBuildInlineMappingForExpression(ctx, sourceType, targetType);
        if (delegateMapping != null)
        {
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
