using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Customizes how enums are mapped.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
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
    /// The strategy to be used to map enums to enums.
    /// </summary>
    public EnumMappingStrategy Strategy { get; }

    /// <summary>
    /// Whether the case should be ignored during mappings.
    /// </summary>
    public bool IgnoreCase { get; set; }

    /// <summary>
    /// The fallback value if an enum cannot be mapped, used instead of throwing.
    /// </summary>
    public object? FallbackValue { get; set; }

    /// <summary>
    /// The strategy to be used to map enums from/to strings.
    /// </summary>
    public EnumNamingStrategy NamingStrategy { get; set; }
}
