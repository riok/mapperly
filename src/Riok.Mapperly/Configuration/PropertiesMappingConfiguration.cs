using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

public record PropertiesMappingConfiguration(
    IReadOnlyCollection<string> IgnoredSources,
    IReadOnlyCollection<string> IgnoredTargets,
    IReadOnlyCollection<PropertyMappingConfiguration> ExplicitMappings,
    IgnoreObsoleteMembersStrategy IgnoreObsoleteMembersStrategy,
    bool MapOnlyPrimitives
);
