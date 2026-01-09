using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration.MethodReferences;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration;

public class MapperConfigurationReader
{
    private readonly Dictionary<MappingConfigurationReference, MappingConfiguration> _resolvedConfigurations = new();
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
                FallbackValue: null,
                [],
                [],
                [],
                mapper.RequiredEnumMappingStrategy,
                mapper.EnumNamingStrategy
            ),
            new MembersMappingConfiguration([], [], [], [], [], mapper.IgnoreObsoleteMembersStrategy, mapper.RequiredMappingStrategy),
            [],
            mapper.UseDeepCloning,
            mapper.StackCloningStrategy,
            supportedFeatures
        );
    }

    public MappingConfiguration MapperConfiguration { get; }

    public MappingConfiguration BuildFor(MappingConfigurationReference reference, bool supportsDeepCloning)
    {
        if (_resolvedConfigurations.TryGetValue(reference, out var resolved))
        {
            return resolved;
        }

        return BuildWithIncludedMappings([], reference, supportsDeepCloning);
    }

    private MappingConfiguration BuildWithIncludedMappings(
        HashSet<IMethodSymbol> visitedMethods,
        MappingConfigurationReference reference,
        bool supportsDeepCloning,
        bool reverse = false
    )
    {
        if (reference.Method == null)
        {
            return supportsDeepCloning ? MapperConfiguration : MapperConfiguration with { UseDeepCloning = false };
        }

        var enumConfig = BuildEnumConfig(reference, reverse);
        var membersConfig = BuildMembersConfig(reference, reverse);
        var derivedTypesConfig = BuildDerivedTypeConfigs(reference.Method, reverse);

        var configuration = new MappingConfiguration(
            MapperConfiguration.Mapper,
            enumConfig,
            membersConfig,
            derivedTypesConfig,
            supportsDeepCloning && MapperConfiguration.Mapper.UseDeepCloning,
            MapperConfiguration.StackCloningStrategy,
            MapperConfiguration.SupportedFeatures
        );

        configuration = IncludeConfigurations(configuration, visitedMethods, reference, supportsDeepCloning);
        _resolvedConfigurations[reference] = configuration;

        return configuration;
    }

    private EnumMappingConfiguration BuildEnumConfig(MappingConfigurationReference configRef, bool reverse = false)
    {
        if (configRef.Method == null)
        {
            return MapperConfiguration.Enum;
        }

        var configData = _dataAccessor.AccessFirstOrDefault<MapEnumAttribute, EnumConfiguration>(configRef.Method);
        var explicitMappings = _dataAccessor
            .Access<MapEnumValueAttribute, EnumValueMappingConfiguration>(configRef.Method, reverse)
            .ToList();

        var ignoredSources = _dataAccessor
            .Access<MapperIgnoreSourceValueAttribute, MapperIgnoreEnumValueConfiguration>(configRef.Method)
            .Select(x => x.Value)
            .ToList();
        var ignoredTargets = _dataAccessor
            .Access<MapperIgnoreTargetValueAttribute, MapperIgnoreEnumValueConfiguration>(configRef.Method)
            .Select(x => x.Value)
            .ToList();
        if (reverse)
        {
            (ignoredSources, ignoredTargets) = (ignoredTargets, ignoredSources);
        }

        var requiredMapping = _dataAccessor
            .AccessFirstOrDefault<MapperRequiredMappingAttribute, MapperRequiredMappingConfiguration>(configRef.Method, reverse)
            ?.RequiredMappingStrategy;

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

    private MembersMappingConfiguration BuildMembersConfig(MappingConfigurationReference configRef, bool reverse = false)
    {
        if (configRef.Method == null)
        {
            return MapperConfiguration.Members;
        }

        if (reverse)
        {
            configRef = configRef with { Target = configRef.Source, Source = configRef.Target, Reverse = true };
        }

        var (ignoredSourceMembers, ignoredTargetMembers) = GetIgnoredMembers(configRef, reverse);

        var memberValueConfigurations = _dataAccessor.Access<MapValueAttribute, MemberValueMappingConfiguration>(configRef.Method).ToList();
        var memberConfigurations = _dataAccessor
            .Access<MapPropertyAttribute, MemberMappingConfiguration>(configRef.Method, reverse)
            .Concat(_dataAccessor.Access<MapPropertyFromSourceAttribute, MemberMappingConfiguration>(configRef.Method))
            .ToList();
        var nestedMembersConfigurations = _dataAccessor
            .Access<MapNestedPropertiesAttribute, NestedMembersMappingConfiguration>(configRef.Method)
            .ToList();
        var ignoreObsolete = _dataAccessor
            .AccessFirstOrDefault<MapperIgnoreObsoleteMembersAttribute>(configRef.Method)
            ?.IgnoreObsoleteStrategy;
        var requiredMapping = _dataAccessor
            .AccessFirstOrDefault<MapperRequiredMappingAttribute, MapperRequiredMappingConfiguration>(configRef.Method, reverse)
            ?.RequiredMappingStrategy;

        var hasMemberConfigs = ignoredSourceMembers.Count > 0 || ignoredTargetMembers.Count > 0 || memberConfigurations.Count > 0;

        if (hasMemberConfigs && (configRef.Source.IsEnum() || configRef.Target.IsEnum()))
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.MemberConfigurationOnNonMemberMapping, configRef.Method);
            return MapperConfiguration.Members;
        }

        if (hasMemberConfigs && IsQueryableToQueryableMapping(configRef))
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.MemberConfigurationOnQueryableProjectionMapping, configRef.Method);
            return MapperConfiguration.Members;
        }

        ValidateMemberConfigurations(memberValueConfigurations, memberConfigurations);

        return new MembersMappingConfiguration(
            ignoredSourceMembers,
            ignoredTargetMembers,
            memberValueConfigurations,
            memberConfigurations,
            nestedMembersConfigurations,
            ignoreObsolete,
            requiredMapping
        );
    }

    private IReadOnlyCollection<DerivedTypeMappingConfiguration> BuildDerivedTypeConfigs(IMethodSymbol method, bool reverse = false)
    {
        return _dataAccessor
            .Access<MapDerivedTypeAttribute, DerivedTypeMappingConfiguration>(method, reverse)
            .Concat(_dataAccessor.Access<MapDerivedTypeAttribute<object, object>, DerivedTypeMappingConfiguration>(method, reverse))
            .ToList();
    }

    private MappingConfiguration IncludeConfigurations(
        MappingConfiguration configuration,
        HashSet<IMethodSymbol> visitedMethods,
        MappingConfigurationReference reference,
        bool supportsDeepCloning
    )
    {
        var includedMappingConfigs = _dataAccessor.Access<IncludeMappingConfigurationAttribute, IncludeMappingConfiguration>(
            reference.Method!
        );

        foreach (var includedCfg in includedMappingConfigs)
        {
            var reverse = includedCfg.Reverse ?? false;
            var cfg = BuildConfigToInclude(includedCfg.Name, reference, visitedMethods, supportsDeepCloning, reverse);

            if (cfg != null)
            {
                configuration = configuration.Include(cfg);
            }
        }

        return configuration;
    }

    private MappingConfiguration? BuildConfigToInclude(
        IMethodReferenceConfiguration name,
        MappingConfigurationReference configRef,
        HashSet<IMethodSymbol> visitedMethods,
        bool supportsDeepCloning,
        bool reverse = false
    )
    {
        var typeMapping =
            (ITypeMapping?)_mappings.FindNamedNewInstanceMapping(name.FullName, out var ambiguousName)
            ?? _mappings.FindExistingInstanceNamedMapping(name.FullName, out ambiguousName);

        var methodSymbol = (typeMapping as IUserMapping)?.Method;

        if (!ValidateIncludedConfig(ambiguousName, configRef, name.FullName, typeMapping, methodSymbol, reverse))
        {
            return null;
        }

        if (!visitedMethods.Add(methodSymbol))
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.CircularReferencedMapping, methodSymbol, methodSymbol.ToDisplayString());
            return null;
        }

        var sourceType = reverse ? typeMapping.TargetType : typeMapping.SourceType;
        var targetType = reverse ? typeMapping.SourceType : typeMapping.TargetType;

        var includedReference = new MappingConfigurationReference(methodSymbol, sourceType, targetType);

        if (!_resolvedConfigurations.TryGetValue(includedReference, out var config))
        {
            config = BuildWithIncludedMappings(visitedMethods, includedReference, supportsDeepCloning, reverse);
        }

        return config;
    }

    private bool ValidateIncludedConfig(
        bool ambiguousName,
        MappingConfigurationReference configRef,
        string mappingToIncludeName,
        [NotNullWhen(true)] ITypeMapping? mappingToInclude,
        [NotNullWhen(true)] IMethodSymbol? mappingToIncludeMethodSymbol,
        bool reverse = false
    )
    {
        if (ambiguousName)
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingAmbiguous, configRef.Method, mappingToIncludeName);
            return false;
        }

        if (mappingToInclude is null || mappingToIncludeMethodSymbol is null)
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingNotFound, configRef.Method, mappingToIncludeName);
            return false;
        }

        var sourceType = reverse ? mappingToInclude.TargetType : mappingToInclude.SourceType;
        var targetType = reverse ? mappingToInclude.SourceType : mappingToInclude.TargetType;

        var typeCheckerResult = _genericTypeChecker.InferAndCheckTypes(
            configRef.Method!.TypeParameters,
            (sourceType, configRef.Source),
            (targetType, configRef.Target)
        );

        if (typeCheckerResult.Success)
        {
            return true;
        }

        if (ReferenceEquals(configRef.Source, typeCheckerResult.FailedArgument))
        {
            _diagnostics.ReportDiagnostic(
                DiagnosticDescriptors.SourceTypeIsNotAssignableToTheIncludedSourceType,
                configRef.Method,
                configRef.Source,
                sourceType
            );
        }
        else
        {
            _diagnostics.ReportDiagnostic(
                DiagnosticDescriptors.TargetTypeIsNotAssignableToTheIncludedTargetType,
                configRef.Method,
                configRef.Target,
                targetType
            );
        }

        return false;
    }

    private void ValidateMemberConfigurations(
        List<MemberValueMappingConfiguration> memberValueConfigurations,
        List<MemberMappingConfiguration> memberConfigurations
    )
    {
        foreach (var invalidMemberConfig in memberValueConfigurations.Where(x => !x.IsValid))
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.InvalidMapValueAttributeUsage, invalidMemberConfig.Location);
        }

        foreach (var invalidMemberConfig in memberConfigurations.Where(x => !x.IsValid))
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.InvalidMapPropertyAttributeUsage, invalidMemberConfig.Location);
        }
    }

    private (List<string> IgnoredSourceMembers, List<string> IgnoredTargetMembers) GetIgnoredMembers(
        MappingConfigurationReference configRef,
        bool reverse
    )
    {
        var ignoredSourceMembers = _dataAccessor
            .Access<MapperIgnoreSourceAttribute>(configRef.Method!)
            .Select(x => x.Source)
            .WhereNotNull()
            .ToList();

        var ignoredTargetMembers = _dataAccessor
            .Access<MapperIgnoreTargetAttribute>(configRef.Method!)
            .Select(x => x.Target)
            .WhereNotNull()
            .ToList();

        return reverse ? (ignoredTargetMembers, ignoredSourceMembers) : (ignoredSourceMembers, ignoredTargetMembers);
    }

    private bool IsQueryableToQueryableMapping(MappingConfigurationReference configRef)
    {
        var queryableType = _types.Get(typeof(IQueryable<>));
        return configRef.Source.ImplementsGeneric(queryableType, out _) && configRef.Target.ImplementsGeneric(queryableType, out _);
    }
}
