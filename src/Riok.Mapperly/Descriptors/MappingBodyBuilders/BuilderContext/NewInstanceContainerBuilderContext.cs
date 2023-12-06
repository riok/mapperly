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
    public IReadOnlyDictionary<string, string> RootTargetNameCasingMapping { get; }

    public NewInstanceContainerBuilderContext(MappingBuilderContext builderContext, T mapping)
        : base(builderContext, mapping)
    {
        RootTargetNameCasingMapping = MemberConfigsByRootTargetName.ToDictionary(x => x.Key, x => x.Key, StringComparer.OrdinalIgnoreCase);
    }

    public void AddInitMemberMapping(MemberAssignmentMapping mapping)
    {
        SetSourceMemberMapped(mapping.SourcePath);
        Mapping.AddInitMemberMapping(mapping);
    }

    public void AddConstructorParameterMapping(ConstructorParameterMapping mapping)
    {
        var paramName = RootTargetNameCasingMapping.GetValueOrDefault(mapping.Parameter.Name, defaultValue: mapping.Parameter.Name);
        MemberConfigsByRootTargetName.Remove(paramName);
        SetSourceMemberMapped(mapping.DelegateMapping.SourcePath);
        Mapping.AddConstructorParameterMapping(mapping);
    }
}
