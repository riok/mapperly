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
    /// Whether this user mapping should be ignored.
    /// </summary>
    public bool Ignore { get; set; }
}
