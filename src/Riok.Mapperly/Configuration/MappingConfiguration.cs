using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Configuration;

public record MappingConfiguration(
    MapperAttribute Mapper,
    EnumMappingConfiguration Enum,
    MembersMappingConfiguration Members,
    IReadOnlyCollection<DerivedTypeMappingConfiguration> DerivedTypes,
    bool UseDeepCloning,
    SupportedFeatures SupportedFeatures
)
{
    public MappingConfiguration MergeWith(MappingConfiguration result2)
    {
        return this with
        {
            Enum = Enum.MergeWith(result2.Enum),
            Members = Members.MergeWith(result2.Members),
            DerivedTypes = DerivedTypes.Concat(result2.DerivedTypes).ToList(),
        };
    }
}
