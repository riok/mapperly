namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Ignores multiple source properties from the mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class MapperIgnoreSourcesAttribute : Attribute
{
    /// <summary>
    /// Ignores multiple source properties from the mapping.
    /// </summary>
    /// <param name="sources">Source property names to ignore. The use of `nameof()` is encouraged.</param>
    public MapperIgnoreSourcesAttribute(params string[] sources)
    {
        Sources = sources;
    }

    /// <summary>
    /// Gets the names of source properties which should be ignored from the mapping.
    /// </summary>
    public IEnumerable<string> Sources { get; }
}
