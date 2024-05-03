using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Maps a property from the source object.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapPropertyFromSourceAttribute : Attribute
{
    private const string PropertyAccessSeparatorStr = ".";
    private const char PropertyAccessSeparator = '.';

    /// <summary>
    /// Maps the specified target property from the source object.
    /// </summary>
    /// <param name="target">The name of the target property. The use of `nameof()` is encouraged. A path can be specified by joining property names with a '.'.</param>
    public MapPropertyFromSourceAttribute(string target)
        : this(target.Split(PropertyAccessSeparator)) { }

    /// <summary>
    /// Maps the specified target property from the source object.
    /// </summary>
    /// <param name="target">The path of the target property. The use of `nameof()` is encouraged.</param>
    public MapPropertyFromSourceAttribute(string[] target)
    {
        Target = target;
    }

    /// <summary>
    /// Gets the name of the target property.
    /// </summary>
    public IReadOnlyCollection<string> Target { get; }

    /// <summary>
    /// Gets the full name of the target property path.
    /// </summary>
    public string TargetFullName => string.Join(PropertyAccessSeparatorStr, Target);

    /// <summary>
    /// Gets or sets the format of the <c>ToString</c> conversion (implementing <see cref="IFormattable" />).
    /// </summary>
    public string? StringFormat { get; set; }

    /// <summary>
    /// Gets or sets the name of a format provider field or property to be used for conversions accepting a format provider (implementing <see cref="IFormattable"/>).
    /// If <see langword="null"/> the default format provider (annotated with <see cref="FormatProviderAttribute"/> and <see cref="FormatProviderAttribute.Default"/> <see langword="true"/>)
    /// or none (if no default format provider is provided) is used.
    /// </summary>
    public string? FormatProvider { get; set; }

    /// <summary>
    /// Reference to a unique named mapping method which should be used to map this member.
    /// </summary>
    public string? Use { get; set; }
}
