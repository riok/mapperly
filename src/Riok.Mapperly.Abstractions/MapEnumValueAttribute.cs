namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Customizes how enum values are mapped
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class MapEnumValueAttribute : Attribute
{
    /// <summary>
    /// Customizes how enum values are mapped
    /// </summary>
    /// <param name="source">The value to map from</param>
    /// <param name="target">The value to map to</param>
    public MapEnumValueAttribute(object source, object target)
    {
        Source = (Enum)source;
        Target = (Enum)target;
    }

    /// <summary>
    /// What to map to
    /// </summary>
    public Enum Target { get; }

    /// <summary>
    /// What to map from
    /// </summary>
    public Enum Source { get; }
}
