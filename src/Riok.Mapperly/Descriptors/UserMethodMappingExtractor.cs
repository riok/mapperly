using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Configuration;
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
        var methods = mapperSymbol.GetMembers().OfType<IMethodSymbol>().Where(method => IsMappingMethodCandidate(ctx, method));
        foreach (var method in methods)
        {
            var mapping =
                BuilderUserDefinedMapping(ctx, method)
                ?? BuildUserImplementedMapping(
                    ctx,
                    method,
                    receiver: null,
                    allowPartial: false,
                    isStatic: mapperSymbol.IsStatic,
                    isExternal: false
                );
            if (mapping != null)
                yield return mapping;
        }

        // static mapper cannot have base methods
        if (mapperSymbol.IsStatic)
            yield break;

        // extract user implemented mappings from base methods
        var baseAndInterfaceMethods = mapperSymbol.AllInterfaces.SelectMany(ctx.SymbolAccessor.GetAllMethods);
        if (mapperSymbol.BaseType is { } mapperBaseSymbol)
        {
            baseAndInterfaceMethods = baseAndInterfaceMethods.Concat(ctx.SymbolAccessor.GetAllMethods(mapperBaseSymbol));
        }

        baseAndInterfaceMethods = baseAndInterfaceMethods.Distinct(SymbolTypeEqualityComparer.MethodDefault);

        foreach (var mapping in BuildUserImplementedMappings(ctx, baseAndInterfaceMethods, null, isStatic: false, isExternal: true))
        {
            yield return mapping;
        }
    }

    internal static IEnumerable<INewInstanceUserMapping> ExtractNamedUserImplementedNewInstanceMappings(
        SimpleMappingBuilderContext ctx,
        ITypeSymbol mapperSymbol,
        string name
    )
    {
        return mapperSymbol
            .GetMembers(name)
            .OfType<IMethodSymbol>()
            .Where(m => IsMappingMethodCandidate(ctx, m, requireAttribute: false))
            .Select(m => BuildUserImplementedMapping(ctx, m, null, allowPartial: true, isStatic: mapperSymbol.IsStatic, isExternal: false))
            .OfType<INewInstanceUserMapping>();
    }

    internal static IEnumerable<IUserMapping> ExtractUserImplementedMappings(
        SimpleMappingBuilderContext ctx,
        ITypeSymbol type,
        string? receiver,
        bool isStatic,
        bool isExternal
    )
    {
        var methods = ctx
            .SymbolAccessor.GetAllMethods(type)
            .Concat(type.AllInterfaces.SelectMany(ctx.SymbolAccessor.GetAllMethods))
            .Distinct(SymbolTypeEqualityComparer.MethodDefault);
        return BuildUserImplementedMappings(ctx, methods, receiver, isStatic, isExternal);
    }

    private static IEnumerable<IUserMapping> BuildUserImplementedMappings(
        SimpleMappingBuilderContext ctx,
        IEnumerable<IMethodSymbol> methods,
        string? receiver,
        bool isStatic,
        bool isExternal
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
            var mapping = BuildUserImplementedMapping(ctx, method, receiver, true, isStatic, isExternal);
            if (mapping != null)
                yield return mapping;
        }
    }

    private static bool IsMappingMethodCandidate(SimpleMappingBuilderContext ctx, IMethodSymbol method, bool requireAttribute = true)
    {
        requireAttribute &= !ctx.Configuration.Mapper.AutoUserMappings;

        // ignore all non ordinary methods (eg. ctor, operators, etc.) and methods declared on the object type (eg. ToString)
        return method.MethodKind == MethodKind.Ordinary
            && ctx.SymbolAccessor.IsDirectlyAccessible(method)
            && !SymbolEqualityComparer.Default.Equals(method.ReceiverType, ctx.Compilation.ObjectType)
            && !ctx.SymbolAccessor.HasAttribute<ObjectFactoryAttribute>(method)
            && (
                !requireAttribute
                || ctx.SymbolAccessor.HasAttribute<UserMappingAttribute>(method)
                || method.IsPartialDefinition && ctx.SymbolAccessor.HasAttribute<MapperAttribute>(method.ContainingType)
            );
    }

    private static IUserMapping? BuildUserImplementedMapping(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method,
        string? receiver,
        bool allowPartial,
        bool isStatic,
        bool isExternal
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

        if (userMappingConfig.Ignore == true)
            return null;

        if (method.ReturnsVoid)
        {
            return new UserImplementedExistingTargetMethodMapping(
                receiver,
                method,
                userMappingConfig.Default,
                parameters.Source,
                parameters.Target!.Value,
                parameters.ReferenceHandler,
                isExternal
            );
        }

        return new UserImplementedMethodMapping(
            receiver,
            method,
            userMappingConfig.Default,
            parameters.Source,
            ctx.SymbolAccessor.UpgradeNullable(method.ReturnType),
            parameters.ReferenceHandler,
            isExternal
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
                ctx.Configuration.Mapper.UseReferenceHandling,
                ctx.SymbolAccessor.UpgradeNullable(methodSymbol.ReturnType),
                GetTypeSwitchNullArm(methodSymbol, runtimeTargetTypeParams),
                ctx.Compilation.ObjectType.WithNullableAnnotation(NullableAnnotation.NotAnnotated)
            );
        }

        if (!BuildParameters(ctx, methodSymbol, out var parameters))
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, methodSymbol, methodSymbol.Name);
            return null;
        }

        if (methodSymbol.IsGenericMethod)
        {
            return new UserDefinedNewInstanceGenericTypeMapping(
                methodSymbol,
                parameters,
                ctx.SymbolAccessor.UpgradeNullable(methodSymbol.ReturnType),
                ctx.Configuration.Mapper.UseReferenceHandling,
                GetTypeSwitchNullArm(methodSymbol, parameters),
                ctx.Compilation.ObjectType.WithNullableAnnotation(NullableAnnotation.NotAnnotated)
            );
        }

        if (parameters.Target.HasValue)
        {
            return new UserDefinedExistingTargetMethodMapping(
                methodSymbol,
                parameters.Source,
                parameters.Target.Value,
                parameters.ReferenceHandler,
                ctx.Configuration.Mapper.UseReferenceHandling
            );
        }

        var userMappingConfig = GetUserMappingConfig(ctx, methodSymbol, out _);
        return new UserDefinedNewInstanceMethodMapping(
            methodSymbol,
            userMappingConfig.Default,
            parameters.Source,
            parameters.ReferenceHandler,
            ctx.SymbolAccessor.UpgradeNullable(methodSymbol.ReturnType),
            ctx.Configuration.Mapper.UseReferenceHandling
        );
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

    private static NullFallbackValue? GetTypeSwitchNullArm(IMethodSymbol method, MappingMethodParameters parameters)
    {
        // target is always the return type for runtime target mappings
        Debug.Assert(parameters.Target == null);
        var targetType = method.ReturnType;
        var sourceType = parameters.Source.Type;

        // no polymorphism for extension methods...
        // for type parameters:
        // for the target type we assume a non-nullable by default
        // for the source type we assume a nullable by default
        var targetCanBeNull = targetType is ITypeParameterSymbol tpsTarget ? tpsTarget.IsNullable() ?? false : targetType.IsNullable();
        var sourceCanBeNull = sourceType is ITypeParameterSymbol tpsSource ? tpsSource.IsNullable() ?? true : sourceType.IsNullable();
        return !sourceCanBeNull
            ? null
            : targetCanBeNull
                ? NullFallbackValue.Default
                : NullFallbackValue.ThrowArgumentNullException;
    }

    private static UserMappingConfiguration GetUserMappingConfig(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method,
        out bool hasAttribute
    )
    {
        var userMappingAttr = ctx.AttributeAccessor.AccessFirstOrDefault<UserMappingAttribute, UserMappingConfiguration>(method);
        hasAttribute = userMappingAttr != null;
        return userMappingAttr ?? new UserMappingConfiguration();
    }
}
