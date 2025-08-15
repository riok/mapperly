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
        // If the method doesn't return void
        //  1) If the method returns bool and has an out parameter, we assign the result into the out parameter
        //  2) Otherwise, the target parameter is not allowed (original behavior)

        MethodParameter? targetParameter = null;
        MethodParameter? resultOutParameter = null;

        if (method.ReturnsVoid)
        {
            targetParameter = FindTargetParameter(ctx, method, sourceParameter.Value, refHandlerParameter);
            if (!targetParameter.HasValue)
            {
                parameters = null;
                return false;
            }
        }
        else if (method.ReturnType.SpecialType == SpecialType.System_Boolean)
        {
            // If the method returns bool, there might be an out parameter
            resultOutParameter = FindOutParameter(ctx, method);
        }

        var targetParameterOrdinal = targetParameter?.Ordinal ?? -1;
        var resultOutParameterOrdinal = resultOutParameter?.Ordinal ?? -1;

        var additionalParameterSymbols = method
            .Parameters.Where(p =>
                p.Ordinal != sourceParameter.Value.Ordinal
                && p.Ordinal != targetParameterOrdinal
                && p.Ordinal != refHandlerParameterOrdinal
                && p.Ordinal != resultOutParameterOrdinal
            )
            .ToList();

        if (!allowAdditionalParameters && additionalParameterSymbols.Count > 0)
        {
            parameters = null;
            return false;
        }

        // additional parameters should not be attributed as target or ref handler
        var hasInvalidAdditionalParameter = additionalParameterSymbols.Exists(p =>
            p.Type.TypeKind is TypeKind.TypeParameter or TypeKind.Error
            || ctx.SymbolAccessor.HasAttribute<ReferenceHandlerAttribute>(p)
            || ctx.SymbolAccessor.HasAttribute<MappingTargetAttribute>(p)
        );
        if (hasInvalidAdditionalParameter)
        {
            parameters = null;
            return false;
        }

        var additionalParameters = additionalParameterSymbols.Select(p => ctx.SymbolAccessor.WrapMethodParameter(p)).ToList();
        parameters = new MappingMethodParameters(
            sourceParameter.Value,
            targetParameter,
            refHandlerParameter,
            additionalParameters,
            resultOutParameter
        );
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

        MethodParameter? resultOutParameter = null;

        // We only look for an out parameter if the method has a boolean return type
        // This is a pattern for bool TryMap(object source, Type targetType, out object result)
        if (method.ReturnType.SpecialType == SpecialType.System_Boolean)
        {
            resultOutParameter = FindOutParameter(ctx, method);
            if (resultOutParameter.HasValue)
                expectedParameterCount++;
        }

        if (method.Parameters.Length != expectedParameterCount)
        {
            parameters = null;
            return false;
        }

        parameters = new RuntimeTargetTypeMappingMethodParameters(
            sourceParameter.Value,
            targetTypeParameter.Value,
            refHandlerParameter,
            resultOutParameter
        );
        return true;
    }

    private static MethodParameter? FindOutParameter(SimpleMappingBuilderContext ctx, IMethodSymbol method)
    {
        // The FindTargetParameter is going to be the System.Type. Should we add a check here to make sure that the out parameter
        // can be assigned to the return type?
        var resultOutParameter = method.Parameters.FirstOrDefault(p => p.RefKind == RefKind.Out);
        return resultOutParameter == null ? null : ctx.SymbolAccessor.WrapOptionalMethodParameter(resultOutParameter);
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
