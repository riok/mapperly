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
    /// Whether the case should be ignored for enum mappings.
    /// </summary>
    public bool EnumMappingIgnoreCase { get; set; }

    /// <summary>
    /// Specifies the behaviour in the case when the mapper tries to return <c>null</c> in a mapping method with a non-nullable return type.
    /// If set to <c>true</c> an <see cref="ArgumentNullException"/> is thrown.
    /// If set to <c>false</c> the mapper tries to return a default value.
    /// For a <see cref="string"/> this is <see cref="string.Empty"/>,
    /// for value types <c>default</c>
    /// and for reference types <c>new()</c> if a parameterless constructor exists or else an <see cref="ArgumentNullException"/> is thrown.
    /// </summary>
    public bool ThrowOnMappingNullMismatch { get; set; } = true;

    /// <summary>
    /// Specifies the behaviour in the case when the mapper tries to set a non-nullable property to a <c>null</c> value.
    /// If set to <c>true</c> an <see cref="ArgumentNullException"/> is thrown.
    /// If set to <c>false</c> the property assignment is ignored.
    /// </summary>
    public bool ThrowOnPropertyMappingNullMismatch { get; set; }

    /// <summary>
    /// Whether to always deep copy objects.
    /// Eg. when the type <c>Person[]</c> should be mapped to the same type <c>Person[]</c>,
    /// with <c><see cref="UseDeepCloning"/>=true</c>, the same array is reused.
    /// With <c><see cref="UseDeepCloning"/>=false</c>, the array and each person is cloned.
    /// </summary>
    public bool UseDeepCloning { get; set; }
}
