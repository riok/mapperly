using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public class ObjectPropertyMappingBuilderContext
{
    private readonly Dictionary<PropertyPath, PropertyNullDelegateMapping> _nullDelegateMappings = new();

    public ObjectPropertyMappingBuilderContext(MappingBuilderContext builderContext, ObjectPropertyMapping mapping)
    {
        BuilderContext = builderContext;
        Mapping = mapping;
    }

    public MappingBuilderContext BuilderContext { get; }

    public ObjectPropertyMapping Mapping { get; }

    public void AddPropertyMapping(PropertyMapping propertyMapping)
        => AddPropertyMapping(Mapping, propertyMapping);

    public void AddNullDelegatePropertyMapping(PropertyMapping propertyMapping)
    {
        var nullConditionSourcePath = new PropertyPath(propertyMapping.SourcePath.PathWithoutTrailingNonNullable().ToList());
        var container = GetOrCreateNullDelegateMappingForPath(nullConditionSourcePath);
        AddPropertyMapping(container, propertyMapping);
    }

    private void AddPropertyMapping(IPropertyMappingContainer container, PropertyMapping mapping)
    {
        container.AddPropertyMappings(BuildNullPropertyInitializers(mapping.TargetPath));
        container.AddPropertyMapping(mapping);
    }

    private IEnumerable<PropertyNullInitializerDelegateMapping> BuildNullPropertyInitializers(PropertyPath path)
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

            yield return new PropertyNullInitializerDelegateMapping(nullablePath);
        }
    }

    private PropertyNullDelegateMapping GetOrCreateNullDelegateMappingForPath(PropertyPath nullConditionSourcePath)
    {
        // if there is already an exact match return that
        if (_nullDelegateMappings.TryGetValue(nullConditionSourcePath, out var mapping))
            return mapping;

        IPropertyMappingContainer parentMapping = Mapping;

        // try to reuse parent path mappings and wrap inside them
        foreach (var nullablePath in nullConditionSourcePath.ObjectPathNullableSubPaths().Reverse())
        {
            if (_nullDelegateMappings.TryGetValue(new PropertyPath(nullablePath), out var parentMappingHolder))
            {
                parentMapping = parentMappingHolder;
            }
        }

        mapping = new PropertyNullDelegateMapping(
            nullConditionSourcePath,
            parentMapping,
            BuilderContext.MapperConfiguration.ThrowOnPropertyMappingNullMismatch);
        _nullDelegateMappings[nullConditionSourcePath] = mapping;
        parentMapping.AddPropertyMapping(mapping);
        return mapping;
    }
}
