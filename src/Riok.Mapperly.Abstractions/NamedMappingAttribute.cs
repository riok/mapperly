using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Specifies a custom name for the mapping. This can be used to reference in other mapping configurations.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class NamedMappingAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the custom name specified for the mapping.
    /// </summary>
    public string Name { get; } = name;
}
