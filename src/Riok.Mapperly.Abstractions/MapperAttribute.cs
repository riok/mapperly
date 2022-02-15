namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Marks an abstract class or an interface as a mapper.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
public sealed class MapperAttribute : Attribute
{
    /// <summary>
    /// Name of the generated mapper class.
    /// <c>null</c> to fallback to the default name.
    /// </summary>
    public string? ImplementationName { get; set; }

    /// <summary>
    /// Name of the instance field, <c>null</c> if none should be generated.
    /// </summary>
    public string? InstanceName { get; set; } = "Instance";

    /// <summary>
    /// The default enum mapping strategy.
    /// Can be overwritten on specific enums via mapping method configurations.
    /// </summary>
    public EnumMappingStrategy EnumMappingStrategy { get; set; } = EnumMappingStrategy.ByValue;

    /// <summary>
    /// Whether to always deep copy objects.
    /// Eg. when the type <c>Person[]</c> should be mapped to the same type <c>Person[]</c>,
    /// with <c><see cref="UseDeepCloning"/>=true</c>, the same array is reused.
    /// With <c><see cref="UseDeepCloning"/>=false</c>, the array and each person is cloned.
    /// </summary>
    public bool UseDeepCloning { get; set; }
}
