using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

public static class UserMethodMappingExtractor
{
    internal static IEnumerable<IUserMapping> ExtractUserMappings(SimpleMappingBuilderContext ctx, ITypeSymbol mapperSymbol)
    {
        // extract user implemented and user defined mappings from mapper
        foreach (var methodSymbol in ExtractMethods(mapperSymbol))
        {
            var mapping =
                BuilderUserDefinedMapping(ctx, methodSymbol)
                ?? BuildUserImplementedMapping(ctx, methodSymbol, receiver: null, allowPartial: false, mapperSymbol.IsStatic);
            if (mapping != null)
                yield return mapping;
        }

        // static mapper cannot have base methods
        if (mapperSymbol.IsStatic)
            yield break;

        // extract user implemented mappings from base methods
        var methods = mapperSymbol.AllInterfaces.SelectMany(ctx.SymbolAccessor.GetAllMethods);
        if (mapperSymbol.BaseType is { } mapperBaseSymbol)
        {
            methods = methods.Concat(ctx.SymbolAccessor.GetAllMethods(mapperBaseSymbol));
        }

        foreach (var mapping in BuildUserImplementedMappings(ctx, methods, null, false))
        {
            yield return mapping;
        }
    }

    internal static IEnumerable<IUserMapping> ExtractUserImplementedMappings(
        SimpleMappingBuilderContext ctx,
        ITypeSymbol type,
        string? receiver,
        bool isStatic
    )
    {
        var methods = ctx.SymbolAccessor.GetAllMethods(type).Concat(type.AllInterfaces.SelectMany(ctx.SymbolAccessor.GetAllMethods));
        return BuildUserImplementedMappings(ctx, methods, receiver, isStatic);
    }

    private static IEnumerable<IMethodSymbol> ExtractMethods(ITypeSymbol mapperSymbol) => mapperSymbol.GetMembers().OfType<IMethodSymbol>();

    private static IEnumerable<IUserMapping> BuildUserImplementedMappings(
        SimpleMappingBuilderContext ctx,
        IEnumerable<IMethodSymbol> methods,
        string? receiver,
        bool isStatic
    )
    {
        foreach (var method in methods)
        {
            if (!IsMappingMethodCandidate(ctx, method))
                continue;

            // Partial method declarations are allowed for base classes,
            // but still treated as user implemented methods,
            // since the user should provide an implementation elsewhere.
            // This is the case if a partial mapper class is extended.
            var mapping = BuildUserImplementedMapping(ctx, method, receiver, true, isStatic);
            if (mapping != null)
                yield return mapping;
        }
    }

    private static bool IsMappingMethodCandidate(SimpleMappingBuilderContext ctx, IMethodSymbol method)
    {
        // ignore all non ordinary methods (eg. ctor, operators, etc.) and methods declared on the object type (eg. ToString)
        return method.MethodKind == MethodKind.Ordinary
            && ctx.SymbolAccessor.IsDirectlyAccessible(method)
            && !SymbolEqualityComparer.Default.Equals(method.ReceiverType, ctx.Compilation.ObjectType);
    }

    private static IUserMapping? BuildUserImplementedMapping(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method,
        string? receiver,
        bool allowPartial,
        bool isStatic
    )
    {
        var userMappingConfig = GetUserMappingConfig(ctx, method, out var hasAttribute);
        var valid = !method.IsGenericMethod && (allowPartial || !method.IsPartialDefinition) && (!isStatic || method.IsStatic);

        if (!valid || !BuildParameters(ctx, method, out var parameters))
        {
            if (hasAttribute)
            {
                var name = receiver == null ? method.Name : receiver + method.Name;
                ctx.ReportDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, method, name);
            }

            return null;
        }

        if (userMappingConfig.Ignore)
            return null;

        if (method.ReturnsVoid)
        {
            return new UserImplementedExistingTargetMethodMapping(
                receiver,
                method,
                parameters.Source,
                parameters.Target!.Value,
                parameters.ReferenceHandler
            );
        }

        return new UserImplementedMethodMapping(
            receiver,
            method,
            parameters.Source,
            ctx.SymbolAccessor.UpgradeNullable(method.ReturnType),
            parameters.ReferenceHandler
        );
    }

    private static IUserMapping? BuilderUserDefinedMapping(SimpleMappingBuilderContext ctx, IMethodSymbol methodSymbol)
    {
        if (!methodSymbol.IsPartialDefinition)
            return null;

        if (methodSymbol.IsAsync)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, methodSymbol, methodSymbol.Name);
            return null;
        }

        if (!methodSymbol.IsGenericMethod && BuildRuntimeTargetTypeMappingParameters(ctx, methodSymbol, out var runtimeTargetTypeParams))
        {
            return new UserDefinedNewInstanceRuntimeTargetTypeParameterMapping(
                methodSymbol,
                runtimeTargetTypeParams,
                ctx.MapperConfiguration.UseReferenceHandling,
                ctx.SymbolAccessor.UpgradeNullable(methodSymbol.ReturnType),
                GetTypeSwitchNullArm(methodSymbol, runtimeTargetTypeParams, null),
                ctx.Compilation.ObjectType.WithNullableAnnotation(NullableAnnotation.NotAnnotated)
            );
        }

        if (!BuildParameters(ctx, methodSymbol, out var parameters))
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, methodSymbol, methodSymbol.Name);
            return null;
        }

        if (BuildGenericTypeParameters(methodSymbol, parameters, out var typeParameters))
        {
            return new UserDefinedNewInstanceGenericTypeMapping(
                methodSymbol,
                typeParameters.Value,
                parameters,
                ctx.SymbolAccessor.UpgradeNullable(methodSymbol.ReturnType),
                ctx.MapperConfiguration.UseReferenceHandling,
                GetTypeSwitchNullArm(methodSymbol, parameters, typeParameters),
                ctx.Compilation.ObjectType.WithNullableAnnotation(NullableAnnotation.NotAnnotated)
            );
        }

        if (methodSymbol.IsGenericMethod)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, methodSymbol, methodSymbol.Name);
            return null;
        }

        if (parameters.Target.HasValue)
        {
            return new UserDefinedExistingTargetMethodMapping(
                methodSymbol,
                parameters.Source,
                parameters.Target.Value,
                parameters.ReferenceHandler,
                ctx.MapperConfiguration.UseReferenceHandling
            );
        }

        return new UserDefinedNewInstanceMethodMapping(
            methodSymbol,
            parameters.Source,
            parameters.ReferenceHandler,
            ctx.SymbolAccessor.UpgradeNullable(methodSymbol.ReturnType),
            ctx.MapperConfiguration.UseReferenceHandling
        );
    }

    private static bool BuildGenericTypeParameters(
        IMethodSymbol methodSymbol,
        MappingMethodParameters parameters,
        [NotNullWhen(true)] out GenericMappingTypeParameters? typeParameters
    )
    {
        if (!methodSymbol.IsGenericMethod)
        {
            typeParameters = null;
            return false;
        }

        var targetType = parameters.Target?.Type ?? methodSymbol.ReturnType;
        var targetTypeParameter = methodSymbol.TypeParameters.FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x, targetType));
        var sourceTypeParameter = methodSymbol.TypeParameters.FirstOrDefault(x =>
            SymbolEqualityComparer.Default.Equals(x, parameters.Source.Type)
        );

        var expectedTypeParametersCount = 0;
        if (targetTypeParameter != null)
        {
            expectedTypeParametersCount++;
        }

        if (sourceTypeParameter != null && !SymbolEqualityComparer.Default.Equals(sourceTypeParameter, targetTypeParameter))
        {
            expectedTypeParametersCount++;
        }

        if (methodSymbol.TypeParameters.Length != expectedTypeParametersCount)
        {
            typeParameters = null;
            return false;
        }

        typeParameters = new GenericMappingTypeParameters(sourceTypeParameter, targetTypeParameter);
        return true;
    }

    private static bool BuildRuntimeTargetTypeMappingParameters(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method,
        [NotNullWhen(true)] out RuntimeTargetTypeMappingMethodParameters? parameters
    )
    {
        var expectedParametersCount = 0;

        // reference handler parameter is always annotated
        var refHandlerParameter = BuildReferenceHandlerParameter(ctx, method);
        var refHandlerParameterOrdinal = refHandlerParameter?.Ordinal ?? -1;
        if (refHandlerParameter.HasValue)
        {
            expectedParametersCount++;
        }

        // source parameter is the first parameter (except if the reference handler is the first parameter)
        var sourceParameterSymbol = method.Parameters.FirstOrDefault(p => p.Ordinal != refHandlerParameterOrdinal);
        if (sourceParameterSymbol == null)
        {
            parameters = null;
            return false;
        }

        var sourceParameter = ctx.SymbolAccessor.WrapMethodParameter(sourceParameterSymbol);
        expectedParametersCount++;

        // target type parameter is the second parameter (except if the reference handler is the first or the second parameter)
        var targetTypeParameterSymbol = method.Parameters.FirstOrDefault(p =>
            p.Ordinal != sourceParameter.Ordinal && p.Ordinal != refHandlerParameterOrdinal
        );
        if (
            targetTypeParameterSymbol == null
            || !SymbolEqualityComparer.Default.Equals(targetTypeParameterSymbol.Type, ctx.Types.Get<Type>())
        )
        {
            parameters = null;
            return false;
        }

        var targetTypeParameter = ctx.SymbolAccessor.WrapMethodParameter(targetTypeParameterSymbol);
        expectedParametersCount++;

        if (method.Parameters.Length != expectedParametersCount)
        {
            parameters = null;
            return false;
        }

        parameters = new RuntimeTargetTypeMappingMethodParameters(sourceParameter, targetTypeParameter, refHandlerParameter);
        return true;
    }

    private static bool BuildParameters(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method,
        [NotNullWhen(true)] out MappingMethodParameters? parameters
    )
    {
        var expectedParameterCount = 1;

        // reference handler parameter is always annotated
        var refHandlerParameter = BuildReferenceHandlerParameter(ctx, method);
        var refHandlerParameterOrdinal = refHandlerParameter?.Ordinal ?? -1;
        if (refHandlerParameter.HasValue)
        {
            expectedParameterCount++;
        }

        // source parameter is the first parameter (except if the reference handler is the first parameter)
        var sourceParameterSymbol = method.Parameters.FirstOrDefault(p => p.Ordinal != refHandlerParameterOrdinal);
        if (sourceParameterSymbol == null)
        {
            parameters = null;
            return false;
        }

        var sourceParameter = ctx.SymbolAccessor.WrapMethodParameter(sourceParameterSymbol);

        // target parameter is the second parameter (except if the reference handler is the first or the second parameter)
        // if the method returns void, a target parameter is required
        // if the method doesnt return void, a target parameter is not allowed
        var targetParameter = ctx.SymbolAccessor.WrapOptionalMethodParameter(
            method.Parameters.FirstOrDefault(p => p.Ordinal != sourceParameter.Ordinal && p.Ordinal != refHandlerParameterOrdinal)
        );
        if (method.ReturnsVoid == !targetParameter.HasValue)
        {
            parameters = null;
            return false;
        }

        if (targetParameter.HasValue)
        {
            expectedParameterCount++;
        }

        if (method.Parameters.Length != expectedParameterCount)
        {
            parameters = null;
            return false;
        }

        parameters = new MappingMethodParameters(sourceParameter, targetParameter, refHandlerParameter);
        return true;
    }

    private static MethodParameter? BuildReferenceHandlerParameter(SimpleMappingBuilderContext ctx, IMethodSymbol method)
    {
        var refHandlerParameterSymbol = method.Parameters.FirstOrDefault(p =>
            ctx.SymbolAccessor.HasAttribute<ReferenceHandlerAttribute>(p)
        );
        if (refHandlerParameterSymbol == null)
            return null;

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

        if (!ctx.MapperConfiguration.UseReferenceHandling)
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

    private static NullFallbackValue GetTypeSwitchNullArm(
        IMethodSymbol method,
        MappingMethodParameters parameters,
        GenericMappingTypeParameters? typeParameters
    )
    {
        var targetCanBeNull = typeParameters?.TargetNullable ?? parameters.Target?.Type.IsNullable() ?? method.ReturnType.IsNullable();
        return targetCanBeNull ? NullFallbackValue.Default : NullFallbackValue.ThrowArgumentNullException;
    }

    private static UserMappingAttribute GetUserMappingConfig(SimpleMappingBuilderContext ctx, IMethodSymbol method, out bool hasAttribute)
    {
        var userMappingAttr = ctx.AttributeAccessor.AccessFirstOrDefault<UserMappingAttribute>(method);
        hasAttribute = userMappingAttr != null;
        return userMappingAttr ?? new UserMappingAttribute { Ignore = !ctx.MapperConfiguration.AutoUserMappings };
    }
}
