using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Specifies options for a property mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapPropertyAttribute : Attribute
{
    private const string PropertyAccessSeparatorStr = ".";
    private const char PropertyAccessSeparator = '.';

    /// <summary>
    /// Maps a specified source property to the specified target property.
    /// </summary>
    /// <param name="source">The name of the source property. The use of `nameof()` is encouraged. A path can be specified by joining property names with a '.'.</param>
    /// <param name="target">The name of the target property. The use of `nameof()` is encouraged. A path can be specified by joining property names with a '.'.</param>
    public MapPropertyAttribute(string source, string target)
        : this(source.Split(PropertyAccessSeparator), target.Split(PropertyAccessSeparator)) { }

    /// <summary>
    /// Maps a specified source property to the specified target property.
    /// </summary>
    /// <param name="source">The path of the source property. The use of `nameof()` is encouraged.</param>
    /// <param name="target">The path of the target property. The use of `nameof()` is encouraged.</param>
    public MapPropertyAttribute(string[] source, string[] target)
    {
        Source = source;
        Target = target;
    }

    /// <summary>
    /// Gets the name of the source property.
    /// </summary>
    public IReadOnlyCollection<string> Source { get; }

    /// <summary>
    /// Gets the full name of the source property path.
    /// </summary>
    public string SourceFullName => string.Join(PropertyAccessSeparatorStr, Source);

    /// <summary>
    /// Gets the name of the target property.
    /// </summary>
    public IReadOnlyCollection<string> Target { get; }

    /// <summary>
    /// Gets or sets the format of the <c>ToString</c> conversion (implementing <see cref="IFormattable" />).
    /// </summary>
    public string? StringFormat { get; set; }

    /// <summary>
    /// Gets or sets the name of a format provider field or property to be used for conversions accepting a format provider (implementing <see cref="IFormattable"/>).
    /// If <c>null</c> the default format provider (annotated with <see cref="FormatProviderAttribute"/> and <see cref="FormatProviderAttribute.Default"/> <c>true</c>)
    /// or none (if no default format provider is provided) is used.
    /// </summary>
    public string? FormatProvider { get; set; }

    /// <summary>
    /// Gets the full name of the target property path.
    /// </summary>
    public string TargetFullName => string.Join(PropertyAccessSeparatorStr, Target);
}
