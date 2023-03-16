using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An object mapping creating the target instance via a new() call.
/// </summary>
public interface INewInstanceObjectPropertyMapping : IMapping
{
    void AddConstructorParameterMapping(ConstructorParameterMapping mapping);

    void AddInitPropertyMapping(PropertyAssignmentMapping mapping);
}
