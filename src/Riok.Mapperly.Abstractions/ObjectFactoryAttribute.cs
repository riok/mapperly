using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Marks a method as an object factory.
/// An object factory can be used to instantiate or resolve target objects.
/// An object factory method needs to be a method with a non-void return type.
/// By default, it can be generic with constraints and can have one or none parameters.
/// If it has one parameter, the source object is provided as an argument.
/// If <see cref="MapToParameters"/> is enabled, source members are mapped to the factory method parameters instead.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class ObjectFactoryAttribute : Attribute
{
    /// <summary>
    /// Whether Mapperly should map source members to factory method parameters.
    /// </summary>
    public bool MapToParameters { get; set; }
}
