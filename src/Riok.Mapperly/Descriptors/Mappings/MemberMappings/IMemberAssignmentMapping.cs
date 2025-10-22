using System.Diagnostics.CodeAnalysis;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a member assignment mapping or a container of member assignment mappings.
/// </summary>
public interface IMemberAssignmentMapping : IAssignmentMappings
{
    MemberMappingInfo MemberInfo { get; }

    bool TryGetMemberAssignmentMappingContainer([NotNullWhen(true)] out IMemberAssignmentMappingContainer? container);
}
