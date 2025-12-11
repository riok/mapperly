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

    /// <summary>
    /// Matches a property by converting both source and target property names to snake_case before comparison.
    /// For example, "FirstName" would match "first_name".
    /// </summary>
    SnakeCase,

    /// <summary>
    /// Matches a property by converting both source and target property names to SNAKE_CASE before comparison.
    /// For example, "FirstName" would match "FIRST_NAME".
    /// </summary>
    UpperSnakeCase,
}
