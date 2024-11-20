using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

public record EnumMappingConfiguration(
    EnumMappingStrategy Strategy,
    bool IgnoreCase,
    AttributeValue? FallbackValue,
    IReadOnlyCollection<IFieldSymbol> IgnoredSourceMembers,
    IReadOnlyCollection<IFieldSymbol> IgnoredTargetMembers,
    IReadOnlyCollection<EnumValueMappingConfiguration> ExplicitMappings,
    RequiredMappingStrategy RequiredMappingStrategy,
    EnumNamingStrategy NamingStrategy
)
{
    public bool HasExplicitConfigurations => ExplicitMappings.Count > 0 || IgnoredSourceMembers.Count > 0 || IgnoredTargetMembers.Count > 0;
}
