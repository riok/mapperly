using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An implementation of an <see cref="INewInstanceBuilderContext{T}"/>
/// which supports containers (<seealso cref="MembersContainerBuilderContext{T}"/>).
/// </summary>
/// <typeparam name="T"></typeparam>
public class NewInstanceContainerBuilderContext<T> : MembersContainerBuilderContext<T>, INewInstanceBuilderContext<T>
    where T : INewInstanceObjectMemberMapping, IMemberAssignmentTypeMapping
{
    public NewInstanceContainerBuilderContext(MappingBuilderContext builderContext, T mapping)
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
