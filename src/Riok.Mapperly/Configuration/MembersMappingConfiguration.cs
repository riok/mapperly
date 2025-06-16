using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Configuration;

public record MembersMappingConfiguration(
    IReadOnlyCollection<string> IgnoredSources,
    IReadOnlyCollection<string> IgnoredTargets,
    IReadOnlyCollection<MemberValueMappingConfiguration> ValueMappings,
    IReadOnlyCollection<MemberMappingConfiguration> ExplicitMappings,
    IReadOnlyCollection<NestedMembersMappingConfiguration> NestedMappings,
    IgnoreObsoleteMembersStrategy? IgnoreObsoleteMembersStrategy,
    RequiredMappingStrategy? RequiredMappingStrategy
)
{
    public IEnumerable<string> GetMembersWithExplicitConfigurations(MappingSourceTarget sourceTarget)
    {
        var members = sourceTarget switch
        {
            MappingSourceTarget.Source => ExplicitMappings.Where(x => x.Source.PathCount > 0).Select(x => x.Source.RootName),
            MappingSourceTarget.Target => ExplicitMappings
                .Select(x => x.Target.RootName)
                .Concat(ValueMappings.Select(x => x.Target.RootName)),
            _ => throw new ArgumentOutOfRangeException(nameof(sourceTarget), sourceTarget, "Neither source or target"),
        };
        return members.Distinct();
    }

    public MembersMappingConfiguration Include(MembersMappingConfiguration? otherConfiguration)
    {
        return new MembersMappingConfiguration(
            IgnoredSources.Concat(otherConfiguration?.IgnoredSources ?? []).ToList(),
            IgnoredTargets.Concat(otherConfiguration?.IgnoredTargets ?? []).ToList(),
            ValueMappings.Concat(otherConfiguration?.ValueMappings ?? []).ToList(),
            ExplicitMappings.Concat(otherConfiguration?.ExplicitMappings ?? []).ToList(),
            NestedMappings.Concat(otherConfiguration?.NestedMappings ?? []).ToList(),
            IgnoreObsoleteMembersStrategy ?? otherConfiguration?.IgnoreObsoleteMembersStrategy,
            RequiredMappingStrategy ?? otherConfiguration?.RequiredMappingStrategy
        );
    }
}
