namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Determines what member accessibility Mapperly will attempt to map.
/// </summary>
[Flags]
public enum MemberVisibility
{
    /// <summary>
    /// Maps all accessible members.
    /// </summary>
    AllAccessible = All | Accessible,

    /// <summary>
    /// Maps all members, even members which are not directly accessible by the mapper are mapped
    /// by using accessors with the UnsafeAccessorAttribute. This can only be used for .NET 8.0 and later.
    /// </summary>
    All = Public | Internal | Protected | Private,

    /// <summary>
    /// Maps only accessible members.
    /// If not set, the UnsafeAccessorAttribute is used to generate mappings for inaccessible members.
    /// </summary>
    Accessible = 1 << 0,

    /// <summary>
    /// Maps public members.
    /// </summary>
    Public = 1 << 1,

    /// <summary>
    /// Maps internal members.
    /// </summary>
    Internal = 1 << 2,

    /// <summary>
    /// Maps protected members.
    /// </summary>
    Protected = 1 << 3,

    /// <summary>
    /// Maps private members.
    /// </summary>
    Private = 1 << 4,
}
