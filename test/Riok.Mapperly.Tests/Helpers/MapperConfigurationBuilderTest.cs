using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;

namespace Riok.Mapperly.Tests.Helpers;

public class MapperConfigurationBuilderTest
{
    [Fact]
    public void ShouldMergeMapperConfigurations()
    {
        var mapperConfiguration = NewMapperConfiguration();
        var defaultMapperConfiguration = new MapperConfiguration
        {
            PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseInsensitive,
            EnumMappingStrategy = EnumMappingStrategy.ByValue,
            EnumMappingIgnoreCase = false,
            ThrowOnMappingNullMismatch = false,
            ThrowOnPropertyMappingNullMismatch = false,
            AllowNullPropertyAssignment = false,
            UseDeepCloning = false,
            EnabledConversions = MappingConversionType.Dictionary,
            UseReferenceHandling = false,
            IgnoreObsoleteMembersStrategy = IgnoreObsoleteMembersStrategy.Target,
            RequiredMappingStrategy = RequiredMappingStrategy.Target,
            PreferParameterlessConstructors = false,
        };

        var mapper = MapperConfigurationMerger.Merge(mapperConfiguration, defaultMapperConfiguration);
        mapper.PropertyNameMappingStrategy.Should().Be(PropertyNameMappingStrategy.CaseSensitive);
        mapper.EnumMappingStrategy.Should().Be(EnumMappingStrategy.ByName);
        mapper.EnumMappingIgnoreCase.Should().BeTrue();
        mapper.ThrowOnMappingNullMismatch.Should().BeTrue();
        mapper.ThrowOnPropertyMappingNullMismatch.Should().BeTrue();
        mapper.AllowNullPropertyAssignment.Should().BeTrue();
        mapper.UseDeepCloning.Should().BeTrue();
        mapper.EnabledConversions.Should().Be(MappingConversionType.Constructor);
        mapper.UseReferenceHandling.Should().BeTrue();
        mapper.IgnoreObsoleteMembersStrategy.Should().Be(IgnoreObsoleteMembersStrategy.Source);
        mapper.RequiredMappingStrategy.Should().Be(RequiredMappingStrategy.Source);
        mapper.PreferParameterlessConstructors.Should().BeTrue();
    }

    [Fact]
    public void ShouldMergeMapperConfigurationsWithEmptyDefaultMapperConfiguration()
    {
        var mapperConfiguration = NewMapperConfiguration();
        var mapper = MapperConfigurationMerger.Merge(mapperConfiguration, new());
        mapper.PropertyNameMappingStrategy.Should().Be(PropertyNameMappingStrategy.CaseSensitive);
        mapper.EnumMappingStrategy.Should().Be(EnumMappingStrategy.ByName);
        mapper.EnumMappingIgnoreCase.Should().BeTrue();
        mapper.ThrowOnMappingNullMismatch.Should().BeTrue();
        mapper.ThrowOnPropertyMappingNullMismatch.Should().BeTrue();
        mapper.AllowNullPropertyAssignment.Should().BeTrue();
        mapper.UseDeepCloning.Should().BeTrue();
        mapper.EnabledConversions.Should().Be(MappingConversionType.Constructor);
        mapper.UseReferenceHandling.Should().BeTrue();
        mapper.IgnoreObsoleteMembersStrategy.Should().Be(IgnoreObsoleteMembersStrategy.Source);
        mapper.RequiredMappingStrategy.Should().Be(RequiredMappingStrategy.Source);
        mapper.UseReferenceHandling.Should().BeTrue();
        mapper.PreferParameterlessConstructors.Should().BeTrue();
    }

    [Fact]
    public void ShouldMergeMapperConfigurationsWithEmptyMapperConfiguration()
    {
        var mapperConfiguration = new MapperConfiguration();
        var defaultMapperConfiguration = new MapperConfiguration
        {
            PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseInsensitive,
            EnumMappingStrategy = EnumMappingStrategy.ByName,
            EnumMappingIgnoreCase = true,
        };

        var mapper = MapperConfigurationMerger.Merge(mapperConfiguration, defaultMapperConfiguration);
        mapper.PropertyNameMappingStrategy.Should().Be(PropertyNameMappingStrategy.CaseInsensitive);
        mapper.EnumMappingStrategy.Should().Be(EnumMappingStrategy.ByName);
        mapper.EnumMappingIgnoreCase.Should().BeTrue();
    }

    private MapperConfiguration NewMapperConfiguration()
    {
        return new MapperConfiguration
        {
            PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseSensitive,
            EnumMappingStrategy = EnumMappingStrategy.ByName,
            EnumMappingIgnoreCase = true,
            ThrowOnMappingNullMismatch = true,
            ThrowOnPropertyMappingNullMismatch = true,
            AllowNullPropertyAssignment = true,
            UseDeepCloning = true,
            EnabledConversions = MappingConversionType.Constructor,
            UseReferenceHandling = true,
            IgnoreObsoleteMembersStrategy = IgnoreObsoleteMembersStrategy.Source,
            RequiredMappingStrategy = RequiredMappingStrategy.Source,
            PreferParameterlessConstructors = false,
        };
    }
}
