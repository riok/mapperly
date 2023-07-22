using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration;

public class MapperConfiguration
{
    private readonly MappingConfiguration _defaultConfiguration;
    private readonly AttributeDataAccessor _dataAccessor;

    public MapperConfiguration(SymbolAccessor symbolAccessor, ISymbol mapperSymbol)
    {
        _dataAccessor = new AttributeDataAccessor(symbolAccessor);
        Mapper = _dataAccessor.AccessSingle<MapperAttribute>(mapperSymbol);
        _defaultConfiguration = new MappingConfiguration(
            new EnumMappingConfiguration(
                Mapper.EnumMappingStrategy,
                Mapper.EnumMappingIgnoreCase,
                null,
                Array.Empty<IFieldSymbol>(),
                Array.Empty<IFieldSymbol>(),
                Array.Empty<EnumValueMappingConfiguration>()
            ),
            new PropertiesMappingConfiguration(
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<PropertyMappingConfiguration>(),
                Array.Empty<NestedPropertyMappingConfiguration>(),
                Mapper.IgnoreObsoleteMembersStrategy
            ),
            Array.Empty<DerivedTypeMappingConfiguration>()
        );
    }

    public MapperAttribute Mapper { get; }

    public MappingConfiguration BuildFor(MappingConfigurationReference reference)
    {
        if (reference.Method == null)
            return _defaultConfiguration;

        var enumConfig = BuildEnumConfig(reference);
        var propertiesConfig = BuildPropertiesConfig(reference.Method);
        var derivedTypesConfig = BuildDerivedTypeConfigs(reference.Method);
        return new MappingConfiguration(enumConfig, propertiesConfig, derivedTypesConfig);
    }

    private IReadOnlyCollection<DerivedTypeMappingConfiguration> BuildDerivedTypeConfigs(IMethodSymbol method)
    {
        return _dataAccessor
            .Access<MapDerivedTypeAttribute, DerivedTypeMappingConfiguration>(method)
            .Concat(_dataAccessor.Access<MapDerivedTypeAttribute<object, object>, DerivedTypeMappingConfiguration>(method))
            .ToList();
    }

    private PropertiesMappingConfiguration BuildPropertiesConfig(IMethodSymbol method)
    {
        var ignoredSourceProperties = _dataAccessor
            .Access<MapperIgnoreSourceAttribute>(method)
            .Select(x => x.Source)
            .WhereNotNull()
            .ToList();
        var ignoredTargetProperties = _dataAccessor
            .Access<MapperIgnoreTargetAttribute>(method)
            .Select(x => x.Target)
            // deprecated MapperIgnoreAttribute, but it is still supported by Mapperly.
#pragma warning disable CS0618
            .Concat(_dataAccessor.Access<MapperIgnoreAttribute>(method).Select(x => x.Target))
#pragma warning restore CS0618
            .WhereNotNull()
            .ToList();
        var explicitMappings = _dataAccessor.Access<MapPropertyAttribute, PropertyMappingConfiguration>(method).ToList();
        var ignoreObsolete = _dataAccessor.Access<MapperIgnoreObsoleteMembersAttribute>(method).FirstOrDefault() is not { } methodIgnore
            ? _defaultConfiguration.Properties.IgnoreObsoleteMembersStrategy
            : methodIgnore.IgnoreObsoleteStrategy;
        var nestedMappings = _dataAccessor.Access<MapperNestedPropertiesAttribute, NestedPropertyMappingConfiguration>(method).ToList();
        return new PropertiesMappingConfiguration(
            ignoredSourceProperties,
            ignoredTargetProperties,
            explicitMappings,
            nestedMappings,
            ignoreObsolete
        );
    }

    private EnumMappingConfiguration BuildEnumConfig(MappingConfigurationReference configRef)
    {
        if (configRef.Method == null || !configRef.Source.IsEnum() && !configRef.Target.IsEnum())
            return _defaultConfiguration.Enum;

        var configData = _dataAccessor.AccessFirstOrDefault<MapEnumAttribute, EnumConfiguration>(configRef.Method);
        var explicitMappings = _dataAccessor.Access<MapEnumValueAttribute, EnumValueMappingConfiguration>(configRef.Method).ToList();
        var ignoredSources = _dataAccessor
            .Access<MapperIgnoreSourceValueAttribute, MapperIgnoreEnumValueConfiguration>(configRef.Method)
            .Select(x => x.Value)
            .ToList();
        var ignoredTargets = _dataAccessor
            .Access<MapperIgnoreTargetValueAttribute, MapperIgnoreEnumValueConfiguration>(configRef.Method)
            .Select(x => x.Value)
            .ToList();
        return new EnumMappingConfiguration(
            configData?.Strategy ?? _defaultConfiguration.Enum.Strategy,
            configData?.IgnoreCase ?? _defaultConfiguration.Enum.IgnoreCase,
            configData?.FallbackValue,
            ignoredSources,
            ignoredTargets,
            explicitMappings
        );
    }
}
