using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public class MappingCollection
{
    /// <summary>
    /// The first callable mapping of each type pair.
    /// Contains mappings to build and already built mappings
    /// </summary>
    private readonly Dictionary<TypeMappingKey, INewInstanceMapping> _mappings = new();

    /// <summary>
    /// A list of all method mappings (extra mappings and mappings)
    /// </summary>
    private readonly List<MethodMapping> _methodMappings = new();

    /// <summary>
    /// A list of all user mappings.
    /// </summary>
    private readonly List<IUserMapping> _userMappings = new();

    /// <summary>
    /// Queue of mappings which don't have the body built yet
    /// </summary>
    private readonly PriorityQueue<(IMapping, MappingBuilderContext), MappingBodyBuildingPriority> _mappingsToBuildBody = new();

    /// <summary>
    /// All existing target mappings
    /// </summary>
    private readonly Dictionary<TypeMappingKey, IExistingTargetMapping> _existingTargetMappings = new();

    public IReadOnlyCollection<MethodMapping> MethodMappings => _methodMappings;

    /// <inheritdoc cref="_userMappings"/>
    public IReadOnlyCollection<IUserMapping> UserMappings => _userMappings;

    public INewInstanceMapping? Find(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        _mappings.TryGetValue(new TypeMappingKey(sourceType, targetType), out var mapping);
        return mapping;
    }

    public IExistingTargetMapping? FindExistingInstanceMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        _existingTargetMappings.TryGetValue(new TypeMappingKey(sourceType, targetType), out var mapping);
        return mapping;
    }

    public void EnqueueToBuildBody(IMapping mapping, MappingBuilderContext ctx) =>
        _mappingsToBuildBody.Enqueue((mapping, ctx), mapping.BodyBuildingPriority);

    public void Add(ITypeMapping mapping)
    {
        if (mapping is IUserMapping userMapping)
        {
            _userMappings.Add(userMapping);
        }

        switch (mapping)
        {
            case INewInstanceMapping newInstanceMapping:
                AddNewInstanceMapping(newInstanceMapping);
                break;
            case IExistingTargetMapping existingTargetMapping:
                AddExistingTargetMapping(existingTargetMapping);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mapping), mapping.GetType().FullName + " mappings are not supported");
        }
    }

    public void AddNewInstanceMapping(INewInstanceMapping mapping)
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

    public void AddExistingTargetMapping(IExistingTargetMapping mapping)
    {
        if (mapping.CallableByOtherMappings && FindExistingInstanceMapping(mapping.SourceType, mapping.TargetType) is null)
        {
            _existingTargetMappings.Add(new TypeMappingKey(mapping), mapping);
        }
    }

    public IEnumerable<(IMapping, MappingBuilderContext)> DequeueMappingsToBuildBody() => _mappingsToBuildBody.DequeueAll();
}
