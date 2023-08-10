using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An implementation of <see cref="INewValueTupleBuilderContext{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public class NewValueTupleConstructorBuilderContext<T> : MembersMappingBuilderContext<T>, INewValueTupleBuilderContext<T>
    where T : INewValueTupleMapping
{
    public NewValueTupleConstructorBuilderContext(MappingBuilderContext builderContext, T mapping)
        : base(builderContext, mapping) { }

    public void AddTupleConstructorParameterMapping(ValueTupleConstructorParameterMapping mapping)
    {
        MemberConfigsByRootTargetName.Remove(mapping.Parameter.Name);
        SetMembersMapped(mapping.DelegateMapping);
        Mapping.AddConstructorParameterMapping(mapping);
    }
}
