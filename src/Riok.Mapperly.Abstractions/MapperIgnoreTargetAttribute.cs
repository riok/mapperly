using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Ignores a target property from the mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperIgnoreTargetAttribute : Attribute
{
    /// <summary>
    /// Ignores the specified target property from the mapping.
    /// </summary>
    /// <param name="target">The name of the target property to ignore. The use of `nameof()` is encouraged.</param>
    public MapperIgnoreTargetAttribute(string target)
    {
        Target = target;
    }

    /// <summary>
    /// Gets the target property name which should be ignored from the mapping.
    /// </summary>
    public string Target { get; }

    /// <summary>
    /// Gets or sets the justification for ignoring the target property.
    /// This is only used for documentation purposes and does not have any effect on the mapping.
    /// </summary>
    /// <remarks>
    /// You can enforce the presence of justifications by setting the diagnostic severity of <c>RMG096</c> in your
    /// <c>.editorconfig</c> to any value other than <c>hidden</c>.
    /// </remarks>
    public string? Justification { get; set; }
}
