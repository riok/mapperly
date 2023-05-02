namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Obsolete.
/// Ignores a property from the mapping.
/// </summary>
/// <remarks>
/// This attribute is obsolete and was renamed to <seealso cref="MapperIgnoreTargetAttribute"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Obsolete("Renamed to " + nameof(MapperIgnoreTargetAttribute))]
public sealed class MapperIgnoreAttribute : Attribute
{
    /// <summary>
    /// Ignores the specified target property from the mapping.
    /// </summary>
    /// <param name="target">The name of the target property to ignore. The use of `nameof()` is encouraged.</param>
    public MapperIgnoreAttribute(string target)
    {
        Target = target;
    }

    /// <summary>
    /// Gets the property name which should be ignored from the mapping.
    /// </summary>
    public string Target { get; }
}
