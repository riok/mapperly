using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Abstractions.ReferenceHandling.Internal;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

public static class UserMethodMappingExtractor
{
    public static IEnumerable<IUserMapping> ExtractUserMappings(SimpleMappingBuilderContext ctx, ITypeSymbol mapperSymbol)
    {
        // extract user implemented and user defined mappings from mapper
        foreach (var methodSymbol in ExtractMethods(mapperSymbol))
        {
            var mapping =
                BuilderUserDefinedMapping(ctx, methodSymbol, mapperSymbol.IsStatic)
                ?? BuildUserImplementedMapping(ctx, methodSymbol, false, mapperSymbol.IsStatic);
            if (mapping != null)
                yield return mapping;
        }

        // static mapper cannot have base methods
        if (mapperSymbol.IsStatic)
            yield break;

        // extract user implemented mappings from base methods
        foreach (var method in ExtractBaseMethods(ctx.Compilation.ObjectType, mapperSymbol))
        {
            // Partial method declarations are allowed for base classes,
            // but still treated as user implemented methods,
            // since the user should provide an implementation elsewhere.
            // This is the case if a partial mapper class is extended.
            var mapping = BuildUserImplementedMapping(ctx, method, true, mapperSymbol.IsStatic);
            if (mapping != null)
                yield return mapping;
        }
    }

    private static IEnumerable<IMethodSymbol> ExtractMethods(ITypeSymbol mapperSymbol) => mapperSymbol.GetMembers().OfType<IMethodSymbol>();

    private static IEnumerable<IMethodSymbol> ExtractBaseMethods(INamedTypeSymbol objectType, ITypeSymbol mapperSymbol)
    {
        var baseMethods = mapperSymbol.BaseType?.GetAllMethods() ?? Enumerable.Empty<ISymbol>();
        var intfMethods = mapperSymbol.AllInterfaces.SelectMany(x => x.GetAllMethods());
        return baseMethods
            .Concat(intfMethods)
            .OfType<IMethodSymbol>()
            // ignore all non ordinary methods (eg. ctor, operators, etc.) and methods declared on the object type (eg. ToString)
            .Where(
                x =>
                    x.MethodKind == MethodKind.Ordinary
                    && x.IsAccessible(true)
                    && !SymbolEqualityComparer.Default.Equals(x.ReceiverType, objectType)
            );
    }

    private static IUserMapping? BuildUserImplementedMapping(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method,
        bool allowPartial,
        bool isStatic
    )
    {
        var valid =
            method is { ReturnsVoid: false, IsGenericMethod: false }
            && (allowPartial || !method.IsPartialDefinition)
            && (!isStatic || method.IsStatic);
        return valid && BuildParameters(ctx, method, out var parameters)
            ? new UserImplementedMethodMapping(method, parameters.Source, parameters.ReferenceHandler)
            : null;
    }

    private static IUserMapping? BuilderUserDefinedMapping(SimpleMappingBuilderContext ctx, IMethodSymbol methodSymbol, bool isStatic)
    {
        if (!methodSymbol.IsPartialDefinition)
            return null;

        if (!isStatic && methodSymbol.IsStatic)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.PartialStaticMethodInInstanceMapper, methodSymbol, methodSymbol.Name);
            return null;
        }

        if (methodSymbol.IsAsync)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, methodSymbol, methodSymbol.Name);
            return null;
        }

        // where method has at least one Type parameter ie Map(object obj, Type type);
        if (!methodSymbol.IsGenericMethod && BuildRuntimeTargetTypeMappingParameters(ctx, methodSymbol, out var runtimeTargetTypeParams))
        {
            return new UserDefinedNewInstanceRuntimeTargetTypeParameterMapping(
                methodSymbol,
                runtimeTargetTypeParams,
                ctx.MapperConfiguration.UseReferenceHandling,
                ctx.Types.Get<PreserveReferenceHandler>(),
                GetTypeSwitchNullArm(methodSymbol, runtimeTargetTypeParams, null),
                ctx.Compilation.ObjectType
            );
        }

        if (!BuildParameters(ctx, methodSymbol, out var parameters))
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, methodSymbol, methodSymbol.Name);
            return null;
        }

        // where method has a generic target type mapping ie Map<T>(object obj);
        if (BuildGenericTypeParameters(methodSymbol, parameters, out var typeParameters))
        {
            return new UserDefinedNewInstanceGenericTypeMapping(
                methodSymbol,
                typeParameters.Value,
                parameters,
                ctx.MapperConfiguration.UseReferenceHandling,
                ctx.Types.Get<PreserveReferenceHandler>(),
                GetTypeSwitchNullArm(methodSymbol, parameters, typeParameters),
                ctx.Compilation.ObjectType
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
                ctx.MapperConfiguration.UseReferenceHandling,
                ctx.Types.Get<PreserveReferenceHandler>()
            );
        }

        return new UserDefinedNewInstanceMethodMapping(
            methodSymbol,
            parameters.Source,
            parameters.ReferenceHandler,
            parameters.Parameters,
            ctx.MapperConfiguration.UseReferenceHandling,
            ctx.Types.Get<PreserveReferenceHandler>()
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

        var targetType = parameters.Target?.Type ?? methodSymbol.ReturnType.UpgradeNullable();
        var targetTypeParameter = methodSymbol.TypeParameters.FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x, targetType));
        var sourceTypeParameter = methodSymbol.TypeParameters.FirstOrDefault(
            x => SymbolEqualityComparer.Default.Equals(x, parameters.Source.Type)
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

        typeParameters = new GenericMappingTypeParameters(
            sourceTypeParameter,
            parameters.Source.Type.NullableAnnotation,
            targetTypeParameter,
            targetType.NullableAnnotation
        );
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
        var sourceParameter = MethodParameter.Wrap(method.Parameters.FirstOrDefault(p => p.Ordinal != refHandlerParameterOrdinal));
        expectedParametersCount++;
        if (sourceParameter == null)
        {
            parameters = null;
            return false;
        }

        // target type parameter is the second parameter (except if the reference handler is the first or the second parameter)
        var targetTypeParameter = MethodParameter.Wrap(
            method.Parameters.FirstOrDefault(p => p.Ordinal != sourceParameter.Value.Ordinal && p.Ordinal != refHandlerParameterOrdinal)
        );
        expectedParametersCount++;
        if (targetTypeParameter == null || !SymbolEqualityComparer.Default.Equals(targetTypeParameter.Value.Type, ctx.Types.Get<Type>()))
        {
            parameters = null;
            return false;
        }

        if (method.Parameters.Length != expectedParametersCount)
        {
            parameters = null;
            return false;
        }

        parameters = new RuntimeTargetTypeMappingMethodParameters(sourceParameter.Value, targetTypeParameter.Value, refHandlerParameter);
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
        var sourceParameter = MethodParameter.Wrap(method.Parameters.FirstOrDefault(p => p.Ordinal != refHandlerParameterOrdinal));
        if (sourceParameter == null)
        {
            parameters = null;
            return false;
        }

        // target parameter is the second parameter (except if the reference handler is the first or the second parameter)
        // if the method returns void, a target parameter is required
        // if the method doesnt return void, a target parameter is not allowed
        MethodParameter? targetParameter = null;
        if (method.ReturnsVoid)
        {
            targetParameter = MethodParameter.Wrap(
                method.Parameters.FirstOrDefault(p => p.Ordinal != sourceParameter.Value.Ordinal && p.Ordinal != refHandlerParameterOrdinal)
            );
            if (!targetParameter.HasValue)
            {
                parameters = null;
                return false;
            }
            expectedParameterCount++;
        }

        var parma = method.Parameters.Skip(expectedParameterCount).Select(MethodParameter.Wrap10).WhereNotNull().ToArray();
        // if (method.Parameters.Length != expectedParameterCount)
        // {
        //     parameters = null;
        //     return false;
        // }

        parameters = new MappingMethodParameters(sourceParameter.Value, targetParameter, refHandlerParameter, parma);

        return true;
    }

    private static MethodParameter? BuildReferenceHandlerParameter(SimpleMappingBuilderContext ctx, IMethodSymbol method)
    {
        var refHandlerParameterSymbol = method.Parameters.FirstOrDefault(p => p.HasAttribute(ctx.Types.Get<ReferenceHandlerAttribute>()));
        if (refHandlerParameterSymbol == null)
            return null;

        var refHandlerParameter = new MethodParameter(refHandlerParameterSymbol);
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
}
