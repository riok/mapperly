namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Defines the strategy to use when mapping a property to another property.
/// </summary>
public enum PropertyNameMappingStrategy
{
    /// <summary>
    /// Matches a property by its name in case sensitive manner.
    /// </summary>
    CaseSensitive,

    /// <summary>
    /// Matches a property by its name in case insensitive manner.
    /// </summary>
    CaseInsensitive,
}
