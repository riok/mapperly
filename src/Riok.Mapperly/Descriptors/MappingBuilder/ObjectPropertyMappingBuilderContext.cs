using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public class ObjectPropertyMappingBuilderContext<T>
    : ObjectPropertyMappingBuilderContext
    where T : ObjectPropertyMapping
{
    public ObjectPropertyMappingBuilderContext(MappingBuilderContext builderContext, T mapping)
        : base(builderContext, mapping)
    {
        Mapping = mapping;
    }

    public new T Mapping { get; }
}

public class ObjectPropertyMappingBuilderContext
{
    private readonly Dictionary<PropertyPath, PropertyNullDelegateAssignmentMapping> _nullDelegateMappings = new();

    public ObjectPropertyMappingBuilderContext(MappingBuilderContext builderContext, ObjectPropertyMapping mapping)
    {
        BuilderContext = builderContext;
        Mapping = mapping;

        IgnoredTargetProperties = GetIgnoredTargetProperties();
        TargetProperties = GetTargetProperties();
        PropertyConfigsByRootTargetName = GetPropertyConfigurations();
    }

    public MappingBuilderContext BuilderContext { get; }

    public ObjectPropertyMapping Mapping { get; }

    public HashSet<string> IgnoredTargetProperties { get; }

    public Dictionary<string, IPropertySymbol> TargetProperties { get; }

    public Dictionary<string, List<MapPropertyAttribute>> PropertyConfigsByRootTargetName { get; }

    public void AddPropertyAssignmentMapping(PropertyAssignmentMapping propertyMapping)
        => AddPropertyAssignmentMapping(Mapping, propertyMapping);

    public void AddUnmatchedIgnoredPropertiesDiagnostics()
    {
        foreach (var notFoundIgnoredProperty in IgnoredTargetProperties)
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.IgnoredPropertyNotFound,
                notFoundIgnoredProperty,
                Mapping.TargetType);
        }
    }

    public void AddUnmatchedTargetPropertiesDiagnostics()
    {
        foreach (var propertyConfig in PropertyConfigsByRootTargetName.Values.SelectMany(x => x))
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingTargetPropertyNotFound,
                propertyConfig.TargetFullName,
                Mapping.TargetType);
        }
    }

    public void AddNullDelegatePropertyAssignmentMapping(PropertyAssignmentMapping propertyMapping)
    {
        var nullConditionSourcePath = new PropertyPath(propertyMapping.SourcePath.PathWithoutTrailingNonNullable().ToList());
        var container = GetOrCreateNullDelegateMappingForPath(nullConditionSourcePath);
        AddPropertyAssignmentMapping(container, propertyMapping);
    }

    private void AddPropertyAssignmentMapping(IPropertyAssignmentMappingContainer container, PropertyAssignmentMapping mapping)
    {
        container.AddPropertyMappings(BuildNullPropertyInitializers(mapping.TargetPath));
        container.AddPropertyMapping(mapping);
    }

    private IEnumerable<PropertyNullAssignmentInitializerMapping> BuildNullPropertyInitializers(PropertyPath path)
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

            yield return new PropertyNullAssignmentInitializerMapping(nullablePath);
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
        parentMapping.AddPropertyMapping(mapping);
        return mapping;
    }

    private HashSet<string> GetIgnoredTargetProperties()
    {
        return BuilderContext
            .ListConfiguration<MapperIgnoreAttribute>()
            .Select(x => x.Target)
            .ToHashSet();
    }

    private Dictionary<string, IPropertySymbol> GetTargetProperties()
    {
        return Mapping.TargetType
            .GetAllMembers()
            .OfType<IPropertySymbol>()
            .Where(x => x.IsAccessible())
            .DistinctBy(x => x.Name)
            .Where(x => !IgnoredTargetProperties.Remove(x.Name))
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    private Dictionary<string, List<MapPropertyAttribute>> GetPropertyConfigurations()
    {
        return BuilderContext
            .ListConfiguration<MapPropertyAttribute>()
            .GroupBy(x => x.Target.First())
            .ToDictionary(x => x.Key, x => x.ToList());
    }
}
