using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// A given method is marked as user implemented mapping with this attribute.
/// If <see cref="MapperAttribute.AutoUserMappings"/> is <c>true</c>,
/// this attribute allows to ignore a user implemented mapping method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class UserMappingAttribute : Attribute
{
    /// <summary>
    /// If set to <c>true</c>, this user mapping acts as the default mapping for the given type pair.
    /// Only one mapping per type-pair in a mapper can be set to <c>true</c>.
    ///
    /// If no mapping for a given type-pair has a value of <c>true</c> for <see cref="Default"/>
    /// the first mapping encountered without an explicit value of <c>false</c> is considered the default mapping.
    /// </summary>
    public bool Default { get; set; }

    /// <summary>
    /// Whether this user mapping should be ignored.
    /// </summary>
    public bool Ignore { get; set; }
}
