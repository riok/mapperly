using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A mapping from type to another by mapping each property.
/// A <see cref="MethodMapping"/> implementation of <see cref="IMemberAssignmentTypeMapping"/>.
/// </summary>
public abstract class ObjectMemberMethodMapping :
    MethodMapping,
    IMemberAssignmentTypeMapping
{
    private readonly ObjectMemberExistingTargetMapping _mapping;

    protected ObjectMemberMethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        : base(sourceType, targetType)
    {
        _mapping = new ObjectMemberExistingTargetMapping(sourceType, targetType);
    }

    public bool HasMemberMapping(IMemberAssignmentMapping mapping)
        => _mapping.HasMemberMapping(mapping);

    public void AddMemberMapping(IMemberAssignmentMapping mapping)
        => _mapping.AddMemberMapping(mapping);

    public bool HasMemberMappingContainer(IMemberAssignmentMappingContainer container)
        => _mapping.HasMemberMappingContainer(container);

    public void AddMemberMappingContainer(IMemberAssignmentMappingContainer container)
        => _mapping.AddMemberMappingContainer(container);

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
        => BuildBody(ctx, targetAccess);

    protected IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx, ExpressionSyntax target)
        => _mapping.Build(ctx, target);
}
