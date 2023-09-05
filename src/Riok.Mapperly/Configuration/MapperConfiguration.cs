using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Riok.Mapperly.Configuration;

/// <summary>
/// Contains all the properties of <see cref="MapperAttribute"/> and <see cref="MapperDefaultsAttribute"/> but all of them are nullable.
/// This is needed to evaluate which properties are set in the <see cref="MapperAttribute"/> and <see cref="MapperDefaultsAttribute"/>.
/// <remarks>
/// Newly added properties must also be added to <see cref="Riok.Mapperly.Configuration.MapperConfigurationMerger"/>.
/// </remarks>
/// </summary>
public record MapperConfiguration
{
    public static readonly MapperConfiguration Default = new();

    /// <summary>
    /// Strategy on how to match mapping property names.
    /// </summary>
    public PropertyNameMappingStrategy? PropertyNameMappingStrategy { get; init; }

    /// <summary>
    /// The default enum mapping strategy.
    /// Can be overwritten on specific enums via mapping method configurations.
    /// </summary>
    public EnumMappingStrategy? EnumMappingStrategy { get; init; }

    /// <summary>
    /// Whether the case should be ignored for enum mappings.
    /// </summary>
    public bool? EnumMappingIgnoreCase { get; init; }

    /// <summary>
    /// Specifies the behaviour in the case when the mapper tries to return <c>null</c> in a mapping method with a non-nullable return type.
    /// If set to <c>true</c> an <see cref="ArgumentNullException"/> is thrown.
    /// If set to <c>false</c> the mapper tries to return a default value.
    /// For a <see cref="string"/> this is <see cref="string.Empty"/>,
    /// for value types <c>default</c>
    /// and for reference types <c>new()</c> if a parameterless constructor exists or else an <see cref="ArgumentNullException"/> is thrown.
    /// </summary>
    public bool? ThrowOnMappingNullMismatch { get; init; }

    /// <summary>
    /// Specifies the behaviour in the case when the mapper tries to set a non-nullable property to a <c>null</c> value.
    /// If set to <c>true</c> an <see cref="ArgumentNullException"/> is thrown.
    /// If set to <c>false</c> the property assignment is ignored.
    /// This is ignored for required init properties and <see cref="IQueryable{T}"/> projection mappings.
    /// </summary>
    public bool? ThrowOnPropertyMappingNullMismatch { get; init; }

    /// <summary>
    /// Specifies whether <c>null</c> values are assigned to the target.
    /// If <c>true</c> (default), the source is <c>null</c>, and the target does allow <c>null</c> values,
    /// <c>null</c> is assigned.
    /// If <c>false</c>, <c>null</c> values are never assigned to the target property.
    /// This is ignored for required init properties and <see cref="IQueryable{T}"/> projection mappings.
    /// </summary>
    public bool? AllowNullPropertyAssignment { get; init; }

    /// <summary>
    /// Whether to always deep copy objects.
    /// Eg. when the type <c>Person[]</c> should be mapped to the same type <c>Person[]</c>,
    /// when <c>false</c>, the same array is reused.
    /// when <c>true</c>, the array and each person is cloned.
    /// </summary>
    public bool? UseDeepCloning { get; init; }

    /// <summary>
    /// Enabled conversions which Mapperly automatically implements.
    /// By default all supported type conversions are enabled.
    /// <example>
    /// Eg. to disable all automatically implemented conversions:<br />
    /// <c>EnabledConversions = MappingConversionType.None</c>
    /// </example>
    /// <example>
    /// Eg. to disable <c>ToString()</c> method calls:<br />
    /// <c>EnabledConversions = MappingConversionType.All &amp; ~MappingConversionType.ToStringMethod</c>
    /// </example>
    /// </summary>
    public MappingConversionType? EnabledConversions { get; init; }

    /// <summary>
    /// Enables the reference handling feature.
    /// Disabled by default for performance reasons.
    /// When enabled, an <see cref="IReferenceHandler"/> instance is passed through the mapping methods
    /// to keep track of and reuse existing target object instances.
    /// </summary>
    public bool? UseReferenceHandling { get; init; }

    /// <summary>
    /// The ignore obsolete attribute strategy. Determines how <see cref="ObsoleteAttribute"/> marked members are mapped.
    /// Defaults to <see cref="IgnoreObsoleteMembersStrategy.None"/>.
    /// </summary>
    public IgnoreObsoleteMembersStrategy? IgnoreObsoleteMembersStrategy { get; init; }

    /// <summary>
    /// Defines the strategy used when emitting warnings for unmapped members.
    /// By default this is <see cref="RequiredMappingStrategy.Both"/>, emitting warnings for unmapped source and target members.
    /// </summary>
    public RequiredMappingStrategy? RequiredMappingStrategy { get; init; }

    /// <summary>
    /// Determines the access level of members that Mapperly will map.
    /// </summary>
    public MemberVisibility? IncludedMembers { get; init; }
}
