using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Maps a target property from an additional mapping method parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapPropertyFromParameterAttribute : Attribute
{
    private const string PropertyAccessSeparatorStr = ".";
    private const char PropertyAccessSeparator = '.';

    /// <summary>
    /// Maps the specified target property from the specified mapping method parameter.
    /// </summary>
    /// <param name="target">The name of the target property. The use of `nameof()` is encouraged. A path can be specified by joining property names with a '.'.</param>
    /// <param name="parameter">The name of the mapping method parameter.</param>
    public MapPropertyFromParameterAttribute(string target, string parameter)
        : this(target.Split(PropertyAccessSeparator), parameter) { }

    /// <summary>
    /// Maps the specified target property from the specified mapping method parameter.
    /// </summary>
    /// <param name="target">The path of the target property. The use of `nameof()` is encouraged.</param>
    /// <param name="parameter">The name of the mapping method parameter.</param>
    public MapPropertyFromParameterAttribute(string[] target, string parameter)
    {
        Target = target;
        Parameter = parameter;
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
    /// Gets the name of the mapping method parameter.
    /// </summary>
    public string Parameter { get; }

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

    /// <summary>
    /// When set to true, RMG089 is not emitted.
    /// </summary>
    public bool SuppressNullMismatchDiagnostic { get; set; }
}
