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
    public IReadOnlyDictionary<string, string> RootTargetNameCasingMapping { get; }

    public NewInstanceBuilderContext(MappingBuilderContext builderContext, T mapping)
        : base(builderContext, mapping)
    {
        RootTargetNameCasingMapping = MemberConfigsByRootTargetName.ToDictionary(x => x.Key, x => x.Key, StringComparer.OrdinalIgnoreCase);
    }

    public void AddInitMemberMapping(MemberAssignmentMapping mapping)
    {
        SetSourceMemberMapped(mapping.SourceGetter.MemberPath);
        Mapping.AddInitMemberMapping(mapping);
    }

    public void AddConstructorParameterMapping(ConstructorParameterMapping mapping)
    {
        var paramName = RootTargetNameCasingMapping.GetValueOrDefault(mapping.Parameter.Name, defaultValue: mapping.Parameter.Name);
        MemberConfigsByRootTargetName.Remove(paramName);
        SetSourceMemberMapped(mapping.DelegateMapping.SourceGetter.MemberPath);
        Mapping.AddConstructorParameterMapping(mapping);
    }
}
