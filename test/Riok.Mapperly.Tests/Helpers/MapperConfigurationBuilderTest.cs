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
                .ShouldBe(
                    GetValue(property.PropertyType, true),
                    $"the property {property.Name} does not match, is it missing in the merger?"
                );
        }
    }

    [Fact]
    public void ShouldMergeMapperConfigurationsWithEmptyDefaultMapperConfiguration()
    {
        var mapperConfiguration = NewMapperConfiguration();
        var mapper = MapperConfigurationMerger.Merge(mapperConfiguration, new());
        mapper.PropertyNameMappingStrategy.ShouldBe(PropertyNameMappingStrategy.CaseSensitive);
        mapper.EnumMappingStrategy.ShouldBe(EnumMappingStrategy.ByName);
        mapper.EnumMappingIgnoreCase.ShouldBeTrue();
        mapper.ThrowOnMappingNullMismatch.ShouldBeTrue();
        mapper.ThrowOnPropertyMappingNullMismatch.ShouldBeTrue();
        mapper.AllowNullPropertyAssignment.ShouldBeTrue();
        mapper.UseDeepCloning.ShouldBeTrue();
        mapper.EnabledConversions.ShouldBe(MappingConversionType.Constructor);
        mapper.UseReferenceHandling.ShouldBeTrue();
        mapper.IgnoreObsoleteMembersStrategy.ShouldBe(IgnoreObsoleteMembersStrategy.Source);
        mapper.RequiredMappingStrategy.ShouldBe(RequiredMappingStrategy.Source);
        mapper.UseReferenceHandling.ShouldBeTrue();
        mapper.PreferParameterlessConstructors.ShouldBeTrue();
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
        mapper.PropertyNameMappingStrategy.ShouldBe(PropertyNameMappingStrategy.CaseInsensitive);
        mapper.EnumMappingStrategy.ShouldBe(EnumMappingStrategy.ByName);
        mapper.EnumMappingIgnoreCase.ShouldBeTrue();
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

        if (type.IsEnum)
            return type.GetEnumValues().GetValue(modifiedValue ? 1 : 0);

        throw new InvalidOperationException("Unsupported type " + type);
    }
}
