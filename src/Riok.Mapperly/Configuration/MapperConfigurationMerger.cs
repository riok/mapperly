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

        mapper.StackCloningStrategy =
            mapperConfiguration.StackCloningStrategy ?? defaultMapperConfiguration.StackCloningStrategy ?? mapper.StackCloningStrategy;

        mapper.EnabledConversions =
            mapperConfiguration.EnabledConversions ?? defaultMapperConfiguration.EnabledConversions ?? mapper.EnabledConversions;

        mapper.UseReferenceHandling =
            mapperConfiguration.UseReferenceHandling ?? defaultMapperConfiguration.UseReferenceHandling ?? mapper.UseReferenceHandling;

        mapper.IgnoreObsoleteMembersStrategy =
            mapperConfiguration.IgnoreObsoleteMembersStrategy
            ?? defaultMapperConfiguration.IgnoreObsoleteMembersStrategy
            ?? mapper.IgnoreObsoleteMembersStrategy;

        mapper.RequiredMappingStrategy =
            mapperConfiguration.RequiredMappingStrategy
            ?? defaultMapperConfiguration.RequiredMappingStrategy
            ?? mapper.RequiredMappingStrategy;

        mapper.RequiredEnumMappingStrategy =
            mapperConfiguration.RequiredEnumMappingStrategy
            ?? defaultMapperConfiguration.RequiredEnumMappingStrategy
            ?? mapperConfiguration.RequiredMappingStrategy
            ?? defaultMapperConfiguration.RequiredMappingStrategy
            ?? mapper.RequiredMappingStrategy;

        mapper.IncludedMembers =
            mapperConfiguration.IncludedMembers ?? defaultMapperConfiguration.IncludedMembers ?? mapper.IncludedMembers;

        mapper.IncludedConstructors =
            mapperConfiguration.IncludedConstructors ?? defaultMapperConfiguration.IncludedConstructors ?? mapper.IncludedConstructors;

        mapper.PreferParameterlessConstructors =
            mapperConfiguration.PreferParameterlessConstructors
            ?? defaultMapperConfiguration.PreferParameterlessConstructors
            ?? mapper.PreferParameterlessConstructors;

        mapper.AutoUserMappings =
            mapperConfiguration.AutoUserMappings ?? defaultMapperConfiguration.AutoUserMappings ?? mapper.AutoUserMappings;

        mapper.EnumNamingStrategy =
            mapperConfiguration.EnumNamingStrategy ?? defaultMapperConfiguration.EnumNamingStrategy ?? mapper.EnumNamingStrategy;

        return mapper;
    }
}
