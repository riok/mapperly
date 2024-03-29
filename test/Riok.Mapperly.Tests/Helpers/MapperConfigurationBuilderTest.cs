using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;

namespace Riok.Mapperly.Tests.Helpers;

public class MapperConfigurationBuilderTest
{
    [Fact]
    public void ShouldMergeMapperConfigurations()
    {
        var ignoredProperties = new HashSet<string> { "TypeId" };
        var properties = typeof(MapperConfiguration).GetProperties();

        var mapperConfiguration = new MapperConfiguration();
        var defaultMapperConfiguration = new MapperConfiguration();
        foreach (var property in properties.Where(x => !ignoredProperties.Contains(x.Name)))
        {
            property.SetValue(defaultMapperConfiguration, GetValue(property.PropertyType, false));
            property.SetValue(mapperConfiguration, GetValue(property.PropertyType, true));
        }

        var mergedConfiguration = MapperConfigurationMerger.Merge(mapperConfiguration, defaultMapperConfiguration);
        var attributeProperties = typeof(MapperAttribute).GetProperties();
        foreach (var property in attributeProperties.Where(x => !ignoredProperties.Contains(x.Name)))
        {
            property
                .GetValue(mergedConfiguration)
                .Should()
                .Be(GetValue(property.PropertyType, true), $"the property {property.Name} does not match, is it missing in the merger?");
        }
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
            PreferParameterlessConstructors = true,
        };
    }

    private object? GetValue(Type type, bool modifiedValue)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return GetValue(type.GetGenericArguments()[0], modifiedValue);

        if (type == typeof(bool))
            return !modifiedValue;

        if (type == typeof(uint))
            return (uint)1;

        if (type.IsEnum)
            return type.GetEnumValues().GetValue(modifiedValue ? 1 : 0);

        throw new InvalidOperationException("Unsupported type " + type);
    }
}
