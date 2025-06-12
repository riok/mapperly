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
    public MappingConfiguration Include(MappingConfiguration? result2)
    {
        return this with
        {
            Enum = Enum.Include(result2?.Enum),
            Members = Members.Include(result2?.Members),
            DerivedTypes = DerivedTypes.Concat(result2?.DerivedTypes ?? []).ToList(),
        };
    }

    public IgnoreObsoleteMembersStrategy GetIgnoreObsoleteMembersStrategy()
    {
        return Members.IgnoreObsoleteMembersStrategy.GetValueOrDefault(Mapper.IgnoreObsoleteMembersStrategy);
    }
}
