using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class ExpressionMappingBuilder
{
    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Expression))
            return null;

        // Check if target is Expression<Func<TSource, TTarget>>
        if (!ctx.Target.ExtendsOrImplementsGeneric(ctx.Types.Get(typeof(Expression<>)), out var targetExpression))
            return null;

        // Get the Func<TSource, TTarget> type argument
        var funcType = targetExpression.TypeArguments[0] as INamedTypeSymbol;
        if (funcType == null || !funcType.ExtendsOrImplementsGeneric(ctx.Types.Get(typeof(Func<,>)), out var funcTypeArgs))
            return null;

        // Extract source and target types from the Func<TSource, TTarget> type arguments
        var sourceType = funcTypeArgs.TypeArguments[0];
        var targetType = funcTypeArgs.TypeArguments[1];

        var mappingKey = TryBuildMappingKey(ctx, sourceType, targetType);
        var userMapping = ctx.FindMapping(sourceType, targetType) as IUserMapping;
        var inlineCtx = new InlineExpressionMappingBuilderContext(ctx, userMapping, mappingKey);
        if (userMapping is UserImplementedMethodMapping && inlineCtx.FindMapping(sourceType, targetType) is { } inlinedUserMapping)
        {
            return new ExpressionMapping(
                ctx.Source,
                ctx.Target,
                inlinedUserMapping,
                ctx.Configuration.SupportedFeatures.NullableAttributes
            );
        }

        var mapping = inlineCtx.BuildMapping(mappingKey, MappingBuildingOptions.KeepUserSymbol);
        if (mapping == null)
            return null;

        if (ctx.Configuration.Mapper.UseReferenceHandling)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingsDoNotSupportReferenceHandling);
        }

        return new ExpressionMapping(ctx.Source, ctx.Target, mapping, ctx.Configuration.SupportedFeatures.NullableAttributes);
    }

    private static TypeMappingKey TryBuildMappingKey(MappingBuilderContext ctx, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        // if nullable reference types are disabled
        // and there was no explicit nullable annotation,
        // the non-nullable variant is used here.
        // Otherwise, this would lead to a lambda like source => source == null ? throw ... : new ...
        // which is not expected in this case.
        sourceType = ctx.SymbolAccessor.NonNullableIfNullableReferenceTypesDisabled(sourceType, ctx.UserMapping?.SourceType);
        targetType = ctx.SymbolAccessor.NonNullableIfNullableReferenceTypesDisabled(targetType, ctx.UserMapping?.TargetType);

        return new TypeMappingKey(sourceType, targetType);
    }
}
