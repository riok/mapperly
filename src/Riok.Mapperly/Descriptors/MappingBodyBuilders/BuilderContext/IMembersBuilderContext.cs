using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// Context to build member mappings.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public interface IMembersBuilderContext<out T>
    where T : IMapping
{
    T Mapping { get; }

    MappingBuilderContext BuilderContext { get; }

    void IgnoreMembers(IMappableMember member);
    void IgnoreMembers(string memberName);

    void SetMembersMapped(MemberMappingInfo members);

    void SetTargetMemberMapped(IMappableMember targetMember);

    void ConsumeMemberConfigs(MemberMappingInfo members);

    void TryAddSourceMemberAlias(string alias, IMappableMember member);

    IEnumerable<IMappableMember> EnumerateUnmappedTargetMembers();

    IEnumerable<IMappableMember> EnumerateUnmappedOrConfiguredTargetMembers();

    IEnumerable<MemberMappingInfo> MatchMembers(IMappableMember targetMember);
}
