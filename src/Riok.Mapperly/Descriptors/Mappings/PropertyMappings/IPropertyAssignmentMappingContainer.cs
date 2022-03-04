namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// Represents a container of several property mappings.
/// </summary>
public interface IPropertyAssignmentMappingContainer
{
    bool HasPropertyMapping(IPropertyAssignmentMapping mapping);

    void AddPropertyMapping(IPropertyAssignmentMapping mapping);

    void AddPropertyMappings(IEnumerable<IPropertyAssignmentMapping> mappings);
}
