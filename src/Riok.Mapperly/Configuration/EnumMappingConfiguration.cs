using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

public record EnumMappingConfiguration(
    EnumMappingStrategy Strategy,
    bool IgnoreCase,
    IFieldSymbol? FallbackValue,
    IReadOnlyCollection<IFieldSymbol> IgnoredSourceMembers,
    IReadOnlyCollection<IFieldSymbol> IgnoredTargetMembers,
    IReadOnlyCollection<EnumValueMappingConfiguration> ExplicitMappings
)
{
    public bool HasExplicitConfigurations => ExplicitMappings.Count > 0 || IgnoredSourceMembers.Count > 0 || IgnoredTargetMembers.Count > 0;
}
