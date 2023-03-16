using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// A <see cref="IPropertiesBuilderContext{T}"/> for mappings which create the target object via new().
/// </summary>
/// <typeparam name="T">The mapping type.</typeparam>
public interface INewInstanceBuilderContext<out T> : IPropertiesBuilderContext<T>
    where T : IMapping
{
    void AddConstructorParameterMapping(ConstructorParameterMapping mapping);

    void AddInitPropertyMapping(PropertyAssignmentMapping mapping);
}
