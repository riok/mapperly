namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Customizes how enums are mapped.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class MapEnumAttribute : Attribute
{
    /// <summary>
    /// Customizes how enums are mapped.
    /// </summary>
    /// <param name="strategy">The strategy to be used to map enums.</param>
    public MapEnumAttribute(EnumMappingStrategy strategy)
    {
        Strategy = strategy;
    }


    /// <summary>
    /// The strategy to be used to map enums.
    /// </summary>
    public EnumMappingStrategy Strategy { get; }
}
