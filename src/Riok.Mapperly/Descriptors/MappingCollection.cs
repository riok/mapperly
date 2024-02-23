using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public class MappingCollection
{
    /// <summary>
    /// A list of all method mappings.
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
    /// All new instance mappings
    /// </summary>
    private readonly MappingCollectionInstance<INewInstanceMapping> _newInstanceMappings = new();

    /// <summary>
    /// All existing target mappings
    /// </summary>
    private readonly MappingCollectionInstance<IExistingTargetMapping> _existingTargetMappings = new();

    /// <inheritdoc cref="_methodMappings"/>
    public IReadOnlyCollection<MethodMapping> MethodMappings => _methodMappings;

    /// <inheritdoc cref="_userMappings"/>
    public IReadOnlyCollection<IUserMapping> UserMappings => _userMappings;

    /// <inheritdoc cref="MappingCollectionInstance{T}.UsedDuplicatedNonDefaultNonReferencedUserMappings"/>
    public IEnumerable<IUserMapping> UsedDuplicatedNonDefaultNonReferencedUserMappings =>
        _newInstanceMappings.UsedDuplicatedNonDefaultNonReferencedUserMappings.Concat(
            _existingTargetMappings.UsedDuplicatedNonDefaultNonReferencedUserMappings
        );

    public INewInstanceMapping? FindNewInstanceMapping(TypeMappingKey mappingKey) => _newInstanceMappings.Find(mappingKey);

    public INewInstanceMapping? FindNamedNewInstanceMapping(string name, out bool ambiguousName) =>
        _newInstanceMappings.FindNamed(name, out ambiguousName);

    public IExistingTargetMapping? FindExistingInstanceMapping(TypeMappingKey mappingKey) => _existingTargetMappings.Find(mappingKey);

    public IEnumerable<(IMapping, MappingBuilderContext)> DequeueMappingsToBuildBody() => _mappingsToBuildBody.DequeueAll();

    public void EnqueueToBuildBody(ITypeMapping mapping, MappingBuilderContext ctx) =>
        _mappingsToBuildBody.Enqueue((mapping, ctx), mapping.BodyBuildingPriority);

    public MappingCollectionAddResult AddUserMapping(IUserMapping userMapping, bool ignoreDuplicates, string? name)
    {
        _userMappings.Add(userMapping);

        if (userMapping is MethodMapping methodMapping)
        {
            _methodMappings.Add(methodMapping);
        }

        return userMapping switch
        {
            INewInstanceMapping newInstanceMapping
                => _newInstanceMappings.AddUserMapping(newInstanceMapping, ignoreDuplicates, userMapping.Default, name),
            IExistingTargetMapping existingTargetMapping
                => _existingTargetMappings.AddUserMapping(existingTargetMapping, ignoreDuplicates, userMapping.Default, name),
            _ => throw new ArgumentOutOfRangeException(nameof(userMapping), userMapping.GetType().FullName + " mappings are not supported")
        };
    }

    public void AddMapping(ITypeMapping mapping, TypeMappingConfiguration config)
    {
        switch (mapping)
        {
            case INewInstanceMapping newInstanceMapping:
                AddNewInstanceMapping(newInstanceMapping, config);
                break;

            case IExistingTargetMapping existingTargetMapping:
                AddExistingTargetMapping(existingTargetMapping, config);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mapping), mapping.GetType().FullName + " mappings are not supported");
        }
    }

    public void AddNewInstanceMapping(INewInstanceMapping mapping, TypeMappingConfiguration config)
    {
        if (mapping is MethodMapping methodMapping)
        {
            _methodMappings.Add(methodMapping);
        }

        _newInstanceMappings.TryAddAsDefault(mapping, config);
    }

    public void AddExistingTargetMapping(IExistingTargetMapping mapping, TypeMappingConfiguration config)
    {
        if (mapping is MethodMapping methodMapping)
        {
            _methodMappings.Add(methodMapping);
        }

        _existingTargetMappings.TryAddAsDefault(mapping, config);
    }

    private class MappingCollectionInstance<T>
        where T : ITypeMapping
    {
        /// <summary>
        /// Callable mapping of each type pair + config.
        /// Contains mappings to build and already built mappings.
        /// </summary>
        private readonly Dictionary<TypeMappingKey, T> _defaultMappings = new();

        /// <summary>
        /// Named mappings by their names.
        /// </summary>
        private readonly Dictionary<string, T> _namedMappings = new();

        /// <summary>
        /// Duplicated mapping names.
        /// </summary>
        private readonly HashSet<string> _duplicatedMappingNames = new();

        /// <summary>
        /// All default user mapping type keys which have an explicit default value set.
        /// </summary>
        private readonly HashSet<TypeMappingKey> _defaultUserMappingKeys = new();

        /// <summary>
        /// Contains the first duplicated (=second) user implemented mapping
        /// for each type pair group with multiple user mappings
        /// but no default user mapping.
        /// </summary>
        private readonly Dictionary<TypeMappingKey, T> _firstDuplicatedNonDefaultUserMappings = new();

        /// <summary>
        /// All mapping keys for which <see cref="Find"/> was called and returned a non-null result.
        /// </summary>
        private readonly HashSet<TypeMappingKey> _usedMappingKeys = new();

        /// <summary>
        /// All mapping names for which <see cref="FindNamed"/> was called and returned a non-null result.
        /// </summary>
        private readonly HashSet<string> _referencedMappingNames = new();

        /// <inheritdoc cref="_firstDuplicatedNonDefaultUserMappings"/>
        /// <remarks>
        /// Includes only mappings for type-pairs which are actually in use.
        /// </remarks>
        public IEnumerable<IUserMapping> UsedDuplicatedNonDefaultNonReferencedUserMappings =>
            _usedMappingKeys
                .Select(_firstDuplicatedNonDefaultUserMappings.GetValueOrDefault)
                .WhereNotNull()
                .Cast<IUserMapping>()
                .Where(x => !_referencedMappingNames.Contains(x.Method.Name));

        public T? Find(TypeMappingKey mappingKey)
        {
            if (_defaultMappings.TryGetValue(mappingKey, out var mapping))
            {
                _usedMappingKeys.Add(mappingKey);
            }

            return mapping;
        }

        public T? FindNamed(string name, out bool ambiguousName)
        {
            ambiguousName = false;
            if (_namedMappings.TryGetValue(name, out var mapping))
            {
                ambiguousName = _duplicatedMappingNames.Contains(name);
                _referencedMappingNames.Add(name);
            }

            return mapping;
        }

        public MappingCollectionAddResult TryAddAsDefault(T mapping, TypeMappingConfiguration config)
        {
            if (!mapping.CallableByOtherMappings)
                return MappingCollectionAddResult.NotAddedIgnored;

            var mappingKey = new TypeMappingKey(mapping, config);
            if (_defaultMappings.ContainsKey(mappingKey))
                return MappingCollectionAddResult.NotAddedDuplicated;

            _defaultMappings[mappingKey] = mapping;
            return MappingCollectionAddResult.Added;
        }

        public MappingCollectionAddResult AddUserMapping(T mapping, bool ignoreDuplicates, bool? isDefault, string? name)
        {
            if (!mapping.CallableByOtherMappings)
                return MappingCollectionAddResult.NotAddedIgnored;

            if (name != null && !_namedMappings.TryAdd(name, mapping))
            {
                _duplicatedMappingNames.Add(name);
            }

            return isDefault switch
            {
                // the mapping is not a default mapping.
                false => MappingCollectionAddResult.NotAddedIgnored,

                // if a default mapping was already added for this type-pair
                // ignore the current one and return duplicated
                // otherwise overwrite the existing with the new default one.
                true => AddDefaultUserMapping(mapping, ignoreDuplicates),

                // no default value specified
                // add it if none exists yet
                null => TryAddUserMappingAsDefault(mapping, ignoreDuplicates)
            };
        }

        private MappingCollectionAddResult TryAddUserMappingAsDefault(T mapping, bool ignoreDuplicates)
        {
            var addResult = TryAddAsDefault(mapping, TypeMappingConfiguration.Default);
            var mappingKey = new TypeMappingKey(mapping);

            if (ignoreDuplicates && addResult == MappingCollectionAddResult.NotAddedDuplicated)
            {
                addResult = MappingCollectionAddResult.NotAddedIgnored;
            }

            // the mapping was not added due to it being a duplicate,
            // there is no default mapping declared (yet)
            // and no duplicate is registered yet
            // and it is a user mapping implementation
            // then store this as duplicate
            // this is needed to report a diagnostic if multiple non-default mappings
            // are registered for the same type-pair without any default mapping.
            if (
                addResult == MappingCollectionAddResult.NotAddedDuplicated
                && !_defaultUserMappingKeys.Contains(mappingKey)
                && !_firstDuplicatedNonDefaultUserMappings.ContainsKey(mappingKey)
                && mapping is UserImplementedMethodMapping or UserImplementedExistingTargetMethodMapping
            )
            {
                _firstDuplicatedNonDefaultUserMappings.Add(mappingKey, mapping);
            }

            return addResult;
        }

        private MappingCollectionAddResult AddDefaultUserMapping(T mapping, bool ignoreDuplicates)
        {
            var mappingKey = new TypeMappingKey(mapping);
            if (!_defaultUserMappingKeys.Add(mappingKey))
            {
                return ignoreDuplicates ? MappingCollectionAddResult.NotAddedIgnored : MappingCollectionAddResult.NotAddedDuplicatedDefault;
            }

            _firstDuplicatedNonDefaultUserMappings.Remove(mappingKey);
            _defaultMappings[mappingKey] = mapping;
            return MappingCollectionAddResult.Added;
        }
    }
}
