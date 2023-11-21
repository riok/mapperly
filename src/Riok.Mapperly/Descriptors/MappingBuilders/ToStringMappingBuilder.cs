using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class ToStringMappingBuilder
{
    public static NewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.ToStringMethod))
            return null;

        if (ctx.Target.SpecialType != SpecialType.System_String)
            return null;

        if (ctx.MappingKey.Configuration.StringFormat == null)
            return new ToStringMapping(ctx.Source, ctx.Target);

        if (!ctx.Source.Implements(ctx.Types.Get<IFormattable>()))
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.SourceDoesNotImplementIFormattable, ctx.Source);
            return new ToStringMapping(ctx.Source, ctx.Target);
        }

        return new ToStringMapping(ctx.Source, ctx.Target, ctx.MappingKey.Configuration.StringFormat);
    }
}
