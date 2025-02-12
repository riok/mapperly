namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping from one type to another.
/// </summary>
public interface ITypeMapping : IMapping
{
    /// <summary>
    /// Gets a value indicating whether this mapping produces any code or can be omitted completely (eg. direct assignments or delegate mappings).
    /// </summary>
    bool IsSynthetic { get; }

    IEnumerable<TypeMappingKey> BuildAdditionalMappingKeys(TypeMappingConfiguration config);
}
