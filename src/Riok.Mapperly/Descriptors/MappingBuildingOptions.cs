namespace Riok.Mapperly.Descriptors;

[Flags]
public enum MappingBuildingOptions
{
    None = 0,
    Default = MarkAsReusable,

    /// <summary>
    /// Whether the built mapping can be reused and is findable by <see cref="MappingBuilderContext.FindMapping"/>.
    /// </summary>
    MarkAsReusable = 1 << 0,

    /// <summary>
    /// Keeps the existing user symbol (method) and its configuration if present.
    /// </summary>
    KeepUserSymbol = 1 << 1,

    /// <summary>
    /// Ignores the derived types of the configuration.
    /// </summary>
    IgnoreDerivedTypes = 1 << 2,
}
