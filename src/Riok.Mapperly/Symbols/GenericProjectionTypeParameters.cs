using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

public readonly struct GenericProjectionTypeParameters(
    ITypeSymbol sourceType,
    ITypeParameterSymbol? sourceTypeParameter,
    ITypeSymbol targetType,
    ITypeParameterSymbol targetTypeParameter,
    WellKnownTypes wellKnownTypes
)
{
    public ITypeSymbol SourceType { get; } = sourceType;
    public ITypeParameterSymbol? SourceTypeParameter { get; } = sourceTypeParameter;

    public ITypeSymbol TargetType { get; } = targetType;
    public ITypeParameterSymbol TargetTypeParameter { get; } = targetTypeParameter;

    public WellKnownTypes WellKnownTypes { get; } = wellKnownTypes;

    public bool DoesTypesSatisfyTypeParameterConstraints(SymbolAccessor symbolAccessor, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        var sourceTypesSatisfyTypeParameterConstraints =
            SourceTypeParameter is null && sourceType.Implements(WellKnownTypes.Get(typeof(IQueryable)))
            || SourceTypeParameter is not null
                && sourceType.ImplementsGeneric(WellKnownTypes.Get(typeof(IQueryable<>)), out var source)
                && symbolAccessor.DoesTypeSatisfyTypeParameterConstraints(SourceTypeParameter, source.TypeArguments[0]);

        var targetTypesSatisfyTypeParameterConstraints =
            targetType.ImplementsGeneric(WellKnownTypes.Get(typeof(IQueryable<>)), out var target)
            && symbolAccessor.DoesTypeSatisfyTypeParameterConstraints(TargetTypeParameter, target.TypeArguments[0]);

        return sourceTypesSatisfyTypeParameterConstraints && targetTypesSatisfyTypeParameterConstraints;
    }
}
