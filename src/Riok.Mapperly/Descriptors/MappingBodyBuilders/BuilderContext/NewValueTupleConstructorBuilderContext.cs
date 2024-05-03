using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An implementation of <see cref="INewValueTupleBuilderContext{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public class NewValueTupleConstructorBuilderContext<T>(MappingBuilderContext builderContext, T mapping)
    : MembersMappingBuilderContext<T>(builderContext, mapping),
        INewValueTupleBuilderContext<T>
    where T : INewValueTupleMapping
{
    public void AddTupleConstructorParameterMapping(ValueTupleConstructorParameterMapping mapping)
    {
        MemberConfigsByRootTargetName.Remove(mapping.Parameter.Name);
        SetSourceMemberMapped(mapping.DelegateMapping.SourceGetter.MemberPath);
        Mapping.AddConstructorParameterMapping(mapping);
    }
}
