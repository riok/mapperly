using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public class MappingCollection
{
    /// <summary>
    /// A list of all method mappings in order of registration/discovery.
    /// This is kept outside of <see cref="MappingCollectionInstance{T,TUserMapping}"/>
    /// to keep track of the registration order and generate the members in the same order as they are registered
    /// (the user defined order).
    /// </summary>
    private readonly List<MethodMapping> _methodMappings = [];

    /// <summary>
    /// A list of all user mappings.
    /// Does only include mappings which are discovered during the initial user mapping discovery.
    /// If a mapping is discovered afterwards (eg. by a name reference) it is not added to this collection.
    /// This is kept outside of <see cref="MappingCollectionInstance{T,TUserMapping}"/>
    /// to keep track of the registration order and generate the members in the same order as they are registered.
    /// </summary>
    private readonly List<IUserMapping> _userMappings = [];

    /// <summary>
    /// Queue of mappings which don't have the body built yet
    /// </summary>
    private readonly Queue<(IMapping, MappingBuilderContext)> _mappingsToBuildBody = new();

    /// <summary>
    /// All new instance mappings
    /// </summary>
    private readonly MappingCollectionInstance<INewInstanceMapping, INewInstanceUserMapping> _newInstanceMappings = new();

    /// <summary>
    /// All existing target mappings
    /// </summary>
    private readonly MappingCollectionInstance<IExistingTargetMapping, IExistingTargetUserMapping> _existingTargetMappings = new();

    /// <inheritdoc cref="_methodMappings"/>
    public IReadOnlyCollection<MethodMapping> MethodMappings => _methodMappings;

    /// <inheritdoc cref="_userMappings"/>
    public IReadOnlyCollection<IUserMapping> UserMappings => _userMappings;

    /// <inheritdoc cref="MappingCollectionInstance{INewInstanceMapping, INewInstanceUserMapping}.DefaultMappings"/>
    public IReadOnlyDictionary<TypeMappingKey, INewInstanceMapping> NewInstanceMappings => _newInstanceMappings.DefaultMappings;

    /// <inheritdoc cref="MappingCollectionInstance{T,TUserMapping}.UsedDuplicatedNonDefaultNonReferencedUserMappings"/>
    public IEnumerable<IUserMapping> UsedDuplicatedNonDefaultNonReferencedUserMappings =>
        Enumerable
            .Empty<IUserMapping>()
            .Concat(_newInstanceMappings.UsedDuplicatedNonDefaultNonReferencedUserMappings)
            .Concat(_existingTargetMappings.UsedDuplicatedNonDefaultNonReferencedUserMappings);

    public INewInstanceMapping? FindNewInstanceMapping(TypeMappingKey mappingKey) => _newInstanceMappings.Find(mappingKey);

    public INewInstanceMapping? FindNewInstanceUserMappingWithParameters(TypeMappingKey key, ParameterScope scope) =>
        _newInstanceMappings.FindUserMappingWithParameters(key, scope);

    public INewInstanceUserMapping? FindNewInstanceUserMapping(IMethodSymbol method) => _newInstanceMappings.FindUserMapping(method);

    public INewInstanceMapping? FindNamedNewInstanceMapping(string name, out bool ambiguousName) =>
        _newInstanceMappings.FindNamed(name, out ambiguousName);

    public IExistingTargetMapping? FindExistingInstanceMapping(TypeMappingKey mappingKey) => _existingTargetMappings.Find(mappingKey);

    public IExistingTargetMapping? FindExistingInstanceNamedMapping(string name, out bool ambiguousName) =>
        _existingTargetMappings.FindNamed(name, out ambiguousName);

    public IEnumerable<(IMapping, MappingBuilderContext)> DequeueMappingsToBuildBody() => _mappingsToBuildBody.DequeueAll();

    public void EnqueueToBuildBody(ITypeMapping mapping, MappingBuilderContext ctx) => _mappingsToBuildBody.Enqueue((mapping, ctx));

    public MappingCollectionAddResult AddUserMapping(IUserMapping userMapping, string? name)
    {
        _userMappings.Add(userMapping);

        if (userMapping is MethodMapping methodMapping)
        {
            _methodMappings.Add(methodMapping);
        }

        return userMapping switch
        {
            INewInstanceUserMapping newInstanceMapping => _newInstanceMappings.AddUserMapping(
                newInstanceMapping,
                userMapping.Default,
                name
            ),
            IExistingTargetUserMapping existingTargetMapping => _existingTargetMappings.AddUserMapping(
                existingTargetMapping,
                userMapping.Default,
                name
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(userMapping), userMapping.GetType().FullName + " mappings are not supported"),
        };
    }

    public MappingCollectionAddResult AddNewInstanceMapping(INewInstanceMapping mapping, TypeMappingConfiguration config)
    {
        if (mapping is MethodMapping methodMapping)
        {
            _methodMappings.Add(methodMapping);
        }

        return _newInstanceMappings.TryAddAsDefault(mapping, config);
    }

    public MappingCollectionAddResult AddExistingTargetMapping(IExistingTargetMapping mapping, TypeMappingConfiguration config)
    {
        if (mapping is MethodMapping methodMapping)
        {
            _methodMappings.Add(methodMapping);
        }

        return _existingTargetMappings.TryAddAsDefault(mapping, config);
    }

    public void AddNamedNewInstanceUserMappings(string name, IEnumerable<INewInstanceUserMapping> mappings)
    {
        foreach (var mapping in mappings)
        {
            AddNamedNewInstanceUserMapping(name, mapping);
        }
    }

    public void AddNamedNewInstanceUserMapping(string name, INewInstanceUserMapping mapping)
    {
        Debug.Assert(
            mapping.Default != true,
            $"Cannot add a named mapping ({name}, {mapping.Method.Name}) after the initial discovery which is a default mapping"
        );
        _newInstanceMappings.AddNamedUserMapping(name, mapping);
    }

    public void AddNamedExistingInstanceUserMappings(string name, IEnumerable<IExistingTargetUserMapping> mappings)
    {
        foreach (var mapping in mappings)
        {
            AddNamedExistingInstanceUserMapping(name, mapping);
        }
    }

    public void AddNamedExistingInstanceUserMapping(string name, IExistingTargetUserMapping mapping)
    {
        Debug.Assert(
            mapping.Default != true,
            $"Cannot add a named mapping ({name}, {mapping.Method.Name}) after the initial discovery which is a default mapping"
        );
        _existingTargetMappings.AddNamedUserMapping(name, mapping);
    }

    private class MappingCollectionInstance<T, TUserMapping>
        where T : ITypeMapping
        where TUserMapping : T, IUserMapping
    {
        /// <summary>
        /// Default mappings of each type pair + config.
        /// A default mappings is the mapping Mapperly should use to convert from one type to another.
        /// Contains mappings to build and already built mappings.
        /// </summary>
        private readonly Dictionary<TypeMappingKey, T> _defaultMappings = new();

        /// <summary>
        /// Registered user mappings by their methods.
        /// </summary>
        private readonly Dictionary<IMethodSymbol, TUserMapping> _userMappingsByMethod = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// All user mappings registered in this instance.
        /// Used as the canonical source for parameterized and diagnostic queries.
        /// </summary>
        private readonly List<TUserMapping> _userMappings = [];

        /// <summary>
        /// Named mappings by their names.
        /// </summary>
        private readonly Dictionary<string, TUserMapping> _namedMappings = new();

        /// <summary>
        /// Duplicated mapping names.
        /// </summary>
        private readonly HashSet<string> _duplicatedMappingNames = [];

        /// <summary>
        /// All mapping type keys which for which an explicit default mapping is configured.
        /// </summary>
        private readonly HashSet<TypeMappingKey> _explicitDefaultMappingKeys = [];

        /// <summary>
        /// All mapping keys for which <see cref="Find(TypeMappingKey)"/> was called and returned a non-null result.
        /// </summary>
        private readonly HashSet<TypeMappingKey> _usedMappingKeys = [];

        /// <summary>
        /// All mappings for which <see cref="FindNamed"/> was called and returned a non-null result.
        /// </summary>
        private readonly HashSet<TUserMapping> _referencedNamedMappings = [];

        /// <inheritdoc cref="_defaultMappings"/>
        public IReadOnlyDictionary<TypeMappingKey, T> DefaultMappings => _defaultMappings;

        /// <summary>
        /// Returns user mappings that are duplicates (same type pair, no explicit default)
        /// for type-pairs which are actually in use and not referenced by name.
        /// Within each group, the first mapping (which won the _defaultMappings race) is excluded.
        /// </summary>
        public IEnumerable<TUserMapping> UsedDuplicatedNonDefaultNonReferencedUserMappings =>
            _userMappings
                .Where(m => !m.IsExternal && !m.Default.HasValue)
                .GroupBy(m => new TypeMappingKey(m))
                .Where(g => g.Count() > 1 && _usedMappingKeys.Contains(g.Key) && !_explicitDefaultMappingKeys.Contains(g.Key))
                .SelectMany(g => g.Skip(1))
                .Where(m => !_referencedNamedMappings.Contains(m));

        public TUserMapping? FindUserMapping(IMethodSymbol method) => _userMappingsByMethod.GetValueOrDefault(method);

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
                _referencedNamedMappings.Add(mapping);
            }

            return mapping;
        }

        public TUserMapping? FindUserMappingWithParameters(TypeMappingKey key, ParameterScope scope)
        {
            foreach (var mapping in _userMappings)
            {
                if (
                    mapping is IParameterizedMapping { AdditionalSourceParameters.Count: > 0 } pm
                    && SymbolEqualityComparer.IncludeNullability.Equals(key.Source, mapping.SourceType)
                    && SymbolEqualityComparer.IncludeNullability.Equals(key.Target, mapping.TargetType)
                    && scope.TryMatchParameters(pm.AdditionalSourceParameters, out _)
                )
                {
                    return mapping;
                }
            }

            return default;
        }

        public void AddNamedUserMapping(string? name, TUserMapping mapping)
        {
            var isNewUserMappingMethod = _userMappingsByMethod.TryAdd(mapping.Method, mapping);

            if (name == null)
                return;

            if (_namedMappings.TryAdd(name, mapping))
                return;

            // if the name is duplicated
            // and there is already another user mapping instance registered for the same method symbol,
            // this name is not considered duplicated as the mapping just got re-discovered
            if (isNewUserMappingMethod)
            {
                _duplicatedMappingNames.Add(name);
            }
        }

        public MappingCollectionAddResult TryAddAsDefault(T mapping, TypeMappingConfiguration config)
        {
            var mappingKey = new TypeMappingKey(mapping, config);
            var result = _defaultMappings.TryAdd(mappingKey, mapping)
                ? MappingCollectionAddResult.Added
                : MappingCollectionAddResult.NotAddedDuplicated;
            AddAdditionalMappings(mapping, config);
            return result;
        }

        public MappingCollectionAddResult AddUserMapping(TUserMapping mapping, bool? isDefault, string? name)
        {
            AddNamedUserMapping(name, mapping);
            _userMappings.Add(mapping);

            return isDefault switch
            {
                // the mapping is not a default mapping.
                false => MappingCollectionAddResult.NotAddedIgnored,

                // if a default mapping was already added for this type-pair
                // ignore the current one and return duplicated
                // otherwise overwrite the existing with the new default one.
                true => AddDefaultUserMapping(mapping),

                // no default value specified
                // add it if none exists yet
                null => TryAddAsDefault(mapping, TypeMappingConfiguration.Default),
            };
        }

        private MappingCollectionAddResult AddDefaultUserMapping(T mapping)
        {
            var mappingKey = new TypeMappingKey(mapping);
            if (!_explicitDefaultMappingKeys.Add(mappingKey))
                return MappingCollectionAddResult.NotAddedDuplicated;

            _defaultMappings[mappingKey] = mapping;
            AddAdditionalMappings(mapping, TypeMappingConfiguration.Default);
            return MappingCollectionAddResult.Added;
        }

        private void AddAdditionalMappings(T mapping, TypeMappingConfiguration config)
        {
            foreach (var additionalKey in mapping.BuildAdditionalMappingKeys(config))
            {
                _defaultMappings.TryAdd(additionalKey, mapping);
            }
        }
    }
}
