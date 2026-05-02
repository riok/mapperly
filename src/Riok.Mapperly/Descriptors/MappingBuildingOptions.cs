namespace Riok.Mapperly.Descriptors;

[Flags]
public enum MappingBuildingOptions
{
    None = 0,
    Default = MarkAsReusable,

    /// <summary>
    /// Whether the built mapping can be reused and is findable by <see cref="MappingBuilderContext.FindMapping(TypeMappingKey)"/>.
    /// </summary>
    MarkAsReusable = 1 << 0,

    /// <summary>
    /// Keeps the existing user symbol (user mapping / method) and its configuration if present.
    /// </summary>
    KeepUserSymbol = 1 << 1,

    /// <summary>
    /// Ignores the derived types of the configuration.
    /// </summary>
    IgnoreDerivedTypes = 1 << 2,

    /// <summary>
    /// The mapping is embedded in the parent method body (not emitted as a separate method).
    /// Additional parameters from the user mapping are exposed to the inner mapping's body builder,
    /// and unused-parameter diagnostics are reported at this level.
    /// </summary>
    EmbeddedMapping = 1 << 3,
}
