using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Represents a complex object mapping implemented in its own method.
/// Maps each property from the source to the target.
/// </summary>
public class ObjectPropertyExistingTargetMapping : PropertyAssignmentMappingContainer, IExistingTargetMapping, IPropertyAssignmentTypeMapping
{
    public ObjectPropertyExistingTargetMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        SourceType = sourceType;
        TargetType = targetType;
    }

    public ITypeSymbol SourceType { get; }

    public ITypeSymbol TargetType { get; }
}
