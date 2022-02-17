using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.TypeMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class NullableMappingBuilder
{
    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        var sourceIsNullable = ctx.Source.TryGetNonNullable(out var sourceNonNullable);
        var targetIsNullable = ctx.Target.TryGetNonNullable(out var targetNonNullable);
        if (!sourceIsNullable && !targetIsNullable)
            return null;

        var delegateMapping = ctx.BuildDelegateMapping(sourceNonNullable ?? ctx.Source, targetNonNullable ?? ctx.Target);
        return delegateMapping == null
            ? null
            : BuildNullDelegateMapping(ctx, delegateMapping);
    }

    private static TypeMapping BuildNullDelegateMapping(MappingBuilderContext ctx, TypeMapping mapping)
    {
        var nullFallback = GetNullFallbackValue(ctx);
        return mapping switch
        {
            MethodMapping methodMapping => new NullDelegateMethodMapping(
                ctx.Source,
                ctx.Target,
                methodMapping,
                nullFallback),
            _ => new NullDelegateMapping(ctx.Source, ctx.Target, mapping, nullFallback),
        };
    }

    private static NullFallbackValue GetNullFallbackValue(MappingBuilderContext ctx)
    {
        if (ctx.Target.IsNullable())
            return NullFallbackValue.Default;

        if (ctx.MapperConfiguration.ThrowOnMappingNullMismatch)
            return NullFallbackValue.ThrowArgumentNullException;

        if (!ctx.Target.IsReferenceType || ctx.Target.IsNullable())
            return NullFallbackValue.Default;

        if (ctx.Target.SpecialType == SpecialType.System_String)
            return NullFallbackValue.EmptyString;

        if (ctx.Target.HasAccessibleParameterlessConstructor())
            return NullFallbackValue.CreateInstance;

        ctx.ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, ctx.Target);
        return NullFallbackValue.ThrowArgumentNullException;
    }
}
