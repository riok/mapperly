using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

[DebuggerDisplay("{Source.Name} => {Target.Name}")]
public readonly struct TypeMappingKey(
    ITypeSymbol source,
    ITypeSymbol target,
    TypeMappingConfiguration? config = null,
    bool includeNullability = true
)
{
    private static readonly IEqualityComparer<ISymbol?> _comparer = SymbolEqualityComparer.IncludeNullability;

    public TypeMappingKey(ITypeMapping mapping, TypeMappingConfiguration? config = null, bool includeNullability = true)
        : this(mapping.SourceType, mapping.TargetType, config, includeNullability) { }

    public ITypeSymbol Source { get; } = includeNullability ? source : source.NonNullable();

    public ITypeSymbol Target { get; } = includeNullability ? target : target.NonNullable();

    public TypeMappingConfiguration Configuration { get; } = config ?? TypeMappingConfiguration.Default;

    public TypeMappingKey NonNullableSource() => new(Source.NonNullable(), Target, Configuration);

    public TypeMappingKey NonNullableTarget() => new(Source, Target.NonNullable(), Configuration);

    public TypeMappingKey NonNullable() => new(Source.NonNullable(), Target.NonNullable(), Configuration);

    private bool Equals(TypeMappingKey other) =>
        _comparer.Equals(Source, other.Source) && _comparer.Equals(Target, other.Target) && Configuration.Equals(other.Configuration);

    public override bool Equals(object? obj) => obj is TypeMappingKey other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _comparer.GetHashCode(Source);
            hashCode = (hashCode * 397) ^ _comparer.GetHashCode(Target);
            hashCode = (hashCode * 397) ^ EqualityComparer<TypeMappingConfiguration?>.Default.GetHashCode(Configuration);
            return hashCode;
        }
    }

    public static bool operator ==(TypeMappingKey left, TypeMappingKey right) => left.Equals(right);

    public static bool operator !=(TypeMappingKey left, TypeMappingKey right) => !left.Equals(right);
}
