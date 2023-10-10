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
        var ctorMethod = namedTarget.InstanceConstructors
            .Where(ctx.SymbolAccessor.IsAccessible)
            .FirstOrDefault(m => SameWithStricterNullability(m, ctx.Source));

        return ctorMethod == null ? null : new CtorMapping(ctx.Source, ctx.Target);
    }

    private static bool SameWithStricterNullability(IMethodSymbol m, ITypeSymbol src)
    {
        var parameter = m.Parameters[0].Type;
        var paramType = parameter.IsNullableValueType() ? ((INamedTypeSymbol)parameter).TypeArguments[0] : parameter;
        return m.Parameters.Length == 1
            && SymbolEqualityComparer.Default.Equals(paramType, src)
            && src.HasSameOrStricterNullability(paramType);
    }
}
