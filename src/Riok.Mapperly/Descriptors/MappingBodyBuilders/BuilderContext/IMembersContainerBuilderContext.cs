using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An <see cref="IMembersBuilderContext{T}"/> which supports containers.
/// A container groups several member mappings in one not-null checked block.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public interface IMembersContainerBuilderContext<out T> : IMembersBuilderContext<T>
    where T : IMemberAssignmentTypeMapping
{
    void AddTypeMapping(ITypeMapping typeMapping);

    void AddMemberAssignmentMapping(IMemberAssignmentMapping memberMapping);

    void AddNullDelegateMemberAssignmentMapping(IMemberAssignmentMapping memberMapping);
}
