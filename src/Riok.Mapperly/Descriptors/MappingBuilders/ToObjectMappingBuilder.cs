using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class ToObjectMappingBuilder
{
    public static NewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.ExplicitCast))
            return null;

        if (ctx.Target.SpecialType != SpecialType.System_Object)
            return null;

        if (!ctx.UseCloning)
            return new CastMapping(ctx.Source, ctx.Target);

        if (ctx.Source.SpecialType == SpecialType.System_Object)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.MappedObjectToObjectWithoutDeepClone, ctx.Source.Name, ctx.Target.Name);
            return new DirectAssignmentMapping(ctx.Source);
        }

        return new CastMapping(ctx.Source, ctx.Target, ctx.FindOrBuildMapping(ctx.Source, ctx.Source));
    }
}
