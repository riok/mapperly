namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Defines the strategy used when emitting warnings for unmapped members.
/// </summary>
[Flags]
public enum RequiredMappingStrategy
{
    /// <summary>
    /// Warnings are not emitted for unmapped source or target members.
    /// </summary>
    None = 0,

    /// <summary>
    /// Warnings are emitted for both unmapped source and target members.
    /// </summary>
    Both = ~None,

    /// <summary>
    /// Warnings are emitted for unmapped source members but not for target members.
    /// </summary>
    Source = 1 << 0,

    /// <summary>
    /// Warnings are emitted for unmapped target members but not for source members.
    /// </summary>
    Target = 1 << 1
}
