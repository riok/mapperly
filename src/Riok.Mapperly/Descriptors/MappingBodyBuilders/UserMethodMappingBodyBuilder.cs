using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

/// <summary>
/// Mapping body builder for user defined methods.
/// </summary>
public static class UserMethodMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedExistingTargetMethodMapping mapping)
    {
        // UserDefinedExistingTargetMethodMapping handles null already
        var delegateMapping = ctx.BuildExistingTargetMapping(
            new TypeMappingKey(mapping).NonNullable(),
            MappingBuildingOptions.KeepUserSymbol
        );
        if (delegateMapping != null)
        {
            mapping.SetDelegateMapping(delegateMapping);
            return;
        }

        ctx.ReportDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping, mapping.SourceType, mapping.TargetType);
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedNewInstanceMethodMapping mapping)
    {
        var options = MappingBuildingOptions.KeepUserSymbol;

        // the delegate mapping is not embedded
        // and is therefore reusable if there are no additional parameters
        // if embedded, only the original mapping is callable by others
        if (mapping is { InternalReferenceHandlingEnabled: true, AdditionalSourceParameters.Count: 0 })
        {
            options |= MappingBuildingOptions.MarkAsReusable;
        }

        var delegateMapping = ctx.BuildMapping(new TypeMappingKey(mapping), options);
        if (delegateMapping != null)
        {
            mapping.SetDelegateMapping(delegateMapping);
            return;
        }

        ctx.ReportDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping, mapping.SourceType, mapping.TargetType);
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedExpressionMethodMapping mapping)
    {
        // For Expression mappings, the source and target types come from Expression<Func<TSource, TTarget>>
        // We need to build an ExpressionMapping using the InlineExpressionMappingBuilderContext
        var sourceType = mapping.ExpressionSourceType;
        var targetType = mapping.ExpressionTargetType;

        var mappingKey = new TypeMappingKey(sourceType, targetType);
        var userMapping = ctx.FindMapping(sourceType, targetType) as IUserMapping;
        var inlineCtx = new InlineExpressionMappingBuilderContext(ctx, userMapping, mappingKey);

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
