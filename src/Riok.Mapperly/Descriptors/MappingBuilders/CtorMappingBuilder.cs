using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class CtorMappingBuilder
{
    public static CtorMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Constructor))
            return null;

        if (ctx.Target is not INamedTypeSymbol namedTarget)
            return null;

        // resolve ctors which have the source as single argument
        var ctor = FindSingleArgCtor(namedTarget, ctx.Source, ctx.SymbolAccessor);
        if (ctor == null)
            return null;

        return new CtorMapping(ctx.Source, ctx.Target, ctx.InstanceConstructors.BuildForConstructor(ctor));
    }

    internal static bool CtorAcceptsSourceType(INamedTypeSymbol target, ITypeSymbol sourceType, SymbolAccessor symbolAccessor) =>
        FindSingleArgCtor(target, sourceType, symbolAccessor) != null;

    private static IMethodSymbol? FindSingleArgCtor(INamedTypeSymbol target, ITypeSymbol sourceType, SymbolAccessor symbolAccessor) =>
        target
            .InstanceConstructors.Where(symbolAccessor.IsConstructorAccessible)
            .FirstOrDefault(m =>
                m.Parameters.Length == 1
                && SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type.NonNullable(), sourceType.NonNullable())
                && sourceType.HasSameOrStricterNullability(m.Parameters[0].Type)
            );
}
