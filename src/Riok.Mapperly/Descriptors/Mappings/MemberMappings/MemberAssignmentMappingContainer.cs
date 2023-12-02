using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// A default implementation for <see cref="IMemberAssignmentMappingContainer"/>.
/// </summary>
public abstract class MemberAssignmentMappingContainer(IMemberAssignmentMappingContainer? parent = null) : IMemberAssignmentMappingContainer
{
    private readonly HashSet<IMemberAssignmentMapping> _delegateMappings = new();
    private readonly HashSet<IMemberAssignmentMappingContainer> _childContainers = new();

    public virtual IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        var childContainerStatements = _childContainers.SelectMany(x => x.Build(ctx, targetAccess));
        var mappings = _delegateMappings.OrderBy(x => x.TargetPath.Path.Count).SelectMany(m => m.Build(ctx, targetAccess));
        return childContainerStatements.Concat(mappings);
    }

    public void AddMemberMappingContainer(IMemberAssignmentMappingContainer container)
    {
        if (!HasMemberMappingContainer(container))
        {
            _childContainers.Add(container);
        }
    }

    public bool HasMemberMappingContainer(IMemberAssignmentMappingContainer container) =>
        _childContainers.Contains(container) || parent?.HasMemberMappingContainer(container) == true;

    public void AddMemberMapping(IMemberAssignmentMapping mapping)
    {
        if (!HasMemberMapping(mapping))
        {
            _delegateMappings.Add(mapping);
        }
    }

    public bool HasMemberMapping(IMemberAssignmentMapping mapping) =>
        _delegateMappings.Contains(mapping) || parent?.HasMemberMapping(mapping) == true;
}
