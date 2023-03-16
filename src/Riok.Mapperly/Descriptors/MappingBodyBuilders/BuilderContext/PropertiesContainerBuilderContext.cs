using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An <see cref="IPropertiesContainerBuilderContext{T}"/> implementation.
/// </summary>
/// <typeparam name="T">The type of mapping.</typeparam>
public class PropertiesContainerBuilderContext<T> :
    PropertiesMappingBuilderContext<T>,
    IPropertiesContainerBuilderContext<T>
    where T : IPropertyAssignmentTypeMapping
{
    private readonly Dictionary<PropertyPath, PropertyNullDelegateAssignmentMapping> _nullDelegateMappings = new();

    public PropertiesContainerBuilderContext(
        MappingBuilderContext builderContext,
        T mapping)
        : base(builderContext, mapping)
    {
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
}
