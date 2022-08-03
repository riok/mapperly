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
            var mapping = BuilderUserDefinedMapping(ctx, methodSymbol, mapperSymbol.IsStatic)
                ?? BuildUserImplementedMapping(methodSymbol, false, mapperSymbol.IsStatic);
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
            //  since the user should provide an implementation elsewhere.
            //  This is the case if a partial mapper class is extended.
            var mapping = BuildUserImplementedMapping(method, true, mapperSymbol.IsStatic);
            if (mapping != null)
                yield return mapping;
        }
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedNewInstanceMethodMapping mapping)
    {
        var delegateMapping = ctx.BuildDelegateMapping(mapping.SourceType, mapping.TargetType);
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

    private static TypeMapping? BuildUserImplementedMapping(IMethodSymbol m, bool allowPartial, bool isStatic)
    {
        return IsNewInstanceMappingMethod(m) && (allowPartial || !m.IsPartialDefinition) && (isStatic == m.IsStatic)
            ? new UserImplementedMethodMapping(m)
            : null;
    }

    private static TypeMapping? BuilderUserDefinedMapping(
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
            && !m.IsGenericMethod;

    private static bool IsNewInstanceMappingMethod(IMethodSymbol m)
        => m.Parameters.Length == 1
            && !m.ReturnsVoid
            && !m.IsAsync
            && !m.IsGenericMethod;
}
