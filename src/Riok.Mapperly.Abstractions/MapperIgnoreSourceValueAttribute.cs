namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Ignores a source enum value from the mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class MapperIgnoreSourceValueAttribute : Attribute
{
    /// <summary>
    /// Ignores the specified source enum value from the mapping.
    /// </summary>
    /// <param name="source">The source enum value to ignore.</param>
    public MapperIgnoreSourceValueAttribute(object source)
    {
        SourceValue = (Enum)source;
    }

    /// <summary>
    /// Gets the source enum value which should be ignored from the mapping.
    /// </summary>
    public Enum? SourceValue { get; }
}
