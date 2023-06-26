using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

public class MappingCollection
{
    /// <summary>
    /// The first callable mapping of each type pair.
    /// Contains mappings to build and already built mappings
    /// </summary>
    private readonly Dictionary<TypeMappingKey, List<ITypeMapping>> _mappings = new();

    /// <summary>
    /// A list of all method mappings (extra mappings and mappings)
    /// </summary>
    private readonly List<MethodMapping> _methodMappings = new();

    /// <summary>
    /// A list of all callable user mappings with <see cref="ITypeMapping.CallableByOtherMappings"/> <c>true</c>.
    /// </summary>
    private readonly List<IUserMapping> _callableUserMappings = new();

    /// <summary>
    /// Queue of mappings which don't have the body built yet
    /// </summary>
    private readonly Queue<(IMapping, MappingBuilderContext)> _mappingsToBuildBody = new();

    /// <summary>
    /// All existing target mappings
    /// </summary>
    private readonly Dictionary<TypeMappingKey, IExistingTargetMapping> _existingTargetMappings = new();

    // TODO: Move to context?
    // TODO: look at combining incomplete and scoped. Might prevent clever loop detection in the future.
    // Might become a hashset in the future
    private readonly Dictionary<TypeMappingKey, ITypeMapping> _incompleteMappings = new();

    private readonly Dictionary<TypeMappingKey, ITypeMapping> _scopedMappings = new();

    public IReadOnlyCollection<MethodMapping> MethodMappings => _methodMappings;

    /// <inheritdoc cref="_callableUserMappings"/>
    public IReadOnlyCollection<IUserMapping> CallableUserMappings => _callableUserMappings;

    public void ClearScope() => _scopedMappings.Clear();

    public ITypeMapping? GetIncomplete(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        var key = new TypeMappingKey(sourceType, targetType);
        _incompleteMappings.TryGetValue(key, out var value);
        return value;
    }

    public void AddIncomplete(ITypeMapping mapping)
    {
        _incompleteMappings.Add(new TypeMappingKey(mapping.SourceType, mapping.TargetType), mapping);
    }

    public void PopIncomplete(IMapping mapping)
    {
        var key = new TypeMappingKey(mapping.SourceType, mapping.TargetType);
        _incompleteMappings.Remove(key);
    }

    public ITypeMapping? Find(ITypeSymbol sourceType, ITypeSymbol targetType, MethodParameter[] parameters)
    {
        if (_incompleteMappings.Count > 0)
        {
            var key = new TypeMappingKey(sourceType, targetType);
            if (_incompleteMappings.TryGetValue(key, out var value))
            {
                return value;
            }
        }

        if (_scopedMappings.TryGetValue(new TypeMappingKey(sourceType, targetType), out var scopedMapping))
            return scopedMapping;

        if (!_mappings.TryGetValue(new TypeMappingKey(sourceType, targetType), out var mapping))
        {
            return null;
        }

        foreach (var item in mapping.OrderByDescending(x => x.Parameters.Length))
        {
            if (MethodParameter.MappableTo(parameters, item.Parameters))
                return item;
        }

        return null;
    }

    public IExistingTargetMapping? FindExistingInstanceMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        _existingTargetMappings.TryGetValue(new TypeMappingKey(sourceType, targetType), out var mapping);
        return mapping;
    }

    public void EnqueueToBuildBody(IMapping mapping, MappingBuilderContext ctx) => _mappingsToBuildBody.Enqueue((mapping, ctx));

    public void Add(ITypeMapping mapping)
    {
        if (mapping is IUserMapping { CallableByOtherMappings: true } userMapping)
        {
            _callableUserMappings.Add(userMapping);

            var key = new TypeMappingKey(mapping);
            if (_mappings.TryGetValue(key, out var value))
            {
                value.Add(mapping);
            }
            else
            {
                var list = new List<ITypeMapping>() { mapping };
                _mappings.Add(new TypeMappingKey(mapping), list);
            }
        }

        if (mapping is MethodMapping methodMapping)
        {
            _methodMappings.Add(methodMapping);
        }

        if (
            mapping.CallableByOtherMappings
            && Find(mapping.SourceType, mapping.TargetType, mapping.Parameters) is null
            && mapping is not IUserMapping
        )
        {
            if (mapping.Parameters.Length == 0)
            {
                var key = new TypeMappingKey(mapping);
                if (_mappings.TryGetValue(key, out var value))
                {
                    value.Add(mapping);
                }
                else
                {
                    var list = new List<ITypeMapping>() { mapping };
                    _mappings.Add(new TypeMappingKey(mapping), list);
                }
            }
            else
            {
                _scopedMappings.Add(new TypeMappingKey(mapping), mapping);
            }
        }
    }

    public void AddExistingTargetMapping(IExistingTargetMapping mapping) =>
        _existingTargetMappings.Add(new TypeMappingKey(mapping), mapping);

    public IEnumerable<(IMapping, MappingBuilderContext)> DequeueMappingsToBuildBody() => _mappingsToBuildBody.DequeueAll();
}
