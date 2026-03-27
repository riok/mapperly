using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class CtorMappingBuilder
{
    public static CtorMapping? TryBuildNullableMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Constructor))
            return null;

        if (!ctx.Source.TryGetNonNullable(out _))
            return null;

        if (ctx.Target.TryGetNonNullable(out _))
            return null;

        // If e member mappings are configured (e.g. [MapProperty]),
        // do not use it as it bypasses property-by-property mapping
        if (ctx.Configuration.Members.ExplicitMappings.Count > 0)
            return null;

        if (ctx.Target is not INamedTypeSymbol namedTarget)
            return null;

        // resolve ctors which have the source as single argument
        var ctor = FindSingleArgCtor(namedTarget, ctx.Source, ctx.SymbolAccessor);
        if (ctor == null)
            return null;

        // if another constructor is explicitly marked with [MapperConstructor],
        // do not use the copy constructor and let the member mapping builder handle it
        if (HasMapperConstructorOnDifferentCtor(namedTarget, ctor, ctx.SymbolAccessor))
            return null;

        return new CtorMapping(ctx.Source, ctx.Target, ctx.InstanceConstructors.BuildForConstructor(ctor));
    }

    public static CtorMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Constructor))
            return null;

        if (ctx.Target is not INamedTypeSymbol namedTarget)
            return null;

        // If e member mappings are configured (e.g. [MapProperty]),
        // do not use it as it bypasses property-by-property mapping
        if (ctx.Configuration.Members.ExplicitMappings.Count > 0)
            return null;

        // resolve ctors which have the source as single argument
        var ctor = FindSingleArgCtor(namedTarget, ctx.Source, ctx.SymbolAccessor);
        if (ctor == null)
            return null;

        // if another constructor is explicitly marked with [MapperConstructor],
        // do not use the copy constructor and let the member mapping builder handle it
        if (HasMapperConstructorOnDifferentCtor(namedTarget, ctor, ctx.SymbolAccessor))
            return null;

        return new CtorMapping(ctx.Source, ctx.Target, ctx.InstanceConstructors.BuildForConstructor(ctor));
    }

    private static IMethodSymbol? FindSingleArgCtor(INamedTypeSymbol target, ITypeSymbol sourceType, SymbolAccessor symbolAccessor) =>
        target
            .InstanceConstructors.Where(symbolAccessor.IsConstructorAccessible)
            .FirstOrDefault(m =>
                m.Parameters.Length == 1
                && SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type.NonNullable(), sourceType.NonNullable())
                && sourceType.HasSameOrStricterNullability(m.Parameters[0].Type)
            );

    private static bool HasMapperConstructorOnDifferentCtor(
        INamedTypeSymbol target,
        IMethodSymbol copyCtor,
        SymbolAccessor symbolAccessor
    ) =>
        target.InstanceConstructors.Any(ctor =>
            !SymbolEqualityComparer.Default.Equals(ctor, copyCtor) && symbolAccessor.HasAttribute<MapperConstructorAttribute>(ctor)
        );
}
