using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Ignores a source enum value from the mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
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

    /// <summary>
    /// Gets or sets the justification for ignoring the source enum value.
    /// This is only used for documentation purposes and does not have any effect on the mapping.
    /// </summary>
    /// <remarks>
    /// You can enforce the presence of justifications by setting the diagnostic severity of <c>RMG096</c> in your
    /// <c>.editorconfig</c> to any value other than <c>hidden</c>.
    /// </remarks>
    public string? Justification { get; set; }
}
