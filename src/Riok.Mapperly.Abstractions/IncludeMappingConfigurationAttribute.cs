using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// An attribute used to include the mapping configuration of another mapping method in the attributed method.
/// The configuration to include can be identified either by the method name or by a custom name specified
/// using the <see cref="NamedMappingAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class IncludeMappingConfigurationAttribute : Attribute
{
    /// <summary>
    /// Includes the mapping configuration of another mapping method into the attributed method.
    /// The referenced mapping configuration can be identified by either its method name or a custom name specified
    /// using <see cref="NamedMappingAttribute"/>.
    /// </summary>
    /// <param name="name">
    /// The name of the mapping configuration to include. This can be the method name of the other mapping
    /// method or a custom name provided via <see cref="NamedMappingAttribute"/>.
    /// </param>
    public IncludeMappingConfigurationAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the name of the mapping configuration to include.
    /// </summary>
    public string Name { get; }
}
