namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// When to build the body of this mapping.
/// The body of mappings with a higher priority
/// is built before the bodies of mappings with a lower priority.
/// </summary>
public enum MappingBodyBuildingPriority
{
    /// <summary>
    /// Default mapping priority.
    /// </summary>
    Default,

    /// <summary>
    /// Priority for mappings which require the body of user mappings to be built.
    /// (Depend on the user mapping bodies).
    /// </summary>
    AfterUserMappings,
}
