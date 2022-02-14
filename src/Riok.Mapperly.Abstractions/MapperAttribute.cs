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
}
