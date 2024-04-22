using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

public record MembersMappingConfiguration(
    IReadOnlyCollection<string> IgnoredSources,
    IReadOnlyCollection<string> IgnoredTargets,
    IReadOnlyCollection<MemberMappingConfiguration> ExplicitMappings,
    IReadOnlyCollection<NestedMembersMappingConfiguration> NestedMappings,
    IgnoreObsoleteMembersStrategy IgnoreObsoleteMembersStrategy,
    RequiredMappingStrategy RequiredMappingStrategy
);
