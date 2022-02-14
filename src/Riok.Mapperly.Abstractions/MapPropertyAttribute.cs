namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Specifies options for a property mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class MapPropertyAttribute : Attribute
{
    /// <summary>
    /// Maps a specified source property to the specified target property.
    /// </summary>
    /// <param name="source">The name of the source property. The use of `nameof()` is encouraged.</param>
    /// <param name="target">The name of the target property. The use of `nameof()` is encouraged.</param>
    public MapPropertyAttribute(string source, string target)
    {
        Source = source;
        Target = target;
    }

    /// <summary>
    /// Gets the name of the source property.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Gets the name of the target property.
    /// </summary>
    public string Target { get; }
}
