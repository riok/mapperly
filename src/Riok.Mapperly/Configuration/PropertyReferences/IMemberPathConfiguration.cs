namespace Riok.Mapperly.Configuration.PropertyReferences;

/// <summary>
/// A user-configured member path.
/// </summary>
public interface IMemberPathConfiguration
{
    /// <summary>
    /// The name of the root member.
    /// </summary>
    string RootName { get; }

    /// <summary>
    /// The full name e.g. A.B.C
    /// </summary>
    string FullName { get; }

    /// <summary>
    /// The member names of the path / each segment, e.g. [A,B,C]
    /// </summary>
    IEnumerable<string> MemberNames { get; }

    /// <summary>
    /// The number of path segments in this path.
    /// </summary>
    int PathCount { get; }
}
