namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Defines the strategy to use when mapping an enum to another enum.
/// </summary>
public enum EnumMappingStrategy
{
    /// <summary>
    /// Matches enum members by their values.
    /// </summary>
    ByValue,

    /// <summary>
    /// Matches enum members by their names.
    /// </summary>
    ByName,

    /// <summary>
    /// Matches enum members by their values.
    /// Checks if the value is defined in the enum.
    /// </summary>
    ByValueCheckDefined,
}
