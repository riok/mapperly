using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

/// <summary>
/// Method
/// </summary>
public readonly struct GenericMappingTypeParameters
{
    private readonly NullableAnnotation _sourceNullableAnnotation;
    private readonly NullableAnnotation _targetNullableAnnotation;

    public GenericMappingTypeParameters(
        ITypeParameterSymbol? sourceType,
        NullableAnnotation sourceNullableAnnotation,
        ITypeParameterSymbol? targetType,
        NullableAnnotation targetNullableAnnotation
    )
    {
        SourceType = sourceType;
        _sourceNullableAnnotation = sourceNullableAnnotation;
        TargetType = targetType;
        _targetNullableAnnotation = targetNullableAnnotation;

        SourceNullable = sourceType?.IsNullable(_sourceNullableAnnotation);
        TargetNullable = targetType?.IsNullable(_targetNullableAnnotation);
    }

    public ITypeParameterSymbol? SourceType { get; }

    public bool? SourceNullable { get; }

    public ITypeParameterSymbol? TargetType { get; }

    public bool? TargetNullable { get; }

    public bool CanConsumeTypes(Compilation compilation, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        return SourceType?.CanConsumeType(compilation, _sourceNullableAnnotation, sourceType) != false
            && TargetType?.CanConsumeType(compilation, _targetNullableAnnotation, targetType) != false;
    }
}
