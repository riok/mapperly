using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
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

        var differentSourceType = !SymbolEqualityComparer.IncludeNullability.Equals(ctx.Source, mapping.SourceType);
        var differentTargetType = !SymbolEqualityComparer.IncludeNullability.Equals(ctx.Target, mapping.TargetType);

        // use a delegate mapping,
        // otherwise the user-defined method mapping may get built twice
        // (if it is returned here directly it is re-added to the mappings to be built)
        if (!differentSourceType && !differentTargetType)
            return new DelegateMapping(mapping.SourceType, mapping.TargetType, mapping);

        if (differentSourceType)
        {
            mapping = TryMapSource(ctx, mapping);
            if (mapping == null)
                return null;
        }

        return differentTargetType ? TryMapTarget(ctx, mapping) : mapping;
    }

    public static IExistingTargetMapping? TryBuildExistingTargetMapping(MappingBuilderContext ctx)
    {
        if (ctx.MappingKey.Configuration.UseNamedMapping == null)
            return null;

        var useNamedMapping = ctx.MappingKey.Configuration.UseNamedMapping;
        var existingTargetMapping = ctx.FindExistingTargetNamedMapping(useNamedMapping);
        if (existingTargetMapping is null)
            return null;

        var source = ctx.Source;
        var target = ctx.Target;

        var differentSourceType = !SymbolEqualityComparer.IncludeNullability.Equals(source, existingTargetMapping.SourceType);
        var differentTargetType = !SymbolEqualityComparer.IncludeNullability.Equals(target, existingTargetMapping.TargetType);

        if (!differentSourceType && !differentTargetType)
        {
            return new DelegateExistingTargetMapping(source, target, existingTargetMapping);
        }

        if (differentSourceType)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.ReferencedMappingSourceTypeMismatch,
                useNamedMapping,
                existingTargetMapping.SourceType.ToDisplayString(),
                ctx.Source.ToDisplayString()
            );
        }

        if (differentTargetType)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.ReferencedMappingTargetTypeMismatch,
                useNamedMapping,
                existingTargetMapping.TargetType.ToDisplayString(),
                ctx.Target.ToDisplayString()
            );
        }

        return TryBuildCompositeExistingTargetMapping(ctx, existingTargetMapping);
    }

    private static IExistingTargetMapping? TryBuildCompositeExistingTargetMapping(
        MappingBuilderContext ctx,
        IExistingTargetMapping existingTargetMapping
    )
    {
        var sourceMapping = ctx.FindOrBuildMapping(ctx.Source, existingTargetMapping.SourceType);
        if (sourceMapping == null)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping, ctx.Source, existingTargetMapping.SourceType);

            return null;
        }

        var targetMapping = ctx.FindOrBuildMapping(ctx.Target, existingTargetMapping.TargetType);
        if (targetMapping == null)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.CouldNotCreateMapping,
                ctx.Target.ToDisplayString(),
                existingTargetMapping.TargetType.ToDisplayString()
            );

            return null;
        }

        return new CompositeExistingTargetMapping(existingTargetMapping, sourceMapping, targetMapping);
    }

    private static INewInstanceMapping? TryMapTarget(MappingBuilderContext ctx, INewInstanceMapping mapping)
    {
        // only report if there are other differences than nullability
        if (!SymbolEqualityComparer.Default.Equals(ctx.Target, mapping.TargetType))
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.ReferencedMappingTargetTypeMismatch,
                ctx.MappingKey.Configuration.UseNamedMapping!,
                mapping.TargetType.ToDisplayString(),
                ctx.Target.ToDisplayString()
            );
        }

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

    private static INewInstanceMapping? TryMapSource(MappingBuilderContext ctx, INewInstanceMapping mapping)
    {
        // only report if there are other differences than nullability
        if (!SymbolEqualityComparer.Default.Equals(ctx.Source, mapping.SourceType))
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.ReferencedMappingSourceTypeMismatch,
                ctx.MappingKey.Configuration.UseNamedMapping!,
                mapping.SourceType.ToDisplayString(),
                ctx.Source.ToDisplayString()
            );
        }

        var inputMapping = ctx.FindOrBuildMapping(ctx.Source, mapping.SourceType);
        if (inputMapping == null)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping, ctx.Source, mapping.SourceType);
            return null;
        }

        return new CompositeMapping(mapping, inputMapping);
    }
}
