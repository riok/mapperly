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
    RequiredMappingStrategy RequiredMappingStrategy
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

    public MembersMappingConfiguration Include(MembersMappingConfiguration? result2Members)
    {
        return new MembersMappingConfiguration(
            IgnoredSources.Concat(result2Members?.IgnoredSources ?? []).ToList(),
            IgnoredTargets.Concat(result2Members?.IgnoredTargets ?? []).ToList(),
            ValueMappings.Concat(result2Members?.ValueMappings ?? []).ToList(),
            ExplicitMappings.Concat(result2Members?.ExplicitMappings ?? []).ToList(),
            NestedMappings.Concat(result2Members?.NestedMappings ?? []).ToList(),
            IgnoreObsoleteMembersStrategy ?? result2Members?.IgnoreObsoleteMembersStrategy,
            RequiredMappingStrategy
        );
    }
}
