using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration;

public class MapperConfigurationReader
{
    private readonly AttributeDataAccessor _dataAccessor;
    private readonly MappingCollection _mappings;
    private readonly GenericTypeChecker _genericTypeChecker;
    private readonly DiagnosticCollection _diagnostics;
    private readonly WellKnownTypes _types;

    public MapperConfigurationReader(
        AttributeDataAccessor dataAccessor,
        MappingCollection mappings,
        GenericTypeChecker genericTypeChecker,
        DiagnosticCollection diagnostics,
        WellKnownTypes types,
        ISymbol mapperSymbol,
        MapperConfiguration defaultMapperConfiguration,
        SupportedFeatures supportedFeatures
    )
    {
        _dataAccessor = dataAccessor;
        _mappings = mappings;
        _genericTypeChecker = genericTypeChecker;
        _diagnostics = diagnostics;
        _types = types;

        var mapperConfiguration = _dataAccessor.AccessSingle<MapperAttribute, MapperConfiguration>(mapperSymbol);
        var mapper = MapperConfigurationMerger.Merge(mapperConfiguration, defaultMapperConfiguration);

        MapperConfiguration = new MappingConfiguration(
            mapper,
            new EnumMappingConfiguration(
                mapper.EnumMappingStrategy,
                mapper.EnumMappingIgnoreCase,
                null,
                [],
                [],
                [],
                mapper.RequiredEnumMappingStrategy,
                mapper.EnumNamingStrategy
            ),
            new MembersMappingConfiguration([], [], [], [], [], mapper.IgnoreObsoleteMembersStrategy, mapper.RequiredMappingStrategy),
            [],
            mapper.UseDeepCloning,
            supportedFeatures
        );
    }

    public MappingConfiguration MapperConfiguration { get; }

    public MappingConfiguration BuildFor(MappingConfigurationReference reference, bool supportsDeepCloning)
    {
        return BuildWithIncludedMappings([], reference, supportsDeepCloning)!;
    }

    private MappingConfiguration? BuildWithIncludedMappings(
        HashSet<IMethodSymbol> visitedMethods,
        MappingConfigurationReference reference,
        bool supportsDeepCloning
    )
    {
        if (reference.Method == null)
            return supportsDeepCloning ? MapperConfiguration : MapperConfiguration with { UseDeepCloning = false };

        var enumConfig = BuildEnumConfig(reference);
        var membersConfig = BuildMembersConfig(reference);
        var derivedTypesConfig = BuildDerivedTypeConfigs(reference.Method);
        var configuration = new MappingConfiguration(
            MapperConfiguration.Mapper,
            enumConfig,
            membersConfig,
            derivedTypesConfig,
            supportsDeepCloning && MapperConfiguration.Mapper.UseDeepCloning,
            MapperConfiguration.SupportedFeatures
        );

        var includeMapping = _dataAccessor.AccessFirstOrDefault<IncludeMappingConfigurationAttribute>(reference.Method)?.Name;
        if (includeMapping is null)
        {
            return configuration;
        }

        var typeMapping =
            (ITypeMapping?)_mappings.FindNamedNewInstanceMapping(includeMapping, out var ambiguousName)
            ?? _mappings.FindExistingInstanceNamedMapping(includeMapping, out ambiguousName);
        var methodSymbol = (typeMapping as IUserMapping)?.Method;

        if (!ValidateIncludedMapping(ambiguousName, reference, typeMapping, includeMapping, methodSymbol))
        {
            return configuration;
        }

        if (!visitedMethods.Add(methodSymbol))
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.CircularReferencedMapping, methodSymbol, methodSymbol.ToDisplayString());
            return null;
        }

        var includedReference = new MappingConfigurationReference(methodSymbol, typeMapping.SourceType, typeMapping.TargetType);

        var includedConfiguration = BuildWithIncludedMappings(visitedMethods, includedReference, supportsDeepCloning);
        return includedConfiguration != null ? configuration.Include(includedConfiguration) : configuration;
    }

    private bool ValidateIncludedMapping(
        bool ambiguousName,
        MappingConfigurationReference configRef,
        [NotNullWhen(true)] ITypeMapping? newInstanceMapping,
        string includeMapping,
        [NotNullWhen(true)] IMethodSymbol? methodSymbol
    )
    {
        if (ambiguousName)
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingAmbiguous, configRef.Method, includeMapping);
            return false;
        }

        if (newInstanceMapping is null)
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingNotFound, configRef.Method, includeMapping);
            return false;
        }

        var typeCheckerResult = _genericTypeChecker.InferAndCheckTypes(
            configRef.Method!.TypeParameters,
            (newInstanceMapping.SourceType, configRef.Source),
            (newInstanceMapping.TargetType, configRef.Target)
        );

        if (!typeCheckerResult.Success)
        {
            if (ReferenceEquals(configRef.Source, typeCheckerResult.FailedArgument))
            {
                _diagnostics.ReportDiagnostic(
                    DiagnosticDescriptors.SourceTypeIsNotAssignableToTheIncludedSourceType,
                    configRef.Method,
                    configRef.Source,
                    newInstanceMapping.SourceType
                );
            }
            else
            {
                _diagnostics.ReportDiagnostic(
                    DiagnosticDescriptors.TargetTypeIsNotAssignableToTheIncludedTargetType,
                    configRef.Method,
                    configRef.Target,
                    newInstanceMapping.TargetType
                );
            }

            return false;
        }

        if (methodSymbol == null)
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingNotFound, configRef.Method, includeMapping);
            return false;
        }

        return true;
    }

    private IReadOnlyCollection<DerivedTypeMappingConfiguration> BuildDerivedTypeConfigs(IMethodSymbol method)
    {
        return _dataAccessor
            .Access<MapDerivedTypeAttribute, DerivedTypeMappingConfiguration>(method)
            .Concat(_dataAccessor.Access<MapDerivedTypeAttribute<object, object>, DerivedTypeMappingConfiguration>(method))
            .ToList();
    }

    private MembersMappingConfiguration BuildMembersConfig(MappingConfigurationReference configRef)
    {
        if (configRef.Method == null)
            return MapperConfiguration.Members;

        var ignoredSourceMembers = _dataAccessor
            .Access<MapperIgnoreSourceAttribute>(configRef.Method)
            .Select(x => x.Source)
            .WhereNotNull()
            .ToList();
        var ignoredTargetMembers = _dataAccessor
            .Access<MapperIgnoreTargetAttribute>(configRef.Method)
            .Select(x => x.Target)
            .WhereNotNull()
            .ToList();
        var memberValueConfigurations = _dataAccessor.Access<MapValueAttribute, MemberValueMappingConfiguration>(configRef.Method).ToList();
        var memberConfigurations = _dataAccessor
            .Access<MapPropertyAttribute, MemberMappingConfiguration>(configRef.Method)
            .Concat(_dataAccessor.Access<MapPropertyFromSourceAttribute, MemberMappingConfiguration>(configRef.Method))
            .ToList();
        var nestedMembersConfigurations = _dataAccessor
            .Access<MapNestedPropertiesAttribute, NestedMembersMappingConfiguration>(configRef.Method)
            .ToList();
        var ignoreObsolete = _dataAccessor
            .AccessFirstOrDefault<MapperIgnoreObsoleteMembersAttribute>(configRef.Method)
            ?.IgnoreObsoleteStrategy;
        var requiredMapping = _dataAccessor.AccessFirstOrDefault<MapperRequiredMappingAttribute>(configRef.Method)?.RequiredMappingStrategy;

        // ignore the required mapping / ignore obsolete as the same attribute is used for other mapping types
        // e.g. enum to enum
        var hasMemberConfigs = ignoredSourceMembers.Count > 0 || ignoredTargetMembers.Count > 0 || memberConfigurations.Count > 0;
        if (hasMemberConfigs && (configRef.Source.IsEnum() || configRef.Target.IsEnum()))
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.MemberConfigurationOnNonMemberMapping, configRef.Method);
            return MapperConfiguration.Members;
        }

        if (
            hasMemberConfigs
            && configRef.Source.ImplementsGeneric(_types.Get(typeof(IQueryable<>)), out _)
            && configRef.Target.ImplementsGeneric(_types.Get(typeof(IQueryable<>)), out _)
        )
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.MemberConfigurationOnQueryableProjectionMapping, configRef.Method);
            return MapperConfiguration.Members;
        }

        foreach (var invalidMemberConfig in memberValueConfigurations.Where(x => !x.IsValid))
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.InvalidMapValueAttributeUsage, invalidMemberConfig.Location);
        }

        foreach (var invalidMemberConfig in memberConfigurations.Where(x => !x.IsValid))
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.InvalidMapPropertyAttributeUsage, invalidMemberConfig.Location);
        }

        return new MembersMappingConfiguration(
            ignoredSourceMembers,
            ignoredTargetMembers,
            memberValueConfigurations,
            memberConfigurations,
            nestedMembersConfigurations,
            ignoreObsolete,
            requiredMapping ?? MapperConfiguration.Members.RequiredMappingStrategy
        );
    }

    private EnumMappingConfiguration BuildEnumConfig(MappingConfigurationReference configRef)
    {
        if (configRef.Method == null)
            return MapperConfiguration.Enum;

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
        var requiredMapping = _dataAccessor.AccessFirstOrDefault<MapperRequiredMappingAttribute>(configRef.Method)?.RequiredMappingStrategy;

        // ignore the required mapping as the same attribute is used for other mapping types
        // e.g. object to object
        var hasEnumConfigs = configData != null || explicitMappings.Count > 0 || ignoredSources.Count > 0 || ignoredTargets.Count > 0;
        if (hasEnumConfigs && !configRef.Source.IsEnum() && !configRef.Target.IsEnum())
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.EnumConfigurationOnNonEnumMapping, configRef.Method);
            return MapperConfiguration.Enum;
        }

        return new EnumMappingConfiguration(
            configData?.Strategy ?? MapperConfiguration.Enum.Strategy,
            configData?.IgnoreCase ?? MapperConfiguration.Enum.IgnoreCase,
            configData?.FallbackValue,
            ignoredSources,
            ignoredTargets,
            explicitMappings,
            requiredMapping ?? MapperConfiguration.Enum.RequiredMappingStrategy,
            configData?.NamingStrategy ?? MapperConfiguration.Enum.NamingStrategy
        );
    }
}
