namespace Riok.Mapperly.Configuration;

public record MappingConfiguration(
    EnumMappingConfiguration Enum,
    PropertiesMappingConfiguration Properties,
    IReadOnlyCollection<DerivedTypeMappingConfiguration> DerivedTypes
);
