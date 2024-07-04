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
        [NotNullWhen(true)] out MappingMethodParameters? parameters
    )
    {
        // the source param is always required
        var expectedParameterCount = 1;

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

        var targetParameter = FindTargetParameter(ctx, method, sourceParameter.Value, refHandlerParameter);

        // If the method returns void, a target parameter is required
        // if the method doesn't return void, a target parameter is not allowed.
        if (method.ReturnsVoid == !targetParameter.HasValue)
        {
            parameters = null;
            return false;
        }

        if (targetParameter.HasValue)
        {
            expectedParameterCount++;
        }

        parameters = new MappingMethodParameters(sourceParameter.Value, targetParameter, refHandlerParameter);
        return method.Parameters.Length == expectedParameterCount;
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
