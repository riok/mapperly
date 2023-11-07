namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Defines the strategy to use when mapping members marked with <see cref="ObsoleteAttribute"/>.
/// Note that <see cref="MapPropertyAttribute"/> will always map <see cref="ObsoleteAttribute"/> marked members,
/// even if they are ignored.
/// </summary>
[Flags]
public enum IgnoreObsoleteMembersStrategy
{
    /// <summary>
    /// Maps <see cref="ObsoleteAttribute"/> marked members.
    /// </summary>
    None = 0,

    /// <summary>
    /// Will not map <see cref="ObsoleteAttribute"/> marked source or target members.
    /// </summary>
    Both = ~None,

    /// <summary>
    /// Ignores source <see cref="ObsoleteAttribute"/> marked members.
    /// </summary>
    Source = 1 << 0,

    /// <summary>
    /// Ignores target <see cref="ObsoleteAttribute"/> marked members.
    /// </summary>
    Target = 1 << 1,
}
