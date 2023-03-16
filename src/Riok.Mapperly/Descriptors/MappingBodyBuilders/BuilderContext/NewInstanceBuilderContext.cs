using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An implementation of <see cref="INewInstanceBuilderContext{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public class NewInstanceBuilderContext<T> :
    PropertiesMappingBuilderContext<T>,
    INewInstanceBuilderContext<T>
    where T : INewInstanceObjectPropertyMapping
{
    public NewInstanceBuilderContext(MappingBuilderContext builderContext, T mapping)
        : base(builderContext, mapping)
    {
    }

    public void AddInitPropertyMapping(PropertyAssignmentMapping mapping)
    {
        SetSourcePropertyMapped(mapping.SourcePath);
        Mapping.AddInitPropertyMapping(mapping);
    }

    public void AddConstructorParameterMapping(ConstructorParameterMapping mapping)
    {
        PropertyConfigsByRootTargetName.Remove(mapping.Parameter.Name);
        SetSourcePropertyMapped(mapping.DelegateMapping.SourcePath);
        Mapping.AddConstructorParameterMapping(mapping);
    }
}
