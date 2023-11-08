using System.Collections.Immutable;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// Immutable wrapper for <see cref="ImmutableDictionary&lt;TypeMappingKey, int&gt;"/> which tracks the parent types for a mapping.
/// Used to detect self referential loops.
/// </summary>
/// <param name="parentTypes">Dictionary tracking how many times a type has been seen.</param>
public readonly struct MappingRecursionDepthTracker(ImmutableDictionary<TypeMappingKey, int> parentTypes)
{
    /// <summary>
    /// Increments how many times a <see cref="TypeMappingKey"/> has been mapped.
    /// Used to track how many times a parent context has mapped a type.
    /// </summary>
    /// <param name="typeMappingKey">The mapped type.</param>
    /// <returns>A new <see cref="MappingRecursionDepthTracker"/> with the updated key.</returns>
    public MappingRecursionDepthTracker AddOrIncrement(TypeMappingKey typeMappingKey)
    {
        var mappingRecursionCount = parentTypes.GetValueOrDefault(typeMappingKey);
        var newParentTypes = parentTypes.SetItem(typeMappingKey, mappingRecursionCount + 1);
        return new(newParentTypes);
    }

    /// <summary>
    /// Gets the number of times a <see cref="TypeMappingKey"/> has been mapped by the parent contexts.
    /// </summary>
    /// <param name="typeMappingKey">The candidate mapping.</param>
    /// <returns>The number of times the <see cref="TypeMappingKey"/> has been mapped.</returns>
    public int GetDepth(TypeMappingKey typeMappingKey) => parentTypes.GetValueOrDefault(typeMappingKey);

    /// <summary>
    /// Creates a new <see cref="MappingRecursionDepthTracker"/> containing the initial type mapping.
    /// </summary>
    /// <param name="mappingKey">Initial <see cref="TypeMappingKey"/> value.</param>
    /// <returns>A <see cref="MappingRecursionDepthTracker"/> containing the initial type mapping.</returns>
    public static MappingRecursionDepthTracker Create(TypeMappingKey mappingKey)
    {
        var dict = ImmutableDictionary<TypeMappingKey, int>.Empty;
        return new(dict.Add(mappingKey, 1));
    }
}
