using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class UserMethodMappingBuilder
{
    public static IEnumerable<IUserMapping> ExtractUserMappings(
        SimpleMappingBuilderContext ctx,
        ITypeSymbol mapperSymbol)
    {
        // extract user implemented and user defined mappings from mapper
        foreach (var methodSymbol in ExtractMethods(mapperSymbol))
        {
            var mapping = BuilderUserDefinedMapping(ctx, methodSymbol, mapperSymbol.IsStatic)
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

    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedNewInstanceMethodMapping mapping)
    {
        var delegateMapping = mapping.CallableByOtherMappings
            ? ctx.BuildDelegateMapping(mapping.SourceType, mapping.TargetType)
            : ctx.BuildMappingWithUserSymbol(mapping.SourceType, mapping.TargetType);
        if (delegateMapping != null)
        {
            mapping.SetDelegateMapping(delegateMapping);
            return;
        }

        ctx.ReportDiagnostic(
            DiagnosticDescriptors.CouldNotCreateMapping,
            mapping.SourceType,
            mapping.TargetType);
    }

    private static IEnumerable<IMethodSymbol> ExtractMethods(ITypeSymbol mapperSymbol)
        => mapperSymbol.GetMembers().OfType<IMethodSymbol>();

    private static IEnumerable<IMethodSymbol> ExtractBaseMethods(INamedTypeSymbol objectType, ITypeSymbol mapperSymbol)
    {
        var baseMethods = mapperSymbol.BaseType?.GetAllMembers() ?? Enumerable.Empty<ISymbol>();
        var intfMethods = mapperSymbol.AllInterfaces.SelectMany(x => x.GetAllMembers());
        return baseMethods
            .Concat(intfMethods)
            .OfType<IMethodSymbol>()

            // ignore all non ordinary methods (eg. ctor, operators, etc.) and methods declared on the object type (eg. ToString)
            .Where(x =>
                x.MethodKind == MethodKind.Ordinary
                && x.IsAccessible(true)
                && !SymbolEqualityComparer.Default.Equals(x.ReceiverType, objectType));
    }

    private static IUserMapping? BuildUserImplementedMapping(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method,
        bool allowPartial,
        bool isStatic)
    {
        var valid = BuildParameters(ctx, method, out var parameters)
            && !method.ReturnsVoid
            && (allowPartial || !method.IsPartialDefinition)
            && isStatic == method.IsStatic;
        return valid
            ? new UserImplementedMethodMapping(method, parameters.Source, parameters.ReferenceHandler)
            : null;
    }

    private static IUserMapping? BuilderUserDefinedMapping(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol methodSymbol,
        bool isStatic)
    {
        if (!methodSymbol.IsPartialDefinition)
            return null;

        if (isStatic != methodSymbol.IsStatic)
        {
            ctx.ReportDiagnostic(
                isStatic ? DiagnosticDescriptors.PartialInstanceMethodInStaticMapper : DiagnosticDescriptors.PartialStaticMethodInInstanceMapper,
                methodSymbol,
                methodSymbol.Name);
            return null;
        }

        if (methodSymbol.IsAsync || methodSymbol.IsGenericMethod)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.UnsupportedMappingMethodSignature,
                methodSymbol,
                methodSymbol.Name);
            return null;
        }

        if (!BuildParameters(ctx, methodSymbol, out var parameters))
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.UnsupportedMappingMethodSignature,
                methodSymbol,
                methodSymbol.Name);
            return null;
        }

        if (parameters.Target.HasValue)
        {
            return new UserDefinedExistingInstanceMethodMapping(
                methodSymbol,
                parameters.Source,
                parameters.Target.Value,
                parameters.ReferenceHandler,
                ctx.MapperConfiguration.UseReferenceHandling,
                ctx.Types.PreserveReferenceHandler);
        }

        return new UserDefinedNewInstanceMethodMapping(
            methodSymbol,
            parameters.Source,
            parameters.ReferenceHandler,
            ctx.MapperConfiguration.UseReferenceHandling,
            ctx.Types.PreserveReferenceHandler);
    }

    private static bool BuildParameters(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method,
        out MappingMethodParameters parameters)
    {
        // reference handler parameter is always annotated
        var refHandlerParameter = BuildReferenceHandlerParameter(ctx, method);
        var refHandlerParameterOrdinal = refHandlerParameter?.Ordinal ?? -1;

        // source parameter is the first parameter (except if the reference handler is the first parameter)
        var sourceParameter = MethodParameter.Wrap(method.Parameters.FirstOrDefault(p => p.Ordinal != refHandlerParameterOrdinal));
        if (sourceParameter == null)
        {
            parameters = default;
            return false;
        }

        // target parameter is the second parameter (except if the reference handler is the first or the second parameter)
        // if the method returns void, a target parameter is required
        // if the method doesnt return void, a target parameter is not allowed
        var targetParameter = MethodParameter.Wrap(method.Parameters.FirstOrDefault(p => p.Ordinal != sourceParameter.Value.Ordinal && p.Ordinal != refHandlerParameterOrdinal));
        if (method.ReturnsVoid == (targetParameter == null))
        {
            parameters = default;
            return false;
        }

        parameters = new MappingMethodParameters(
            sourceParameter.Value,
            targetParameter,
            refHandlerParameter);
        return true;
    }

    private static MethodParameter? BuildReferenceHandlerParameter(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method)
    {
        var refHandlerParameterSymbol = method.Parameters.FirstOrDefault(p => p.HasAttribute(ctx.Types.ReferenceHandlerAttribute));
        if (refHandlerParameterSymbol == null)
            return null;

        var refHandlerParameter = new MethodParameter(refHandlerParameterSymbol);
        if (!SymbolEqualityComparer.Default.Equals(ctx.Types.IReferenceHandler, refHandlerParameter.Type))
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.ReferenceHandlerParameterWrongType,
                refHandlerParameterSymbol,
                method.ContainingType.ToDisplayString(),
                method.Name,
                ctx.Types.IReferenceHandler.ToDisplayString(),
                refHandlerParameterSymbol.ToDisplayString());
        }

        if (!ctx.MapperConfiguration.UseReferenceHandling)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.ReferenceHandlingNotEnabled,
                refHandlerParameterSymbol,
                method.ContainingType.ToDisplayString(),
                method.Name);
        }

        return refHandlerParameter;
    }
}
