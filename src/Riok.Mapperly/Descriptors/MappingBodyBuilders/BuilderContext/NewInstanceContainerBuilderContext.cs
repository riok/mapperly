using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An implementation of an <see cref="INewInstanceBuilderContext{T}"/>
/// which supports containers (<seealso cref="PropertiesContainerBuilderContext{T}"/>).
/// </summary>
/// <typeparam name="T"></typeparam>
public class NewInstanceContainerBuilderContext<T> :
    PropertiesContainerBuilderContext<T>,
    INewInstanceBuilderContext<T>
    where T : INewInstanceObjectPropertyMapping, IPropertyAssignmentTypeMapping
{
    public NewInstanceContainerBuilderContext(MappingBuilderContext builderContext, T mapping)
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
