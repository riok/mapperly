using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.TypeMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class UserMethodMappingBuilder
{
    public static IEnumerable<TypeMapping> ExtractUserMappings(
        SimpleMappingBuilderContext ctx,
        ITypeSymbol mapperSymbol)
    {
        var isAbstractMapperSyntax = mapperSymbol.TypeKind == TypeKind.Class;
        foreach (var methodSymbol in ExtractMethods(ctx.Compilation.ObjectType, mapperSymbol))
        {
            var mapping = BuilderUserMapping(ctx, isAbstractMapperSyntax, methodSymbol);
            if (mapping == null)
                continue;

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

    private static IEnumerable<IMethodSymbol> ExtractMethods(INamedTypeSymbol objectType, ITypeSymbol mapperSymbol)
    {
        return mapperSymbol.GetAllMembers()
            .Concat(mapperSymbol.AllInterfaces.SelectMany(x => x.GetAllMembers()))
            .OfType<IMethodSymbol>()

            // ignore all non ordinary methods (eg. ctor, operators, etc.) and methods declared on the object type (eg. ToString)
            .Where(x =>
                x.MethodKind == MethodKind.Ordinary
                && !SymbolEqualityComparer.Default.Equals(x.ReceiverType, objectType));
    }

    private static TypeMapping? BuilderUserMapping(
        SimpleMappingBuilderContext ctx,
        bool isAbstractMapperDefinition,
        IMethodSymbol methodSymbol)
    {
        // async and generic methods are not supported
        if (methodSymbol.IsAsync || methodSymbol.IsGenericMethod)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.UnsupportedMappingMethodSignature,
                methodSymbol,
                methodSymbol.Name);
            return null;
        }

        // if the method has exactly two parameters, returns void, and has no body (is abstract)
        // it is handled as a user defined mapping method which maps to an existing instance.
        if (methodSymbol.Parameters.Length == 2 && methodSymbol.ReturnsVoid && methodSymbol.IsAbstract)
            return new UserDefinedExistingInstanceMethodMapping(methodSymbol, isAbstractMapperDefinition);

        // all other mappings only support non-void single parameter methods
        if (methodSymbol.Parameters.Length != 1 || methodSymbol.ReturnsVoid)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.UnsupportedMappingMethodSignature,
                methodSymbol,
                methodSymbol.Name);
            return null;
        }

        // if the method has a body (is not abstract)
        // and is accessible it is a user implemented method mapping
        if (!methodSymbol.IsAbstract)
        {
            return methodSymbol.IsAccessible(true)
                ? new UserImplementedMethodMapping(methodSymbol)
                : null;
        }

        // else it is a user defined mapping which creates a new instance of the target class
        return new UserDefinedNewInstanceMethodMapping(methodSymbol, isAbstractMapperDefinition);
    }
}
