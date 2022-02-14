using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.TypeMappings;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class CtorMappingBuilder
{
    public static CtorMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (ctx.Target is not INamedTypeSymbol namedTarget)
            return null;

        // resolve ctors which have the source as single argument
        var ctorMethod = namedTarget.InstanceConstructors
            .FirstOrDefault(m =>
                m.Parameters.Length == 1
                && SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, ctx.Source));

        return ctorMethod == null
            ? null
            : new CtorMapping(ctx.Source, ctx.Target);
    }
}
