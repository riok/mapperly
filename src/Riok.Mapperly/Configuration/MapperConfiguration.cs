using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors;

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
                Array.Empty<EnumValueMappingConfiguration>()
            ),
            new PropertiesMappingConfiguration(Array.Empty<string>(), Array.Empty<string>(), Array.Empty<PropertyMappingConfiguration>()),
            Array.Empty<DerivedTypeMappingConfiguration>()
        );
    }

    public MapperAttribute Mapper { get; }

    public MappingConfiguration ForMethod(IMethodSymbol? method)
    {
        if (method == null)
            return _defaultConfiguration;

        var enumConfig = BuildEnumConfig(method);
        var propertiesConfig = BuildPropertiesConfig(method);
        var derivedTypesConfig = BuildDerivedTypeConfigs(method);
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
        var ignoredSourceProperties = _dataAccessor.Access<MapperIgnoreSourceAttribute>(method).Select(x => x.Source).ToList();
        var ignoredTargetProperties = _dataAccessor
            .Access<MapperIgnoreTargetAttribute>(method)
            .Select(x => x.Target)
            // deprecated MapperIgnoreAttribute, but it is still supported by Mapperly.
#pragma warning disable CS0618
            .Concat(_dataAccessor.Access<MapperIgnoreAttribute>(method).Select(x => x.Target))
#pragma warning restore CS0618
            .ToList();
        var explicitMappings = _dataAccessor.Access<MapPropertyAttribute, PropertyMappingConfiguration>(method).ToList();
        return new PropertiesMappingConfiguration(ignoredSourceProperties, ignoredTargetProperties, explicitMappings);
    }

    private EnumMappingConfiguration BuildEnumConfig(IMethodSymbol method)
    {
        var configData = _dataAccessor.AccessFirstOrDefault<MapEnumAttribute, EnumConfiguration>(method);
        var explicitMappings = _dataAccessor.Access<MapEnumValueAttribute, EnumValueMappingConfiguration>(method).ToList();
        return new EnumMappingConfiguration(
            configData?.Strategy ?? _defaultConfiguration.Enum.Strategy,
            configData?.IgnoreCase ?? _defaultConfiguration.Enum.IgnoreCase,
            configData?.FallbackValue,
            explicitMappings
        );
    }
}
