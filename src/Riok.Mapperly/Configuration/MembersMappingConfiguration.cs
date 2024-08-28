using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Configuration;

public record MembersMappingConfiguration(
    IReadOnlyCollection<string> IgnoredSources,
    IReadOnlyCollection<string> IgnoredTargets,
    IReadOnlyCollection<MemberValueMappingConfiguration> ValueMappings,
    IReadOnlyCollection<MemberMappingConfiguration> ExplicitMappings,
    IReadOnlyCollection<NestedMembersMappingConfiguration> NestedMappings,
    IgnoreObsoleteMembersStrategy IgnoreObsoleteMembersStrategy,
    RequiredMappingStrategy RequiredMappingStrategy
)
{
    public IEnumerable<string> GetMembersWithExplicitConfigurations(MappingSourceTarget sourceTarget)
    {
        var members = sourceTarget switch
        {
            MappingSourceTarget.Source => ExplicitMappings.Where(x => x.Source.Path.Count > 0).Select(x => x.Source.Path[0]),
            MappingSourceTarget.Target => ExplicitMappings
                .Select(x => x.Target.Path[0])
                .Concat(ValueMappings.Select(x => x.Target.Path[0])),
            _ => throw new ArgumentOutOfRangeException(nameof(sourceTarget), sourceTarget, "Neither source or target"),
        };
        return members.Distinct();
    }
}
