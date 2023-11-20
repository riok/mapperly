using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

/// <summary>
/// Represents the <see cref="MapEnumAttribute"/>
/// with enum values as typed constants.
/// </summary>
public class EnumConfiguration(EnumMappingStrategy strategy)
{
    /// <summary>
    /// The strategy to be used to map enums.
    /// </summary>
    public EnumMappingStrategy Strategy { get; } = strategy;

    /// <summary>
    /// Whether the case should be ignored during mappings.
    /// </summary>
    public bool? IgnoreCase { get; set; }

    /// <summary>
    /// The fallback value if an enum cannot be mapped, used instead of throwing.
    /// </summary>
    public IFieldSymbol? FallbackValue { get; set; }
}
