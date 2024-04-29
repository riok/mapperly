using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Maps all properties from a nested path on the source to the root of the target.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapNestedPropertiesAttribute : Attribute
{
    private const string PropertyAccessSeparatorStr = ".";
    private const char PropertyAccessSeparator = '.';

    /// <summary>
    /// Maps all members of the specified source property to the root of the target.
    /// </summary>
    /// <param name="source">
    /// The name of the source property that will be flattened. The use of `nameof()` is encouraged. A path can be specified by joining property names with a '.'.
    /// </param>
    public MapNestedPropertiesAttribute(string source)
        : this(source.Split(PropertyAccessSeparator)) { }

    /// <summary>
    /// Maps all members of the specified source property to the root of the target.
    /// </summary>
    /// <param name="source">The path of the source property that will be flattened. The use of `nameof()` is encouraged.</param>
    public MapNestedPropertiesAttribute(string[] source)
    {
        Source = source;
    }

    /// <summary>
    /// Gets the name of the source property to flatten.
    /// </summary>
    public IReadOnlyCollection<string> Source { get; }

    /// <summary>
    /// Gets the full name of the source property path to flatten.
    /// </summary>
    public string SourceFullName => string.Join(PropertyAccessSeparatorStr, Source);
}
