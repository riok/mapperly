using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Defines the strategy used when emitting warnings for unmapped members.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperRequiredMappingAttribute : Attribute
{
    /// <summary>
    /// Defines the strategy used when emitting warnings for unmapped members.
    /// </summary>
    /// <param name="requiredMappingStrategy">The strategy used when emitting warnings for unmapped members.</param>
    public MapperRequiredMappingAttribute(RequiredMappingStrategy requiredMappingStrategy)
    {
        RequiredMappingStrategy = requiredMappingStrategy;
    }

    /// <summary>
    /// The strategy used when emitting warnings for unmapped members.
    /// </summary>
    public RequiredMappingStrategy RequiredMappingStrategy { get; }
}
