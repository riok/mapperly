using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An object mapping creating the target instance via a new() call.
/// </summary>
public interface INewInstanceObjectMemberMapping : IMapping
{
    void AddConstructorParameterMapping(ConstructorParameterMapping mapping);

    void AddInitMemberMapping(MemberAssignmentMapping mapping);
}
