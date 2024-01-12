using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Used to set mapper default values in the assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperDefaultsAttribute : MapperAttribute
{
    /// <summary>
    /// Controls the priority of constructors used in mapping.
    ///  When enabled, a parameterless constructor is prioritized over constructors with parameters.
    ///  When disabled, accessible constructors are ordered in descending order by their parameter count.
    /// </summary>
    public bool PreferParameterlessConstructors { get; set; } = true;
}
