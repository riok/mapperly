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

    public EnumMappingConfiguration Include(EnumMappingConfiguration? otherConfiguration)
    {
        return this with
        {
            FallbackValue = FallbackValue ?? otherConfiguration?.FallbackValue,
            IgnoredSourceMembers = IgnoredSourceMembers.Concat(otherConfiguration?.IgnoredSourceMembers ?? []).ToList(),
            IgnoredTargetMembers = IgnoredTargetMembers.Concat(otherConfiguration?.IgnoredTargetMembers ?? []).ToList(),
            ExplicitMappings = ExplicitMappings.Concat(otherConfiguration?.ExplicitMappings ?? []).ToList(),
        };
    }
}
