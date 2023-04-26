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
        var ctorMethod = namedTarget.InstanceConstructors.FirstOrDefault(
            m =>
                m.Parameters.Length == 1
                && SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, ctx.Source)
                && ctx.Source.HasSameOrStricterNullability(m.Parameters[0].Type)
        );

        return ctorMethod == null ? null : new CtorMapping(ctx.Source, ctx.Target);
    }
}
