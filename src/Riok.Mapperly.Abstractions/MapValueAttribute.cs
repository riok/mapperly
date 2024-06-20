using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Specifies a constant value mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapValueAttribute : Attribute
{
    private const string PropertyAccessSeparatorStr = ".";
    private const char PropertyAccessSeparator = '.';

    /// <summary>
    /// Maps a constant value to a target member.
    /// </summary>
    /// <param name="target">The target member path.</param>
    /// <param name="value">The value to assign to the <paramref name="target"/>, needs to be of the same type as the <paramref name="target"/>.</param>
    public MapValueAttribute(string target, object? value)
        : this(target.Split(PropertyAccessSeparator), value) { }

    /// <summary>
    /// Maps a constant value to a target member.
    /// </summary>
    /// <param name="target">The target member path.</param>
    /// <param name="value">The value to assign to the <paramref name="target"/>, needs to be of the same type as the <paramref name="target"/>.</param>
    public MapValueAttribute(string[] target, object? value)
    {
        Target = target;
        Value = value;
    }

    /// <summary>
    /// Maps a method generated value to a target member.
    /// Requires the usage of the <see cref="Use"/> property.
    /// </summary>
    /// <param name="target">The target member path.</param>
    public MapValueAttribute(string target)
        : this(target, null) { }

    /// <summary>
    /// Maps a method generated value to a target member.
    /// Requires the usage of the <see cref="Use"/> property.
    /// </summary>
    /// <param name="target">The target member path, the usage of nameof is encouraged.</param>
    public MapValueAttribute(string[] target)
        : this(target, null) { }

    /// <summary>
    /// Gets the name of the target property.
    /// </summary>
    public IReadOnlyCollection<string> Target { get; }

    /// <summary>
    /// Gets the full name of the target property path.
    /// </summary>
    public string TargetFullName => string.Join(PropertyAccessSeparatorStr, Target);

    /// <summary>
    /// Gets the value to be assigned to <see cref="Target"/>.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets or sets the method name of the method which generates the value to be assigned to <see cref="Target"/>.
    /// Either this property or <see cref="Value"/> needs to be set.
    /// The return type of the referenced method must exactly match the type of <see cref="Target"/>
    /// and needs to be parameterless.
    /// The usage of nameof is encouraged.
    /// </summary>
    public string? Use { get; set; }
}
