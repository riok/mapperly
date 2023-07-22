namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Maps all properties from a nested path to the root destination.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MapperNestedPropertiesAttribute : Attribute
{
    private const string PropertyAccessSeparatorStr = ".";
    private const char PropertyAccessSeparator = '.';

    /// <summary>
    /// Maps a specified source property to the specified target property.
    /// </summary>
    /// <param name="source">The name of the source property. The use of `nameof()` is encouraged. A path can be specified by joining property names with a '.'.</param>
    public MapperNestedPropertiesAttribute(string source)
        : this(source.Split(PropertyAccessSeparator)) { }

    /// <summary>
    /// Maps a specified source property to the specified target property.
    /// </summary>
    /// <param name="source">The path of the source property. The use of `nameof()` is encouraged.</param>
    public MapperNestedPropertiesAttribute(string[] source)
    {
        Source = source;
    }

    /// <summary>
    /// Gets the name of the source property.
    /// </summary>
    public IReadOnlyCollection<string> Source { get; }

    /// <summary>
    /// Gets the full name of the source property path.
    /// </summary>
    public string SourceFullName => string.Join(PropertyAccessSeparatorStr, Source);
}
