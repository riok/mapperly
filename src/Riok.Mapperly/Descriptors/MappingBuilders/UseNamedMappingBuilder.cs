using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class UseNamedMappingBuilder
{
    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (ctx.MappingKey.Configuration.UseNamedMapping == null)
            return null;

        var mapping = ctx.FindNamedMapping(ctx.MappingKey.Configuration.UseNamedMapping);
        if (mapping == null)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingNotFound, ctx.MappingKey.Configuration.UseNamedMapping);
            return null;
        }

        mapping = MapSourceIfNeeded(ctx, mapping);
        if (mapping == null)
            return null;

        return MapTargetIfNeeded(ctx, mapping);
    }

    private static INewInstanceMapping? MapTargetIfNeeded(MappingBuilderContext ctx, INewInstanceMapping mapping)
    {
        if (SymbolEqualityComparer.IncludeNullability.Equals(ctx.Target, mapping.TargetType))
            return mapping;

        var outputMapping = ctx.FindOrBuildMapping(mapping.TargetType, ctx.Target);
        if (outputMapping == null)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.CouldNotCreateMapping,
                mapping.TargetType.ToDisplayString(),
                ctx.Target.ToDisplayString()
            );
            return null;
        }

        return new CompositeMapping(outputMapping, mapping);
    }

    private static INewInstanceMapping? MapSourceIfNeeded(MappingBuilderContext ctx, INewInstanceMapping mapping)
    {
        if (SymbolEqualityComparer.IncludeNullability.Equals(ctx.Source, mapping.SourceType))
            return mapping;

        var inputMapping = ctx.FindOrBuildMapping(ctx.Source, mapping.SourceType);
        if (inputMapping == null)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping, ctx.Source, mapping.SourceType);
            return null;
        }

        return new CompositeMapping(mapping, inputMapping);
    }
}
