using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Descriptors;

public class MappingCollection
{
    // this includes mappings to build and already built mappings
    private readonly Dictionary<TypeMappingKey, ITypeMapping> _mappings = new();

    // additional user defined mappings
    // (with same signature as already defined mappings but with different names)
    private readonly List<ITypeMapping> _extraMappings = new();

    // a list of all method mappings (extra mappings and mappings)
    private readonly List<MethodMapping> _methodMappings = new();

    // a list of all mappings (extra mappings and mappings)
    private readonly List<ITypeMapping> _allMappings = new();

    public IReadOnlyCollection<MethodMapping> MethodMappings => _methodMappings;

    public IReadOnlyCollection<ITypeMapping> All => _allMappings;

    public ITypeMapping? FindMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        _mappings.TryGetValue(new TypeMappingKey(sourceType, targetType), out var mapping);
        return mapping;
    }

    public void AddMapping(ITypeMapping mapping)
    {
        _allMappings.Add(mapping);
        if (mapping is MethodMapping methodMapping)
        {
            _methodMappings.Add(methodMapping);
        }

        if (mapping.CallableByOtherMappings && FindMapping(mapping.SourceType, mapping.TargetType) is null)
        {
            _mappings.Add(new TypeMappingKey(mapping), mapping);
            return;
        }

        _extraMappings.Add(mapping);
    }

    private readonly struct TypeMappingKey
    {
        private static readonly IEqualityComparer<ISymbol?> _comparer = SymbolEqualityComparer.IncludeNullability;

        private readonly ITypeSymbol _source;
        private readonly ITypeSymbol _target;

        public TypeMappingKey(ITypeMapping mapping)
        {
            _source = mapping.SourceType;
            _target = mapping.TargetType;
        }

        public TypeMappingKey(ITypeSymbol source, ITypeSymbol target)
        {
            _source = source;
            _target = target;
        }

        private bool Equals(TypeMappingKey other)
            => _comparer.Equals(_source, other._source)
                && _comparer.Equals(_target, other._target);

        public override bool Equals(object? obj)
            => obj is TypeMappingKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _comparer.GetHashCode(_source);
                hashCode = (hashCode * 397) ^ _comparer.GetHashCode(_target);
                return hashCode;
            }
        }

        public static bool operator ==(TypeMappingKey left, TypeMappingKey right)
            => left.Equals(right);

        public static bool operator !=(TypeMappingKey left, TypeMappingKey right)
            => !left.Equals(right);
    }
}
