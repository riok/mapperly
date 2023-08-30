using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

public static class MapperConfigurationMerger
{
    public static MapperAttribute Merge(MapperConfiguration mapperConfiguration, MapperConfiguration defaultMapperConfiguration)
    {
        var mapper = new MapperAttribute();
        mapper.PropertyNameMappingStrategy =
            mapperConfiguration.PropertyNameMappingStrategy
            ?? defaultMapperConfiguration.PropertyNameMappingStrategy
            ?? mapper.PropertyNameMappingStrategy;

        mapper.EnumMappingStrategy =
            mapperConfiguration.EnumMappingStrategy ?? defaultMapperConfiguration.EnumMappingStrategy ?? mapper.EnumMappingStrategy;

        mapper.EnumMappingIgnoreCase =
            mapperConfiguration.EnumMappingIgnoreCase ?? defaultMapperConfiguration.EnumMappingIgnoreCase ?? mapper.EnumMappingIgnoreCase;

        mapper.ThrowOnMappingNullMismatch =
            mapperConfiguration.ThrowOnMappingNullMismatch
            ?? defaultMapperConfiguration.ThrowOnMappingNullMismatch
            ?? mapper.ThrowOnMappingNullMismatch;

        mapper.ThrowOnPropertyMappingNullMismatch =
            mapperConfiguration.ThrowOnPropertyMappingNullMismatch
            ?? defaultMapperConfiguration.ThrowOnPropertyMappingNullMismatch
            ?? mapper.ThrowOnPropertyMappingNullMismatch;

        mapper.AllowNullPropertyAssignment =
            mapperConfiguration.AllowNullPropertyAssignment
            ?? defaultMapperConfiguration.AllowNullPropertyAssignment
            ?? mapper.AllowNullPropertyAssignment;

        mapper.UseDeepCloning = mapperConfiguration.UseDeepCloning ?? defaultMapperConfiguration.UseDeepCloning ?? mapper.UseDeepCloning;

        mapper.EnabledConversions =
            mapperConfiguration.EnabledConversions ?? defaultMapperConfiguration.EnabledConversions ?? mapper.EnabledConversions;

        mapper.UseReferenceHandling =
            mapperConfiguration.UseReferenceHandling ?? defaultMapperConfiguration.UseReferenceHandling ?? mapper.UseReferenceHandling;

        mapper.IgnoreObsoleteMembersStrategy =
            mapperConfiguration.IgnoreObsoleteMembersStrategy
            ?? defaultMapperConfiguration.IgnoreObsoleteMembersStrategy
            ?? mapper.IgnoreObsoleteMembersStrategy;

        return mapper;
    }
}
