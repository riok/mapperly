using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public class ObjectPropertyMappingBuilderContext<T>
    : ObjectPropertyMappingBuilderContext
    where T : IPropertyAssignmentTypeMapping
{
    public ObjectPropertyMappingBuilderContext(MappingBuilderContext builderContext, T mapping)
        : base(builderContext, mapping)
    {
        Mapping = mapping;
    }

    protected new T Mapping { get; }
}

public class ObjectPropertyMappingBuilderContext
{
    private readonly Dictionary<PropertyPath, PropertyNullDelegateAssignmentMapping> _nullDelegateMappings = new();
    private readonly HashSet<string> _unmappedSourcePropertyNames;
    private readonly IReadOnlyCollection<string> _ignoredUnmatchedTargetPropertyNames;
    private readonly IReadOnlyCollection<string> _ignoredUnmatchedSourcePropertyNames;

    protected ObjectPropertyMappingBuilderContext(MappingBuilderContext builderContext, IPropertyAssignmentTypeMapping mapping)
    {
        BuilderContext = builderContext;
        Mapping = mapping;

        _unmappedSourcePropertyNames = GetSourcePropertyNames();
        TargetProperties = GetTargetProperties();

        var ignoredTargetPropertyNames = GetIgnoredTargetProperties();
        IgnoredSourcePropertyNames = GetIgnoredSourceProperties();

        _ignoredUnmatchedSourcePropertyNames = InitIgnoredUnmatchedProperties(IgnoredSourcePropertyNames, _unmappedSourcePropertyNames);
        _ignoredUnmatchedTargetPropertyNames = InitIgnoredUnmatchedProperties(ignoredTargetPropertyNames, TargetProperties.Keys);

        _unmappedSourcePropertyNames.ExceptWith(IgnoredSourcePropertyNames);
        TargetProperties.RemoveRange(ignoredTargetPropertyNames);

        PropertyConfigsByRootTargetName = GetPropertyConfigurations();
    }

    public MappingBuilderContext BuilderContext { get; }

    public IPropertyAssignmentTypeMapping Mapping { get; }

    public IReadOnlyCollection<string> IgnoredSourcePropertyNames { get; }

    public Dictionary<string, IPropertySymbol> TargetProperties { get; }

    public Dictionary<string, List<MapPropertyAttribute>> PropertyConfigsByRootTargetName { get; }

    public void AddDiagnostics()
    {
        AddUnmatchedIgnoredTargetPropertiesDiagnostics();
        AddUnmatchedIgnoredSourcePropertiesDiagnostics();
        AddUnmatchedTargetPropertiesDiagnostics();
        AddUnmatchedSourcePropertiesDiagnostics();
    }

    public void AddPropertyAssignmentMapping(IPropertyAssignmentMapping propertyMapping)
        => AddPropertyAssignmentMapping(Mapping, propertyMapping);

    public void AddNullDelegatePropertyAssignmentMapping(IPropertyAssignmentMapping propertyMapping)
    {
        var nullConditionSourcePath = new PropertyPath(propertyMapping.SourcePath.PathWithoutTrailingNonNullable().ToList());
        var container = GetOrCreateNullDelegateMappingForPath(nullConditionSourcePath);
        AddPropertyAssignmentMapping(container, propertyMapping);
    }

    private void AddPropertyAssignmentMapping(IPropertyAssignmentMappingContainer container, IPropertyAssignmentMapping mapping)
    {
        SetSourcePropertyMapped(mapping.SourcePath);
        AddNullPropertyInitializers(container, mapping.TargetPath);
        container.AddPropertyMapping(mapping);
    }

    protected void SetSourcePropertyMapped(PropertyPath sourcePath)
        => _unmappedSourcePropertyNames.Remove(sourcePath.Path.First().Name);

    private void AddNullPropertyInitializers(IPropertyAssignmentMappingContainer container, PropertyPath path)
    {
        foreach (var nullableTrailPath in path.ObjectPathNullableSubPaths())
        {
            var nullablePath = new PropertyPath(nullableTrailPath);
            var type = nullablePath.Member.Type;
            if (!type.HasAccessibleParameterlessConstructor())
            {
                BuilderContext.ReportDiagnostic(
                    DiagnosticDescriptors.NoParameterlessConstructorFound,
                    type);
                continue;
            }

            container.AddPropertyMappingContainer(new PropertyNullAssignmentInitializerMapping(nullablePath));
        }
    }

    private PropertyNullDelegateAssignmentMapping GetOrCreateNullDelegateMappingForPath(PropertyPath nullConditionSourcePath)
    {
        // if there is already an exact match return that
        if (_nullDelegateMappings.TryGetValue(nullConditionSourcePath, out var mapping))
            return mapping;

        IPropertyAssignmentMappingContainer parentMapping = Mapping;

        // try to reuse parent path mappings and wrap inside them
        foreach (var nullablePath in nullConditionSourcePath.ObjectPathNullableSubPaths().Reverse())
        {
            if (_nullDelegateMappings.TryGetValue(new PropertyPath(nullablePath), out var parentMappingHolder))
            {
                parentMapping = parentMappingHolder;
            }
        }

        mapping = new PropertyNullDelegateAssignmentMapping(
            nullConditionSourcePath,
            parentMapping,
            BuilderContext.MapperConfiguration.ThrowOnPropertyMappingNullMismatch);
        _nullDelegateMappings[nullConditionSourcePath] = mapping;
        parentMapping.AddPropertyMappingContainer(mapping);
        return mapping;
    }

    private HashSet<string> InitIgnoredUnmatchedProperties(IEnumerable<string> allProperties, IEnumerable<string> mappedProperties)
    {
        var unmatched = new HashSet<string>(allProperties);
        unmatched.ExceptWith(mappedProperties);
        return unmatched;
    }

    private HashSet<string> GetIgnoredTargetProperties()
    {
        return BuilderContext
            .ListConfiguration<MapperIgnoreTargetAttribute>()
            .Select(x => x.Target)
            // deprecated MapperIgnoreAttribute, but it is still supported by Mapperly.
#pragma warning disable CS0618
            .Concat(BuilderContext.ListConfiguration<MapperIgnoreAttribute>().Select(x => x.Target))
#pragma warning restore CS0618
            .ToHashSet();
    }

    private HashSet<string> GetIgnoredSourceProperties()
    {
        return BuilderContext
            .ListConfiguration<MapperIgnoreSourceAttribute>()
            .Select(x => x.Source)
            .ToHashSet();
    }

    private HashSet<string> GetSourcePropertyNames()
    {
        return Mapping.SourceType
            .GetAllAccessibleProperties()
            .Select(x => x.Name)
            .ToHashSet();
    }

    private Dictionary<string, IPropertySymbol> GetTargetProperties()
    {
        return Mapping.TargetType
            .GetAllAccessibleProperties()
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    private Dictionary<string, List<MapPropertyAttribute>> GetPropertyConfigurations()
    {
        return BuilderContext
            .ListConfiguration<MapPropertyAttribute>()
            .GroupBy(x => x.Target.First())
            .ToDictionary(x => x.Key, x => x.ToList());
    }

    private void AddUnmatchedIgnoredTargetPropertiesDiagnostics()
    {
        foreach (var notFoundIgnoredProperty in _ignoredUnmatchedTargetPropertyNames)
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.IgnoredTargetPropertyNotFound,
                notFoundIgnoredProperty,
                Mapping.TargetType);
        }
    }

    private void AddUnmatchedIgnoredSourcePropertiesDiagnostics()
    {
        foreach (var notFoundIgnoredProperty in _ignoredUnmatchedSourcePropertyNames)
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.IgnoredSourcePropertyNotFound,
                notFoundIgnoredProperty,
                Mapping.SourceType);
        }
    }

    private void AddUnmatchedTargetPropertiesDiagnostics()
    {
        foreach (var propertyConfig in PropertyConfigsByRootTargetName.Values.SelectMany(x => x))
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingTargetPropertyNotFound,
                propertyConfig.TargetFullName,
                Mapping.TargetType);
        }
    }

    private void AddUnmatchedSourcePropertiesDiagnostics()
    {
        foreach (var sourcePropertyName in _unmappedSourcePropertyNames)
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.SourcePropertyNotMapped,
                sourcePropertyName,
                Mapping.SourceType,
                Mapping.TargetType);
        }
    }
}
