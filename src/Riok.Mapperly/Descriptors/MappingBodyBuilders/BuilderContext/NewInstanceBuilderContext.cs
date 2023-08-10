using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An implementation of <see cref="INewInstanceBuilderContext{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public class NewInstanceBuilderContext<T> : MembersMappingBuilderContext<T>, INewInstanceBuilderContext<T>
    where T : INewInstanceObjectMemberMapping
{
    public NewInstanceBuilderContext(MappingBuilderContext builderContext, T mapping)
        : base(builderContext, mapping) { }

    public void AddInitMemberMapping(MemberAssignmentMapping mapping)
    {
        SetMembersMapped(mapping);
        Mapping.AddInitMemberMapping(mapping);
    }

    public void AddConstructorParameterMapping(ConstructorParameterMapping mapping)
    {
        MemberConfigsByRootTargetName.Remove(mapping.Parameter.Name);
        SetMembersMapped(mapping.DelegateMapping);
        Mapping.AddConstructorParameterMapping(mapping);
    }
}
