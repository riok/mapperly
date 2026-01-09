using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// An attribute used to include the mapping configuration of another mapping method in the attributed method.
/// Use the other mapping's method name to identify the configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class IncludeMappingConfigurationAttribute : Attribute
{
    /// <summary>
    /// An attribute used to include the mapping configuration of another mapping method in the attributed method.
    /// Use the other mapping's method name to identify the configuration.
    /// </summary>
    /// <param name="name">
    /// The name of the mapping configuration to include. Use the method name of the other mapping
    /// method.
    /// </param>
    public IncludeMappingConfigurationAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the name of the mapping configuration to include.
    /// </summary>
    public string Name { get; }

    public bool Reverse { get; set; }
}
