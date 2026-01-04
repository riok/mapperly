using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class NullableMappingBuilder
{
    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!TryBuildNonNullableMappingKey(ctx, out var mappingKey))
            return null;

        var delegateMapping = ctx.BuildMapping(mappingKey, MappingBuildingOptions.KeepUserSymbol);
        return delegateMapping == null ? null : BuildNullDelegateMapping(ctx, delegateMapping);
    }

    public static IExistingTargetMapping? TryBuildExistingTargetMapping(MappingBuilderContext ctx)
    {
        if (!TryBuildNonNullableMappingKey(ctx, out var mappingKey))
            return null;

        var delegateMapping = ctx.FindOrBuildExistingTargetMapping(mappingKey);
        return delegateMapping == null ? null : new NullDelegateExistingTargetMapping(ctx.Source, ctx.Target, delegateMapping);
    }

    private static bool TryBuildNonNullableMappingKey(MappingBuilderContext ctx, out TypeMappingKey mappingKey)
    {
        var sourceIsNullable = ctx.Source.TryGetNonNullable(out var sourceNonNullable);
        var targetIsNullable = ctx.Target.TryGetNonNullable(out var targetNonNullable);
        if (!sourceIsNullable && !targetIsNullable)
        {
            mappingKey = default;
            return false;
        }

        mappingKey = new TypeMappingKey(sourceNonNullable ?? ctx.Source, targetNonNullable ?? ctx.Target);

        if (sourceIsNullable && !targetIsNullable)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NullableSourceTypeToNonNullableTargetType, ctx.Source, ctx.Target);
        }

        return true;
    }

    private static INewInstanceMapping BuildNullDelegateMapping(MappingBuilderContext ctx, INewInstanceMapping mapping)
    {
        var nullFallback = ctx.GetNullFallbackValue();
        var additionalSourceParameters = GetAdditionalSourceParameters(ctx);

        return mapping switch
        {
            NewInstanceMethodMapping methodMapping => new NullDelegateMethodMapping(
                ctx.Source,
                ctx.Target,
                methodMapping,
                nullFallback,
                ctx.Configuration.SupportedFeatures.NullableAttributes
            )
            {
                AdditionalSourceParameters = additionalSourceParameters,
            },
            _ => new NullDelegateMapping(ctx.Source, ctx.Target, mapping, nullFallback),
        };
    }

    private static IReadOnlyCollection<MethodParameter> GetAdditionalSourceParameters(MappingBuilderContext ctx)
    {
        if (ctx.UserSymbol == null)
            return [];

        var refHandlerParameter = FindReferenceHandlerParameter(ctx, ctx.UserSymbol);
        var refHandlerParameterOrdinal = refHandlerParameter?.Ordinal ?? -1;

        var sourceParameter = FindSourceParameter(ctx, ctx.UserSymbol, refHandlerParameter);
        if (!sourceParameter.HasValue)
            return [];

        var sourceParameterOrdinal = sourceParameter.Value.Ordinal;
        var additionalParameterSymbols = ctx
            .UserSymbol.Parameters.Where(p =>
                p.Ordinal != sourceParameterOrdinal
                && p.Ordinal != refHandlerParameterOrdinal
                && ctx.SymbolAccessor.HasAttribute<MapAdditionalSourceAttribute>(p)
            )
            .ToList();

        return additionalParameterSymbols.Select(p => ctx.SymbolAccessor.WrapMethodParameter(p)).ToList();
    }

    private static MethodParameter? FindSourceParameter(
        MappingBuilderContext ctx,
        IMethodSymbol method,
        MethodParameter? refHandlerParameter
    )
    {
        var refHandlerParameterOrdinal = refHandlerParameter?.Ordinal ?? -1;

        // source parameter is the first parameter not annotated as reference handler or mapping target
        var sourceParameterSymbol = method.Parameters.FirstOrDefault(p =>
            p.Ordinal != refHandlerParameterOrdinal
            && !ctx.SymbolAccessor.HasAttribute<ReferenceHandlerAttribute>(p)
            && !ctx.SymbolAccessor.HasAttribute<MappingTargetAttribute>(p)
        );
        return ctx.SymbolAccessor.WrapOptionalMethodParameter(sourceParameterSymbol);
    }

    private static MethodParameter? FindReferenceHandlerParameter(MappingBuilderContext ctx, IMethodSymbol method)
    {
        var refHandlerParameterSymbol = method.Parameters.FirstOrDefault(p =>
            ctx.SymbolAccessor.HasAttribute<ReferenceHandlerAttribute>(p)
        );
        if (refHandlerParameterSymbol == null)
            return null;

        // the reference handler parameter cannot also be the target parameter
        if (ctx.SymbolAccessor.HasAttribute<MappingTargetAttribute>(refHandlerParameterSymbol))
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, method, method.Name);
        }

        var refHandlerParameter = ctx.SymbolAccessor.WrapMethodParameter(refHandlerParameterSymbol);
        if (!SymbolEqualityComparer.Default.Equals(ctx.Types.Get<IReferenceHandler>(), refHandlerParameter.Type))
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.ReferenceHandlerParameterWrongType,
                refHandlerParameterSymbol,
                ctx.UserSymbol?.ContainingType.ToDisplayString() ?? method.ContainingType.ToDisplayString(),
                method.Name,
                ctx.Types.Get<IReferenceHandler>().ToDisplayString(),
                refHandlerParameterSymbol.Type.ToDisplayString()
            );
        }

        if (!ctx.Configuration.Mapper.UseReferenceHandling)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.ReferenceHandlingNotEnabled,
                refHandlerParameterSymbol,
                ctx.UserSymbol?.ContainingType.ToDisplayString() ?? method.ContainingType.ToDisplayString(),
                method.Name
            );
        }

        return refHandlerParameter;
    }
}
