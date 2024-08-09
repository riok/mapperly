namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a container of several <see cref="IMemberAssignmentMapping"/>.
/// </summary>
public interface IMemberAssignmentMappingContainer : IAssignmentMappings
{
    bool HasMemberMapping(IMemberAssignmentMapping mapping);

    void AddMemberMapping(IMemberAssignmentMapping mapping);

    bool HasMemberMappingContainer(IMemberAssignmentMappingContainer container);

    void AddMemberMappingContainer(IMemberAssignmentMappingContainer container);
}
