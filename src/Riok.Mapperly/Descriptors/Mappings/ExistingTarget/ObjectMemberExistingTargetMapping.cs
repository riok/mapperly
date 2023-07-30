using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Symbols;

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

    public ImmutableEquatableArray<MethodParameter> Parameters { get; } = ImmutableEquatableArray<MethodParameter>.Empty;

    public MappingBodyBuildingPriority BodyBuildingPriority => MappingBodyBuildingPriority.Default;
}
