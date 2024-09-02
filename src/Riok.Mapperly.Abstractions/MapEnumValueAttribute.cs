using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Customizes how enum values are mapped
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapEnumValueAttribute : Attribute
{
    /// <summary>
    /// Customizes how enum values are mapped
    /// </summary>
    /// <param name="source">The value to map from</param>
    /// <param name="target">The value to map to</param>
    public MapEnumValueAttribute(object source, object target)
    {
        Source = source;
        Target = target;
    }

    /// <summary>
    /// What to map to
    /// </summary>
    public object Target { get; }

    /// <summary>
    /// What to map from
    /// </summary>
    public object Source { get; }
}
