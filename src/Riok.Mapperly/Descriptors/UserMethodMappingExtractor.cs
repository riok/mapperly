using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
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
                BuildUserDefinedMapping(ctx, method)
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
    ) => ExtractNamedUserImplementedMappings<INewInstanceUserMapping>(ctx, mapperSymbol, name);

    internal static IEnumerable<IExistingTargetUserMapping> ExtractNamedUserImplementedExistingInstanceMappings(
        SimpleMappingBuilderContext ctx,
        ITypeSymbol mapperSymbol,
        string name
    ) => ExtractNamedUserImplementedMappings<IExistingTargetUserMapping>(ctx, mapperSymbol, name);

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

        // ignore all non-ordinary methods (e.g. ctor, operators, etc.) and methods declared on the object type (e.g. ToString)
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

        if (!valid || !UserMappingMethodParameterExtractor.BuildParameters(ctx, method, false, out var parameters))
        {
            if (!hasAttribute)
                return null;

            var name = receiver == null ? method.Name : receiver + method.Name;
            ctx.ReportDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, method, name);
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

        var (targetType, targetTypeNullability) = BuildTargetType(ctx, method, parameters.Source.Name);
        return new UserImplementedMethodMapping(
            receiver,
            method,
            userMappingConfig.Default,
            parameters.Source,
            targetType,
            parameters.ReferenceHandler,
            isExternal,
            targetTypeNullability
        );
    }

    private static (ITypeSymbol, UserImplementedMethodMapping.TargetNullability) BuildTargetType(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method,
        string sourceParameterName
    )
    {
        var targetType = ctx.SymbolAccessor.UpgradeNullable(method.ReturnType);
        if (!targetType.IsNullable() || ctx.SymbolAccessor.TryHasAttribute<NotNullAttribute>(method.GetReturnTypeAttributes()))
        {
            return (targetType, UserImplementedMethodMapping.TargetNullability.NeverNull);
        }

        var targetNotNullIfSourceNotNull = ctx
            .AttributeAccessor.TryAccess<NotNullIfNotNullAttribute>(method.GetReturnTypeAttributes())
            .Any(attr => string.Equals(attr.ParameterName, sourceParameterName, StringComparison.Ordinal));
        var nullability = targetNotNullIfSourceNotNull
            ? UserImplementedMethodMapping.TargetNullability.NotNullIfSourceNotNull
            : UserImplementedMethodMapping.TargetNullability.Nullable;
        return (targetType, nullability);
    }

    private static IUserMapping? BuildUserDefinedMapping(SimpleMappingBuilderContext ctx, IMethodSymbol methodSymbol)
    {
        if (!methodSymbol.IsPartialDefinition)
            return null;

        if (methodSymbol.IsAsync)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, methodSymbol, methodSymbol.Name);
            return null;
        }

        if (TryBuildRuntimeTargetTypeMapping(ctx, methodSymbol) is { } userMapping)
            return userMapping;

        if (!UserMappingMethodParameterExtractor.BuildParameters(ctx, methodSymbol, true, out var parameters))
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
            )
            {
                AdditionalSourceParameters = parameters.AdditionalParameters,
            };
        }

        var userMappingConfig = GetUserMappingConfig(ctx, methodSymbol, out _);
        if (userMappingConfig.Default == true && parameters.AdditionalParameters.Count > 0)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.MappingMethodWithAdditionalParametersCannotBeDefaultMapping,
                methodSymbol,
                methodSymbol.Name
            );
        }

        var mapping = new UserDefinedNewInstanceMethodMapping(
            methodSymbol,
            parameters.AdditionalParameters.Count == 0 ? userMappingConfig.Default : false,
            parameters.Source,
            parameters.ReferenceHandler,
            ctx.SymbolAccessor.UpgradeNullable(methodSymbol.ReturnType),
            ctx.Configuration.Mapper.UseReferenceHandling
        )
        {
            AdditionalSourceParameters = parameters.AdditionalParameters,
        };
        return mapping;
    }

    private static UserDefinedNewInstanceRuntimeTargetTypeParameterMapping? TryBuildRuntimeTargetTypeMapping(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol methodSymbol
    )
    {
        if (methodSymbol.IsGenericMethod)
            return null;

        if (
            !UserMappingMethodParameterExtractor.BuildRuntimeTargetTypeMappingParameters(ctx, methodSymbol, out var runtimeTargetTypeParams)
        )
        {
            return null;
        }

        return new UserDefinedNewInstanceRuntimeTargetTypeParameterMapping(
            methodSymbol,
            runtimeTargetTypeParams,
            ctx.Configuration.Mapper.UseReferenceHandling,
            ctx.SymbolAccessor.UpgradeNullable(methodSymbol.ReturnType),
            GetTypeSwitchNullArm(methodSymbol, runtimeTargetTypeParams),
            ctx.Compilation.ObjectType.WithNullableAnnotation(NullableAnnotation.NotAnnotated)
        );
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
        return !sourceCanBeNull ? null
            : targetCanBeNull ? NullFallbackValue.Default
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

    private static IEnumerable<T> ExtractNamedUserImplementedMappings<T>(
        SimpleMappingBuilderContext ctx,
        ITypeSymbol mapperSymbol,
        string name
    )
    {
        return ctx
            .SymbolAccessor.GetAllDirectlyAccessibleMethods(mapperSymbol, name)
            .Where(m => IsMappingMethodCandidate(ctx, m, requireAttribute: false))
            .Select(m => BuildUserImplementedMapping(ctx, m, null, allowPartial: true, isStatic: mapperSymbol.IsStatic, isExternal: false))
            .OfType<T>();
    }
}
