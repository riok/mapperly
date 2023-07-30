using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

public readonly struct TypeMappingKey
{
    private static readonly IEqualityComparer<ISymbol?> _comparer = SymbolEqualityComparer.IncludeNullability;
    private readonly ITypeSymbol _source;
    private readonly ITypeSymbol _target;
    private readonly ImmutableEquatableArray<MethodParameter> _parameters;

    public TypeMappingKey(ITypeMapping mapping, bool includeNullability = true)
        : this(mapping.SourceType, mapping.TargetType, mapping.Parameters, includeNullability) { }

    public TypeMappingKey(IExistingTargetMapping mapping, bool includeNullability = true)
        : this(mapping.SourceType, mapping.TargetType, mapping.Parameters, includeNullability) { }

    public TypeMappingKey(
        ITypeSymbol source,
        ITypeSymbol target,
        ImmutableEquatableArray<MethodParameter> parameters,
        bool includeNullability = true
    )
    {
        _parameters = parameters;
        _source = includeNullability ? source : source.NonNullable();
        _target = includeNullability ? target : target.NonNullable();
    }

    private bool Equals(TypeMappingKey other) =>
        _comparer.Equals(_source, other._source) && _comparer.Equals(_target, other._target) && _parameters.Equals(other._parameters);

    public override bool Equals(object? obj) => obj is TypeMappingKey other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _comparer.GetHashCode(_source);
            hashCode = (hashCode * 397) ^ _comparer.GetHashCode(_target);
            hashCode = (hashCode * 397) ^ _parameters.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(TypeMappingKey left, TypeMappingKey right) => left.Equals(right);

    public static bool operator !=(TypeMappingKey left, TypeMappingKey right) => !left.Equals(right);
}
