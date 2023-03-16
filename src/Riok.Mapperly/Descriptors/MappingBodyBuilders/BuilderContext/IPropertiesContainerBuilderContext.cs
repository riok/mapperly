using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An <see cref="IPropertiesBuilderContext{T}"/> which supports containers.
/// A container groups several property mappings in one not-null checked block.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public interface IPropertiesContainerBuilderContext<out T> : IPropertiesBuilderContext<T>
    where T : IPropertyAssignmentTypeMapping
{
    void AddPropertyAssignmentMapping(IPropertyAssignmentMapping propertyMapping);

    void AddNullDelegatePropertyAssignmentMapping(IPropertyAssignmentMapping propertyMapping);
}
