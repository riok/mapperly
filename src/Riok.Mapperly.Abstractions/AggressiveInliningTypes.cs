namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Defines when to apply MethodImpl(MethodImplOptions.AggressiveInlining) to generated mapping methods.
/// </summary>
public enum AggressiveInliningTypes
{
    /// <summary>
    /// Never apply MethodImpl.AggressiveInlining.
    /// </summary>
    None = 0,

    /// <summary>
    /// Apply MethodImpl.AggressiveInlining to value type (struct) mappings.
    /// </summary>
    ValueTypes = 1 << 0,

    /// <summary>
    /// Apply MethodImpl.AggressiveInlining to reference type (class) mappings.
    /// </summary>
    ReferenceTypes = 1 << 1,

    /// <summary>
    /// Apply MethodImpl.AggressiveInlining to all mappings.
    /// </summary>
    All = ~None,
}
