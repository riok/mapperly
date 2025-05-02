using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// An attribute that specifies a custom name for the mapping configuration defined by the attributed method.
/// This name can be used to uniquely identify the mapping configuration and reference it from other mapping methods.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public class NamedMappingAttribute : Attribute
{
    /// <summary>
    /// Gets the custom name defined for the mapping configuration.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Specifies a custom name for the mapping configuration defined by the attributed mapping method.
    /// By default, the method name identifies the mapping configuration. This attribute allows specifying an
    /// explicit, unique name to reference this configuration from other mapping methods.
    /// </summary>
    /// <param name="name">The custom name that identifies the mapping configuration.</param>
    public NamedMappingAttribute(string name)
    {
        Name = name;
    }
}
