using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Represents a complex object mapping implemented in its own method.
/// Maps each property from the source to the target.
/// </summary>
public class ObjectMemberExistingTargetMapping : MemberAssignmentMappingContainer, IExistingTargetMapping, IMemberAssignmentTypeMapping
{
    public ObjectMemberExistingTargetMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        SourceType = sourceType;
        TargetType = targetType;
    }

    public ITypeSymbol SourceType { get; }

    public ITypeSymbol TargetType { get; }

    public bool CallableByOtherMappings => true;

    public bool IsSynthetic => false;

    public MappingBodyBuildingPriority BodyBuildingPriority => MappingBodyBuildingPriority.Default;
}
