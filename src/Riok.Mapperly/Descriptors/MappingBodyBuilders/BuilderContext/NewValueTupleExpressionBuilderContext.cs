using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An implementation of <see cref="INewValueTupleBuilderContext{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public class NewValueTupleExpressionBuilderContext<T> : MembersContainerBuilderContext<T>, INewValueTupleBuilderContext<T>
    where T : INewValueTupleMapping, IMemberAssignmentTypeMapping
{
    public NewValueTupleExpressionBuilderContext(MappingBuilderContext builderContext, T mapping)
        : base(builderContext, mapping) { }

    public void AddTupleConstructorParameterMapping(ValueTupleConstructorParameterMapping mapping)
    {
        if (MemberConfigsByRootTargetName.TryGetValue(mapping.Parameter.Name, out var value))
        {
            // remove the mapping used to map the tuple constructor
            value.RemoveAll(x => x.Target.Path.Count == 1);

            // remove from dictionary and target members if there aren't any more mappings
            if (!value.Any())
            {
                MemberConfigsByRootTargetName.Remove(mapping.Parameter.Name);
                TargetMembers.Remove(mapping.Parameter.Name);
            }
        }

        SetSourceMemberMapped(mapping.DelegateMapping.SourcePath);
        Mapping.AddConstructorParameterMapping(mapping);
    }
}
