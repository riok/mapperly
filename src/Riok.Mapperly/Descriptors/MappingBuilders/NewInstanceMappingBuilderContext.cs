using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public class NewInstanceMappingBuilderContext : ObjectPropertyMappingBuilderContext<NewInstanceObjectPropertyMapping>
{
    public NewInstanceMappingBuilderContext(MappingBuilderContext builderContext, NewInstanceObjectPropertyMapping mapping) : base(builderContext, mapping)
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
