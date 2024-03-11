using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Represents a complex object mapping implemented in its own method.
/// Maps each property from the source to the target.
/// </summary>
[DebuggerDisplay("{GetType().Name}({SourceType} => {TargetType})")]
public class ObjectMemberExistingTargetMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    : MemberAssignmentMappingContainer,
        IExistingTargetMapping,
        IMemberAssignmentTypeMapping
{
    public ITypeSymbol SourceType { get; } = sourceType;

    public ITypeSymbol TargetType { get; } = targetType;

    public bool IsSynthetic => false;
}
