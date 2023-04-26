using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public class MappingCollection
{
    // this includes mappings to build and already built mappings
    private readonly Dictionary<TypeMappingKey, ITypeMapping> _mappings = new();

    // a list of all method mappings (extra mappings and mappings)
    private readonly List<MethodMapping> _methodMappings = new();

    // queue of mappings which don't have the body built yet
    private readonly Queue<(IMapping, MappingBuilderContext)> _mappingsToBuildBody = new();

    // a list of existing target mappings
    private readonly Dictionary<TypeMappingKey, IExistingTargetMapping> _existingTargetMappings = new();

    public IReadOnlyCollection<MethodMapping> MethodMappings => _methodMappings;

    public ITypeMapping? Find(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        _mappings.TryGetValue(new TypeMappingKey(sourceType, targetType), out var mapping);
        return mapping;
    }

    public IExistingTargetMapping? FindExistingInstanceMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        _existingTargetMappings.TryGetValue(new TypeMappingKey(sourceType, targetType), out var mapping);
        return mapping;
    }

    public void EnqueueToBuildBody(IMapping mapping, MappingBuilderContext ctx) => _mappingsToBuildBody.Enqueue((mapping, ctx));

    public void Add(ITypeMapping mapping)
    {
        if (mapping is MethodMapping methodMapping)
        {
            _methodMappings.Add(methodMapping);
        }

        if (mapping.CallableByOtherMappings && Find(mapping.SourceType, mapping.TargetType) is null)
        {
            _mappings.Add(new TypeMappingKey(mapping), mapping);
        }
    }

    public void AddExistingTargetMapping(IExistingTargetMapping mapping) =>
        _existingTargetMappings.Add(new TypeMappingKey(mapping), mapping);

    public IEnumerable<(IMapping, MappingBuilderContext)> DequeueMappingsToBuildBody() => _mappingsToBuildBody.DequeueAll();

    private readonly struct TypeMappingKey
    {
        private static readonly IEqualityComparer<ISymbol?> _comparer = SymbolEqualityComparer.IncludeNullability;

        private readonly ITypeSymbol _source;
        private readonly ITypeSymbol _target;

        public TypeMappingKey(ITypeMapping mapping)
            : this(mapping.SourceType, mapping.TargetType) { }

        public TypeMappingKey(IExistingTargetMapping mapping)
            : this(mapping.SourceType, mapping.TargetType) { }

        public TypeMappingKey(ITypeSymbol source, ITypeSymbol target)
        {
            _source = source;
            _target = target;
        }

        private bool Equals(TypeMappingKey other) => _comparer.Equals(_source, other._source) && _comparer.Equals(_target, other._target);

        public override bool Equals(object? obj) => obj is TypeMappingKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _comparer.GetHashCode(_source);
                hashCode = (hashCode * 397) ^ _comparer.GetHashCode(_target);
                return hashCode;
            }
        }

        public static bool operator ==(TypeMappingKey left, TypeMappingKey right) => left.Equals(right);

        public static bool operator !=(TypeMappingKey left, TypeMappingKey right) => !left.Equals(right);
    }
}
