using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

internal static class UserMappingMethodParameterExtractor
{
    public static bool BuildParameters(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method,
        bool allowAdditionalParameters,
        [NotNullWhen(true)] out MappingMethodParameters? parameters
    )
    {
        var refHandlerParameter = FindReferenceHandlerParameter(ctx, method);
        var refHandlerParameterOrdinal = refHandlerParameter?.Ordinal ?? -1;

        var sourceParameter = FindSourceParameter(ctx, method, refHandlerParameter);
        if (!sourceParameter.HasValue)
        {
            parameters = null;
            return false;
        }

        // If the method returns void, a target parameter is required
        // if the method doesn't return void, a target parameter is not allowed.
        MethodParameter? targetParameter = null;
        if (method.ReturnsVoid)
        {
            targetParameter = FindTargetParameter(ctx, method, sourceParameter.Value, refHandlerParameter);
            if (!targetParameter.HasValue)
            {
                parameters = null;
                return false;
            }
        }

        var targetParameterOrdinal = targetParameter?.Ordinal ?? -1;
        var additionalParameterSymbols = method
            .Parameters.Where(p =>
                p.Ordinal != sourceParameter.Value.Ordinal && p.Ordinal != targetParameterOrdinal && p.Ordinal != refHandlerParameterOrdinal
            )
            .ToList();
        if (!allowAdditionalParameters && additionalParameterSymbols.Count > 0)
        {
            parameters = null;
            return false;
        }

        // Validate additional parameters
        var hasInvalidAdditionalParameter = additionalParameterSymbols.Exists(p =>
            p.Type.TypeKind is TypeKind.TypeParameter or TypeKind.Error
            || ctx.SymbolAccessor.HasAttribute<ReferenceHandlerAttribute>(p)
            || ctx.SymbolAccessor.HasAttribute<MappingTargetAttribute>(p)
            || (
                ctx.SymbolAccessor.HasAttribute<MapAdditionalSourceAttribute>(p)
                && (
                    ctx.SymbolAccessor.HasAttribute<ReferenceHandlerAttribute>(p)
                    || ctx.SymbolAccessor.HasAttribute<MappingTargetAttribute>(p)
                )
            )
        );

        // Check for MapAdditionalSourceAttribute on source, target, or ref handler parameters
        // We need to check the original parameter symbols, not the wrapped MethodParameter
        var sourceOrTargetParameterSymbols = new List<IParameterSymbol>();
        if (sourceParameter.HasValue)
        {
            sourceOrTargetParameterSymbols.Add(method.Parameters[sourceParameter.Value.Ordinal]);
        }
        if (targetParameter.HasValue)
        {
            sourceOrTargetParameterSymbols.Add(method.Parameters[targetParameter.Value.Ordinal]);
        }
        if (refHandlerParameter.HasValue)
        {
            sourceOrTargetParameterSymbols.Add(method.Parameters[refHandlerParameter.Value.Ordinal]);
        }

        if (hasInvalidAdditionalParameter)
        {
            parameters = null;
            return false;
        }

        var additionalParameters = additionalParameterSymbols.Select(p => ctx.SymbolAccessor.WrapMethodParameter(p)).ToList();
        parameters = new MappingMethodParameters(sourceParameter.Value, targetParameter, refHandlerParameter, additionalParameters);
        return true;
    }

    public static bool BuildRuntimeTargetTypeMappingParameters(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method,
        [NotNullWhen(true)] out RuntimeTargetTypeMappingMethodParameters? parameters
    )
    {
        // existing target instance runtime typed mappings are not supported
        if (method.ReturnsVoid)
        {
            parameters = null;
            return false;
        }

        // the source and target type param is always required
        // and runtime target type mappings do not support additional parameters
        var expectedParameterCount = 2;

        var refHandlerParameter = FindReferenceHandlerParameter(ctx, method);
        if (refHandlerParameter.HasValue)
        {
            expectedParameterCount++;
        }

        var sourceParameter = FindSourceParameter(ctx, method, refHandlerParameter);
        if (!sourceParameter.HasValue)
        {
            parameters = null;
            return false;
        }

        // the target type param needs to exist
        // and needs to be of type System.Type
        var targetTypeParameter = FindTargetParameter(ctx, method, sourceParameter.Value, refHandlerParameter);
        if (!targetTypeParameter.HasValue || !SymbolEqualityComparer.Default.Equals(targetTypeParameter.Value.Type, ctx.Types.Get<Type>()))
        {
            parameters = null;
            return false;
        }

        if (method.Parameters.Length != expectedParameterCount)
        {
            parameters = null;
            return false;
        }

        parameters = new RuntimeTargetTypeMappingMethodParameters(sourceParameter.Value, targetTypeParameter.Value, refHandlerParameter);
        return true;
    }

    private static MethodParameter? FindSourceParameter(
        SimpleMappingBuilderContext ctx,
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

    private static MethodParameter? FindTargetParameter(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method,
        MethodParameter sourceParameter,
        MethodParameter? refHandlerParameter
    )
    {
        var refHandlerParameterOrdinal = refHandlerParameter?.Ordinal ?? -1;

        // The target parameter is the first parameter,
        // which is not the source parameter,
        // and is not annotated as reference handling parameter.
        // It may be annotated as mapping target
        // (for example, if it is the very first parameter, which is often the case in extension methods).
        var targetParameterSymbol = method.Parameters.FirstOrDefault(p =>
            p.Ordinal != sourceParameter.Ordinal
            && p.Ordinal != refHandlerParameterOrdinal
            && !ctx.SymbolAccessor.HasAttribute<ReferenceHandlerAttribute>(p)
        );
        return ctx.SymbolAccessor.WrapOptionalMethodParameter(targetParameterSymbol);
    }

    private static MethodParameter? FindReferenceHandlerParameter(SimpleMappingBuilderContext ctx, IMethodSymbol method)
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
                method.ContainingType.ToDisplayString(),
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
                method.ContainingType.ToDisplayString(),
                method.Name
            );
        }

        return refHandlerParameter;
    }
}
