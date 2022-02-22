using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class UserMethodMappingBuilder
{
    public static IEnumerable<TypeMapping> ExtractUserMappings(
        SimpleMappingBuilderContext ctx,
        ITypeSymbol mapperSymbol)
    {
        // extract user implemented and user defined mappings from mapper
        foreach (var methodSymbol in ExtractMethods(mapperSymbol))
        {
            var mapping = BuilderUserDefinedMapping(ctx, methodSymbol) ?? BuildUserImplementedMapping(methodSymbol);
            if (mapping != null)
                yield return mapping;
        }

        // extract user implemented mappings from base methods
        foreach (var method in ExtractBaseMethods(ctx.Compilation.ObjectType, mapperSymbol))
        {
            var mapping = BuildUserImplementedMapping(method);
            if (mapping != null)
                yield return mapping;
        }
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedNewInstanceMethodMapping mapping)
    {
        mapping.DelegateMapping = ctx.BuildDelegateMapping(mapping.SourceType, mapping.TargetType);
        if (mapping.DelegateMapping == null)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.CouldNotCreateMapping,
                mapping.SourceType,
                mapping.TargetType);
        }
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

    private static TypeMapping? BuildUserImplementedMapping(IMethodSymbol m)
    {
        return IsNewInstanceMappingMethod(m) && !m.IsAbstract && !m.IsPartialDefinition
            ? new UserImplementedMethodMapping(m)
            : null;
    }

    private static TypeMapping? BuilderUserDefinedMapping(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol methodSymbol)
    {
        if (!methodSymbol.IsPartialDefinition)
            return null;

        if (IsExistingInstanceMappingMethod(methodSymbol))
            return new UserDefinedExistingInstanceMethodMapping(methodSymbol);

        if (IsNewInstanceMappingMethod(methodSymbol))
            return new UserDefinedNewInstanceMethodMapping(methodSymbol);

        ctx.ReportDiagnostic(
            DiagnosticDescriptors.UnsupportedMappingMethodSignature,
            methodSymbol,
            methodSymbol.Name);
        return null;
    }

    private static bool IsExistingInstanceMappingMethod(IMethodSymbol m)
        => m.Parameters.Length == 2
            && m.ReturnsVoid
            && !m.IsAsync
            && !m.IsStatic
            && !m.IsGenericMethod;

    private static bool IsNewInstanceMappingMethod(IMethodSymbol m)
        => m.Parameters.Length == 1
            && !m.ReturnsVoid
            && !m.IsAsync
            && !m.IsStatic
            && !m.IsGenericMethod;
}
