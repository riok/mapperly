using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Ignores a target enum value from the mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperIgnoreTargetValueAttribute : Attribute
{
    /// <summary>
    /// Ignores the specified target enum value from the mapping.
    /// </summary>
    /// <param name="target">The target enum value to ignore.</param>
    public MapperIgnoreTargetValueAttribute(object target)
    {
        TargetValue = (Enum)target;
    }

    /// <summary>
    /// Gets the target enum value which should be ignored from the mapping.
    /// </summary>
    public Enum? TargetValue { get; }
}
