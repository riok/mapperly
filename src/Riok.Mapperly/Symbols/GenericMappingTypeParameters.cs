using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

/// <summary>
/// Method
/// </summary>
public readonly struct GenericMappingTypeParameters(ITypeParameterSymbol? sourceType, ITypeParameterSymbol? targetType)
{
    public ITypeParameterSymbol? SourceType { get; } = sourceType;

    public bool? SourceNullable { get; } = sourceType?.IsNullable();

    public ITypeParameterSymbol? TargetType { get; } = targetType;

    public bool? TargetNullable { get; } = targetType?.IsNullable();

    public bool DoesTypesSatisfyTypeParameterConstraints(SymbolAccessor symbolAccessor, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        return (SourceType == null || symbolAccessor.DoesTypeSatisfyTypeParameterConstraints(SourceType, sourceType))
            && (TargetType == null || symbolAccessor.DoesTypeSatisfyTypeParameterConstraints(TargetType, targetType));
    }
}
