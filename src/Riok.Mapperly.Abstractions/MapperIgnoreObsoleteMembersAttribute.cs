using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Specifies options for obsolete ignoring strategy.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperIgnoreObsoleteMembersAttribute : Attribute
{
    /// <summary>
    /// Specifies options for obsolete ignoring strategy.
    /// </summary>
    /// <param name="ignoreObsoleteStrategy">The strategy to be used to map <see cref="ObsoleteAttribute"/> marked members. Defaults to <see cref="IgnoreObsoleteMembersStrategy.Both"/>.</param>
    public MapperIgnoreObsoleteMembersAttribute(IgnoreObsoleteMembersStrategy ignoreObsoleteStrategy = IgnoreObsoleteMembersStrategy.Both)
    {
        IgnoreObsoleteStrategy = ignoreObsoleteStrategy;
    }

    /// <summary>
    /// The strategy used to map <see cref="ObsoleteAttribute"/> marked members.
    /// </summary>
    public IgnoreObsoleteMembersStrategy IgnoreObsoleteStrategy { get; }
}
