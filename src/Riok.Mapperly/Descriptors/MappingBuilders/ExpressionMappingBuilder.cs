using System.Linq.Expressions;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class ExpressionMappingBuilder
{
    public static NewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Queryable))
            return null;

        // FIXME: what about source? should we check that it is void?

        if (!ctx.Target.ImplementsGeneric(ctx.Types.Get(typeof(Expression<>)), out var targetExpr))
            return null;

        if (!targetExpr.TypeArguments[0].ImplementsGeneric(ctx.Types.Get(typeof(Func<,>)), out var targetFunc))
            return null;

        var sourceType = targetFunc.TypeArguments[0];
        var targetType = targetFunc.TypeArguments[1];
        var mappingKey = new TypeMappingKey(sourceType, targetType);

        var inlineCtx = new InlineExpressionMappingBuilderContext(ctx, mappingKey);
        var mapping = inlineCtx.BuildMapping(mappingKey, MappingBuildingOptions.KeepUserSymbol);
        if (mapping == null)
            return null;

        if (ctx.MapperConfiguration.UseReferenceHandling)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingsDoNotSupportReferenceHandling);
        }

        return new ExpressionProjectionMapping(ctx.Source, ctx.Target, mapping);
    }
}
