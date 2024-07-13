using Riok.Mapperly.Descriptors.Constructors;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An object mapping creating the target instance via a new() call.
/// </summary>
public interface INewInstanceObjectMemberMapping : INewInstanceMapping
{
    IInstanceConstructor Constructor { get; set; }

    bool HasConstructor { get; }

    void AddConstructorParameterMapping(ConstructorParameterMapping mapping);

    void AddInitMemberMapping(MemberAssignmentMapping mapping);
}
