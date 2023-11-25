using System.Linq.Expressions;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class QueryableMappingBuilder
{
    public static NewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Queryable))
            return null;

        if (!ctx.Source.ImplementsGeneric(ctx.Types.Get(typeof(IQueryable<>)), out var sourceQueryable))
            return null;

        if (!ctx.Target.ImplementsGeneric(ctx.Types.Get(typeof(IQueryable<>)), out var targetQueryable))
            return null;

        var sourceType = sourceQueryable.TypeArguments[0];
        var targetType = targetQueryable.TypeArguments[0];

        var funcType = ctx.Types
            .Get(typeof(Expression<>))
            .Construct(ctx.Types.Get(typeof(Func<,>)).Construct(sourceType, targetType))
            .NonNullable();

        var mapping = ctx.FindOrBuildMapping(ctx.Types.Get(typeof(void)), funcType);
        if (mapping == null)
            return null;

        if (ctx.MapperConfiguration.UseReferenceHandling)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingsDoNotSupportReferenceHandling);
        }

        return new QueryableProjectionMapping(ctx.Source, ctx.Target, mapping);
    }
}
