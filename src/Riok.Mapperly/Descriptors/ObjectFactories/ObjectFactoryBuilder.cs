using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

public static class ObjectFactoryBuilder
{
    public static ObjectFactoryCollection ExtractObjectFactories(SimpleMappingBuilderContext ctx, ITypeSymbol mapperSymbol)
    {
        var objectFactoryAttribute = ctx.GetTypeSymbol(typeof(ObjectFactoryAttribute));

        var objectFactories = mapperSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.HasAttribute(objectFactoryAttribute))
            .Select(x => BuildObjectFactory(ctx, x))
            .WhereNotNull()
            .ToList();

        return new ObjectFactoryCollection(objectFactories);
    }

    private static ObjectFactory? BuildObjectFactory(SimpleMappingBuilderContext ctx, IMethodSymbol methodSymbol)
    {
        if (methodSymbol.IsAsync
            || methodSymbol.Parameters.Length != 0
            || methodSymbol.IsPartialDefinition
            || methodSymbol.MethodKind != MethodKind.Ordinary
            || methodSymbol.ReturnsVoid)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.InvalidObjectFactorySignature, methodSymbol, methodSymbol.Name);
            return null;
        }

        if (!methodSymbol.IsGenericMethod)
            return new ObjectFactory(methodSymbol);

        if (methodSymbol.TypeParameters.Length != 1)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.InvalidObjectFactorySignature, methodSymbol, methodSymbol.Name);
            return null;
        }

        if (methodSymbol.ReturnType.TypeKind != TypeKind.TypeParameter)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.InvalidObjectFactorySignature, methodSymbol, methodSymbol.Name);
            return null;
        }

        return new GenericObjectFactory(methodSymbol, ctx.Compilation);
    }
}
