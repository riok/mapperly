using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

public record EnumMappingConfiguration(
    EnumMappingStrategy Strategy,
    bool IgnoreCase,
    IReadOnlyCollection<EnumValueMappingConfiguration> ExplicitMappings
);
