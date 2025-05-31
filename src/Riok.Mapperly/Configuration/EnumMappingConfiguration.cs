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

    public EnumMappingConfiguration MergeWith(EnumMappingConfiguration result2Enum)
    {
        return this with
        {
            FallbackValue = FallbackValue ?? result2Enum.FallbackValue,
            IgnoredSourceMembers = IgnoredSourceMembers.Concat(result2Enum.IgnoredSourceMembers).ToList(),
            IgnoredTargetMembers = IgnoredTargetMembers.Concat(result2Enum.IgnoredTargetMembers).ToList(),
            ExplicitMappings = ExplicitMappings.Concat(result2Enum.ExplicitMappings).ToList(),
        };
    }
}
