namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// Represents a container of several property mappings.
/// </summary>
public interface IPropertyMappingContainer
{
    bool HasPropertyMapping(IPropertyMapping mapping);

    void AddPropertyMapping(IPropertyMapping mapping);

    void AddPropertyMappings(IEnumerable<IPropertyMapping> mappings);
}
