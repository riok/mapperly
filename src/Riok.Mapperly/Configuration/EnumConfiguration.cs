using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

/// <summary>
/// Represents the <see cref="MapEnumAttribute"/>
/// with enum values as typed constants.
/// </summary>
public class EnumConfiguration(EnumMappingStrategy strategy)
{
    /// <summary>
    /// The strategy to be used to map enums to enums.
    /// </summary>
    public EnumMappingStrategy Strategy { get; } = strategy;

    /// <summary>
    /// Whether the case should be ignored during mappings.
    /// </summary>
    public bool? IgnoreCase { get; set; }

    /// <summary>
    /// The fallback value if an enum cannot be mapped, used instead of throwing.
    /// </summary>
    public AttributeValue? FallbackValue { get; set; }

    /// <summary>
    /// The strategy to be used to map enums from/to strings.
    /// </summary>
    public EnumNamingStrategy NamingStrategy { get; set; }
}
