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
    RequiredEnumMappingStrategy RequiredEnumMappingStrategy,
    EnumNamingStrategy NamingStrategy
)
{
    public bool HasExplicitConfigurations => ExplicitMappings.Count > 0 || IgnoredSourceMembers.Count > 0 || IgnoredTargetMembers.Count > 0;

    /// <summary>
    /// Gets the required mapping strategy for enum members. When RequiredEnumMappingStrategy == RequiredEnumMappingStrategy.Inherit, RequiredMappingStrategy is used.
    /// </summary>
    /// <returns></returns>
    public RequiredMappingStrategy GetRequiredEnumMemberMappingStrategy() =>
        RequiredEnumMappingStrategy == RequiredEnumMappingStrategy.Inherit
            ? RequiredMappingStrategy
            : RequiredEnumMappingStrategy switch
            {
                RequiredEnumMappingStrategy.Source => RequiredMappingStrategy.Source,
                RequiredEnumMappingStrategy.Target => RequiredMappingStrategy.Target,
                RequiredEnumMappingStrategy.Both => RequiredMappingStrategy.Both,
                _ => RequiredMappingStrategy.None,
            };
}
